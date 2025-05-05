using System;

using FlatRedBall;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using FlatRedBall.Input;
using static FlatRedBall.Input.Mouse;
using static Sand.Constants;
using Sand.Stuff;

namespace Sand;

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

		//----------------------------------------
		//move window left a bit for dev comfort |
		//----------------------------------------
		var pos = Window.Position;
		pos.X -= 1300;
		pos.Y -= 250;
		Window.Position = pos;
		//----------------------------------------

		FlatRedBallServices.InitializeFlatRedBall(this, graphics);

		Camera.Main.UsePixelCoordinates(true, STUFF_WIDTH * STUFF_SCALE, STUFF_HEIGHT * STUFF_SCALE); // makes the camera 2x as big (1 => 4 squares)
		FlatRedBallServices.GraphicsOptions.SetResolution(STUFF_WIDTH * STUFF_SCALE, STUFF_HEIGHT * STUFF_SCALE); // Makes the viewport twice as big
		IsMouseVisible = true;
		Window.AllowUserResizing = false;

		base.Initialize();
	}

	protected override void Update(GameTime gameTime)
	{
		FlatRedBallServices.Update(gameTime);

		//if (TimeManager.CurrentFrame % 5 == 0)
		//{
		//	_world.AddStuffTopMiddle(new StuffSand());
		//}

		if ((InputManager.Mouse.ButtonPushed(MouseButtons.LeftButton) || InputManager.Mouse.ButtonDown(MouseButtons.RightButton)) && InputManager.Mouse.IsInGameWindow())
		{
			var h =FlatRedBallServices.GraphicsDevice.Viewport.Height;
			_world.AddStuffIfEmpty(new StuffWater(), InputManager.Mouse.X / STUFF_SCALE, (h - InputManager.Mouse.Y) / STUFF_SCALE);
		}

		_world.Update();

		if (TimeManager.CurrentFrame % PRINT_STUFF_WORLD_FRAMES == 0)
		{
			_world.Print();
		}

		if (InputManager.Keyboard.KeyReleased(Keys.Escape))
		{
			Logger.Instance.Dispose();
			this.Exit();
			return;
			//throw new Exception("End game triggered with ESC");
		}

		base.Update(gameTime);
	}

	protected override void Draw(GameTime gameTime)
	{
		FlatRedBallServices.Draw();
		_world.Draw();
		base.Draw(gameTime);
	}
}
