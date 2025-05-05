using System.Runtime.CompilerServices;

namespace Sand;

public class Constants
{
	//---------------------------------------------------
	//	DIMENSIONS										|
	//---------------------------------------------------
	/// <summary>The number of X-axis elements.</summary>
	public const int WIDTH = 100;//51;
	/// <summary>The number of Y-axis elements.</summary>
	public const int HEIGHT = 50;//200;
	/// <summary>A Stuff element is this many pixels per side.</summary>
	public const int STUFF_SCALE = 5/*2*/;

	//---------------------------------------------------
	//	PRINTING										|
	//---------------------------------------------------
	/// <summary>Logs the entire StuffWorld elements. Makes text logs explode in size</summary>]
	public const bool PRINT_STUFF_WORLD = true;
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
