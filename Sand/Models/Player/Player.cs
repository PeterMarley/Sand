using FlatRedBall;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Sprite = FlatRedBall.Sprite;
using static Sand.Constants;
using FlatRedBall.Graphics.Animation;
using AnimationFrame = FlatRedBall.Graphics.Animation.AnimationFrame;
using Point = System.Drawing.Point;
using static Sand.Constants;
using FlatRedBall.Input;
using FlatRedBall.Math.Geometry;
using Sand.Extensions;

namespace Sand;

public class Player
{

	//===================================================
	// Fields/ Properties
	//===================================================
	private Sprite Sprite { get; set; }
	private bool LookingRight { get; set; } = true;
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

	public AxisAlignedRectangle Hitbox { get; private set; }


	//===================================================
	// CTOR
	//===================================================
	public Player()
	{
		CreateSprite();
		CreateHitxbox();

		void CreateHitxbox()
		{
			Hitbox = new AxisAlignedRectangle
			{
				Position = Sprite.Position,
				Height = Sprite.Height * FRACTION_OF_SPRITE_VISIBLE,
				Width = Sprite.Width * FRACTION_OF_SPRITE_VISIBLE,
				Color = Color.Red
			};
			ShapeManager.AddAxisAlignedRectangle(Hitbox); // <--- Makes shape visible
			Hitbox.Visible = SHOW_PLAYER;
		}
		void CreateSprite()
		{
			/// source: https://lucky-loops.itch.io/character-satyr
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
			Sprite.TextureScale = 5;
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
	}


	public void TurnDirectionFacing()
	{

		try
		{
			var pressingA = InputManager.Keyboard.KeyDown(Microsoft.Xna.Framework.Input.Keys.A);
			var pressingD = InputManager.Keyboard.KeyDown(Microsoft.Xna.Framework.Input.Keys.D);

			if (pressingA)
			{
				LookingRight = false;
				Sprite.CurrentChainName = AnimChainName_Float;
			}

			if (pressingD)
			{
				LookingRight = true;
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

			if (LookingRight)
			{
				Sprite.ScaleX = Math.Abs(Sprite.ScaleX);
				LookingRight = true;
			}
			else
			{
				Sprite.ScaleX = -Math.Abs(Sprite.ScaleX);
				LookingRight = false;
			}
		}
		catch (Exception ex)
		{
			throw;
		}

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

	//public void PrintPosition()
	//{
	//	var (top, right, bottom, left) = this.GetPositionStuff();

	//	var abs = Math.Abs(left);
	//	var adj = (abs < 10 ? "  " : (abs < 100 ? " " : (abs < 1000 ? "" : "")));
	//	//var xyz =
	//	//   $"\n ----{spriteGridTop}---- \n" +
	//	//	"|            |\n" +
	//	//	"|            |\n" +
	//	//   $"{spriteGridLeft}         {adj}|     HEIGHT=[{Player.Sprite.Height}]\n" +
	//	//   $"|            {spriteGridRight}\n" +
	//	//	"|            |\n" +
	//	//	"|            |\n" +
	//	//   $" ----{spriteGridBottom}----\n\n " +
	//	//   $"WIDTH=[{Player.Sprite.Width}]";
	//	var xyz =
	//	   $"\n --- [{top}] --- \n" +
	//		"|            |\n" +
	//		"|            |\n" +
	//	   $"[{left}]\n" +
	//	   $"|           [{right}]\n" +
	//		"|            |\n" +
	//		"|            |\n" +
	//	   $" --- [{bottom}] ---\n\n " +
	//	   $"WIDTH=[{Width}]";
	//	Logger.Instance.LogInfo(xyz);
	//}

	#endregion
}
