using FlatRedBall;
using FlatRedBall.Graphics;
using Microsoft.Xna.Framework.Graphics;
using Sand.Services;
using System.Collections.Generic;
using System.Drawing;
using static Sand.Constants;
using Color = Microsoft.Xna.Framework.Color;

namespace Sand;

public class StuffCell : IDrawableBatch
{
	private const int NOT_MOVED_DORMANT_TRIGGER = 10;

	//===========================================
	// CTOR
	//===========================================
	public StuffCell(int offset, StuffCellSetup setup = StuffCellSetup.Empty)
	{
		//========================================
		// PREPARE THE WORLD DATA STRUCTURE
		//========================================
		Offset = offset;
		CellMatrix = new Stuff[STUFF_CELL_WIDTH, STUFF_CELL_HEIGHT];

		//========================================
		// PREPARE THE CHUNKS
		//========================================
		Chunks = [];

		int chunkWidth = STUFF_CELL_WIDTH / STUFF_CELL_CHUNKS_LONG;
		int chunkHeight = STUFF_CELL_HEIGHT / STUFF_CELL_CHUNKS_LONG;

		var cursor = new Point(0, 0);
		var leftToRight = true;

		// these outer two loops are the row and cols of chunks
		for (int chunkRow = 0; chunkRow < STUFF_CELL_CHUNKS_LONG; chunkRow++)
		{
			for (int chunkCol = 0; chunkCol < STUFF_CELL_CHUNKS_LONG; chunkCol++)
			{
				// create new chunk
				Chunks.Add(new StuffCellChunk(cursor, chunkWidth, chunkHeight));

				leftToRight = !leftToRight;

				// IF NOT LAST column in row of chunks - move the cursor over one column to new chunk
				// OTHERWISE set the col back to first for new row
				cursor = new Point(chunkCol != STUFF_CELL_CHUNKS_LONG/*chunkCols*/ - 1 ? cursor.X + chunkWidth : 0, cursor.Y);
			}

			// move the cursor up one row to new chunk - doesn't matter if new cursor falls off the
			// grid as this is last operation of the nested loops
			cursor = new Point(cursor.X, cursor.Y + chunkHeight);
		}

		//========================================
		// INITIALISE THE STUFF IN THIS CELL
		//========================================
		switch (setup)
		{
			case StuffCellSetup.Empty:
			default:
				break;
			case StuffCellSetup.StoneAroundEdges:
				for (int x = 0; x < STUFF_CELL_WIDTH; x++)
				{
					for (int y = 0; y < STUFF_CELL_HEIGHT; y++)
					{
						if (x == 0 || x == STUFF_CELL_WIDTH - 1 // if at far left or right
						|| y == 0 || y == STUFF_CELL_HEIGHT - 1) // if at far top or bottom
						{
							ForceAddStuff(Stuffs.BASIC_STONE, x, y);
						}
					}
				}
				break;
			case StuffCellSetup.StoneAroundEdges2:
				for (int x = 0; x < STUFF_CELL_WIDTH; x++)
				{
					for (int y = 0; y < STUFF_CELL_HEIGHT; y++)
					{
						if (x == 0 || x == STUFF_CELL_WIDTH - 1 // if at far left or right
						|| y == 0 || y == STUFF_CELL_HEIGHT - 1) // if at far top or bottom
						{
							ForceAddStuff(Stuffs.BASIC_STONE, x, y);
						}
					}
				}
				break;
			case StuffCellSetup.WaterBottomHalf:
				for (int x = 0; x < STUFF_CELL_WIDTH; x++)
				{
					for (int y = 0; y < STUFF_CELL_HEIGHT / 2; y++)
					{
						SafeAddStuffIfEmpty(Stuffs.BASIC_WATER, x, y);
					}
				}
				break;
			case StuffCellSetup.WaterSloshingAbout:
				var i = 0;
				for (int x = 0; x < STUFF_CELL_WIDTH; x++)
				{
					for (int y = 0; y < STUFF_CELL_HEIGHT; y++)
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

		//========================================
		// PREPARE THE TEXTURE AND SPRITE
		//========================================
		WorldTexture = new Texture2D(FlatRedBallServices.GraphicsDevice, STUFF_CELL_WIDTH, STUFF_CELL_HEIGHT);
		WorldTexture.SetData(GetColorData());

		WorldSprite = SpriteManager.AddManualSprite(WorldTexture);
		WorldSprite.Width = Camera.Main.OrthogonalWidth;
		WorldSprite.Height = Camera.Main.OrthogonalHeight;

		WorldSprite.X += RESOLUTION_X / 2 + (offset * RESOLUTION_X);
		WorldSprite.Y += RESOLUTION_Y / 2;
		WorldSprite.Z = Z_IND_WORLD;

		//========================================
		// ADD TO SPRITE MANAGER
		//========================================
		SpriteManager.AddDrawableBatch(this);
	}

	//===========================================
	// Fields / Properties
	//===========================================
	#region Fields / Properties

	public int Offset { get; private set; }
	public float Top => WorldSprite.Top;
	public float Right => WorldSprite.Right;
	public float Bottom => WorldSprite.Bottom;
	public float Left => WorldSprite.Left;

	/// <summary>1st index is X, 2nd index is Y.</summary>
	private Stuff[,] CellMatrix { get; set; }
	private List<StuffCellChunk> Chunks { get; set; }
	private Texture2D WorldTexture { get; set; }
	private Sprite WorldSprite { get; set; }

	#endregion Fields / Properties

	//===========================================
	// IDrawableBatch
	//===========================================
	#region IDrawableBatch

	public float X => Z_IND_WORLD;
	public float Y => Z_IND_WORLD;
	public float Z => Z_IND_WORLD;
	public bool UpdateEveryFrame => true;
	public void Draw(Camera camera)
	{
		if (SHOW_WORLD)
		{
			WorldTexture.SetData(GetColorData());
			SpriteManager.ManualUpdate(WorldSprite);
		}
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
	}

	#endregion IDrawableBatch

	//===========================================
	// Add Stuff
	//===========================================
	#region Add Stuff

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
		if (x >= 0 && x < STUFF_CELL_WIDTH && y >= 0 && y < STUFF_CELL_HEIGHT)
		{
			var stuff = StuffFactory.Instance.Get(stuffType);
			CellMatrix[x, y] = stuff;//.SetPosition(x, y);
								//stuff.X = x;
								//stuff.Y = y;
		}
	}
	public void SafeAddStuffIfEmpty(string stuffType, int x, int y)
	{
		if (x >= 0 && x < STUFF_CELL_WIDTH && y >= 0 && y < STUFF_CELL_HEIGHT && CellMatrix[x, y] == null)
		{
			var stuff = StuffFactory.Instance.Get(stuffType);
			CellMatrix[x, y] = stuff;//.SetPosition(x, y);
								//stuff.X = x;
								//stuff.Y = y;
		}
	}

	#endregion Add Stuff

	public PlayerCollisionResult Collision(Player player, int initialGravMagnitude, int initialMoveFactor)
	{

		var (bottomLeft, topRight) = CoordManager.Corners(player.Hitbox);

		var top = topRight.StuffPosition.Y;
		var right = topRight.StuffPosition.X;
		var bottom = bottomLeft.StuffPosition.Y;
		var left = bottomLeft.StuffPosition.X;


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
		if (bottom >= 0 && bottom < STUFF_CELL_HEIGHT)
		{
			// iterate across that row within the sprites x coords and check if downward movement should be arrested
			for (var x = left; x < right; x++)
			{
				// if outside the world bound just continue to next in row
				if (x < 0 || x >= STUFF_CELL_WIDTH)
				{
					continue;
				}

				// if there is something here then stop
				if (CellMatrix[x, bottom] != null)
				{
					if (CellMatrix[x, bottom].Phase == Phase.Liquid)
					{
						inLiquid = true;
						adjustedMoveFactor = initialMoveFactor / 2;
					}
					else
					{
						allowDown = false;
						break;
					}
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
		if (top >= 0 && top < STUFF_CELL_HEIGHT)
		{
			// iterate across that row within the sprites x coords and check if downward movement should be arrested
			for (var x = left; x < right; x++)
			{
				// if outside the world bound just continue to next in row
				if (x < 0 || x >= STUFF_CELL_WIDTH)
				{
					continue;
				}

				// if there is something here then stop
				if (CellMatrix[x, top] != null)
				{
					if (CellMatrix[x, top].Phase == Phase.Liquid)
					{
						inLiquid = true;
						adjustedMoveFactor = initialMoveFactor / 2;
					}
					else
					{
						allowUp = false;
						break;
					}
				}
			}
		}
		// if bottom coord index is not safe
		else if (top >= STUFF_CELL_HEIGHT)
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
		if (left >= 0 && left < STUFF_CELL_WIDTH)
		{
			// iterate across that row within the sprites x coords and check if downward movement should be arrested
			for (var y = top - 1; y >= lastY; y--)
			{
				// if outside the world bound just continue to next in row
				if (y < 0 || y >= STUFF_CELL_HEIGHT)
				{
					continue;
				}

				// if there is something here
				if (CellMatrix[left, y] != null)
				{
					// if its a liquid, slow down
					if (CellMatrix[left, y] is { Phase: Phase.Liquid })
					{
						inLiquid = true;
						adjustedMoveFactor = initialMoveFactor / 2;
					}
					else if (CellMatrix[left, y] is { Phase: Phase.Solid } or { Phase: Phase.Powder })
					{
						//===========================
						// WALK UPHILL LEFT
						//===========================

						// check up to n "pixels" above the blocker, and if space then offset left movement's Y by that much and allowLeft
						if (y == lastY)
						{
							for (var yCursorOffset = 1; yCursorOffset <= UPHILL_WALK_VERT_THRESHOLD; yCursorOffset++)
							{
								if (y + yCursorOffset >= STUFF_CELL_HEIGHT || y + yCursorOffset < 0)
								{
									break;
								}

								var stuff = CellMatrix[left, y + yCursorOffset];

								// if the left blocker has space above, allow left, but require a single y coord offset of the height of a Stuff
								if (stuff == null || stuff is { Phase: Phase.Liquid })
								{
									allowLeft = true;
									moveLeftOffsetY += (yCursorOffset * STUFF_TO_PIXEL_SCALE) + (1 * STUFF_TO_PIXEL_SCALE);
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
		if (right >= 0 && right < STUFF_CELL_WIDTH)
		{
			// iterate across that row within the sprites x coords and check if downward movement should be arrested
			for (var y = top - 1; y >= lastY; y--)
			{
				// if outside the world bound just continue to next in row
				if (y < 0 || y >= STUFF_CELL_HEIGHT)
				{
					continue;
				}

				// if there is something here
				if (CellMatrix[right, y] != null)
				{
					// if its a liquid, slow down
					if (CellMatrix[right, y] is { Phase: Phase.Liquid })
					{
						inLiquid = true;
						adjustedMoveFactor = initialMoveFactor / 2;
					}
					else if (CellMatrix[right, y] is { Phase: Phase.Solid } or { Phase: Phase.Powder })
					{
						//===========================
						// WALK UPHILL RIGHT
						//===========================

						// check up to n "pixels" above the blocker, and if space then offset left movement's Y by that much and allowLeft
						if (y == lastY)
						{
							for (var yCursorOffset = 1; yCursorOffset <= UPHILL_WALK_VERT_THRESHOLD; yCursorOffset++)
							{
								if (y + yCursorOffset >= STUFF_CELL_HEIGHT || y + yCursorOffset < 0)
								{
									break;
								}

								var stuff = CellMatrix[right, y + yCursorOffset];

								// if the left blocker has space above, allow left, but require a single y coord offset of the height of a Stuff
								if (stuff == null || stuff is { Phase: Phase.Liquid })
								{
									allowRight = true;
									moveRightOffsetY += (yCursorOffset * STUFF_TO_PIXEL_SCALE) + (1 * STUFF_TO_PIXEL_SCALE);
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
				}
			}
		}
		// if bottom coord index is not safe
		else if (right >= STUFF_CELL_WIDTH)
		{
			allowRight = false;
		}
		#endregion

		return new PlayerCollisionResult(
		
			allowUp: allowUp,
			allowRight: allowRight,
			allowDown: allowDown,
			allowLeft: allowLeft,
			inLiquid: inLiquid,
			moveFactor: adjustedMoveFactor,
			gravMagnitude: adjustedGravMagnitude,
			moveLeftOffsetY: moveLeftOffsetY,
			moveRightOffsetY: moveRightOffsetY
		);
	}
	public Color[] GetColorData()
	{
		var colorData = new Color[STUFF_CELL_HEIGHT * STUFF_CELL_WIDTH];
		var colorIndex = 0;

		for (var y = STUFF_CELL_HEIGHT - 1; y >= 0; y--)
		{
			for (var x = 0; x < STUFF_CELL_WIDTH; x++)
			{
				colorData[colorIndex++] = CellMatrix[x, y] == null ? Color.Transparent : CellMatrix[x, y].Color;
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
		return CellMatrix[x, y];
	}
	public bool Move(Point from, Point to)
	{
		var stuffAtSource = CellMatrix[from.X, from.Y];
		var stuffAtTarget = CellMatrix[to.X, to.Y];

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
				CellMatrix[to.X, to.Y] = stuffAtSource;
				CellMatrix[from.X, from.Y] = null;
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
					for (int cursorY = to.Y; !hasDisplaced && cursorY < STUFF_CELL_HEIGHT; cursorY++)
					{

						///////////////////////////////////////////////////////
						// lets try randomly going left, on or right of yCoord in water column droping
						// there if empty, so 1/3 itll land left, on or right of cursorY
						///////////////////////////////////////////////////////
						for (var i = 0; !hasDisplaced && i < Randoms.Instance.Ind_leftRightMid.Length; i++)
						{
							var adjCursorX = waterColumnX + Randoms.Instance.Ind_leftRightMid[i];
							if (adjCursorX >= 0 && adjCursorX < STUFF_CELL_WIDTH && CellMatrix[adjCursorX, cursorY] == null)
							{
								// move this displaced liquid to here
								CellMatrix[adjCursorX, cursorY] = stuffTarget;//.SetPosition(adjCursorX, cursorY); ;
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
			var stuffSource = CellMatrix[from.X, from.Y];
			var stuffTarget = CellMatrix[to.X, to.Y];
			// if not stuff at target fall to here and finish
			if (stuffTarget == null)
			{
				// update world
				CellMatrix[to.X, to.Y] = stuffSource;
				CellMatrix[from.X, from.Y] = null;
				return true;
			}

			return false;
		}
	}
	public List<StuffCellChunk> GetChunksToUpdate()
	{
		var chunksToUpdate = new List<StuffCellChunk>();

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

		return chunksToUpdate;
	}

}
