using FlatRedBall;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using FlatRedBall.Input;
using static FlatRedBall.Input.Mouse;
using System.Threading.Tasks;
using static Sand.Constants;

namespace Sand;

public partial class SandGame : Microsoft.Xna.Framework.Game
{
	GraphicsDeviceManager graphics;

	private DrawableWorld _world;
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


		//FlatRedBallServices.Game.Window.Position = new Point(0, 0);

		//var pos = Window.Position;
		//pos.X -= 3000;
		//pos.Y -= 250;
		//Window.Position = pos;
		//----------------------------------------

		//boiler plate
		FlatRedBallServices.InitializeFlatRedBall(this, graphics);

		// set up camera and viewport
		Camera.Main.UsePixelCoordinates(true, STUFF_WIDTH * STUFF_SCALE, STUFF_HEIGHT * STUFF_SCALE); // makes the camera 2x as big (1 => 4 squares)
		FlatRedBallServices.GraphicsOptions.SetResolution(STUFF_WIDTH * STUFF_SCALE, STUFF_HEIGHT * STUFF_SCALE); // Makes the viewport twice as big
		IsMouseVisible = true;
		Window.AllowUserResizing = false;

#if DEBUG

		//============================================================================
		// SETs to comfy dev left screen
		//============================================================================
		var pos = Window.Position;
		//pos.X -= 2500;
		//pos.Y -= 250;
		Window.Position = pos;
		//============================================================================
#else
		//============================================================================
		// SETs to full screen borderless windowed
		//============================================================================
		var p = new Point(Window.ClientBounds.Left, Window.ClientBounds.Bottom);
		Window.Position = new Point(0, 0);//p;
		var w = FlatRedBallServices.Game.GraphicsDevice.Adapter.CurrentDisplayMode.Width;
		var h = FlatRedBallServices.Game.GraphicsDevice.Adapter.CurrentDisplayMode.Height;
		FlatRedBallServices.GraphicsOptions.SetResolution(w, h);
		//============================================================================
#endif




		_tLoadMaterials = Task.Run(StuffFactory.Instance.LoadMaterialsAsync);
		
		base.Initialize();
	}

	private int FRAME_COUNT_BETWEEN_UPDATE = 60;
	private int FRAME_COUNT_BETWEEN_DRAW = 120;
	private bool didPrep = false;
	protected override void Update(GameTime gameTime)
	{
		if (InputManager.Keyboard.KeyReleased(Keys.Escape))
		{
			Logger.Instance.Dispose();
			this.Exit();
			return;
		}

		if (!_tLoadMaterials.IsCompleted)
		{
			return;
		}
		else if (!didPrep)
		{
			//_world = StuffWorldFactory.GetDevStuffWorld_000();
			//_world = StuffWorldFactory.WaterBottom3Y();
			//_world = StuffWorldFactory.WaterBottomHalf();
			//_world = WorldFactory.SandAlmostEverywhere();
			_world = WorldFactory.GetDevStuffWorld_002();
			//_world = WorldFactory.WaterBottomHalf();

			SpriteManager.AddDrawableBatch(_world);
			didPrep = true;
		}

		FlatRedBallServices.Update(gameTime);

		Randoms.Instance.Refresh();

		if (InputManager.Mouse.IsInGameWindow())
		{
			var x = InputManager.Mouse.X / STUFF_SCALE;
			var y = (FlatRedBallServices.GraphicsDevice.Viewport.Height - InputManager.Mouse.Y) / STUFF_SCALE;

			if ((InputManager.Mouse.ButtonPushed(MouseButtons.LeftButton) || InputManager.Mouse.ButtonDown(MouseButtons.LeftButton)))
			{
				_world.SafeAddStuffIfEmpty_InSquare(Stuffs.BASIC_WATER, x, y, 10);
				//_world.SafeAddStuffIfEmpty(Stuffs.BASIC_WATER, x, y);

			}

			if ((InputManager.Mouse.ButtonPushed(MouseButtons.RightButton) || InputManager.Mouse.ButtonDown(MouseButtons.RightButton)))
			{
				_world.SafeAddStuffIfEmpty_InSquare(Stuffs.BASIC_SAND, x, y, 10);
				//_world.SafeAddStuffIfEmpty(Stuffs.BASIC_SAND, x, y);
			}

			if ((InputManager.Mouse.ButtonPushed(MouseButtons.MiddleButton) || InputManager.Mouse.ButtonDown(MouseButtons.MiddleButton)))
			{
				_world.SafeAddStuffIfEmpty_InSquare(Stuffs.BASIC_STONE, x, y, 10);
				//_world.SafeAddStuffIfEmpty(Stuffs.BASIC_SAND, x, y);
			}
		}

		if (TimeManager.CurrentFrame % FRAME_COUNT_BETWEEN_UPDATE == 0)
		{
			_world.Update();
		}

		base.Update(gameTime);
	}

	protected override void Draw(GameTime gameTime)
	{
		FlatRedBallServices.Draw();

		if (!didPrep)
		{
			return;
		}

		if (TimeManager.CurrentFrame % FRAME_COUNT_BETWEEN_DRAW == 0)
		{
			_world.Draw(Camera.Main);
		}

		base.Draw(gameTime);
	}
}
