using FlatRedBall;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Sand.Constants;
using Color = Microsoft.Xna.Framework.Color;

namespace Sand;
public enum WorldSetup
{ 
	Empty,
	StoneAroundEdges,
	StoneAroundEdges2,
	WaterBottomHalf,
	WaterSloshingAbout
}
public class Chunk 
{
	private readonly int _width;
	private readonly int _height;
	private readonly Point _origin;

	/// <summary><code>
	/// /=====================>
	/// \=====================\
	/// /=====================/
	/// \=====================\
	/// >=====================/
	/// </code></summary>
	public List<Point> Points_BottomLeftToTopRight_AlternatingRowDirection { get; private init; }


	public Chunk(Point origin, int chunkWidth, int chunkHeight)
	{
		_width = chunkWidth;
		_height = chunkHeight;
		_origin = origin;


		//=======================================================
		// Pre-Generate useful lists of points
		//=======================================================

		var leftToRight = true;

		Points_BottomLeftToTopRight_AlternatingRowDirection = new List<Point>(chunkWidth * chunkHeight);
		for (int y = origin.Y; y < origin.Y + chunkHeight; y++)
		{
			// switching horizontal scan direction for each row
			if (leftToRight)
			{
				for (int x = origin.X; x < origin.X + chunkWidth + chunkWidth; x++)
				{
					if (x >= 0 && x < STUFF_WIDTH && y >= 0 && y < STUFF_HEIGHT)
					{
						Points_BottomLeftToTopRight_AlternatingRowDirection.Add(new Point(x, y));
					}
				}
			}
			else
			{
				for (int x = origin.X + chunkWidth; x >= origin.X; x--)
				{
					if (x >= 0 && x < STUFF_WIDTH && y >= 0 && y < STUFF_HEIGHT)
					{
						Points_BottomLeftToTopRight_AlternatingRowDirection.Add(new Point(x, y));
					}
				}
			}

		}
	}
}
public class MyDataStructure
{
	private const int NOT_MOVED_DORMANT_TRIGGER = 10;

