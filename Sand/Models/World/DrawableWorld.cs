using FlatRedBall;
using FlatRedBall.Content.Math.Splines;
using FlatRedBall.Graphics;
using FlatRedBall.Input;
using FlatRedBall.Math.Geometry;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Sand.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using static FlatRedBall.Input.Mouse;
using static Sand.Constants;
using Point = System.Drawing.Point;

namespace Sand;


public class DrawableWorld : IDrawableBatch
{
	/// <summary>
	/// Outer array is X, inner array is Y.
	/// <br/><em>[0, 0]</em> represents bottom left of viewport.
	/// <br/><em>[0, yMax]</em> represents top left.
	/// <br/><em>[xMax, yMax]</em> represents top right.
	/// </summary>
	public Player Player { get; private set; }
	//public Layer LayerPlayer { get; init; }
	public Stuff[][] World { get; private set; }
	//public Layer LayerWorld { get; init; }

	public float X { get; set; }
	public float Y { get; set; }
	public float Z { get; init; } = 0;
	public bool UpdateEveryFrame { get; set; } = true;

	public DrawableWorld()
	{
		World = new Stuff[STUFF_WIDTH][];
		for (int x = 0; x < World.Length; x++)
		{
			World[x] = new Stuff[STUFF_HEIGHT];
			//for (int y = 0; y < World[x].Length; y++)
			//{
			//	if (x == 0 || x == World.Length // if at far left or right
			//	|| y == 0 || y == World[x].Length) // if at far top or bottom
			//	{
			//		World[x][y] = StuffFactory.Instance.Get(Stuffs.BASIC_STONE);
			//	}
			//}
		}

		Player = new Player();
	}


	#region Public API

	#region Adding Stuff
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

	#endregion

	#region Moving Stuff

