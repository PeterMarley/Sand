using FlatRedBall;
using FlatRedBall.Content.Math.Splines;
using FlatRedBall.Graphics;
using FlatRedBall.Graphics.Animation;
using FlatRedBall.Input;
using FlatRedBall.Math.Geometry;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Sand.Extensions;
using Sand.Models.Stuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static FlatRedBall.Input.Mouse;
using static Sand.Constants;
using Point = System.Drawing.Point;

namespace Sand;


public class DrawableWorld
{
	private const int PLAYER_MOVE_FACTOR = 15;
	private const int GRAV_MAGNITUDE = 1;

	//========================================
	// The World
	//========================================
	private StuffCell[] StuffCells { get; set; }
	private CheckerBoardBackground Background { get; set; }
	public WorldCoords WorldCoords { get; private set; }

	//========================================
	// The Player
	//========================================
	private Player Player { get; set; }


	//===========================================
	// CTOR
	//===========================================
	public DrawableWorld(StuffCellSetup worldSetup)
	{
		PrepareBackground();
		PrepareWorld(worldSetup);
		PreparePlayer();

		void PrepareBackground()
		{
			Background = new CheckerBoardBackground();
		}
		void PrepareWorld(StuffCellSetup worldSetup)
		{
			StuffCells =
			[
				new StuffCell(-1, worldSetup),
				new StuffCell(0, worldSetup),
				new StuffCell(1, worldSetup)
			];
		}
		void PreparePlayer()
		{
			Player = new Player();

		}
	}

	//===========================================
	// Moving Stuff
	//===========================================
	#region Moving Stuff

