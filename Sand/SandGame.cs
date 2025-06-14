using FlatRedBall;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using FlatRedBall.Input;
using static FlatRedBall.Input.Mouse;
using System.Threading.Tasks;
using static Sand.Constants;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;

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
		Camera.Main.UsePixelCoordinates(true, STUFF_CELL_WIDTH * STUFF_TO_PIXEL_SCALE, STUFF_CELL_HEIGHT * STUFF_TO_PIXEL_SCALE); // makes the camera 2x as big (1 => 4 squares)
		FlatRedBallServices.GraphicsOptions.SetResolution(STUFF_CELL_WIDTH * STUFF_TO_PIXEL_SCALE, STUFF_CELL_HEIGHT * STUFF_TO_PIXEL_SCALE); // Makes the viewport twice as big
		IsMouseVisible = true;
		Window.AllowUserResizing = false;

#if DEBUG

		var configPrintStr =
			$"\n=============================================================\n" +
			$"\tCONFIG VALUES\n" +
			$"===============================================================\n" +
			$"\tSTUFF_SCALE={STUFF_TO_PIXEL_SCALE}\n" +
			$"\tRESOLUTION_X={RESOLUTION_X}\n" +
			$"\tRESOLUTION_Y={RESOLUTION_Y}\n" +
			$"\tSTUFF_WIDTH={STUFF_CELL_WIDTH}\n" +
			$"\tSTUFF_HEIGHT={STUFF_CELL_HEIGHT}\n\n" +
			$"\tPRINT_STUFF_WORLD={PRINT_STUFF_WORLD}\n" +
			$"\tPRINT_STUFF_WORLD_FRAMES={PRINT_STUFF_WORLD_FRAMES}\n\n" +
			$"\tLOG_TO_CONSOLE={LOG_TO_CONSOLE}\n" +
			$"\tLOG_TO_FILE={LOG_TO_FILE}\n\n" +
			$"\tSHOW_STUFF_DORMANCY_COLORS=={SHOW_STUFF_DORMANCY_COLORS}\n" +
			$"===============================================================\n";
		Logger.Instance.LogInfo(configPrintStr);
		FlatRedBallServices.GraphicsOptions.TextureFilter = TextureFilter.Point;

		//============================================================================
		// SETs to comfy dev left screen
		//============================================================================
		var pos = Window.Position;
		pos.X -= 2500;
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
		//FlatRedBallServices.GraphicsOptions.SetResolution(w, h);
		//============================================================================
#endif




		_tLoadMaterials = Task.Run(StuffFactory.Instance.LoadMaterialsAsync);

		//var wsWidth = RESOLUTION_X * 2;
		//var wsHeight = RESOLUTION_Y * 2;

		Camera.Main.Orthogonal = true;
		Camera.Main.OrthogonalWidth = RESOLUTION_X;
		Camera.Main.OrthogonalHeight = RESOLUTION_Y;

		base.Initialize();
	}

	//private int FRAME_COUNT_BETWEEN_UPDATE = 1;
	private int FRAME_COUNT_BETWEEN_DRAW = 1;

	private bool didPrep = false;
	protected override void Update(GameTime gameTime)
	{
		if (InputManager.Keyboard.KeyReleased(Keys.Escape))
		{
			this.Exit();
			return;
		}

		if (!_tLoadMaterials.IsCompleted)
		{
			return;
		}
		else if (!didPrep)
		{
			_world = new DrawableWorld(StuffCellSetup.StoneAroundEdges2);

			//SpriteManager.AddDrawableBatch(_world);
			didPrep = true;
		}

		FlatRedBallServices.Update(gameTime);

		Randoms.Instance.Refresh();

		if (InputManager.Keyboard.KeyPushed(Keys.Enter))
		{
			Debugger.Break();
		}

		//if (TimeManager.CurrentFrame % FRAME_COUNT_BETWEEN_UPDATE == 0)
		//{
		_world.Update();
		//}

		_world.ProcessControlsInput();


		base.Update(gameTime);
	}

	protected override void Draw(GameTime gameTime)
	{
		FlatRedBallServices.Draw();

		if (!didPrep)
		{
			return;
		}

		base.Draw(gameTime);
	}


}
