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

	//========================================
	// The World
	//========================================

	//public StuffCell StuffCell { get; set; }
	public StuffCell[] StuffCells { get; set; }
	private Texture2D WorldTexture { get; set; }
	private Sprite WorldSprite { get; set; }
	private SpriteBatch BgSpriteBatch { get; set; }
	private Texture2D BgTexture { get; set; }

	//========================================
	// The Player
	//========================================

	public Player Player { get; private set; }
	private CheckerBoardBackground Background { get; set; } 
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
			
			//StuffCell = new StuffCell(worldSetup);
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

	#region Public API

	#region Moving Stuff

	public void ApplyGravity(int xIndex, int yIndex)
	{
		//TODO stuff cell naive quick impl
		var stuff = StuffCells[1].GetStuffAt(xIndex, yIndex);

		switch (stuff.Phase)
		{
			case Phase.Powder:
				ApplyGravityPhasePowder(xIndex, yIndex);
				break;
			case Phase.Liquid:
				ApplyGravityPhaseLiquid(xIndex, yIndex);
				break;
		}

		void ApplyGravityPhasePowder(int xIndex, int yIndex)
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
			if ((rowBelowIndex - 2 >= 0 && Move(new(xIndex, yIndex), new(xIndex, rowBelowIndex - 2)) || (rowBelowIndex - 1 >= 0 && Move(new(xIndex, yIndex), new(xIndex, rowBelowIndex - 1)))))
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
					if (colLeftIndex >= 0 && Move(new(xIndex, yIndex), new(colLeftIndex, rowBelowIndex)))
					{
						return;
						//break;
					}
				}
				else
				{
					// check below and right
					if (colRightIndex < STUFF_WIDTH && Move(new(xIndex, yIndex), new(colRightIndex, rowBelowIndex)))
					{
						return;
						//break;
					}
				}
				leftSide = !leftSide;
			}

			// check directly below
			if (Move(new(xIndex, yIndex), new(xIndex, rowBelowIndex)))
			{
				return;
			}

		}

		void ApplyGravityPhaseLiquid(int xIndex, int yIndex)
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
			if ((rowBelowIndex - 2 >= 0 && Move(new(xIndex, yIndex), new(xIndex, rowBelowIndex - 2)) || (rowBelowIndex - 1 >= 0 && Move(new(xIndex, yIndex), new(xIndex, rowBelowIndex - 1)))))
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
					if (colLeftIndex >= 0 && Move(new(xIndex, yIndex), new(colLeftIndex, rowBelowIndex)))
					{
						break;
					}
				}
				else
				{
					// check below and right
					if (colRightIndex < STUFF_WIDTH && Move(new(xIndex, yIndex), new(colRightIndex, rowBelowIndex)))
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
						if (colLeftIndex >= 0 && Move(new(xIndex, yIndex), new(colLeftIndex, yIndex)))
						{
							break;
						}
					}
					else
					{
						// check right
						if (colRightIndex < STUFF_WIDTH && Move(new(xIndex, yIndex), new(colRightIndex, yIndex)))
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

	public bool Move(Point from, Point to)
	{
		//TODO stuff cell naive quick impl
		return StuffCells[1].Move(from, to);
	}

	#endregion

	#region Game Loop Methods called by SandGame
	private const int _FRAME_COUNT_BETWEEN_UPDATE = 1;
	public void Update()
	{
/*		if (TimeManager.CurrentFrame % _FRAME_COUNT_BETWEEN_UPDATE != 0)
		{
			return;
		}
*/
		var p = new Point();

		try
		{
			//=======================================================
			// New version of world update loop (utilising chunks)
			//=======================================================

			// choose which chunks to update
			//var chunksToUpdate = StuffCell.GetChunksToUpdate();
			//TODO stuff cell naive quick impl
			var chunksToUpdate = StuffCells[1].GetChunksToUpdate();

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
					if (StuffCells[1].GetStuffAt(point) != null)
					{
						ApplyGravity(xIndex, yIndex);
					}
				}
			}
		}
		catch (Exception updateException)
		{
			Logger.Instance.LogError(updateException, $"(x,y)=({p.X},{p.Y}), (maxX, maxY)=({STUFF_WIDTH - 1},{STUFF_HEIGHT - 1})");
			throw;
		}
	}

	public void DrawWorldManually(Camera camera)
	{
/*
		try
		{
			//===================================================
			// DRAW BACKGROUND TEXTURE
			//===================================================

			// Required or sprite batch texture will be blurry
			BgSpriteBatch.Begin(
				sortMode: SpriteSortMode.BackToFront,// enable depth sorting
				samplerState: SamplerState.PointClamp // ensure texture isnt stretched, but remains true "pixels" and not blurry
			);
			BgSpriteBatch.Begin(
	sortMode: SpriteSortMode.BackToFront,
	blendState: BlendState.AlphaBlend,
	samplerState: SamplerState.PointClamp,
	depthStencilState: DepthStencilState.Default,
	rasterizerState: RasterizerState.CullNone
);

			float tWidth = BgTexture.Width;
			float tHeight = BgTexture.Height;

			int maxCol = (int)(Constants.RESOLUTION_X / tWidth);
			int maxRow = (int)(Constants.RESOLUTION_Y / tHeight);

			//for (int x = 0; x < maxCol; x++)
			//{
			//	for (int y = 0; y < maxRow; y++)
			//	{
			//		Vector2 v = new Vector2(x * tWidth, y * tHeight);
			//		BgSpriteBatch.Draw(BgTexture, v, Color.White);
			//		//BgSpriteBatch.Draw(BgTexture, v, null, Color.White, 0f, new Vector2(0,0), new Vector2(1, 1), SpriteEffects.None, 0.5f);
			//	}
			//}

			//===================================================
			// DRAW WORLD TEXTURE
			//===================================================

			// interate through the World and get the color data of all Stuffs there
			var colorData = StuffCell.GetColorData();
			if (WorldSprite == null)
			{

				var wsWidth = RESOLUTION_X * 2;
				var wsHeight = RESOLUTION_Y * 2;

				WorldTexture = new Texture2D(FlatRedBallServices.GraphicsDevice, STUFF_WIDTH, STUFF_HEIGHT);
				WorldTexture.SetData(colorData);

				WorldSprite = SpriteManager.AddSprite(WorldTexture);
				WorldSprite.Width = wsWidth;// RESOLUTION_X * 2;
				WorldSprite.Height = wsHeight;// RESOLUTION_Y * 2;
				WorldSprite.X += RESOLUTION_X / 2;
				WorldSprite.Y += RESOLUTION_Y / 2;
				WorldSprite.Z = 0.9f;
				WorldSprite.X = -5000;
				WorldSprite.Y = -5000;


				Camera.Main.Orthogonal = true;
				Camera.Main.OrthogonalWidth = WorldSprite.Width;
				Camera.Main.OrthogonalHeight = WorldSprite.Height;
				Camera.Main.OrthogonalWidth = wsWidth;
				Camera.Main.OrthogonalHeight = wsHeight;
			}
			else
			{
				WorldTexture.SetData(colorData);
				SpriteManager.ManualUpdate(WorldSprite);
			}

			//BgSpriteBatch.Draw(WorldTexture, new Rectangle(0,0, RESOLUTION_X, RESOLUTION_Y), Color.White);
			// change tto
			BgSpriteBatch.Draw(WorldTexture, new Rectangle(0, 0, RESOLUTION_X, RESOLUTION_Y), null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.0f);


			//===================================================
			// DRAW PLAYER TEXTURE
			//===================================================

			BgSpriteBatch.End();
		}
		catch (Exception drawWorldEx)
		{
			Logger.Instance.LogError(drawWorldEx, $"Failed to process and apply the colour of each {nameof(Stuff)} to the {nameof(WorldTexture)}");
			throw;
		}*/
	}

	// Not Implemented
	public void Destroy()
	{
		throw new NotImplementedException();
	}


	private const int PLAYER_MOVE_FACTOR = 15;
	private const int GRAV_MAGNITUDE = 1;
	public void ProcessControlsInput()
	{
		AffectWorld();
		AffectPlayer();

		void AffectWorld()
		{
			if (InputManager.Mouse.IsInGameWindow())
			{
				var x = InputManager.Mouse.X / STUFF_SCALE;
				var y = (FlatRedBallServices.GraphicsDevice.Viewport.Height - InputManager.Mouse.Y) / STUFF_SCALE;

				if (InputManager.Keyboard.KeyDown(Keys.Z))
				{
					Camera.Main.X -= 1 * STUFF_SCALE;
				}

				if (InputManager.Keyboard.KeyDown(Keys.C))
				{
					Camera.Main.X += 1 * STUFF_SCALE;
				}

				if ((InputManager.Mouse.ButtonPushed(MouseButtons.LeftButton) || InputManager.Mouse.ButtonDown(MouseButtons.LeftButton)))
				{
					//TODO stuff cell naive quick impl
					StuffCells[1].SafeAddStuffIfEmpty_InSquare(Stuffs.BASIC_WATER, x, y, 15);
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
					//gravity
					Player.Y -= moveFactor * .7f;
					Player.Falling = true;
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

	#endregion

	#endregion

}
