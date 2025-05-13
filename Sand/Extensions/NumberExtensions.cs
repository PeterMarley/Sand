using FlatRedBall;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Sand.Constants;
namespace Sand.Extensions;

public static class NumberExtensions
{
	private const int HALF_RESOLUTION_X = RESOLUTION_X / 2;
	private const int HALF_RESOLUTION_Y = RESOLUTION_Y / 2;
	private const int STUFF_DIVISOR = STUFF_SCALE * 2;
	public static (int top, int right, int bottom, int left) ToStuffCoord(Sprite sprite)
	{
		var offsetX = sprite.Width / 2;
		var offsetY = sprite.Height / 2;

		/*		EG, GETTING TOP:
		 *
		 * (int)(
		 *		(sprite.Y				// centre of sprite (in world coords)
		 *		+ offsetY				// move to top of sprint
		 *		+ HALF_RESOLUTION_Y		// adjust coordinate origin (center origin sprite VS bottom left origin drawableworld)
		 *		) 
		 * / STUFF_SCALE),				// adjust to stuff units
		 */

		return (
			/*top*/        (int)((sprite.Y + offsetY + HALF_RESOLUTION_Y) / STUFF_DIVISOR),
			/*right*/    (int)((sprite.X + offsetX + HALF_RESOLUTION_X) / STUFF_DIVISOR),
			/*bottom*/    (int)((sprite.Y - offsetY + HALF_RESOLUTION_Y) / STUFF_DIVISOR),
			/*left*/    (int)((sprite.X - offsetX + HALF_RESOLUTION_X) / STUFF_DIVISOR)
		);
	}

	//// ie get the x and y of a stuff, and get its sprite coord
	public static (float xWorld, float yWorld) ToWorldCoords(int xStuff, int yStuff)=> (ToWorldCoordsX(xStuff),ToWorldCoordsY(yStuff));
	public static float ToWorldCoordsX(int xStuff) => xStuff * STUFF_DIVISOR - HALF_RESOLUTION_X;
	public static float ToWorldCoordsY(int yStuff) => yStuff * STUFF_DIVISOR - HALF_RESOLUTION_Y;
}
