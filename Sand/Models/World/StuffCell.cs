using FlatRedBall;
using FlatRedBall.Graphics;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Sand.Constants;
using Color = Microsoft.Xna.Framework.Color;

namespace Sand;

public class StuffCell : IDrawableBatch
{
	private const int NOT_MOVED_DORMANT_TRIGGER = 10;

	public StuffCell(int offset, StuffCellSetup setup = StuffCellSetup.Empty)
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

		Chunks = [];

		const int chunksLong = 10;
		/*const int chunkRows = 10;
		const int chunkCols = 10;*/

		int chunkWidth = STUFF_WIDTH / chunksLong;
		int chunkHeight = STUFF_HEIGHT / chunksLong;

		var cursor = new Point(0, 0);
		var leftToRight = true;

		// these outer two loops are the row and cols of chunks
		for (int chunkRow = 0; chunkRow < chunksLong; chunkRow++)
		{
			for (int chunkCol = 0; chunkCol < chunksLong; chunkCol++)
			{
				// create new chunk
				Chunks.Add(new Chunk(cursor, chunkWidth, chunkHeight));

				leftToRight = !leftToRight;

				// IF NOT LAST column in row of chunks - move the cursor over one column to new chunk
				// OTHERWISE set the col back to first for new row
				cursor = new Point(chunkCol != chunksLong/*chunkCols*/ - 1 ? cursor.X + chunkWidth : 0, cursor.Y);
			}

			// move the cursor up one row to new chunk - doesn't matter if new cursor falls off the
			// grid as this is last operation of the nested loops
			cursor = new Point(cursor.X, cursor.Y + chunkHeight);
		}

		switch (setup)
		{
			case StuffCellSetup.Empty:
			default:
				break;
			case StuffCellSetup.StoneAroundEdges:
				for (int x = 0; x < World.Length; x++)
				{
					for (int y = 0; y < World[x].Length; y++)
					{
						if (x == 0 || x == World.Length - 1 // if at far left or right
						|| y == 0 || y == World[x].Length - 1) // if at far top or bottom
						{
							ForceAddStuff(Stuffs.BASIC_STONE, x, y);
						}
					}
				}
				break;
			case StuffCellSetup.StoneAroundEdges2:
				for (int x = 0; x < World.Length; x++)
				{
					for (int y = 0; y < World[x].Length; y++)
					{
						if (x == 0 || x == World.Length - 1 // if at far left or right
						|| y == 0 || y == World[x].Length - 1) // if at far top or bottom
						{
							ForceAddStuff(Stuffs.BASIC_STONE, x, y);
						}
					}
				}
				break;
			case StuffCellSetup.WaterBottomHalf:
				for (int x = 0; x < World.Length; x++)
				{
					for (int y = 0; y < World[x].Length / 2; y++)
					{
						SafeAddStuffIfEmpty(Stuffs.BASIC_WATER, x, y);
					}
				}
				break;
			case StuffCellSetup.WaterSloshingAbout:
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

		if (WorldSprite == null)
		{

			var wsWidth = RESOLUTION_X * 2;
			var wsHeight = RESOLUTION_Y * 2;

			WorldTexture = new Texture2D(FlatRedBallServices.GraphicsDevice, STUFF_WIDTH, STUFF_HEIGHT);
			WorldTexture.SetData(GetColorData());

			WorldSprite = SpriteManager.AddManualSprite(WorldTexture);
			WorldSprite.Width = wsWidth;// RESOLUTION_X * 2;
			WorldSprite.Height = wsHeight;// RESOLUTION_Y * 2;

			WorldSprite.X += RESOLUTION_X / 2 + (offset * RESOLUTION_X * 2);
			WorldSprite.Y += RESOLUTION_Y / 2;

			// instead offset setting X & Y props to account for:
			//		1. centre of sprite being 0,0 (prefer bottom left is 0,0)
			//		2. the position of this stuff cell in the greater whole

			//// 1.
			//var bottomLeftOriginOffset_x = World.Length / 2f;
			//var bottomLeftOriginOffset_y = World[0].Length / 2f;

			//2.

			/*			WorldSprite.X = initX;
						WorldSprite.Y = initY;*/
			WorldSprite.Z = Z_IND_WORLD;
			/*WorldSprite.X = -5000;
			WorldSprite.Y = -5000;*/


			Camera.Main.Orthogonal = true;
			//Camera.Main.OrthogonalWidth = WorldSprite.Width;
			//Camera.Main.OrthogonalHeight = WorldSprite.Height;
			Camera.Main.OrthogonalWidth = wsWidth;
			Camera.Main.OrthogonalHeight = wsHeight;
		}

		SpriteManager.AddDrawableBatch(this);
	}

