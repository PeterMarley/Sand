using Sand.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sand.Models.StuffWorld;

public static class StuffWorldFactory
{
	/// <summary>
	/// Basic ass stuffworld
	/// </summary>
	/// <returns></returns>
	public static StuffWorld GetDevStuffWorld_000() => new ();

	#region StuffWorld preconfigured setups for dev testing

	public static StuffWorld WaterBottom3Y()
	{
		var world = new StuffWorld();
		for (int x = 0; x < world.World.Length; x++)
		{
			for (int y = 0; y < 3 && y < world.World[x].Length; y++)
			{
				world.SafeAddStuffIfEmpty(Stuffs.BASIC_WATER, x, y);
			}
		}
		return world;
	}

	public static StuffWorld WaterBottomHalf()
	{
		var world = new StuffWorld();
		for (int x = 0; x < world.World.Length; x++)
		{
			for (int y = 0; y < world.World[x].Length / 2; y++)
			{
				world.SafeAddStuffIfEmpty(Stuffs.BASIC_WATER, x, y);
			}
		}
		return world;
	}
	#endregion
}
