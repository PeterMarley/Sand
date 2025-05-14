namespace Sand;

public class Constants
{
	//---------------------------------------------------
	//	DIMENSIONS										|
	//---------------------------------------------------
	/// <summary>A Stuff element is this many pixels per side.</summary>
	public const int STUFF_SCALE = 4;//5;
	/// <summary>Window resolution x</summary>
	public const int RESOLUTION_X = 1500;//1500;
	/// <summary>Window resolution y</summary>
	public const int RESOLUTION_Y = 800;//800;
	/// <summary>The number of X-axis elements</summary>
	public const int STUFF_WIDTH = RESOLUTION_X / STUFF_SCALE;
	/// <summary>The number of Y-axis elements</summary>
	public const int STUFF_HEIGHT = RESOLUTION_Y / STUFF_SCALE;

	//---------------------------------------------------
	//	PRINTING										|
	//---------------------------------------------------
	/// <summary>Logs the entire StuffWorld elements. Makes text logs explode in size</summary>]
	public const bool PRINT_STUFF_WORLD = false;
	/// <summary>The number of frames between each world print</summary>]
	public const int PRINT_STUFF_WORLD_FRAMES = 1;

	//---------------------------------------------------
	//	LOGGING											|
	//---------------------------------------------------
	/// <summary>Controls whether logging goes to file</summary>]
	public const bool LOG_TO_FILE = true;
	/// <summary>Controls whether logging goes to console window (still goes to visual studio output)</summary>]
	public const bool LOG_TO_CONSOLE = true;

	//---------------------------------------------------
	//	DEBUG											|
	//---------------------------------------------------
	/// <summary>Controls whether dormant colours are shown in CMYK</summary>]
	public const bool SHOW_STUFF_DORMANCY_COLORS = false;

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