	/// <summary>Outer array is X, inner array is Y.</summary>
	public Stuff[][] World { get; private set; }
	public List<Chunk> Chunks { get; set; }
	private Texture2D WorldTexture { get; set; }
	private Sprite WorldSprite { get; set; }

	public float X => 0;

	public float Y => 0;

	public float Z => 0;

	public bool UpdateEveryFrame => true;

	public void Draw(Camera camera)
	{
		//===================================================
		// DRAW WORLD TEXTURE
		//===================================================

		// interate through the World and get the color data of all Stuffs there
		var colorData = GetColorData();

/*		if (WorldSprite == null)
		{

			var wsWidth = RESOLUTION_X * 2;
			var wsHeight = RESOLUTION_Y * 2;

			WorldTexture = new Texture2D(FlatRedBallServices.GraphicsDevice, STUFF_WIDTH, STUFF_HEIGHT);
			WorldTexture.SetData(colorData);

			WorldSprite = SpriteManager.AddManualSprite(WorldTexture);
			WorldSprite.Width = RESOLUTION_X;// wsWidth;// RESOLUTION_X * 2;
			WorldSprite.Height = RESOLUTION_Y;// wsHeight;// RESOLUTION_Y * 2;
			WorldSprite.X += RESOLUTION_X / 2;
			WorldSprite.Y += RESOLUTION_Y / 2;
			WorldSprite.Z = Z_IND_WORLD;
			WorldSprite.X = -5000;
			WorldSprite.Y = -5000;


			Camera.Main.Orthogonal = true;
			Camera.Main.OrthogonalWidth = WorldSprite.Width;
			Camera.Main.OrthogonalHeight = WorldSprite.Height;
			Camera.Main.OrthogonalWidth = wsWidth;
			Camera.Main.OrthogonalHeight = wsHeight;
		}*/

		if (SHOW_WORLD)
		{
			WorldTexture.SetData(colorData);
			SpriteManager.ManualUpdate(WorldSprite);
		}


		//BgSpriteBatch.Draw(WorldTexture, new Rectangle(0,0, RESOLUTION_X, RESOLUTION_Y), Color.White);
		// change tto
		//BgSpriteBatch.Draw(WorldTexture, new Rectangle(0, 0, RESOLUTION_X, RESOLUTION_Y), null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.0f);


	}

	public void Update()
	{
		if (WorldSprite != null)
		{
			WorldSprite.Visible = SHOW_WORLD;
		}
	}

	public void Destroy()
	{
		//throw new NotImplementedException();
	}

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

		public int MoveLeftOffsetY;
		public int MoveRightOffsetY;
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
		int moveLeftOffsetY = 0;
		int moveRightOffsetY = 0;

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

