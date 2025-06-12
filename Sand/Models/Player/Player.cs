using AsepriteDotNet;
using FlatRedBall;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sprite = FlatRedBall.Sprite;
using static Sand.Constants;
using FlatRedBall.Graphics.Animation;
using AnimationFrame = FlatRedBall.Graphics.Animation.AnimationFrame;
using Point = System.Drawing.Point;
using FlatRedBall.Input;
using System.Globalization;
using FlatRedBall.Math.Geometry;
using Sand.Extensions;

namespace Sand;

public class Player
{
	public Texture2D Texture { get; private set; }
	private AnimationChain _acIdleRight01;
	private AnimationChain _acIdleLeft01;
	public Sprite Sprite { get; private set; }
	public int StuffHeight => height / Constants.STUFF_SCALE;
	public int StuffWidth => width / Constants.STUFF_SCALE;

	private const int width = 100;
	private const int height = 100;
	private bool lookingRight = true;
	public bool Falling { get; set; }

	private const string AnimChainName_Float = "Float";
	private const string AnimChainName_Run = "Run";
	private const string AnimChainName_Stop = "Stop";
	private const string AnimChainName_Idle = "Idle";
	private const string AnimChainName_Fall = "Fall";
	private const string AnimChainName_Die = "Die";
	private const string AnimChainName_Damage = "Damage";
	private const string AnimChainName_Dash = "Dash";

	private const float FRACTION_OF_SPRITE_VISIBLE = 0.5f;

	//private Circle _hitbox;
	public AxisAlignedRectangle Hitbox { get; private set; }