	public void ApplyGravity(StuffCell cell, int xIndex, int yIndex)
	{
		//TODO stuff cell naive quick impl
		var stuff = cell.GetStuffAt(xIndex, yIndex);

		switch (stuff.Phase)
		{
			case Phase.Powder:
				ApplyGravityPhasePowder(cell, stuff, xIndex, yIndex);
				break;
			case Phase.Liquid:
				ApplyGravityPhaseLiquid(cell, stuff, xIndex, yIndex);
				break;
		}

		void ApplyGravityPhasePowder(StuffCell cell, Stuff stuff, int xIndex, int yIndex)
		{
			if (stuff.CheckDormancy())
			{
				return;
			}

			//==================================================================
			//Check 2 spots below left and right, if all are filled then move on
			//==================================================================

			// if bottom row outside array range just continue as this Stuff cant fall anywhere
			var rowBelowIndex = yIndex - 1;
			if (rowBelowIndex < 0) return;

			// check directly below
			if ((rowBelowIndex - 2 >= 0 && Move(cell, new(xIndex, yIndex), new(xIndex, rowBelowIndex - 2)) || (rowBelowIndex - 1 >= 0 && Move(cell, new(xIndex, yIndex), new(xIndex, rowBelowIndex - 1)))))
			{
				return;
			}

			// check below and left (but alterate sides randomly)
			bool leftSide = FastRandomBoolGenerator.Instance.Next();
			int colLeftIndex = xIndex - 1;
			int colRightIndex = xIndex + 1;

			for (var lateralGravAttempts = 2; lateralGravAttempts > 0; lateralGravAttempts--)
			{
				if (leftSide)
				{
					// check below and left
					if (colLeftIndex >= 0 && Move(cell, new(xIndex, yIndex), new(colLeftIndex, rowBelowIndex)))
					{
						return;
						//break;
					}
				}
				else
				{
					// check below and right
					if (colRightIndex < STUFF_CELL_WIDTH && Move(cell, new(xIndex, yIndex), new(colRightIndex, rowBelowIndex)))
					{
						return;
						//break;
					}
				}
				leftSide = !leftSide;
			}

			// check directly below
			if (Move(cell, new(xIndex, yIndex), new(xIndex, rowBelowIndex)))
			{
				return;
			}

		}

		void ApplyGravityPhaseLiquid(StuffCell cell, Stuff stuff, int xIndex, int yIndex)
		{


			//-----------------------------------------------------------------
			//Check 2 spots below left and right
			//-----------------------------------------------------------------

			// if bottom row outside array range just continue as this Stuff cant fall anywhere
			var rowBelowIndex = yIndex - 1;
			if (rowBelowIndex < 0) return;

			// if dormant, and theres nothing below, finish
			//TODO stuff cell naive quick impl
			if (stuff.CheckDormancy() && StuffCells[1].GetStuffAt(xIndex, rowBelowIndex) != null) // this is not null check basically enables erroneously dormant stuff to right itself
			{
				return;
			}

			// check directly below
			if ((rowBelowIndex - 2 >= 0 && Move(cell, new(xIndex, yIndex), new(xIndex, rowBelowIndex - 2)) || (rowBelowIndex - 1 >= 0 && Move(cell, new(xIndex, yIndex), new(xIndex, rowBelowIndex - 1)))))
			{
				return;
			}
			bool leftFirst = FastRandomBoolGenerator.Instance.Next();
			bool leftSide = leftFirst;
			// check below and left (but alterate sides randomly)
			int colLeftIndex = xIndex - 1;
			int colRightIndex = xIndex + 1;

			for (var lateralGravAttempts = 2; lateralGravAttempts > 0; lateralGravAttempts--)
			{
				if (leftSide)
				{
					// check below and left
					if (colLeftIndex >= 0 && Move(cell, new(xIndex, yIndex), new(colLeftIndex, rowBelowIndex)))
					{
						break;
					}
				}
				else
				{
					// check below and right
					if (colRightIndex < STUFF_CELL_WIDTH && Move(cell, new(xIndex, yIndex), new(colRightIndex, rowBelowIndex)))
					{
						break;
					}
				}
				leftSide = !leftSide;
			}

			if (TimeManager.CurrentFrame % 1 == 0)
			{
				colLeftIndex--;
				colRightIndex++;
				leftSide = leftFirst;

				//-----------------------------------------------------------------
				//Check 2 spots directly left and right - represent fluidic flow
				//-----------------------------------------------------------------

				// check direct lateral movements
				for (var lateralFlowAttempts = 2; lateralFlowAttempts > 0; lateralFlowAttempts--)
				{
					if (leftSide)
					{
						// check left
						if (colLeftIndex >= 0 && Move(cell, new(xIndex, yIndex), new(colLeftIndex, yIndex)))
						{
							break;
						}
					}
					else
					{
						// check right
						if (colRightIndex < STUFF_CELL_WIDTH && Move(cell, new(xIndex, yIndex), new(colRightIndex, yIndex)))
						{
							break;
						}
					}
					leftSide = !leftSide;
				}
			}

			////====================================================================
			//// check left below and right, if all same phase then set as dormant
			////====================================================================

			//// the col left right and row below indices all checked to be valid at this point

			//bool leftDormancyCondition = false;
			//bool rightDormancyCondition = colRightIndex < STUFF_WIDTH;
			//bool belowleftDormancyCondition = rowBelowIndex >= 0;

			//var a = World[xIndex];
			//var b = World[xIndex][yIndex];
			//var c = World[xIndex][yIndex];

			//try
			//{
			//	if (World[colLeftIndex][yIndex] != null && World[colLeftIndex][yIndex].Name == stuff.Name)
			//	{
			//		leftDormancyCondition = true;
			//	}

			//	// if directly right is not empty and is same phase then set dormancy check
			//	if (World[colRightIndex][yIndex] != null && World[colRightIndex][yIndex].Name == stuff.Name)
			//	{
			//		rightDormancyCondition = true;
			//	}

			//	// if directly left is not empty and is same phase then set dormancy check
			//	if (World[xIndex][rowBelowIndex] != null && World[xIndex][rowBelowIndex].Name == stuff.Name)
			//	{
			//		belowleftDormancyCondition = true;
			//	}

			//	// if directly below is not empty and is same phase then set dormancy check
			//	if (leftDormancyCondition && rightDormancyCondition && belowleftDormancyCondition)
			//	{
			//		stuff.Dormant = true;
			//	}
			//}
			//catch (Exception checkingAdjacentEx)
			//{
			//	throw;
			//}


		}
	}
	public bool Move(StuffCell cell, Point from, Point to)
	{
		//TODO stuff cell naive quick impl
		return cell.Move(from, to);
	}