	public void ApplyGravity(int xIndex, int yIndex)
	{
		var stuff = World[xIndex][yIndex];

		//if (stuff == null)
		//{
		//	continue;
		//}
		//else if (stuff.Dormant)
		//{
		//	if (stuff.DormantChecks > 20)
		//	{
		//		stuff.Dormant = false;
		//	}
		//	else
		//	{
		//		continue;
		//	}
		//}

		//if (stuff == null || stuff.Dormant)
		//{
		//	return;
		//}

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

			//-----------------------------------------------------------------
			//Check 2 spots below left and right, if all are filled then move on
			//-----------------------------------------------------------------

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
			if (stuff.CheckDormancy() && World[xIndex][rowBelowIndex] != null) // this is not null check basically enables erroneously dormant stuff to right itself
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

	private const int NOT_MOVED_DORMANT_TRIGGER = 10;
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

			Stuff stuffTarget = World.Get(to);
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
			if (stuffTarget != null && stuffTarget is { Phase: Phase.Liquid})
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

	#endregion

	#region Game Loop Methods called by SandGame

	//private bool leftSide = true;
	public void Update()
	{
		var p = new Point();
		var ltr = true;

		try
		{
			var i = 0;
			for (var yIndex = 0; yIndex < STUFF_HEIGHT; yIndex++)
			{
				for (var xIndexSource = 0; xIndexSource < STUFF_WIDTH; xIndexSource++, i++)
				{

					//==========================================================||
					// we adjust the x index depending if we're going
					//	left to right (ltr) => 0 to last index
					//	right to left (rtl) => last index to 0
					//==========================================================||

					int xIndex = ltr ? xIndexSource : STUFF_WIDTH - 1 - xIndexSource;

					p.X = xIndex;
					p.Y = yIndex;

					// get stuff here
					var stuff = World[xIndex][yIndex];

					// if nothing here then move on to next Stuff **NOTE** dormancy is checked in apply gravity
					if (stuff == null/* || stuff.CheckDormancy()*/)
					{
						continue;
					}

					ApplyGravity(xIndex, yIndex);
				}

				// flip the direction of the next horizontal traversal - for reasons
				ltr = !ltr;
			}
		}
		catch (Exception applyGravEx)
		{
			Logger.Instance.LogError(applyGravEx, $"(x,y)=({p.X},{p.Y}), (maxX, maxY)=({STUFF_WIDTH - 1},{STUFF_HEIGHT - 1})");
			throw;
		}
	}

	public void Print()
	{


		//foreach (var i in World)
		//{
		//	foreach (var j in i)
		//	{
		//		_stringBuilder.Append($" {(j == null ? "--" : j.Id.ToString()[..2])} ");
		//	}
		//	_stringBuilder.AppendLine();
		//}
	}

	private Texture2D WorldTexture;
	private Sprite WorldSprite;

	public void Draw(Camera camera)
	{

		try
		{
			var colorData = new Color[STUFF_HEIGHT * STUFF_WIDTH];

			var cx = 0;
			
			for (var y = World[0].Length - 1; y >= 0; y--)
			{
				for (var x = 0; x < World.Length; x++)
				{
					// if nothing here then color this pixel black
					if (World[x][y] == null)
					{
						colorData[cx++] = Color.Black;
					}
					// otherwise colour by the stuff there
					else
					{
						colorData[cx++] = World[x][y].Color;
					}
				}
			}

			if (WorldSprite == null)
			{

				FlatRedBallServices.GraphicsOptions.TextureFilter = TextureFilter.Point;

				WorldTexture = new Texture2D(FlatRedBallServices.GraphicsDevice, STUFF_WIDTH, STUFF_HEIGHT);
				WorldTexture.SetData(colorData);

				WorldSprite = SpriteManager.AddManualSprite(WorldTexture);

				WorldSprite.Width = RESOLUTION_X * 2;
				WorldSprite.Height = RESOLUTION_Y * 2;
				WorldSprite.X += RESOLUTION_X / 2;
				WorldSprite.Y += RESOLUTION_Y / 2;
				WorldSprite.Z = 0;

				Camera.Main.Orthogonal = true;
				Camera.Main.OrthogonalWidth = WorldSprite.Width;
				Camera.Main.OrthogonalHeight = WorldSprite.Height;
			}
			else
			{
				WorldTexture.SetData(colorData);
				SpriteManager.ManualUpdate(WorldSprite);
			}

		}
		catch (Exception applyGravEx)
		{
			Logger.Instance.LogError(applyGravEx, "Draw");
			throw;
		}
	}

	// Not Implemented
	public void Destroy()
	{
		throw new NotImplementedException();
	}


	private const int PLAYER_MOVE_FACTOR = 10;
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

				if ((InputManager.Mouse.ButtonPushed(MouseButtons.LeftButton) || InputManager.Mouse.ButtonDown(MouseButtons.LeftButton)))
				{
					SafeAddStuffIfEmpty_InSquare(Stuffs.BASIC_WATER, x, y, 10);
					//_world.SafeAddStuffIfEmpty(Stuffs.BASIC_WATER, x, y);

				}

				if ((InputManager.Mouse.ButtonPushed(MouseButtons.RightButton) || InputManager.Mouse.ButtonDown(MouseButtons.RightButton)))
				{
					SafeAddStuffIfEmpty_InSquare(Stuffs.BASIC_SAND, x, y, 10);
					//_world.SafeAddStuffIfEmpty(Stuffs.BASIC_SAND, x, y);
				}

				if ((InputManager.Mouse.ButtonPushed(MouseButtons.MiddleButton) || InputManager.Mouse.ButtonDown(MouseButtons.MiddleButton)))
				{
					ForceAddStuff_InSquare(Stuffs.BASIC_STONE, x, y, 10);
					//_world.SafeAddStuffIfEmpty(Stuffs.BASIC_SAND, x, y);
				}
			}
		}

		void AffectPlayer()
		{

			////Gravity;
			//Player.Sprite.Acceleration.Y -= /*-*/10;
			////Wind Resistance
			//if (Player.Sprite.Acceleration.Y < 0)
			//{
			//	Player.Sprite.Acceleration.Y += 3;
			//}
			

			if (InputManager.Keyboard.KeyDown(Keys.W))
			{
				Player.Sprite.Y += PLAYER_MOVE_FACTOR;

				//Player.Sprite.Acceleration.Y += PLAYER_MOVE_FACTOR;
				//Player.Sprite.Position.Y += 10;

				//if (Player.Sprite.Velocity.Y < 0)
				//{
				//	Player.Sprite.Velocity.Y = 0;
				//}
				
			}

			//Player.Sprite.Acceleration.Y -= PLAYER_MOVE_FACTOR / 4;//gravity;

			if (InputManager.Keyboard.KeyDown(Keys.A))
			{
				Player.Sprite.X -= PLAYER_MOVE_FACTOR;
			}

			if (InputManager.Keyboard.KeyDown(Keys.S))
			{
				Player.Sprite.Y -= PLAYER_MOVE_FACTOR;
			}

			if (InputManager.Keyboard.KeyDown(Keys.D))
			{
				Player.Sprite.X += PLAYER_MOVE_FACTOR;
			}

			// stuff at bottom left of player
			if (InputManager.Keyboard.KeyDown(Keys.E))
			{
				
			}

			if (InputManager.Keyboard.KeyDown(Keys.Insert))
			{
				Player.Sprite.Top = RESOLUTION_Y;
				Player.Sprite.Left = 0;
			}

			if (InputManager.Keyboard.KeyDown(Keys.Home))
			{
				Player.Sprite.Top = RESOLUTION_Y;
				Player.Sprite.Right = RESOLUTION_X;
			}

			if (InputManager.Keyboard.KeyDown(Keys.End))
			{
				Player.Sprite.Bottom = 0;
				Player.Sprite.Right = RESOLUTION_X;
			}

			if (InputManager.Keyboard.KeyDown(Keys.Delete))
			{
				Player.Sprite.Left = 0;
				Player.Sprite.Bottom = 0;
			}

			int i = 0;
			int y1 = 0;

			//int spriteX;// = (int)((Player.Sprite.Left + 1000) / STUFF_SCALE);// - (Player.StuffWidth / 2));// + (RESOLUTION_X / 2);
			//int spriteY;// = (int)((Player.Sprite.Right + 1000) / STUFF_SCALE);// - (Player.StuffWidth / 2));// + (RESOLUTION_X / 2);
			int spriteGridLeft;// = (int)((Player.Sprite.Left + 1000) / STUFF_SCALE);// - (Player.StuffWidth / 2));// + (RESOLUTION_X / 2);
			int spriteGridRight;// = (int)((Player.Sprite.Right + 1000) / STUFF_SCALE);// - (Player.StuffWidth / 2));// + (RESOLUTION_X / 2);
			int spriteGridTop;// = (int)((Player.Sprite.Top + 1000) / STUFF_SCALE);// - (Player.StuffHeight / 2));// + (RESOLUTION_X / 2);
			int spriteGridBottom;// = (int)((Player.Sprite.Bottom + 1000) / STUFF_SCALE);// - (Player.StuffHeight / 2));// + (RESOLUTION_X / 2);
			try
			{

				if (InputManager.Keyboard.KeyPushed(Keys.Q))
				{
					Logger.Instance.LogInfo($"NORMAL X={((int)Player.Sprite.X):D4} Y={((int)Player.Sprite.Y):D4} L={((int)Player.Sprite.Left):D4} R={((int)Player.Sprite.Right):D4} T={((int)Player.Sprite.Top):D4} B={((int)Player.Sprite.Bottom):D4}");
					//var spriteGridLeft = (int)((Player.Sprite.X / STUFF_SCALE));// - (Player.StuffWidth / 2));// + (RESOLUTION_X / 2);
					//var spriteGridRight = (int)((Player.Sprite.X / STUFF_SCALE));// - (Player.StuffWidth / 2));// + (RESOLUTION_X / 2);
					//var spriteGridTop = (int)((Player.Sprite.Y / STUFF_SCALE));// - (Player.StuffHeight / 2));// + (RESOLUTION_X / 2);
					//var spriteGridBottom = (int)((Player.Sprite.Y / STUFF_SCALE));// - (Player.StuffHeight / 2));// + (RESOLUTION_X / 2);
					//var offsetX = Player.Sprite.Width / 2;
					//var offsetY = Player.Sprite.Height / 2;
					//var spriteX = (int)((Player.Sprite.X + 1000) / STUFF_SCALE);// - (Player.StuffWidth / 2));// + (RESOLUTION_X / 2);
					//var spriteY = (int)((Player.Sprite.Y + 1000) / STUFF_SCALE);// - (Player.StuffWidth / 2));// + (RESOLUTION_X / 2);

					////spriteGridLeft = (int)((Player.Sprite.Left + 1000) / STUFF_SCALE);// - (Player.StuffWidth / 2));// + (RESOLUTION_X / 2);
					////spriteGridRight = (int)((Player.Sprite.Right + 1000) / STUFF_SCALE);// - (Player.StuffWidth / 2));// + (RESOLUTION_X / 2);
					////spriteGridTop = (int)((Player.Sprite.Top + 1000) / STUFF_SCALE);// - (Player.StuffHeight / 2));// + (RESOLUTION_X / 2);
					////spriteGridBottom = (int)((Player.Sprite.Bottom + 1000) / STUFF_SCALE);// - (Player.StuffHeight / 2));// + (RESOLUTION_X / 2);
					////Logger.Instance.LogInfo($"CALCD 1 X={spriteX:D4} Y={spriteY:D4}  L={spriteGridLeft:D4} R={spriteGridRight:D4} T={spriteGridTop:D4} B={spriteGridBottom:D4}");

					//var x = Player.Sprite.X;
					//var y = Player.Sprite.Y;

					//spriteGridLeft = (int)((x - offsetX + (RESOLUTION_X / 2)) / STUFF_SCALE);// - (Player.StuffWidth / 2));// + (RESOLUTION_X / 2);
					//spriteGridRight = (int)((x + offsetX + (RESOLUTION_X / 2)) / STUFF_SCALE);// - (Player.StuffWidth / 2));// + (RESOLUTION_X / 2);
					//spriteGridTop = (int)((y + offsetY + (RESOLUTION_Y / 2)) / STUFF_SCALE);// - (Player.StuffHeight / 2));// + (RESOLUTION_X / 2);
					//spriteGridBottom = (int)((y - offsetY + (RESOLUTION_Y / 2)) / STUFF_SCALE);// - (Player.StuffHeight / 2));// + (RESOLUTION_X / 2);

					var (top, right, bottom, left) = NumberExtensions.ToStuffCoord(Player.Sprite);


					spriteGridLeft = left;
					spriteGridRight = right;
					spriteGridTop = top;
					spriteGridBottom = bottom;

					//Logger.Instance.LogInfo($"CALCD 2 X={spriteX:D4} Y={spriteY:D4}  L={spriteGridLeft:D4} R={spriteGridRight:D4} T={spriteGridTop:D4} B={spriteGridBottom:D4}\n");

					var abs = Math.Abs(spriteGridLeft);


					var adj = (abs <= 10 ? "   " : (abs <= 100 ? "  " : (abs <= 1000 ? " " : "")));



					//var xyz =
					//   $"\n ----{spriteGridTop}---- \n" +
					//	"|            |\n" +
					//	"|            |\n" +
					//   $"{spriteGridLeft}         {adj}|     HEIGHT=[{Player.Sprite.Height}]\n" +
					//   $"|            {spriteGridRight}\n" +
					//	"|            |\n" +
					//	"|            |\n" +
					//   $" ----{spriteGridBottom}----\n\n " +
					//   $"WIDTH=[{Player.Sprite.Width}]";

					var xyz =
					   $"\n ----{spriteGridTop / STUFF_SCALE}---- \n" +
						"|            |\n" +
						"|            |\n" +
					   $"{spriteGridLeft / STUFF_SCALE}         {adj}|     HEIGHT=[{Player.Sprite.Height}]\n" +
					   $"|            {spriteGridRight / STUFF_SCALE}\n" +
						"|            |\n" +
						"|            |\n" +
					   $" ----{spriteGridBottom / STUFF_SCALE}----\n\n " +
					   $"WIDTH=[{Player.Sprite.Width}]";

					Logger.Instance.LogInfo(xyz);

					//var c = 0;
					//for (i = spriteGridLeft; i <= spriteGridRight; i++, c++)
					//{
					//	if (i < 0 || i >= World.Length)
					//	{
					//		continue;
					//	}

					//	//sideBuffer[i] = new(x, spriteGridTop);
					//	Point p /*sideBuffer[i]*/ = new(i, spriteGridBottom);
					//	var stuff = World.Get(p);
					//	if (stuff == null)
					//	{
					//		// if empty, fugheddaboudit
					//		continue;
					//	}
					//	else
					//	{
					//		if (stuff is { Phase: Phase.Solid })
					//		{
					//			Player.Sprite.Velocity.Y = 0;
					//			break;
					//		}
					//	}

					//}
				}
				//	//========================================================
				//	// build up a collection of points surrounding the sprite
				//	//========================================================

				//	//** left and rights => for every y between bottom and top, take the left most and right most x

				//	// TODO

				//	//** top and bottoms => for every x between left and right, take the top and bottom most y

				//	//var spriteGridLeft = (int)(Player.Sprite.Left / STUFF_SCALE);// + (RESOLUTION_X / 2);
				//	//var spriteGridRight = (int)(Player.Sprite.Right / STUFF_SCALE);// + (RESOLUTION_X / 2);
				//	//var spriteGridTop = (int)(Player.Sprite.Top / STUFF_SCALE);// + (RESOLUTION_Y / 2);
				//	//var spriteGridBottom = (int)(Player.Sprite.Bottom / STUFF_SCALE);// + (RESOLUTION_Y / 2);

				//	var spriteGridLeft = (int)((Player.Sprite.X / STUFF_SCALE));// - (Player.StuffWidth / 2));// + (RESOLUTION_X / 2);
				//	var spriteGridRight = (int)((Player.Sprite.X / STUFF_SCALE));// - (Player.StuffWidth / 2));// + (RESOLUTION_X / 2);
				//	var spriteGridTop = (int)((Player.Sprite.Y / STUFF_SCALE));// - (Player.StuffHeight / 2));// + (RESOLUTION_X / 2);
				//	var spriteGridBottom = (int)((Player.Sprite.Y / STUFF_SCALE));// - (Player.StuffHeight / 2));// + (RESOLUTION_X / 2);

				//	y1 = spriteGridBottom;
				//	// hold a collection of points representing the points at which the sprite is overlaying the DrawableWorld
				//	//Point[] sideBuffer = new Point[spriteGridRight - spriteGridLeft];




				//	//if (Player.Sprite.Velocity.Y < 0)
				//	//{
				

				//		//foreach (var bottomPoint in sideBuffer)
				//		//{
				//		//	var stuff = World.Get(bottomPoint);
				//		//	if (stuff == null)
				//		//	{
				//		//		// if empty, fugheddaboudit
				//		//		continue;
				//		//	}
				//		//	else
				//		//	{
				//		//		if (stuff is { Phase: Phase.Solid })
				//		//		{
				//		//			Player.Sprite.Velocity.Y = 0;
				//		//		}
				//		//	}
				//		//}
				//	//}

			}
			catch (Exception ex)
			{
				throw;
			}

		//for (var x = 0; x < World.Length; x++)
		//{
		//	for (var y = 0; y < World[x].Length; y++)
		//	{

		//		if (Player.Sprite.Velocity.Y < 0)
		//		{
		//			if (Player.Sprite.Bottom)
		//		}

		//		//if (Player.Sprite.Velocity.Y < 0)
		//		//{
		//		//	// goin down
		//		//}
		//		//else
		//		//{
		//		//	// going up
		//		//}

		//		//if (Player.Sprite.Velocity.X < 0)
		//		//{
		//		//	// goin left
		//		//}
		//		//else
		//		//{
		//		//	// going right
		//		//}
		//	}
		//}
	}
	}

	#endregion

	//#region StuffWorld preconfigured setups for dev testing
	//public void PrepareWaterBottom3Y()
	//{
	//	for (int x = 0; x < _world.Length; x++)
	//	{
	//		for (int y = 0; y < 3 && y < _world[x].Length; y++)
	//		{
	//			SafeAddStuffIfEmpty(Stuffs.BASIC_WATER, x, y);
	//		}
	//	}
	//}

	//public void PrepareWaterBottomHalf()
	//{
	//	for (int x = 0; x < _world.Length; x++)
	//	{
	//		for (int y = 0; y < _world[x].Length / 2; y++)
	//		{
	//			SafeAddStuffIfEmpty(Stuffs.BASIC_WATER, x, y);
	//		}
	//	}
	//}
	//#endregion

	#endregion

	public static readonly Color[] COLOURS = new Color[1024]
	{
		Color.DarkOliveGreen, Color.Bisque, Color.LightSalmon, Color.LemonChiffon, Color.Ivory, 
		Color.DarkSlateGray, Color.DarkKhaki, Color.YellowGreen, Color.DarkCyan, Color.Peru, 
		Color.BlanchedAlmond, Color.Black, Color.DarkGoldenrod, Color.IndianRed, Color.Lavender, 
		Color.Tan, Color.Bisque, Color.Gray, Color.YellowGreen, Color.PeachPuff, Color.Indigo,
		Color.RosyBrown, Color.LightSeaGreen, Color.AntiqueWhite, Color.DimGray, Color.Peru,
		Color.MediumSpringGreen, Color.LightSeaGreen, Color.DeepSkyBlue, Color.IndianRed, Color.MediumSlateBlue,
		Color.DarkKhaki, Color.DarkGoldenrod, Color.Olive, Color.DarkGray, Color.MintCream,
		Color.MediumTurquoise, Color.LightGray, Color.Brown, Color.Salmon,Color.WhiteSmoke, 
		
		
		Color.DarkRed, Color.OldLace, Color.Cyan, Color.LightYellow, Color.MistyRose, Color.Goldenrod, Color.CornflowerBlue,
		Color.Brown, Color.Khaki, Color.LightSteelBlue, Color.Cyan, Color.Lavender, Color.DarkGreen, Color.Olive, Color.LightSeaGreen,
		Color.SaddleBrown, Color.Moccasin, Color.DodgerBlue, Color.NavajoWhite, Color.MidnightBlue, Color.Honeydew, Color.LightGreen, Color.Cornsilk,
		Color.FloralWhite, Color.White, Color.LemonChiffon, Color.DodgerBlue, Color.SandyBrown, Color.Olive, Color.LightPink, Color.Indigo,
		Color.MediumOrchid, Color.Chartreuse, Color.Khaki, Color.BlanchedAlmond, Color.Maroon, Color.PaleGoldenrod, Color.LightGreen, Color.Coral,
		Color.HotPink, Color.Maroon, Color.HotPink, Color.SpringGreen, Color.Orchid, Color.Salmon, Color.DarkTurquoise, Color.LightGray,
		Color.DarkSlateGray, Color.LightBlue, Color.WhiteSmoke, Color.LightGray, Color.Pink, Color.PaleGoldenrod, Color.MistyRose, Color.Indigo,
		Color.DarkSlateGray, Color.Teal, Color.Snow, Color.DarkGoldenrod, Color.BurlyWood, Color.DarkOliveGreen, Color.DeepSkyBlue, Color.DimGray,
		Color.Peru, Color.Coral, Color.OliveDrab, Color.Olive, Color.SeaGreen, Color.Wheat, Color.LightCoral, Color.Aqua,
		Color.DarkOrange, Color.WhiteSmoke, Color.LightGreen, Color.MediumSpringGreen, Color.DarkOliveGreen, Color.LightYellow, Color.PowderBlue, Color.DimGray,
		Color.SaddleBrown, Color.AliceBlue, Color.LightGray, Color.SteelBlue, Color.Fuchsia, Color.Tan, Color.DarkMagenta, Color.Lime,
		Color.Tan, Color.Gray, Color.DeepSkyBlue, Color.Navy, Color.DodgerBlue, Color.Yellow, Color.Wheat, Color.AliceBlue,
		Color.MediumBlue, Color.SlateGray, Color.Azure, Color.DarkOliveGreen, Color.MistyRose, Color.Linen, Color.LawnGreen, Color.Chartreuse,
		Color.LawnGreen, Color.Cyan, Color.DarkBlue, Color.SlateBlue, Color.CornflowerBlue, Color.White, Color.DarkSalmon, Color.DarkSalmon,
		Color.Sienna, Color.Firebrick, Color.LightGray, Color.Wheat, Color.Peru, Color.HotPink, Color.Yellow, Color.Green,
		Color.Magenta, Color.PaleGoldenrod, Color.Navy, Color.Purple, Color.Tomato, Color.RoyalBlue, Color.DarkOrchid, Color.LightBlue,
		Color.Ivory, Color.Coral, Color.MediumSlateBlue, Color.Beige, Color.Khaki, Color.Indigo, Color.AntiqueWhite, Color.Cornsilk,
		Color.Chocolate, Color.Khaki, Color.CornflowerBlue, Color.BlanchedAlmond, Color.MediumPurple, Color.Cornsilk, Color.Thistle, Color.LavenderBlush,
		Color.LightSeaGreen, Color.SlateBlue, Color.HotPink, Color.Yellow, Color.DarkSeaGreen, Color.Sienna, Color.LemonChiffon, Color.Sienna,
		Color.PaleTurquoise, Color.Gold, Color.DarkGray, Color.DarkGray, Color.Plum, Color.MidnightBlue, Color.Peru, Color.PaleVioletRed,
		Color.SeaGreen, Color.CadetBlue, Color.DarkGreen, Color.Chocolate, Color.PaleGreen, Color.MediumSlateBlue, Color.DarkMagenta, Color.LightBlue,
		Color.Goldenrod, Color.Gold, Color.WhiteSmoke, Color.RosyBrown, Color.DarkSlateGray, Color.Peru, Color.Gainsboro, Color.LightSeaGreen,
		Color.SandyBrown, Color.LightBlue, Color.Crimson, Color.Red, Color.DarkGreen, Color.BurlyWood, Color.Yellow, Color.Aquamarine,
		Color.DarkGoldenrod, Color.LavenderBlush, Color.Firebrick, Color.PaleTurquoise, Color.SlateBlue, Color.SkyBlue, Color.HotPink, Color.PaleGoldenrod,
		Color.Chocolate, Color.Firebrick, Color.Olive, Color.AliceBlue, Color.Orange, Color.LightGray, Color.SaddleBrown, Color.LightSlateGray,
		Color.Peru, Color.SlateBlue, Color.DeepSkyBlue, Color.Gold, Color.LightYellow, Color.IndianRed, Color.Chartreuse, Color.Yellow,
		Color.Chocolate, Color.Maroon, Color.Chartreuse, Color.BurlyWood, Color.Silver, Color.SteelBlue, Color.Wheat, Color.DimGray,
		Color.Chartreuse, Color.Teal, Color.Cyan, Color.GhostWhite, Color.CornflowerBlue, Color.CornflowerBlue, Color.LavenderBlush, Color.PaleGreen,
		Color.DarkOrchid, Color.LightBlue, Color.BlueViolet, Color.Cyan, Color.PeachPuff, Color.Turquoise, Color.Maroon, Color.LightGoldenrodYellow,
		Color.GreenYellow, Color.Maroon, Color.LawnGreen, Color.LightGray, Color.Orchid, Color.DarkSeaGreen, Color.Lime, Color.Salmon,
		Color.Maroon, Color.Cornsilk, Color.Aqua, Color.Salmon, Color.DarkGreen, Color.Cornsilk, Color.WhiteSmoke, Color.HotPink,
		Color.Tan, Color.LightGray, Color.DarkSeaGreen, Color.MediumVioletRed, Color.CornflowerBlue, Color.LemonChiffon, Color.NavajoWhite, Color.LightSkyBlue,
		Color.DimGray, Color.Purple, Color.YellowGreen, Color.LimeGreen, Color.Wheat, Color.Aqua, Color.Lime, Color.DarkKhaki,
		Color.DarkSlateBlue, Color.LightGray, Color.DarkOrange, Color.DarkMagenta, Color.DeepSkyBlue, Color.LightPink, Color.LightSkyBlue, Color.Honeydew,
		Color.MediumSpringGreen, Color.GreenYellow, Color.LightGray, Color.Tan, Color.SlateGray, Color.LightCoral, Color.CadetBlue, Color.DarkGoldenrod,
		Color.Peru, Color.LightSalmon, Color.Brown, Color.AliceBlue, Color.MediumSeaGreen, Color.DarkSeaGreen, Color.LightGray, Color.DodgerBlue,
		Color.Red, Color.Pink, Color.Aqua, Color.DarkOliveGreen, Color.Crimson, Color.DeepPink, Color.YellowGreen, Color.Blue,
		Color.NavajoWhite, Color.DarkViolet, Color.Plum, Color.DarkSalmon, Color.BlueViolet, Color.Linen, Color.Moccasin, Color.BlueViolet,
		Color.MintCream, Color.Honeydew, Color.LightBlue, Color.DarkKhaki, Color.MidnightBlue, Color.PaleTurquoise, Color.DeepSkyBlue, Color.LavenderBlush,
		Color.DodgerBlue, Color.Fuchsia, Color.PaleVioletRed, Color.Bisque, Color.Fuchsia, Color.MediumSeaGreen, Color.PaleVioletRed, Color.LightBlue,
		Color.LightGreen, Color.DimGray, Color.DarkMagenta, Color.Olive, Color.Blue, Color.SeaShell, Color.Indigo, Color.Green,
		Color.Salmon, Color.MediumVioletRed, Color.Linen, Color.Khaki, Color.Ivory, Color.Bisque, Color.Goldenrod, Color.PaleGoldenrod,
		Color.MediumPurple, Color.LightSeaGreen, Color.CornflowerBlue, Color.LightSeaGreen, Color.MediumVioletRed, Color.Teal, Color.PaleGoldenrod, Color.WhiteSmoke,
		Color.MediumPurple, Color.Black, Color.DarkOrange, Color.LightGoldenrodYellow, Color.Fuchsia, Color.LightGray, Color.Blue, Color.DarkMagenta,
		Color.PowderBlue, Color.MediumTurquoise, Color.Maroon, Color.PowderBlue, Color.Teal, Color.DarkOrange, Color.OliveDrab, Color.Gold,
		Color.LightCyan, Color.Brown, Color.PowderBlue, Color.AliceBlue, Color.Turquoise, Color.WhiteSmoke, Color.Gray, Color.Moccasin,
		Color.Plum, Color.CornflowerBlue, Color.MediumPurple, Color.Maroon, Color.DarkRed, Color.Lime, Color.Tan, Color.Magenta,
		Color.PaleTurquoise, Color.MediumOrchid, Color.PaleGreen, Color.LightYellow, Color.DarkSalmon, Color.Goldenrod, Color.PeachPuff, Color.Olive,
		Color.ForestGreen, Color.LimeGreen, Color.PaleGreen, Color.AliceBlue, Color.LimeGreen, Color.LightSlateGray, Color.Honeydew, Color.Plum,
		Color.MediumBlue, Color.SeaGreen, Color.Red, Color.Red, Color.HotPink, Color.Teal, Color.Sienna, Color.FloralWhite,
		Color.DarkBlue, Color.LightSkyBlue, Color.Thistle, Color.MediumSeaGreen, Color.DarkGoldenrod, Color.LavenderBlush, Color.Magenta, Color.Ivory,
		Color.Gray, Color.DarkViolet, Color.Bisque, Color.Brown, Color.LemonChiffon, Color.Sienna, Color.Cornsilk, Color.SaddleBrown,
		Color.PapayaWhip, Color.Goldenrod, Color.OliveDrab, Color.Snow, Color.PaleGoldenrod, Color.LemonChiffon, Color.DarkViolet, Color.AntiqueWhite,
		Color.DarkMagenta, Color.Peru, Color.Indigo, Color.Fuchsia, Color.Tomato, Color.SandyBrown, Color.BurlyWood, Color.LightBlue,
		Color.DarkRed, Color.SaddleBrown, Color.DarkSlateBlue, Color.SandyBrown, Color.Wheat, Color.MediumAquamarine, Color.Red, Color.Tan,
		Color.Pink, Color.RosyBrown, Color.DimGray, Color.Sienna, Color.RoyalBlue, Color.LightGoldenrodYellow, Color.LightBlue, Color.LightSalmon,
		Color.Turquoise, Color.SlateBlue, Color.LawnGreen, Color.LightSalmon, Color.Purple, Color.Crimson, Color.LightSlateGray, Color.LavenderBlush,
		Color.LightPink, Color.MediumSeaGreen, Color.MediumAquamarine, Color.Yellow, Color.Cyan, Color.DarkSlateGray, Color.DeepPink, Color.Lavender,
		Color.OliveDrab, Color.DeepSkyBlue, Color.HotPink, Color.Coral, Color.PapayaWhip, Color.PaleTurquoise, Color.MediumPurple, Color.Yellow,
		Color.SeaGreen, Color.PapayaWhip, Color.Chocolate, Color.GreenYellow, Color.PeachPuff, Color.Orange, Color.Beige, Color.Olive,
		Color.Silver, Color.AntiqueWhite, Color.MidnightBlue, Color.Lime, Color.Orange, Color.PeachPuff, Color.WhiteSmoke, Color.YellowGreen,
		Color.Indigo, Color.SlateBlue, Color.Indigo, Color.LightPink, Color.PowderBlue, Color.SlateBlue, Color.Black, Color.Orange,
		Color.MediumSlateBlue, Color.PaleGreen, Color.Firebrick, Color.SeaGreen, Color.DarkSalmon, Color.White, Color.Bisque, Color.OrangeRed,
		Color.Bisque, Color.DarkBlue, Color.Pink, Color.DarkSlateBlue, Color.SandyBrown, Color.Gainsboro, Color.BurlyWood, Color.LightGoldenrodYellow,
		Color.Olive, Color.MediumOrchid, Color.HotPink, Color.SaddleBrown, Color.MediumOrchid, Color.MediumSlateBlue, Color.Olive, Color.LightSeaGreen,
		Color.PeachPuff, Color.LightCoral, Color.Cyan, Color.SeaShell, Color.Azure, Color.Yellow, Color.CadetBlue, Color.MediumVioletRed,
		Color.Ivory, Color.CornflowerBlue, Color.BlueViolet, Color.Black, Color.LightBlue, Color.Green, Color.Beige, Color.DeepSkyBlue,
		Color.LawnGreen, Color.DarkSalmon, Color.Sienna, Color.DarkOrange, Color.IndianRed, Color.SeaGreen, Color.LightCyan, Color.NavajoWhite,
		Color.Firebrick, Color.DarkOrange, Color.DodgerBlue, Color.Magenta, Color.DarkMagenta, Color.Bisque, Color.Magenta, Color.OldLace,
		Color.Orchid, Color.Gray, Color.Crimson, Color.LemonChiffon, Color.DarkKhaki, Color.LimeGreen, Color.DarkOrchid, Color.BlueViolet,
		Color.MediumTurquoise, Color.White, Color.Pink, Color.NavajoWhite, Color.CornflowerBlue, Color.Tan, Color.MediumSpringGreen, Color.Aquamarine,
		Color.PeachPuff, Color.SlateGray, Color.DarkMagenta, Color.Plum, Color.MistyRose, Color.Salmon, Color.DeepSkyBlue, Color.PowderBlue,
		Color.Fuchsia, Color.Turquoise, Color.LightPink, Color.WhiteSmoke, Color.SkyBlue, Color.SeaGreen, Color.PowderBlue, Color.LightGreen,
		Color.MediumBlue, Color.LemonChiffon, Color.DarkCyan, Color.LightSeaGreen, Color.RoyalBlue, Color.LemonChiffon, Color.SandyBrown, Color.Olive,
		Color.MediumSlateBlue, Color.Black, Color.Snow, Color.MediumOrchid, Color.Gainsboro, Color.SlateBlue, Color.HotPink, Color.MidnightBlue,
		Color.LightGoldenrodYellow, Color.MediumSpringGreen, Color.LightSeaGreen, Color.LightSalmon, Color.Aqua, Color.Tomato, Color.Gold, Color.DarkBlue,
		Color.LawnGreen, Color.PaleTurquoise, Color.SlateGray, Color.LawnGreen, Color.Sienna, Color.SlateGray, Color.RosyBrown, Color.Azure,
		Color.DarkGoldenrod, Color.LightYellow, Color.Indigo, Color.PaleGreen, Color.LemonChiffon, Color.Linen, Color.NavajoWhite, Color.Sienna,
		Color.Wheat, Color.MediumTurquoise, Color.Peru, Color.MediumPurple, Color.MidnightBlue, Color.SaddleBrown, Color.LightPink, Color.Linen,
		Color.LightCoral, Color.Lavender, Color.DarkOrchid, Color.Goldenrod, Color.Maroon, Color.DarkOrchid, Color.WhiteSmoke, Color.GhostWhite,
		Color.Goldenrod, Color.IndianRed, Color.SkyBlue, Color.LightSalmon, Color.Violet, Color.LightSkyBlue, Color.DarkGreen, Color.Goldenrod,
		Color.LightYellow, Color.Khaki, Color.MistyRose, Color.Fuchsia, Color.LimeGreen, Color.Aquamarine, Color.White, Color.DarkSalmon,
		Color.LightSalmon, Color.Brown, Color.CadetBlue, Color.LightSteelBlue, Color.DarkSalmon, Color.SlateGray, Color.DarkKhaki, Color.Aquamarine,
		Color.LightSkyBlue, Color.SeaShell, Color.Silver, Color.Purple, Color.MediumSpringGreen, Color.GhostWhite, Color.CadetBlue, Color.LightCoral,
		Color.Silver, Color.DarkOrange, Color.Coral, Color.PaleGoldenrod, Color.SlateGray, Color.Cornsilk, Color.CadetBlue, Color.DeepPink,
		Color.DeepPink, Color.LimeGreen, Color.DarkBlue, Color.LightBlue, Color.DarkOrchid, Color.PapayaWhip, Color.Ivory, Color.Turquoise,
		Color.Olive, Color.RoyalBlue, Color.Red, Color.Lime, Color.Pink, Color.Linen, Color.Chocolate, Color.DarkGreen,
		Color.Honeydew, Color.HotPink, Color.LightGray, Color.Cyan, Color.DimGray, Color.LawnGreen, Color.ForestGreen, Color.Crimson,
		Color.DimGray, Color.AliceBlue, Color.PaleTurquoise, Color.RoyalBlue, Color.SeaShell, Color.LightSteelBlue, Color.BlanchedAlmond, Color.Lavender,
		Color.LightSlateGray, Color.LightSkyBlue, Color.SaddleBrown, Color.Cornsilk, Color.Lavender, Color.LightGray, Color.Gray, Color.Peru,
		Color.DarkOrange, Color.YellowGreen, Color.Ivory, Color.DeepPink, Color.LightGreen, Color.DarkTurquoise, Color.Cornsilk, Color.Chocolate,
		Color.Firebrick, Color.Linen, Color.LightSlateGray, Color.Purple, Color.DarkRed, Color.SeaGreen, Color.LimeGreen, Color.PaleGreen,
		Color.LightPink, Color.SteelBlue, Color.Yellow, Color.Snow, Color.Purple, Color.Cyan, Color.BlueViolet, Color.Plum,
		Color.MediumBlue, Color.LightCoral, Color.Bisque, Color.DarkGoldenrod, Color.Khaki, Color.Beige, Color.LightGreen, Color.BlueViolet,
		Color.ForestGreen, Color.SeaShell, Color.Tomato, Color.Red, Color.LightSeaGreen, Color.Gainsboro, Color.PowderBlue, Color.SlateGray,
		Color.DarkGoldenrod, Color.SeaShell, Color.MediumVioletRed, Color.PaleTurquoise, Color.MediumSeaGreen, Color.MediumBlue, Color.DarkKhaki, Color.DodgerBlue,
		Color.MediumPurple, Color.PaleVioletRed, Color.Snow, Color.LightSlateGray, Color.PaleGoldenrod, Color.Blue, Color.SaddleBrown, Color.DarkCyan,
		Color.Maroon, Color.LightCoral, Color.MediumBlue, Color.DarkOrange, Color.PaleGreen, Color.Thistle, Color.AliceBlue, Color.Yellow,
		Color.SandyBrown, Color.PaleVioletRed, Color.CadetBlue, Color.Gold, Color.Tomato, Color.MistyRose, Color.SpringGreen, Color.Red,
		Color.CadetBlue, Color.GreenYellow, Color.LightGreen, Color.DarkSeaGreen, Color.LightSlateGray, Color.Purple, Color.SlateBlue, Color.DarkRed,
		Color.Black, Color.LawnGreen, Color.DimGray, Color.Magenta, Color.Aquamarine, Color.PaleTurquoise, Color.DarkGoldenrod, Color.Ivory,
		Color.DarkOrange, Color.SandyBrown, Color.DarkOrchid, Color.DeepSkyBlue, Color.SpringGreen, Color.LightSteelBlue, Color.Teal, Color.LightPink,
		Color.PapayaWhip, Color.SkyBlue, Color.SeaShell, Color.LemonChiffon, Color.SaddleBrown, Color.DarkViolet, Color.OliveDrab, Color.Gold,
		Color.Teal, Color.DarkSlateBlue, Color.CornflowerBlue, Color.LightSalmon, Color.PapayaWhip, Color.MediumSpringGreen, Color.Tan, Color.LightGreen,
		Color.AliceBlue, Color.LightSkyBlue, Color.Lime, Color.SlateGray, Color.DeepPink, Color.RosyBrown, Color.WhiteSmoke, Color.SkyBlue,
		Color.MediumTurquoise, Color.MediumSeaGreen, Color.YellowGreen, Color.OldLace, Color.SaddleBrown, Color.MediumBlue, Color.Gold, Color.LawnGreen,
		Color.OliveDrab, Color.Lavender, Color.PaleVioletRed, Color.Brown, Color.MediumAquamarine, Color.Sienna, Color.Olive, Color.OliveDrab,
		Color.DeepPink, Color.Snow, Color.Blue, Color.DarkSalmon, Color.SteelBlue, Color.MediumPurple, Color.DarkGreen, Color.Purple,
		Color.DarkGreen, Color.Violet, Color.SaddleBrown, Color.Aquamarine, Color.DarkTurquoise, Color.PaleTurquoise, Color.DeepSkyBlue, Color.Crimson,
		Color.SeaShell, Color.LightGray, Color.MediumSlateBlue, Color.Orchid, Color.Cyan, Color.MediumPurple, Color.White, Color.Olive,
		Color.MediumAquamarine, Color.SlateBlue, Color.Yellow, Color.Blue, Color.CornflowerBlue, Color.LavenderBlush, Color.LightSlateGray, Color.Khaki,
		Color.DarkGoldenrod, Color.PowderBlue, Color.DarkGreen, Color.DarkGreen, Color.Red, Color.Firebrick, Color.Lime, Color.Black,
		Color.Brown, Color.MediumOrchid, Color.Chartreuse, Color.LightYellow, Color.MintCream, Color.Navy, Color.Plum, Color.DarkViolet,
		Color.LemonChiffon, Color.Wheat, Color.PaleVioletRed, Color.Gainsboro, Color.FloralWhite, Color.ForestGreen, Color.Cyan, Color.Olive,
		Color.LawnGreen, Color.SpringGreen, Color.DarkTurquoise, Color.Lavender, Color.SandyBrown, Color.LightCyan, Color.Salmon, Color.LightCyan,
		Color.Aqua, Color.SeaGreen, Color.LightSlateGray, Color.YellowGreen, Color.DimGray, Color.Cornsilk, Color.Red, Color.MediumTurquoise,
		Color.Lime, Color.Peru, Color.LightCoral, Color.SaddleBrown, Color.LimeGreen, Color.Gray, Color.OliveDrab, Color.SkyBlue,
		Color.DarkMagenta, Color.LavenderBlush, Color.Olive, Color.MintCream, Color.LightYellow, Color.LightYellow, Color.Beige, Color.Orchid,
		Color.LightSalmon, Color.Aqua, Color.BurlyWood, Color.SpringGreen, Color.LightSlateGray, Color.Khaki, Color.MidnightBlue, Color.Indigo,
		Color.Gold, Color.LightCoral, Color.DarkSlateGray, Color.DarkGray, Color.BlueViolet, Color.Magenta, Color.Purple, Color.BlanchedAlmond,
		Color.Moccasin, Color.DarkSeaGreen, Color.DarkGoldenrod, Color.LightYellow, Color.MediumOrchid, Color.PapayaWhip, Color.ForestGreen, Color.Green,
		Color.DarkSeaGreen, Color.Yellow, Color.Moccasin, Color.Wheat, Color.SteelBlue, Color.LightPink, Color.Firebrick, Color.LightCyan,
		Color.SkyBlue, Color.LightYellow, Color.MediumSlateBlue, Color.DarkOrange, Color.SeaGreen, Color.Crimson, Color.DarkTurquoise, Color.Ivory,
	};


}
