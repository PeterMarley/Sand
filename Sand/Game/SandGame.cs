using FlatRedBall;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using FlatRedBall.Input;
using static FlatRedBall.Input.Mouse;
using static Sand.Constants;
using Sand.Stuff;
using System.Threading.Tasks;
using System;
using Sand.Services;

namespace Sand;

public partial class SandGame : Microsoft.Xna.Framework.Game
{
	GraphicsDeviceManager graphics;

	private StuffWorld _world;
	private Task _tLoadMaterials;
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

		//boiler plate
		FlatRedBallServices.InitializeFlatRedBall(this, graphics);

		// set up camera and viewport
		Camera.Main.UsePixelCoordinates(true, STUFF_WIDTH * STUFF_SCALE, STUFF_HEIGHT * STUFF_SCALE); // makes the camera 2x as big (1 => 4 squares)
		FlatRedBallServices.GraphicsOptions.SetResolution(STUFF_WIDTH * STUFF_SCALE, STUFF_HEIGHT * STUFF_SCALE); // Makes the viewport twice as big
		IsMouseVisible = true;
		Window.AllowUserResizing = false;

		_tLoadMaterials = Task.Run(StuffFactory.Instance.LoadMaterialsAsync);

		base.Initialize();
	}

	private int FRAME_COUNT_BETWEEN_UPDATE_DRAW = 1;
	private bool didPrep = false;
	protected override void Update(GameTime gameTime)
	{
		if (!_tLoadMaterials.IsCompleted)
		{
			return;
		}
#if DEBUG
		else if (!didPrep)
		{
			//_world.PrepareWaterBottom3Y();
			//_world.PrepareWaterBottomHalf();

			didPrep = true;
		}
#endif

		FlatRedBallServices.Update(gameTime);

		Randoms.Instance.Refresh();


		if (InputManager.Mouse.IsInGameWindow())
		{
			var x = InputManager.Mouse.X / STUFF_SCALE;
			var y = (FlatRedBallServices.GraphicsDevice.Viewport.Height - InputManager.Mouse.Y) / STUFF_SCALE;

			if ((InputManager.Mouse.ButtonPushed(MouseButtons.LeftButton) || InputManager.Mouse.ButtonDown(MouseButtons.LeftButton)))
			{
				_world.AddStuffIfEmpty(Stuffs.BASIC_SAND, x, y);
			}

			if ((InputManager.Mouse.ButtonPushed(MouseButtons.RightButton) || InputManager.Mouse.ButtonDown(MouseButtons.RightButton)))
			{
				_world.AddStuffIfEmpty(Stuffs.BASIC_SAND, x, y);
				_world.AddStuffIfEmpty(Stuffs.BASIC_SAND, x + 1, y + 1);
			}

			if ((InputManager.Mouse.ButtonPushed(MouseButtons.MiddleButton) || InputManager.Mouse.ButtonDown(MouseButtons.MiddleButton)))
			{
				_world.AddStuffIfEmpty(Stuffs.FLAT_WATER, x, y);
			}
		}

		if (TimeManager.CurrentFrame % FRAME_COUNT_BETWEEN_UPDATE_DRAW == 0)
		{
			_world.Update();
		}

		if (TimeManager.CurrentFrame % (PRINT_STUFF_WORLD_FRAMES * 3) == 0)
		{
			_world.Print();
		}

		if (InputManager.Keyboard.KeyReleased(Keys.Escape))
		{
			Logger.Instance.Dispose();
			this.Exit();
			return;
		}

		base.Update(gameTime);
	}

	protected override void Draw(GameTime gameTime)
	{
		FlatRedBallServices.Draw();

		if (TimeManager.CurrentFrame % FRAME_COUNT_BETWEEN_UPDATE_DRAW == 0)
		{
			_world.Draw();
		}

		base.Draw(gameTime);
	}
}