	public Player()
	{
		//========================================
		// Create VISUAL
		//========================================

		//CreatePlayerTexture001_WeeSquareBoyo();
		//CreatePlayerTexture002_SpriteyMcSpriteSprite();
		CreatePlayerTexture003_SpritesLikeANormalPerson();

		//========================================
		// Create PHYSICAL
		//========================================

		CreatePlayerHitxbox();

		//========================================
		// FUNCTIONS
		//========================================

		void CreatePlayerHitxbox()
		{
			Hitbox = new AxisAlignedRectangle
			{
				Position = Sprite.Position,
				Height = Sprite.Height * FRACTION_OF_SPRITE_VISIBLE,
				Width = Sprite.Width * FRACTION_OF_SPRITE_VISIBLE,
				Color = Color.Red
			};

			ShapeManager.AddAxisAlignedRectangle(Hitbox); // <--- Makes circle visible

			Hitbox.Visible = SHOW_PLAYER;
		}

		/// source: https://lucky-loops.itch.io/character-satyr
		void CreatePlayerTexture003_SpritesLikeANormalPerson()
		{

			var tSpriteSheet = FlatRedBallServices.Load<Texture2D>("C:\\Dev\\FlatRedBall\\Sand\\Sand\\Content\\TextureImages\\satiro-Sheet v1.1.png");
			var timeIdleDefault = 0.1f;

			Sprite = SpriteManager.AddSprite(new AnimationChainList()
			{
				GetAnimationChain(0, 6, AnimChainName_Idle, 1),
				GetAnimationChain(1, 8, AnimChainName_Run),
				GetAnimationChain(2, 4, AnimChainName_Stop),
				GetAnimationChain(3, 6, AnimChainName_Float),
				GetAnimationChain(4, 6, AnimChainName_Fall),
				GetAnimationChain(5, 10, AnimChainName_Die),
				GetAnimationChain(6, 4, AnimChainName_Damage),
				GetAnimationChain(7, 6, AnimChainName_Dash),
			});
			Sprite.X = RESOLUTION_X / 2;
			Sprite.Y = RESOLUTION_Y / 2;
			Sprite.Z = Z_IND_PLAYER;
			Sprite.TextureScale = 10;
			Sprite.CurrentChainName = AnimChainName_Idle;

			Sprite.Visible = SHOW_PLAYER;

			/*			var circle = new Circle();
						circle.Position = Sprite.Position;*/


			AnimationChain GetAnimationChain(int row, int count, string name, int yOffset = 0)
			{

				var chain = new AnimationChain();
				for (var i = 0; i < count; i++)
				{
					const int wide = 32;
					const int high = 32;
					var frameDataRight = new Color[wide * high];
					tSpriteSheet.GetData<Color>(0, new Rectangle(wide * i, (row * high) + yOffset, wide, high), frameDataRight, 0, wide * high);
					var tRight = new Texture2D(FlatRedBallServices.GraphicsDevice, wide, high);
					tRight.SetData(frameDataRight);
					var textureRight = tRight;
					var frameDataLeft = new Color[frameDataRight.Length];
					chain.Add(new AnimationFrame(textureRight, timeIdleDefault));
				}
				chain.Name = name;
				return chain;
			}
		}
		#region
		/*/// source: https://lucky-loops.itch.io/character-satyr
		void CreatePlayerTexture002_SpriteyMcSpriteSprite() 
		{
			
			var tSpriteSheet = FlatRedBallServices.Load<Texture2D>("C:\\Dev\\FlatRedBall\\Sand\\Sand\\Content\\TextureImages\\satiro-Sheet v1.1.png");

			GetFrame(5, 8, 18, 19, out var dIdle01Right_01*//*, out var dIdle01Left_01*//*);
			GetFrame(37, 7, 18, 20, out var dIdle01Right_02*//*, out var dIdle01Left_02*//*);
			GetFrame(69, 7, 18, 20, out var dIdle01Right_03*//*, out var dIdle01Left_03*//*);
			GetFrame(101, 5, 18, 22, out var dIdle01Right_04*//*, out var dIdle01Left_04*//*);
			GetFrame(133, 6, 18, 21, out var dIdle01Right_05*//*, out var dIdle01Left_05*//*);
			GetFrame(165, 6, 18, 20, out var dIdle01Right_06*//*, out var dIdle01Left_06*//*);
			var timeIdleDefault = 0.2f;

			_acIdleRight01 =
			[
				new (dIdle01Right_01, timeIdleDefault),
				new (dIdle01Right_02, timeIdleDefault),
				new (dIdle01Right_03, timeIdleDefault),
				new (dIdle01Right_04, timeIdleDefault),
				new (dIdle01Right_05, timeIdleDefault),
				new (dIdle01Right_06, timeIdleDefault)
			];

			//_acIdleLeft01 =
			//[
			//	new (dIdle01Left_01, timeIdleDefault),
			//	new (dIdle01Left_02, timeIdleDefault),
			//	new (dIdle01Left_03, timeIdleDefault),
			//	new (dIdle01Left_04, timeIdleDefault),
			//	new (dIdle01Left_05, timeIdleDefault),
			//	new (dIdle01Left_06, timeIdleDefault)
			//];

			Sprite = SpriteManager.AddSprite(_acIdleRight01);
			Sprite.X = RESOLUTION_X / 2;
			Sprite.Y = RESOLUTION_Y / 2;
			Sprite.Z = 1;
			Sprite.TextureScale = 10;

			void GetFrame(int x, int y, int w, int h, out Texture2D textureRight*//*, out Texture2D textureLeft*//*)
			{
				var frameDataRight = new Color[w * h];
				tSpriteSheet.GetData<Color>(0, new Rectangle(x, y, w, h), frameDataRight, 0, w * h);
				var tRight = new Texture2D(FlatRedBallServices.GraphicsDevice, w, h);
				tRight.SetData(frameDataRight);
				textureRight = tRight;

				var frameDataLeft = new Color[frameDataRight.Length];

				//var il = 0;
				//var iOuterl = 0;
				//var iInnerl = 0;

				//try
				//{

					
				//	var i = 0;
				//	// outer loop incrementing by w
				//	for (var iOuter = w - 1; iOuter > 0 && iOuter < frameDataLeft.Length; iOuter+=w)
				//	{
				//		iOuterl = iOuter;
				//		il = i;
				//		// inner loop decrementing back from w to 0
				//		for (var iInner = iOuter; iInner >= 0 && iInner < frameDataLeft.Length && i < frameDataLeft.Length; iInner--, i++, il++)
				//		{
				//			iInnerl = iInner;
				//			frameDataLeft[i] = frameDataRight[iInner];
				//		}

				//		//// every w pixels, we ned to w*2 pixels ahead and count back w time
				//		//int iRight = 0;
				//		//{
				//		//	iRight = iOuter + iOuter;
				//		//}
				//		var abcdefg = "";
				//		//frameDataLeft[iOuter] = frameDataRight[iRight];
				//	}

				//}
				//catch (Exception ex)
				//{

				//	throw;
				//}

				//for (var iLeft = w; iLeft > 0 && iLeft < frameDataLeft.Length; iLeft++)
				//{
				//	// every w pixels, we ned to w*2 pixels ahead and count back w time
				//	int iRight = 0;
				//	{
				//		iRight = iLeft + iLeft;
				//	}

				//	frameDataLeft[iLeft] = frameDataRight[iRight];
				//}

				//var tLeft = new Texture2D(FlatRedBallServices.GraphicsDevice, w, h);
				//tLeft.SetData(frameDataLeft);
				//textureLeft = tLeft;
				//return tRight;
			}
			
		}

		void CreatePlayerTexture001_WeeSquareBoyo() 
		{
			Texture = new Texture2D(FlatRedBallServices.GraphicsDevice, width, height);
			var a = new Color[width * height];
			Array.Fill(a, Color.OliveDrab);
			Texture.SetData(a);
			Sprite = SpriteManager.AddSprite(Texture);
			Sprite.X = RESOLUTION_X / 2;
			Sprite.Y = RESOLUTION_Y / 2;
			Sprite.Z = 1;
			Sprite.TextureScale = 1;
		}*/
		#endregion

	}

