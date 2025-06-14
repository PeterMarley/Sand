namespace Sand;

public class Constants
{
	//---------------------------------------------------
	//	DIMENSIONS										|
	//---------------------------------------------------
	/// <summary>Window resolution X</summary>
	public const int RESOLUTION_X = 1500;//1500;
	/// <summary>Window resolution Y</summary>
	public const int RESOLUTION_Y = 800;//800;
	/// <summary>This many pixels per <see cref="Sand.Stuff"/></summary>
	public const int STUFF_TO_PIXEL_SCALE = 4;//5;
	/// <summary><see cref="Sand.StuffCell"/>'s are is this many <see cref="Sand.Stuff"/> wide</summary>
	public const int STUFF_CELL_WIDTH = RESOLUTION_X / STUFF_TO_PIXEL_SCALE;
	/// <summary><see cref="Sand.StuffCell"/>'s are is this many <see cref="Sand.Stuff"/> high</summary>
	public const int STUFF_CELL_HEIGHT = RESOLUTION_Y / STUFF_TO_PIXEL_SCALE;
	/// <summary><see cref="Sand.StuffCell"/> has this many chunks across the x and the y axies - same number up and across for now</summary>
	public const int STUFF_CELL_CHUNKS_LONG = 10;

	//===================================================
	//	PRINTING										|
	//===================================================
	/// <summary>Logs the entire StuffWorld elements. Makes text logs explode in size</summary>]
	public const bool PRINT_STUFF_WORLD = false;
	/// <summary>The number of frames between each world print</summary>]
	public const int PRINT_STUFF_WORLD_FRAMES = 1;
	/// <summary>Print important world coords to console on left click</summary>]
	public const bool PRINT_POSITIONS_ON_CLICK = true;

	//===================================================
	//	LOGGING											|
	//===================================================
	/// <summary>Controls whether logging goes to file</summary>]
	public const bool LOG_TO_FILE = true;
	/// <summary>Controls whether logging goes to console window (still goes to visual studio output)</summary>]
	public const bool LOG_TO_CONSOLE = true;

	//===================================================
	//	DEBUG											|
	//===================================================
	/// <summary>Controls whether dormant colours are shown in CMYK</summary>]
	public const bool SHOW_STUFF_DORMANCY_COLORS = false;
	/// <summary>Controls the player's hitbox is highlighted</summary>]
	public const bool SHOW_PLAYER_HITBOX = true;

	//===================================================
	// VISUAL ELEMENT Z INDICES
	//===================================================
	public const float Z_IND_BACKGROUND = -10f;
	public const float Z_IND_WORLD = 0f;
	public const float Z_IND_PLAYER = 10f;

	public const bool SHOW_BACKGROUND = true;
	public const bool SHOW_WORLD = true;
	public const bool SHOW_PLAYER = true;

}

public class Stuffs
{
	public const string BASIC_SAND = "BasicSand";
	public const string BASIC_WATER = "BasicWater";
	public const string BASIC_STONE = "BasicStone";
	public const string BASIC_LAVA = "BasicLava";
	public const string BASIC_LAVA2 = "BasicLava2";

	//public const string FLAT_WATER = "FlatWater";
}