	public MyDataStructure(WorldSetup setup = WorldSetup.Empty) 
	{
		//========================================
		// PREPARE THE WORLD DATA STRUCTURE
		//========================================

		World = new Stuff[STUFF_WIDTH][];
		for (int x = 0; x < World.Length; x++)
		{
			World[x] = new Stuff[STUFF_HEIGHT];
		}

		//========================================
		// PREPARE THE SUBGRIDS FOR MULTITHREADING!
		//				(OLDE GODS WONT YE SAVE US)
		//========================================

		WorldChunks = [];

		const int chunksLong = 10;
		/*const int chunkRows = 10;
		const int chunkCols = 10;*/

		int chunkWidth = STUFF_WIDTH / chunksLong/*chunkRows*/;
		int chunkHeight = STUFF_HEIGHT / chunksLong/*chunkCols*/;

		var cursor = new Point(0, 0);
		var leftToRight = true;

		// these outer two loops are the row and cols of chunks
		for (int chunkRow = 0; chunkRow < chunksLong/*chunkCols*/; chunkRow++)
		{
			for (int chunkCol = 0; chunkCol < chunksLong/*chunkRows*/; chunkCol++)
			{
				// create new chunk
				WorldChunks.Add(new Chunk(cursor, chunkWidth, chunkHeight));

				leftToRight = !leftToRight;

				// IF NOT LAST column in row of chunks - move the cursor over one column to new chunk
				// OTHERWISE set the col back to first for new row
				cursor = new Point(chunkCol != chunksLong/*chunkCols*/ - 1 ? cursor.X + chunkWidth : 0, cursor.Y);
			}

			// move the cursor up one row to new chunk - doesn't matter if new cursor falls off the
			// grid as this is last operation of the nested loops
			cursor = new Point(cursor.X, cursor.Y + chunkHeight);

		}
		////////////////////////////////////////////////////////////////
		/// THIS WAS THE ORIGINAL CHUNKING ALG
		///////////////////////////////////////////////////////////////////
		//int chunkWidth = STUFF_WIDTH / chunksLong/*chunkRows*/;
		//int chunkHeight = STUFF_HEIGHT / chunksLong/*chunkCols*/;

		//var cursor = new Point(0, 0);
		//var leftToRight = true;

		//// these outer two loops are the row and cols of chunks
		//for (int chunkRow = 0; chunkRow < chunksLong/*chunkCols*/; chunkRow++)
		//{
		//	for (int chunkCol = 0; chunkCol < chunksLong/*chunkRows*/; chunkCol++)
		//	{

		//		// container for all points in a chunk
		//		var chunkPoints = new List<Point>(chunkWidth * chunkHeight);
		//		// inner loops are for the pixels in each chunk
		//		for (int y = cursor.Y; y < cursor.Y + chunkHeight; y++)
		//		{
		//			// switching horizontal scan direction for each row
		//			if (leftToRight)
		//			{
		//				for (int x = cursor.X; x < cursor.X + chunkWidth + chunkWidth; x++)
		//				{
		//					if (x >= 0 && x < STUFF_WIDTH && y >= 0 && y < STUFF_HEIGHT)
		//					{
		//						chunkPoints.Add(new Point(x, y));
		//					}
		//				}
		//			}
		//			else
		//			{
		//				for (int x = cursor.X + chunkWidth; x >= cursor.X; x--)
		//				{
		//					if (x >= 0 && x < STUFF_WIDTH && y >= 0 && y < STUFF_HEIGHT)
		//					{
		//						chunkPoints.Add(new Point(x, y));
		//					}
		//				}
		//			}

		//		}

		//		WorldChunks.Add(chunkPoints);

		//		leftToRight = !leftToRight;

		//		// IF NOT LAST column in row of chunks - move the cursor over one column to new chunk
		//		// OTHERWISE set the col back to first for new row
		//		cursor = new Point(chunkCol != chunksLong/*chunkCols*/ - 1 ? cursor.X + chunkWidth : 0, cursor.Y);
		//	}

		//	// move the cursor up one row to new chunk - doesn't matter if new cursor falls off the
		//	// grid as this is last operation of the nested loops
		//	cursor = new Point(cursor.X, cursor.Y + chunkHeight);

		//}
		////////////////////////////////////////////////////////////////
		switch (setup)
		{
			case WorldSetup.Empty:
			default:
				break;
			case WorldSetup.StoneAroundEdges:
				for (int x = 0; x < World.Length; x++)
				{
					for (int y = 0; y < World[x].Length; y++)
					{
						if (x == 0 || x == World.Length - 1 // if at far left or right
						|| y == 0 || y == World[x].Length - 1) // if at far top or bottom
						{
							ForceAddStuff(Stuffs.BASIC_STONE, x, y);
							//world.World[x][y] = StuffFactory.Instance.Get(Stuffs.BASIC_STONE);
						}
					}
				}
				break;
			case WorldSetup.StoneAroundEdges2:
				for (int x = 0; x < World.Length; x++)
				{
					for (int y = 0; y < World[x].Length; y++)
					{
						if (x == 0 || x == World.Length - 1 // if at far left or right
						|| y == 0 || y == World[x].Length - 1) // if at far top or bottom
						{
							ForceAddStuff(Stuffs.BASIC_STONE, x, y);
							//world.World[x][y] = StuffFactory.Instance.Get(Stuffs.BASIC_STONE);
						}
					}
				}
				break;
			case WorldSetup.WaterBottomHalf:
				for (int x = 0; x < World.Length; x++)
				{
					for (int y = 0; y < World[x].Length / 2; y++)
					{
						SafeAddStuffIfEmpty(Stuffs.BASIC_WATER, x, y);
					}
				}
				break;
			case WorldSetup.WaterSloshingAbout:
				var i = 0;
				for (int x = 0; x < World.Length; x++)
				{
					for (int y = 0; y < World[x].Length; y++)
					{
						if (y > 4)
						{
							SafeAddStuffIfEmpty(Stuffs.BASIC_WATER, x, y);
						}
						i++;
					}
				}
				break;
		}
	}
	/// <summary>Outer array is X, inner array is Y.</summary>
	public Stuff[][] World { get; private set; }
	/// <summary>Each inner list is a chunk's worth of coordinates.</summary>
	public List<Chunk> WorldChunks { get; set; }

