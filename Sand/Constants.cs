using Sand.Stuff;

namespace Sand;

public class Constants
{
	//---------------------------------------------------
	//	DIMENSIONS										|
	//---------------------------------------------------
	//TODO SEE BELOW COMMENT
	/// <summary>The number of X-axis elements.TODO convert width and height to resolution (ie real pixels) and calculate the number of <see cref="Sand.Stuff.AbstractStuff"/>'s wide and high</summary>
	public const int STUFF_WIDTH = 100;//20;//51;
	/// <summary>The number of Y-axis elements.</summary>
	public const int STUFF_HEIGHT = 60;//40;//200;
	/// <summary>A Stuff element is this many pixels per side.</summary>
	public const int STUFF_SCALE = 15/*10*//*2*/;

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
	public const bool LOG_TO_CONSOLE = false;
}

public class Stuffs
{
	public const string BASIC_SAND = "BasicSand";
	public const string BASIC_WATER = "BasicWater";
	//public const string FLAT_WATER = "FlatWater";
}
