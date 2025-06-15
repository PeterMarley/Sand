using FlatRedBall;
using FlatRedBall.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Sand.Services;
using System;
using static FlatRedBall.Input.Mouse;
using static Sand.Constants;
using Point = System.Drawing.Point;

namespace Sand;

public class DrawableWorld
{
	private const int PLAYER_MOVE_FACTOR = 15;
	private const int GRAV_MAGNITUDE = 1;

	//========================================
	// Fields / Properties
	//========================================
	public WorldCoords WorldCoords { get; private set; }
	public StuffCell[] StuffCells { get; private set; }
	private CheckerBoardBackground Background { get; set; }
	private Player Player { get; set; }


	//===========================================
	// CTOR
	//===========================================
	public DrawableWorld(StuffCellSetup worldSetup)
	{		
		Background = new CheckerBoardBackground();
		Player = new Player();
		StuffCells =
		[
			new StuffCell(-1, worldSetup),
			new StuffCell(0, worldSetup),
			new StuffCell(1, worldSetup)
		];
	}

	//===========================================
	// Doing Stuff
	//===========================================
	private void ApplyGravity(StuffCell cell, int xIndex, int yIndex)
	{
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

			// if bottom row outside array range just continue as this Stuff cant fall anywhere
			var rowBelowIndex = yIndex - 1;
			if (rowBelowIndex < 0) return;

			//==================================================================
			//Check 3 spots: below left and right. If all are filled then move on.
			//==================================================================

			// check directly below
			if ((rowBelowIndex - 2 >= 0 && cell.Move(new(xIndex, yIndex), new(xIndex, rowBelowIndex - 2)) || (rowBelowIndex - 1 >= 0 && cell.Move(new(xIndex, yIndex), new(xIndex, rowBelowIndex - 1)))))
			{
				return;
			}

			// check below-left and below-right (alterating sides randomly)
			bool leftSide = FastRandomBoolGenerator.Instance.Next();
			int colLeftIndex = xIndex - 1;
			int colRightIndex = xIndex + 1;

			for (var lateralGravAttempts = 2; lateralGravAttempts > 0; lateralGravAttempts--)
			{
				if (leftSide)
				{
					// check below and left
					if (colLeftIndex >= 0 && cell.Move(new(xIndex, yIndex), new(colLeftIndex, rowBelowIndex)))
					{
						return;
						//break;
					}
				}
				else
				{
					// check below and right
					if (colRightIndex < STUFF_CELL_WIDTH && cell.Move(new(xIndex, yIndex), new(colRightIndex, rowBelowIndex)))
					{
						return;
						//break;
					}
				}
				leftSide = !leftSide;
			}

			// check directly below
			if (cell.Move(new(xIndex, yIndex), new(xIndex, rowBelowIndex)))
			{
				return;
			}

		}
		void ApplyGravityPhaseLiquid(StuffCell cell, Stuff stuff, int xIndex, int yIndex)
		{
			// if bottom row outside array range just continue as this Stuff cant fall anywhere
			var rowBelowIndex = yIndex - 1;
			if (rowBelowIndex < 0) return;

			//==================================================================
			//Check 3 spots: below left and right. If all are filled then move on.
			//==================================================================

			// if dormant, and theres nothing below, finish
			if (stuff.CheckDormancy() && cell.GetStuffAt(xIndex, rowBelowIndex) != null) // this is not null check basically enables erroneously dormant stuff to right itself
			{
				return;
			}

			// check directly below
			if ((rowBelowIndex - 2 >= 0 && cell.Move(new(xIndex, yIndex), new(xIndex, rowBelowIndex - 2)) || (rowBelowIndex - 1 >= 0 && cell.Move(new(xIndex, yIndex), new(xIndex, rowBelowIndex - 1)))))
			{
				return;
			}

			// check below-left and below-right (alterating sides randomly)
			bool leftFirst = FastRandomBoolGenerator.Instance.Next();
			bool leftSide = leftFirst;
			int colLeftIndex = xIndex - 1;
			int colRightIndex = xIndex + 1;

			for (var lateralGravAttempts = 2; lateralGravAttempts > 0; lateralGravAttempts--)
			{
				if (leftSide)
				{
					// check below and left
					if (colLeftIndex >= 0 && cell.Move(new(xIndex, yIndex), new(colLeftIndex, rowBelowIndex)))
					{
						break;
					}
				}
				else
				{
					// check below and right
					if (colRightIndex < STUFF_CELL_WIDTH && cell.Move(new(xIndex, yIndex), new(colRightIndex, rowBelowIndex)))
					{
						break;
					}
				}
				leftSide = !leftSide;
			}

			//==================================================================
			//Check 2 spots directly left and right - represent fluidic flow
			//==================================================================

			if (TimeManager.CurrentFrame % 1 == 0)
			{
				colLeftIndex--;
				colRightIndex++;
				leftSide = leftFirst;

				// check direct lateral movements
				for (var lateralFlowAttempts = 2; lateralFlowAttempts > 0; lateralFlowAttempts--)
				{
					if (leftSide)
					{
						// check left
						if (colLeftIndex >= 0 && cell.Move(new(xIndex, yIndex), new(colLeftIndex, yIndex)))
						{
							break;
						}
					}
					else
					{
						// check right
						if (colRightIndex < STUFF_CELL_WIDTH && cell.Move(new(xIndex, yIndex), new(colRightIndex, yIndex)))
						{
							break;
						}
					}
					leftSide = !leftSide;
				}
			}
		}
	}
	public void Update()
	{
		SetupWorldCoords();
		ProcessChunks();

		void SetupWorldCoords()
		{
			var camBottomLeft = new Vector2((Camera.Main.X - Camera.Main.OrthogonalWidth / 2), (Camera.Main.Y - Camera.Main.OrthogonalHeight / 2));
			var mouseRelative = new Vector2(InputManager.Mouse.X, RESOLUTION_Y - InputManager.Mouse.Y);

			WorldCoords = new WorldCoords(
				mouseRelative: mouseRelative,
				mouseAbsolute: new Vector2(mouseRelative.X + camBottomLeft.X, mouseRelative.Y + camBottomLeft.Y),
				camCentre: new Vector2(Camera.Main.X, Camera.Main.Y),
				camBottomLeft: camBottomLeft,
				camOffset: camBottomLeft,
				camTopRight: new Vector2(Camera.Main.X + (RESOLUTION_X / 2), Camera.Main.Y + (RESOLUTION_Y / 2))
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
			foreach (var cell in StuffCells)
			{
				var p = new Point();

				try
				{
					//=======================================================
					// New version of world update loop (utilising chunks)
					//=======================================================
					
					foreach (var chunk in cell.GetChunksToUpdate())
					{
						foreach (var point in chunk.Points_BottomLeftToTopRight_AlternatingRowDirection)
						{
							p = point;

							if (cell.GetStuffAt(point) != null)
							{
								ApplyGravity(cell, point.X, point.Y);
							}
						}
					}
				}
				catch (Exception chunkException)
				{
					Logger.Instance.LogError(chunkException, $"(x,y)=({p.X},{p.Y}), (maxX, maxY)=({STUFF_CELL_WIDTH - 1},{STUFF_CELL_HEIGHT - 1})");
					throw;
				}
			}
		}

	}
	public void ProcessControlsInput()
	{
		var mousePosition = CoordManager.Convert(WorldCoords.MouseAbsolute);
		var playerPosition = CoordManager.Convert(new Vector2(Player.Hitbox.X, Player.Hitbox.Y));

		AffectWorld(mousePosition);
		AffectPlayer(playerPosition);

		void AffectWorld(SandCoordinate mousePos)
		{
			if (InputManager.Mouse.IsInGameWindow())
			{
				if (InputManager.Keyboard.KeyDown(Keys.Left))
				{
					Camera.Main.X -= 5 * STUFF_TO_PIXEL_SCALE;
				}

				if (InputManager.Keyboard.KeyDown(Keys.Right))
				{
					Camera.Main.X += 5 * STUFF_TO_PIXEL_SCALE;
				}

				if ((InputManager.Mouse.ButtonPushed(MouseButtons.LeftButton) || InputManager.Mouse.ButtonDown(MouseButtons.LeftButton)))
				{
					StuffCells[mousePos.ChunkIndex].SafeAddStuffIfEmpty_InSquare(Stuffs.BASIC_WATER, mousePos.StuffPosition.X, mousePos.StuffPosition.Y, 15);
				}

				if ((InputManager.Mouse.ButtonPushed(MouseButtons.RightButton) || InputManager.Mouse.ButtonDown(MouseButtons.RightButton)))
				{
					StuffCells[mousePos.ChunkIndex].SafeAddStuffIfEmpty_InSquare(Stuffs.BASIC_SAND, mousePos.StuffPosition.X, mousePos.StuffPosition.Y, 15);
				}

				if ((InputManager.Mouse.ButtonPushed(MouseButtons.MiddleButton) || InputManager.Mouse.ButtonDown(MouseButtons.MiddleButton)))
				{
					StuffCells[mousePos.ChunkIndex].SafeAddStuffIfEmpty_InSquare(Stuffs.BASIC_STONE, mousePos.StuffPosition.X, mousePos.StuffPosition.Y, 10);
				}

				if ((InputManager.Mouse.ButtonPushed(MouseButtons.XButton1) || InputManager.Mouse.ButtonDown(MouseButtons.XButton1)))
				{
					StuffCells[mousePos.ChunkIndex].SafeAddStuffIfEmpty_InSquare(Stuffs.BASIC_LAVA2, mousePos.StuffPosition.X, mousePos.StuffPosition.Y, 10);
				}
			}
		}
		void AffectPlayer(SandCoordinate playerPos)
		{
			// TODO current => same for each cell
			//	   eventual => pinpoint where the player is and do all cells as needed


			var cell = StuffCells[playerPos.ChunkIndex];
			var collision = cell.Collision(Player, GRAV_MAGNITUDE, PLAYER_MOVE_FACTOR);

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

			//if (InputManager.Keyboard.KeyPushed(Keys.Q))
			//{
			//	Player.PrintPosition();
			//}

			Player.TurnDirectionFacing();
		}
	}

}