	public void SafeAddStuffIfEmpty_InSquare(string stuffType, int x, int y, int length)
	{
		for (int i = x - length; i < x + length; i++)
		{
			for (int j = y - length; j < y + length; j++)
			{
				SafeAddStuffIfEmpty(stuffType, i, j);
			}
		}
	}
	public void ForceAddStuff_InSquare(string stuffType, int x, int y, int length)
	{
		for (int i = x - length; i < x + length; i++)
		{
			for (int j = y - length; j < y + length; j++)
			{
				ForceAddStuff(stuffType, i, j);
			}
		}
	}

	public void ForceAddStuff(string stuffType, int x, int y)
	{
		if (x >= 0 && x < World.Length && y >= 0 && y < World[0].Length)
		{
			var stuff = StuffFactory.Instance.Get(stuffType);
			World[x][y] = stuff;//.SetPosition(x, y);
								//stuff.X = x;
								//stuff.Y = y;
		}
	}

	public void SafeAddStuffIfEmpty(string stuffType, int x, int y)
	{
		if (x >= 0 && x < World.Length && y >= 0 && y < World[0].Length && World[x][y] == null)
		{
			var stuff = StuffFactory.Instance.Get(stuffType);
			World[x][y] = stuff;//.SetPosition(x, y);
								//stuff.X = x;
								//stuff.Y = y;
		}
	}

	public struct PlayerCollisionResult
	{
		public bool AllowUp;
		public bool AllowRight;
		public bool AllowDown;
		public bool AllowLeft;

		public bool InLiquid;

		public int MoveFactor;
		public int GravMagnitude;
	}