	#endregion Moving Stuff

	public void Update()
	{
		SetupWorldCoords();
		ProcessChunks();

		void SetupWorldCoords()
		{
			var camBottomLeft = new Vector2((Camera.Main.X - Camera.Main.OrthogonalWidth / 2)/* - (RESOLUTION_X / 2)*/, (Camera.Main.Y - Camera.Main.OrthogonalHeight / 2)/* - (RESOLUTION_Y / 2)*/);
			var mRel = new Vector2(InputManager.Mouse.X, RESOLUTION_Y - InputManager.Mouse.Y);

			WorldCoords = new WorldCoords(
			
				mouseRelative: mRel,
				cameraCentre: new Vector2(Camera.Main.X, Camera.Main.Y),
				cameraBottomLeft: camBottomLeft,
				cameraOffset: camBottomLeft,
				cameraTopRight: new Vector2(Camera.Main.X + (RESOLUTION_X / 2), Camera.Main.Y + (RESOLUTION_Y / 2)),
				mouseAbsolute: new Vector2(mRel.X + camBottomLeft.X, mRel.Y + camBottomLeft.Y)
			);

			if (PRINT_POSITIONS_ON_CLICK && InputManager.Mouse.IsInGameWindow() && InputManager.Mouse.ButtonPushed(MouseButtons.LeftButton))
			{
				
				var msg =
$@"
CAMERA
	- ✓ Camera (Centre)   {WorldCoords.CameraCentre}
	- ✓ Camera (btm left) {WorldCoords.CameraBottomLeft} -= aka CAMERA OFFSET =-
	- ✓ Camera (top rigt) {WorldCoords.CameraTopRight}
MOUSE
	- ✓ Mouse Relative    {WorldCoords.MouseRelative}
	- ✓ Mouse Absolute    {WorldCoords.MouseAbsolute}
PLAYER
	- ✓ Player Absolute   {Player.X},{Player.Y}
========================================
";

				Logger.Instance.LogInfo(msg);

			}
		}
		void ProcessChunks()
		{

			/*		if (TimeManager.CurrentFrame % _FRAME_COUNT_BETWEEN_UPDATE != 0)
					{
						return;
					}
			*/
			int i = 0;
			foreach (var cell in StuffCells)
			{
				var p = new Point();

				try
				{
					//=======================================================
					// New version of world update loop (utilising chunks)
					//=======================================================
					// choose which chunks to update
					//var chunksToUpdate = StuffCell.GetChunksToUpdate();
					//TODO stuff cell naive quick impl
					var chunksToUpdate = cell.GetChunksToUpdate();

					foreach (var chunk in chunksToUpdate)
					{
						foreach (var point in chunk.Points_BottomLeftToTopRight_AlternatingRowDirection)
						{
							var xIndex = point.X;
							var yIndex = point.Y;

							// make point viewable to scope that encloses the try catch for error logging
							p = point;

							// if something here then apply gravity **NOTE** dormancy is checked in apply gravity

							//if (StuffCell.GetStuffAt(point) != null)
							//TODO stuff cell naive quick impl
							if (cell.GetStuffAt(point) != null)
							{
								ApplyGravity(cell, xIndex, yIndex);
							}
						}
					}
				}
				catch (Exception updateException)
				{
					Logger.Instance.LogError(updateException, $"(x,y)=({p.X},{p.Y}), (maxX, maxY)=({STUFF_CELL_WIDTH - 1},{STUFF_CELL_HEIGHT - 1})");
					throw;
				}
				i++;
			}
		}

	}
	public SandCoordinate StuffPositionForWorldCoord(Vector2 worldCoord)
	{
		int? chunkIndex = null;
		Point? stuffPosition = null;

		for (int i = 0; i < StuffCells.Length; i++)
		{
			var chunk = StuffCells[i];
			if (worldCoord.X >= chunk.Left && worldCoord.X <= chunk.Right)
			{
				if (worldCoord.Y >= chunk.Bottom && worldCoord.Y < chunk.Top)
				{
					chunkIndex = i;
					break;
				}
			}
		}

		if (chunkIndex.HasValue)
		{
			var offset = StuffCells[chunkIndex.Value].Offset;

			// get origin of cell
			var originX = 0;
			var originY = 0; // bottom left

			// offset this origin by the number or RESOLUTION_X's * OFFSET
			var originXOffset = originX + (offset * RESOLUTION_X);
			Point cellOrigin = new(originXOffset, originY);

			// now get the distance between the origin and the coord
			var positionRelativeToOrigin_X = worldCoord.X - cellOrigin.X;
			var positionRelativeToOrigin_Y = worldCoord.Y - cellOrigin.Y;

			var stuffX = (int)(positionRelativeToOrigin_X / STUFF_TO_PIXEL_SCALE);
			var stuffY = (int)(positionRelativeToOrigin_Y / STUFF_TO_PIXEL_SCALE);

			stuffPosition = new Point(stuffX, stuffY);

			/*			var tX = worldCoord.X;
						//for (int i = 0; i <= chunkIndex; i++)
						//{
						var adj = RESOLUTION_X * offset;
						//tX += (RESOLUTION_X * i) - (RESOLUTION_X * StuffCells[chunkIndex.Value].Offset);
						tX += adj;

						//}
						var temp = new Vector2(tX, worldCoord.Y);

						var spX = 1;

						// the temp now describes


						var spY = 1;

						stuffPosition = new Point((int)(temp.X / STUFF_SCALE), (int)(temp.Y / STUFF_SCALE));
						//var stuffPosition = new Point(STUFF_WIDTH / 2, STUFF_HEIGHT / 2);*/
			var sdfsdf = "";
		}

		//Console.WriteLine($"{nameof(StuffPositionForWorldCoord)} - chunkIndex={chunkIndex} stuffPosition={stuffPosition} (MAX={Constants.STUFF_CELL_WIDTH},{Constants.STUFF_CELL_HEIGHT})");

		return new SandCoordinate()
		{
			ChunkIndex = chunkIndex ?? throw new InvalidOperationException("chunkIndex not found"),
			StuffPosition = stuffPosition ?? throw new InvalidOperationException("stuffPosition not found")
		};

		//return (
		//	chunkIndex ?? throw new InvalidOperationException("chunkIndex not found"),
		//	stuffPosition ?? throw new InvalidOperationException("stuffPosition not found")
		//);
	}
	public void ProcessControlsInput()
	{
		
		var mousePosition = StuffPositionForWorldCoord(WorldCoords.MouseAbsolute);

		AffectWorld(mousePosition);
		AffectPlayer();

		void AffectWorld(SandCoordinate mousePos)
		{
			if (InputManager.Mouse.IsInGameWindow())
			{
				// get x and y of ??? something, unsure
				var x = InputManager.Mouse.X / STUFF_TO_PIXEL_SCALE;
				var y = (FlatRedBallServices.GraphicsDevice.Viewport.Height - InputManager.Mouse.Y) / STUFF_TO_PIXEL_SCALE;

				// get mouse pointer 

				if (InputManager.Keyboard.KeyDown(Keys.Z))
				{
					Camera.Main.X -= 5 * STUFF_TO_PIXEL_SCALE;
				}

				if (InputManager.Keyboard.KeyDown(Keys.C))
				{
					Camera.Main.X += 5 * STUFF_TO_PIXEL_SCALE;
				}

				if ((InputManager.Mouse.ButtonPushed(MouseButtons.LeftButton) /*|| InputManager.Mouse.ButtonDown(MouseButtons.LeftButton)*/))
				{

					//Logger.Instance.LogInfo($"Placing Water @ Chunk Index {chunkIndex} @ Stuff Position {stuffPosition}");
					//TODO stuff cell naive quick impl

					
					StuffCells[mousePos.ChunkIndex].SafeAddStuffIfEmpty_InSquare(Stuffs.BASIC_WATER, mousePos.StuffPosition.X, mousePos.StuffPosition.Y, 15);
					



					//_world.SafeAddStuffIfEmpty(Stuffs.BASIC_WATER, x, y);

				}

				if ((InputManager.Mouse.ButtonPushed(MouseButtons.RightButton) || InputManager.Mouse.ButtonDown(MouseButtons.RightButton)))
				{
					//TODO stuff cell naive quick impl
					StuffCells[1].SafeAddStuffIfEmpty_InSquare(Stuffs.BASIC_SAND, x, y, 10);
					//_world.SafeAddStuffIfEmpty(Stuffs.BASIC_SAND, x, y);
				}

				if ((InputManager.Mouse.ButtonPushed(MouseButtons.MiddleButton) || InputManager.Mouse.ButtonDown(MouseButtons.MiddleButton)))
				{
					//TODO stuff cell naive quick impl
					StuffCells[1].ForceAddStuff_InSquare(Stuffs.BASIC_STONE, x, y, 10);
					//_world.SafeAddStuffIfEmpty(Stuffs.BASIC_SAND, x, y);
				}

				if ((InputManager.Mouse.ButtonPushed(MouseButtons.XButton1) || InputManager.Mouse.ButtonDown(MouseButtons.XButton1)))
				{
					//TODO stuff cell naive quick impl
					StuffCells[1].ForceAddStuff_InSquare(Stuffs.BASIC_LAVA2, x, y, 2);
					//_world.SafeAddStuffIfEmpty(Stuffs.BASIC_SAND, x, y);
				}

				// place stuff at bottom left of player
				if (InputManager.Keyboard.KeyDown(Keys.E))
				{
					var (top, right, bottom, left) = Player.GetPositionStuff(); /*NumberExtensions.ToStuffCoord(Player.Sprite);*/

					//StuffCell.ForceAddStuff_InSquare(Stuffs.BASIC_STONE, left, bottom, 2);

					//TODO stuff cell naive quick impl
					StuffCells[1].ForceAddStuff_InSquare(Stuffs.BASIC_STONE, left, bottom, 2);
				}
			}
		}
		void AffectPlayer()
		{

			// TODO current => same for each cell
			//	   eventual => pinpoint where the player is and do all cells as needed
			/*foreach (var stuffCell in StuffCells)
			{*/
			//TODO stuff cell naive quick impl
			var stuffCell = StuffCells[1];
			var collision = stuffCell.Collision(Player, GRAV_MAGNITUDE, PLAYER_MOVE_FACTOR);

			var allowUp = collision.AllowUp;
			var allowRight = collision.AllowRight;
			var allowDown = collision.AllowDown;
			var allowLeft = collision.AllowLeft;
			var moveFactor = collision.MoveFactor;
			var moveLeftOffsetY = collision.MoveLeftOffsetY;
			var moveRightOffsetY = collision.MoveRightOffsetY;

			//move ↑
			if (allowUp && InputManager.Keyboard.KeyDown(Keys.W))
			{
				Player.Y += (moveFactor * 2);
			}

			// move ←
			if (allowLeft && InputManager.Keyboard.KeyDown(Keys.A))
			{
				Player.X -= moveFactor;
				Player.Y += moveLeftOffsetY;
			}

			// move ↓
			if (allowDown)
			{
				/*//gravity
				Player.Y -= moveFactor * .7f;
				Player.Falling = true;*/
				if (InputManager.Keyboard.KeyDown(Keys.S))
				{
					Player.Y -= moveFactor;
				}
			}
			else
			{
				Player.Falling = false;
			}

			// move →
			if (allowRight && InputManager.Keyboard.KeyDown(Keys.D))
			{
				Player.X += moveFactor;
				Player.Y += moveRightOffsetY;
			}
			/*}*/

			if (InputManager.Keyboard.KeyPushed(Keys.Q))
			{
				Player.PrintPosition();
			}

			Player.TurnDirectionFacing();
		}
	}

}
