using Sand.Stuff;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Sand.Constants;

namespace Sand;

public static class ArrayExtensions
{
	public static bool Move(this IStuff[][] world, Point from, Point to)
	{
		// check for stuff at target

		if (from.X >= 0 && from.X < STUFF_WIDTH && from.Y >= 0 && from.Y < STUFF_HEIGHT
			&& to.X >= 0 && to.X < STUFF_WIDTH && to.Y >= 0 && to.Y < STUFF_HEIGHT)
		{

			var stuffSource = world[from.X][from.Y];
			var stuffTarget = world[to.X][to.Y];
			// if not stuff at target fall to here and finish
			if (stuffTarget == null)
			{
				// update world
				world[to.X][to.Y] = stuffSource;
				world[from.X][from.Y] = null;
				stuffSource.MovedThisUpdate = true;
				return true;
			}
		}

		
		return false;
	}
}