	public PlayerCollisionResult Collision(Player player, int initialGravMagnitude, int initialMoveFactor) //player
	{

		var (top, right, bottom, left) = player.GetPositionStuff();

		var adjustedGravMagnitude = initialGravMagnitude;
		var adjustedMoveFactor = initialMoveFactor;
		bool allowUp = true;
		bool allowRight = true;
		bool allowDown = true;
		bool allowLeft = true;
		bool inLiquid = false;

		//================================================
		// bottom collision
		//================================================
		#region
		// get the row directly below the sprite
		bottom--;
		// if bottom coord index safe
		if (bottom >= 0 && bottom < STUFF_HEIGHT)
		{
			// iterate across that row within the sprites x coords and check if downward movement should be arrested
			for (var x = left; x < right; x++)
			{
				// if outside the world bound just continue to next in row
				if (x < 0 || x >= STUFF_WIDTH)
				{
					continue;
				}

				// if there is something here then stop
				if (World[x][bottom] != null)
				{
					if (World[x][bottom].Phase == Phase.Liquid)
					{
						inLiquid = true;
						adjustedMoveFactor = initialMoveFactor / 2;
					}
					else
					{
						allowDown = false;
						break;
					}
					//Logger.Instance.LogInfo($"Encountering {World[x][bottom].Phase} at bottom - move factor is {moveFactor}");
				}
			}
		}
		// if bottom coord index is not safe
		else if (bottom < 0)
		{
			allowDown = false;
		}
		#endregion
		//================================================
		// TOP collision
		//================================================ww
		#region
		// get the row directly below the sprite
		top++;
		// if bottom coord index safe
		if (top >= 0 && top < STUFF_HEIGHT)
		{
			// iterate across that row within the sprites x coords and check if downward movement should be arrested
			for (var x = left; x < right; x++)
			{
				// if outside the world bound just continue to next in row
				if (x < 0 || x >= STUFF_WIDTH)
				{
					continue;
				}

				// if there is something here then stop
				if (World[x][top] != null)
				{
					if (World[x][top].Phase == Phase.Liquid)
					{
						inLiquid = true;
						adjustedMoveFactor = initialMoveFactor / 2;
					}
					else
					{
						allowUp = false;
						break;
					}
					//Logger.Instance.LogInfo($"Encountering {World[x][top].Phase} at top - move factor is {moveFactor}");
				}
			}
		}
		// if bottom coord index is not safe
		else if (top >= STUFF_HEIGHT)
		{
			allowUp = false;
		}
		#endregion
		//================================================
		// LEFT collision
		//================================================ww
		#region
		// get the row directly below the sprite
		left--;
		// if bottom coord index safe
		if (left >= 0 && left < STUFF_WIDTH)
		{
			// iterate across that row within the sprites x coords and check if downward movement should be arrested
			for (var y = top - 1; y > bottom + 1; y--)
			{
				// if outside the world bound just continue to next in row
				if (y < 0 || y >= STUFF_HEIGHT)
				{
					continue;
				}

				// if there is something here then stop
				if (World[left][y] != null)
				{
					if (World[left][y].Phase == Phase.Liquid)
					{
						inLiquid = true;
						adjustedMoveFactor = initialMoveFactor / 2;
					}
					else
					{
						allowLeft = false;
						break;
					}
					//Logger.Instance.LogInfo($"Encountering {World[left][y].Phase} at left - move factor is {moveFactor}");
				}
			}
		}
		// if bottom coord index is not safe
		else if (left < 0)
		{
			allowLeft = false;
		}
		#endregion
		//================================================
		// RIGHT collision
		//================================================ww
		#region
		// get the row directly below the sprite
		right++;
		// if bottom coord index safe
		if (right >= 0 && right < STUFF_WIDTH)
		{
			// iterate across that row within the sprites x coords and check if downward movement should be arrested
			for (var y = top - 1; y > bottom + 1; y--)
			{
				// if outside the world bound just continue to next in row
				if (y < 0 || y >= STUFF_HEIGHT)
				{
					continue;
				}

				// if there is something here then stop
				if (World[right][y] != null)
				{
					if (World[right][y].Phase == Phase.Liquid)
					{
						inLiquid = true;
						adjustedMoveFactor = initialMoveFactor / 2;
					}
					else
					{
						allowRight = false;
						break;
					}
					//Logger.Instance.LogInfo($"Encountering {World[right][y].Phase} at right - move factor is {moveFactor}");
				}
			}
		}
		// if bottom coord index is not safe
		else if (right >= STUFF_WIDTH)
		{
			allowRight = false;
		}
		#endregion

		return new PlayerCollisionResult()
		{
			AllowUp = allowUp,
			AllowRight = allowRight,
			AllowDown = allowDown,
			AllowLeft = allowLeft,
			InLiquid = inLiquid,
			MoveFactor = adjustedMoveFactor,
			GravMagnitude = adjustedGravMagnitude
		};
	}

	public Color[] GetColorData() 
	{
		var colorData = new Color[STUFF_HEIGHT * STUFF_WIDTH];
		var colorIndex = 0;

		for (var y = World[0].Length - 1; y >= 0; y--)
		{
			for (var x = 0; x < World.Length; x++)
			{
				colorData[colorIndex++] = World[x][y] == null ? Color.White : World[x][y].Color;
			}
		}

		return colorData;
	}

	public Stuff Get(Point p)
	{
		return Get(p.X, p.Y);
	}

	public Stuff Get(int x, int y)
	{
		return World[x][y];
	}

