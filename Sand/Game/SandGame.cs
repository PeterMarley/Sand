using System;

using FlatRedBall;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using FlatRedBall.Input;
using static FlatRedBall.Input.Mouse;
using static Sand.Constants;

namespace Sand
{
	public partial class SandGame : Microsoft.Xna.Framework.Game
	{
		GraphicsDeviceManager graphics;

		private StuffWorld _world;

		public SandGame() : base()
		{
			graphics = new GraphicsDeviceManager(this);
			#region preprocessor directive
#if ANDROID || IOS
			graphics.IsFullScreen = true;
#elif WINDOWS || DESKTOP_GL
			graphics.PreferredBackBufferWidth = 800;
			graphics.PreferredBackBufferHeight = 600;
#endif
			#endregion preprocessor directive
			_world = new StuffWorld();
		}

		protected override void Initialize()
		{
			#region preprocessor directive
#if IOS
			var bounds = UIKit.UIScreen.MainScreen.Bounds;
			var nativeScale = UIKit.UIScreen.MainScreen.Scale;
			var screenWidth = (int)(bounds.Width * nativeScale);
			var screenHeight = (int)(bounds.Height * nativeScale);
			graphics.PreferredBackBufferWidth = screenWidth;
			graphics.PreferredBackBufferHeight = screenHeight;
#endif
			#endregion preprocessor directive

			#region move window to left of dev's monitor
			//----------------------------------------
			//move window left a bit for dev comfort |
			//----------------------------------------
			var pos = Window.Position;
			pos.X -= 1300;
			pos.Y -= 250;
			Window.Position = pos;
			//----------------------------------------
			#endregion

			FlatRedBallServices.InitializeFlatRedBall(this, graphics);

			#region View port
			Camera.Main.UsePixelCoordinates(true, WIDTH * STUFF_SCALE, HEIGHT * STUFF_SCALE); // makes the camera 2x as big (1 => 4 squares)
			FlatRedBallServices.GraphicsOptions.SetResolution(WIDTH * STUFF_SCALE, HEIGHT * STUFF_SCALE); // Makes the viewport twice as big
			#endregion

			IsMouseVisible = true;

			base.Initialize();
		}

		protected override void Update(GameTime gameTime)
		{
			FlatRedBallServices.Update(gameTime);

			if (InputManager.Mouse.ButtonPushed(MouseButtons.RightButton) || TimeManager.CurrentFrame % 5 == 0)
			{
				_world.AddStuffTopMiddle();
			}

			if (TimeManager.CurrentFrame % 1 == 0)
			{
				_world.Update();
				//_world.Print();
				
			}

			if (TimeManager.CurrentFrame % 120 == 0)
			{
				_world.Print();
			}

			if (InputManager.Keyboard.KeyReleased(Keys.Escape))
			{
				throw new Exception("End game triggered with ESC");
			}

			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			FlatRedBallServices.Draw();

			base.Draw(gameTime);
		}
	}
}
