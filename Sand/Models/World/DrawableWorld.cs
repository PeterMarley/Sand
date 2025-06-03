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


public class DrawableWorld : IDrawableBatch
{

	//========================================
	// The World
	//========================================

	///// <summary>Outer array is X, inner array is Y.</summary>
	//public Stuff[][] World { get; private set; }
	/// <summary>Each inner list is a chunk's worth of coordinates.</summary>
	//public List<List<Point>> WorldChunks { get; set; }

	public MyDataStructure Structure { get; set; }

	private Texture2D			WorldTexture;
	private Sprite				WorldSprite;

	//========================================
	// The Player
	//========================================

	public Player Player { get; private set; }

	//========================================
	// IDrawableBatch
	//========================================

	public float X { get; set; }
	public float Y { get; set; }
	public float Z { get; init; } = 0;
	public bool UpdateEveryFrame { get; set; } = true;

	public DrawableWorld(WorldSetup worldSetup)
	{
		PrepareWorld(worldSetup);
		PreparePlayer();

		void PrepareWorld(WorldSetup worldSetup) 
		{
			Structure = new MyDataStructure(worldSetup);
		}
		void PreparePlayer() 
		{
			Player = new Player();

		}
	}


	#region Public API

	#region Adding Stuff

	//public void SafeAddStuffIfEmpty_InSquare(string stuffType, int x, int y, int length)
	//{
	//	for (int i = x - length; i < x + length; i++)
	//	{
	//		for (int j = y - length; j < y + length; j++)
	//		{
	//			SafeAddStuffIfEmpty(stuffType, i, j);
	//		}
	//	}
	//}

	//public void ForceAddStuff_InSquare(string stuffType, int x, int y, int length)
	//{
	//	for (int i = x - length; i < x + length; i++)
	//	{
	//		for (int j = y - length; j < y + length; j++)
	//		{
	//			ForceAddStuff(stuffType, i, j);
	//		}
	//	}
	//}

	//public void ForceAddStuff(string stuffType, int x, int y)
	//{
	//	if (x >= 0 && x < World.Length && y >= 0 && y < World[0].Length)
	//	{
	//		var stuff = StuffFactory.Instance.Get(stuffType);
	//		World[x][y] = stuff;//.SetPosition(x, y);
	//							//stuff.X = x;
	//							//stuff.Y = y;
	//	}
	//}

	//public void SafeAddStuffIfEmpty(string stuffType, int x, int y)
	//{
	//	if (x >= 0 && x < World.Length && y >= 0 && y < World[0].Length && World[x][y] == null)
	//	{
	//		var stuff = StuffFactory.Instance.Get(stuffType);
	//		World[x][y] = stuff;//.SetPosition(x, y);
	//							//stuff.X = x;
	//							//stuff.Y = y;
	//	}
	//}

	#endregion

	#region Moving Stuff

	public void ApplyGravity(int xIndex, int yIndex)
	{
		var stuff = Structure.Get(xIndex, yIndex);

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
			if (stuff.CheckDormancy() && Structure.Get(xIndex, rowBelowIndex) != null) // this is not null check basically enables erroneously dormant stuff to right itself
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
		return Structure.Move(from, to);
	}

	#endregion

	#region Game Loop Methods called by SandGame

	public void Update()
	{
		var p = new Point();

		try
		{
			//=======================================================
			// New version of world update loop (utilising chunks)
			//=======================================================

			// choose which chunks to update
			var chunksToUpdate = Structure.GetChunksToUpdate();

			foreach (var chunk in chunksToUpdate)
			{
				foreach (var point in chunk.Points_BottomLeftToTopRight_AlternatingRowDirection)
				{
					var xIndex = point.X;
					var yIndex = point.Y;

					// make point viewable to scope that encloses the try catch for error logging
					p = point;

					// if something here then apply gravity **NOTE** dormancy is checked in apply gravity
					if (Structure.Get(point) != null)
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

	public void Draw(Camera camera)
	{

		try
		{
			// interate through the World and get the color data of all Stuffs there
			var colorData = Structure.GetColorData();

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
		catch (Exception drawWorldEx)
		{
			Logger.Instance.LogError(drawWorldEx, $"Failed to process and apply the colour of each {nameof(Stuff)} to the {nameof(WorldTexture)}");
			throw;
		}
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
		Player.Turn();

		void AffectWorld()
		{
			if (InputManager.Mouse.IsInGameWindow())
			{
				var x = InputManager.Mouse.X / STUFF_SCALE;
				var y = (FlatRedBallServices.GraphicsDevice.Viewport.Height - InputManager.Mouse.Y) / STUFF_SCALE;

				if ((InputManager.Mouse.ButtonPushed(MouseButtons.LeftButton) || InputManager.Mouse.ButtonDown(MouseButtons.LeftButton)))
				{
					Structure.SafeAddStuffIfEmpty_InSquare(Stuffs.BASIC_WATER, x, y, 15);
					//_world.SafeAddStuffIfEmpty(Stuffs.BASIC_WATER, x, y);

				}

				if ((InputManager.Mouse.ButtonPushed(MouseButtons.RightButton) || InputManager.Mouse.ButtonDown(MouseButtons.RightButton)))
				{
					Structure.SafeAddStuffIfEmpty_InSquare(Stuffs.BASIC_SAND, x, y, 10);
					//_world.SafeAddStuffIfEmpty(Stuffs.BASIC_SAND, x, y);
				}

				if ((InputManager.Mouse.ButtonPushed(MouseButtons.MiddleButton) || InputManager.Mouse.ButtonDown(MouseButtons.MiddleButton)))
				{
					Structure.ForceAddStuff_InSquare(Stuffs.BASIC_STONE, x, y, 10);
					//_world.SafeAddStuffIfEmpty(Stuffs.BASIC_SAND, x, y);
				}

				if ((InputManager.Mouse.ButtonPushed(MouseButtons.XButton1) || InputManager.Mouse.ButtonDown(MouseButtons.XButton1)))
				{
					Structure.ForceAddStuff_InSquare(Stuffs.BASIC_LAVA2, x, y, 2);
					//_world.SafeAddStuffIfEmpty(Stuffs.BASIC_SAND, x, y);
				}

				// place stuff at bottom left of player
				if (InputManager.Keyboard.KeyDown(Keys.E))
				{
					var (top, right, bottom, left) = Player.GetPositionStuff(); /*NumberExtensions.ToStuffCoord(Player.Sprite);*/
					Structure.ForceAddStuff_InSquare(Stuffs.BASIC_STONE, left, bottom, 2);
				}
			}
		}

		void AffectPlayer()
		{

			var collision = Structure.Collision(Player, GRAV_MAGNITUDE, PLAYER_MOVE_FACTOR);

			var allowUp = collision.AllowUp;
			var allowRight = collision.AllowRight;
			var allowDown = collision.AllowDown;
			var allowLeft = collision.AllowLeft;
			var moveFactor = collision.MoveFactor;

			//move ↑
			if (allowUp && InputManager.Keyboard.KeyDown(Keys.W))
			{
				Player.Y += (moveFactor * 2);
			}

			// move ←
			if (allowLeft && InputManager.Keyboard.KeyDown(Keys.A))
			{
				Player.X -= moveFactor;
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
			}

			if (InputManager.Keyboard.KeyPushed(Keys.Q))
			{
				Player.PrintPosition();
			}
		}
	}

	#endregion

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