	public void Turn()
	{

		try
		{
			var pressingA = InputManager.Keyboard.KeyDown(Microsoft.Xna.Framework.Input.Keys.A);
			var pressingD = InputManager.Keyboard.KeyDown(Microsoft.Xna.Framework.Input.Keys.D);

			if (pressingA)
			{
				lookingRight = false;
				Sprite.CurrentChainName = AnimChainName_Float;
			}

			if (pressingD)
			{
				lookingRight = true;
				Sprite.CurrentChainName = AnimChainName_Float;
			}

			if (!pressingA && !pressingD)
			{
				if (Falling)
				{
					Sprite.CurrentChainName = AnimChainName_Fall;
				}
				else
				{
					Sprite.CurrentChainName = AnimChainName_Idle;
				}
			}


			if (lookingRight)
			{
				Sprite.ScaleX = Math.Abs(Sprite.ScaleX);
				lookingRight = true;
			}
			else
			{
				Sprite.ScaleX = -Math.Abs(Sprite.ScaleX);
				lookingRight = false;
			}
		}
		catch (Exception ex)
		{

			throw;
		}

	}

	public Point GetPositionWindow()
	{
		throw new NotImplementedException();
	}

	public (int top, int right, int bottom, int left) GetPositionStuff()
	{
		/*		return NumberExtensions.ToStuffCoord(Sprite);
		 *		
		*/
		return GetHitboxBounds();
}

	public (int top, int right, int bottom, int left) GetHitboxBounds()
	{
		return NumberExtensions.ToStuffCoord(Hitbox);
	}

	//public float x => Sprite.X;
	#region Dimensions
	private const float Y_OFFSET = 21;
	public float X
	{
		get => Hitbox.X;
		set
		{
			/*Sprite.X = value;
			Hitbox.Position = Sprite.Position;*/
			Hitbox.X = value;

			var spritePos = Hitbox.Position;
			spritePos.Y += Y_OFFSET;
			Sprite.Position = spritePos;
		}
	}
	public float Y
	{
		get => Hitbox.Y;
		set
		{
			Hitbox.Y = value;

			var spritePos = Hitbox.Position;
			spritePos.Y += Y_OFFSET;
			Sprite.Position = spritePos;
		}
	}
	//public float Y { get => Sprite.Y; set { Sprite.Y = value; _hitbox.Position = Sprite.Position; } }

	///*CIRCLE*//*public float Top { get => Sprite.Top;		set { Sprite.Top = value;		_hitbox.Position = Sprite.Position; } }*/
	///*AXIS RECT*/public float Top { get => _hitbox.Position.Y + _hitbox.Height / 2; set { Sprite.Top = value; _hitbox.Position = Sprite.Position; } }

	///*CIRCLE*//*public float Right { get => Sprite.Right;	set { Sprite.Right = value;		_hitbox.Position = Sprite.Position; } }*/
	///*AXIS RECT*/public float Right { get => Sprite.Position.X + Sprite.Width / 2; set { Sprite.Right = value; _hitbox.Position = Sprite.Position; } }

	///*CIRCLE*//*public float Bottom	{ get => Sprite.Bottom;	set { Sprite.Bottom = value;	_hitbox.Position = Sprite.Position; } }*/
	///*AXIS RECT*/public float Bottom { get => _hitbox.Position.Y - _hitbox.Height / 2; set { Sprite.Bottom = value; _hitbox.Position = Sprite.Position; } }

	///*CIRCLE*//*public float Left { get => Sprite.Left;		set { Sprite.Left = value;		_hitbox.Position = Sprite.Position; } }*/
	///*AXIS RECT*/public float Left { get => Sprite.Position.X - _hitbox.Width / 2; set { Sprite.Left = value; _hitbox.Position = Sprite.Position; } }



	public float Width
	{
		get => Sprite.Width * FRACTION_OF_SPRITE_VISIBLE;
		set
		{
			Sprite.Width = value;
			Hitbox.Position = Sprite.Position;
		}
	}
	public float Height
	{
		get => Sprite.Height * FRACTION_OF_SPRITE_VISIBLE;
		set
		{
			Sprite.Height = value; 
			Hitbox.Position = Sprite.Position;
		}
	}

	public void PrintPosition()
	{
		var (top, right, bottom, left) = this.GetPositionStuff();

		var abs = Math.Abs(left);
		var adj = (abs < 10 ? "  " : (abs < 100 ? " " : (abs < 1000 ? "" : "")));
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
		   $"\n --- [{top}] --- \n" +
			"|            |\n" +
			"|            |\n" +
		   $"[{left}]\n" +
		   $"|           [{right}]\n" +
			"|            |\n" +
			"|            |\n" +
		   $" --- [{bottom}] ---\n\n " +
		   $"WIDTH=[{Width}]";
		Logger.Instance.LogInfo(xyz);
	}

	#endregion
}