		const int UPHILL_WALK_VERT_THRESHOLD = 8;
		var lastY = bottom + 2;

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
			for (var y = top - 1; y >= lastY; y--)
			{
				// if outside the world bound just continue to next in row
				if (y < 0 || y >= STUFF_HEIGHT)
				{
					continue;
				}

				// if there is something here
				if (World[left][y] != null)
				{
					// if its a liquid, slow down
					if (World[left][y] is { Phase: Phase.Liquid })
					{
						inLiquid = true;
						adjustedMoveFactor = initialMoveFactor / 2;
					}
					else if (World[left][y] is { Phase: Phase.Solid } or { Phase: Phase.Powder })
					{
						//===========================
						// WALK UPHILL LEFT
						//===========================

						// check up to n "pixels" above the blocker, and if space then offset left movement's Y by that much and allowLeft
						if (y == lastY)
						{
							for (var yCursorOffset = 1; yCursorOffset <= UPHILL_WALK_VERT_THRESHOLD; yCursorOffset++)
							{
								if (y + yCursorOffset >= STUFF_HEIGHT || y + yCursorOffset < 0)
								{
									break;
								}

								var stuff = World[left][y + yCursorOffset];

								// if the left blocker has space above, allow left, but require a single y coord offset of the height of a Stuff
								if (stuff == null || stuff is { Phase: Phase.Liquid })
								{
									allowLeft = true;
									moveLeftOffsetY += (yCursorOffset * STUFF_SCALE) + (1 * STUFF_SCALE);
									break;
								}
								else if (yCursorOffset == UPHILL_WALK_VERT_THRESHOLD)
								{
									// else dont allow left
									allowLeft = false;
									break;
								}
							}	
						}
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
		//================================================
		#region
		// get the row directly below the sprite
		right++;
		// if bottom coord index safe
		if (right >= 0 && right < STUFF_WIDTH)
		{
			// iterate across that row within the sprites x coords and check if downward movement should be arrested
			for (var y = top - 1; y >= lastY; y--)
			{
				// if outside the world bound just continue to next in row
				if (y < 0 || y >= STUFF_HEIGHT)
				{
					continue;
				}

				// if there is something here
				if (World[right][y] != null)
				{
					// if its a liquid, slow down
					if (World[right][y] is { Phase: Phase.Liquid })
					{
						inLiquid = true;
						adjustedMoveFactor = initialMoveFactor / 2;
					}
					else if (World[right][y] is { Phase: Phase.Solid } or { Phase: Phase.Powder })
					{
						//===========================
						// WALK UPHILL RIGHT
						//===========================

						// check up to n "pixels" above the blocker, and if space then offset left movement's Y by that much and allowLeft
						if (y == lastY)
						{
							for (var yCursorOffset = 1; yCursorOffset <= UPHILL_WALK_VERT_THRESHOLD; yCursorOffset++)
							{
								if (y + yCursorOffset >= STUFF_HEIGHT || y + yCursorOffset < 0)
								{
									break;
								}

								var stuff = World[right][y + yCursorOffset];

								// if the left blocker has space above, allow left, but require a single y coord offset of the height of a Stuff
								if (stuff == null || stuff is { Phase: Phase.Liquid })
								{
									allowRight = true;
									moveRightOffsetY += (yCursorOffset * STUFF_SCALE) + (1 * STUFF_SCALE);
									break;
								}
								else if (yCursorOffset == UPHILL_WALK_VERT_THRESHOLD)
								{
									// else dont allow left
									allowRight = false;
									break;
								}
							}
						}
					}
					//Logger.Instance.LogInfo($"Encountering {World[left][y].Phase} at left - move factor is {moveFactor}");
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
			GravMagnitude = adjustedGravMagnitude,
			MoveLeftOffsetY = moveLeftOffsetY,
			MoveRightOffsetY = moveRightOffsetY
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
				colorData[colorIndex++] = World[x][y] == null ? Color.Transparent : World[x][y].Color;
			}
		}

		return colorData;
	}

	public Stuff GetStuffAt(Point p)
	{
		return GetStuffAt(p.X, p.Y);
	}

	public Stuff GetStuffAt(int x, int y)
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

			Stuff stuffTarget = GetStuffAt(to);
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

			for (int iChunk = 0; iChunk < Chunks.Count; iChunk++)
			{
				// this if is the gate deciding which chunks are to be updated this Update invocation
				if (alteratingSeed)// every other chunk
				{
					chunksToUpdate.Add(Chunks[iChunk]);
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