	public bool Move(Point from, Point to)
	{
		var stuffAtSource = World[from.X][from.Y];
		var stuffAtTarget = World[to.X][to.Y];

		if (stuffAtSource == null) return true;

		var didMove = stuffAtSource.Phase switch
		{
			Phase.Powder => MovePowder(from, to),
			Phase.Liquid => MoveLiquid(from, to),
			_ => false,
		};

		if (!didMove)
		{
			if (stuffAtSource.DormantChecks > NOT_MOVED_DORMANT_TRIGGER)
			{
				stuffAtSource.Dormant = true;
			}

			//if (stuffAtTarget != null)
			//{
			//	if (stuffAtTarget.NotMovedCount > NOT_MOVED_DORMANT_TRIGGER)
			//	{
			//		stuffAtTarget.Dormant = true;
			//	}
			//}
		}
		else
		{
			stuffAtSource.Dormant = false;
			if (stuffAtTarget != null)
			{
				stuffAtTarget.Dormant = false;
			}
		}

		return false;

		bool MovePowder(Point from, Point to)
		{
			// check for stuff at target
			var didMove = false;

			Stuff stuffTarget = Get(to);
			var sourceHasStuff = stuffAtSource != null;
			var targetHasStuff = stuffTarget != null;

			// if not stuff at target fall to here and finish
			if (stuffTarget == null // nothing at target
			|| stuffTarget is not { Phase: Phase.Solid or Phase.Powder }) // or thing at target is not a solid
			{
				World[to.X][to.Y] = stuffAtSource;
				World[from.X][from.Y] = null;
				didMove = true;
			}

			var hasDisplacedLiquid = false;
			if (stuffTarget != null && stuffTarget is { Phase: Phase.Liquid })
			{
				hasDisplacedLiquid = LiquidDisplacement();
			}

			return didMove || hasDisplacedLiquid;

			bool LiquidDisplacement()
			{
				//=======================================================================
				// LIQUID DISPLACEMENT
				//=======================================================================
				var hasDisplaced = false;

				// if stuff at the target, but target NOT solid (we know that source IS solid), then
				// fall here - liquid, gas displacement means the thing pushed out of the way has
				// to go up
				if (targetHasStuff && stuffTarget is not { Phase: Phase.Solid or Phase.Powder })
				{
					// up upwards in Y-axis (checkling one left and right also) from displaced liqud
					// until empty stuff found - staying within the water column.
					// Once we fall out of bounds of array just stop looping as the target is already
					// garbage awaiting collection.
					var waterColumnX = to.X;
					for (int cursorY = to.Y; !hasDisplaced && cursorY < World[0].Length; cursorY++)
					{

						///////////////////////////////////////////////////////
						// lets try randomly going left, on or right of yCoord in water column droping
						// there if empty, so 1/3 itll land left, on or right of cursorY
						///////////////////////////////////////////////////////
						for (var i = 0; !hasDisplaced && i < Randoms.Instance.Ind_leftRightMid.Length; i++)
						{
							var adjCursorX = waterColumnX + Randoms.Instance.Ind_leftRightMid[i];
							if (adjCursorX >= 0 && adjCursorX < World.Length && World[adjCursorX][cursorY] == null)
							{
								// move this displaced liquid to here
								World[adjCursorX][cursorY] = stuffTarget;//.SetPosition(adjCursorX, cursorY); ;
								hasDisplaced = true;// BREAKS both loops
								stuffAtTarget.Dormant = false;
							}
						}
					}

					// no empty spot found so "destroy" the displayers stuff (for now)
					// TODO this will need updated when screen can move
				}
				//}

				return hasDisplaced;
			}
		}

		bool MoveLiquid(Point from, Point to)
		{
			var stuffSource = World[from.X][from.Y];
			var stuffTarget = World[to.X][to.Y];
			// if not stuff at target fall to here and finish
			if (stuffTarget == null)
			{
				// update world
				World[to.X][to.Y] = stuffSource;
				World[from.X][from.Y] = null;
				return true;
			}

			return false;
		}
	}

	public List<Chunk> GetChunksToUpdate()
	{
		var chunksToUpdate = new List<Chunk>();

		try
		{
			#region Every other chunk, oscillating first chunk position

			// choose which chunks to update
			var currentFrame = TimeManager.CurrentFrame;
			var alteratingSeed = currentFrame % 2 == 0;

			for (int iChunk = 0; iChunk < WorldChunks.Count; iChunk++)
			{
				// this if is the gate deciding which chunks are to be updated this Update invocation
				if (alteratingSeed)// every other chunk
				{
					chunksToUpdate.Add(WorldChunks[iChunk]);
					alteratingSeed = false;
				}
				else
				{
					alteratingSeed = true;
				}
			}

			#endregion
		}
		catch (Exception ex)
		{
			throw;
		}

		return chunksToUpdate;
	}
}
