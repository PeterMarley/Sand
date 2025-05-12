using System.Drawing;

namespace Sand;

public static class WorldFactory
{

	#region old ass worlds
	/// <summary>
	/// Basic ass stuffworld
	/// </summary>
	/// <returns></returns>
	//public static StuffWorldOriginal GetDevStuffWorld_001() => new ();

	//#region StuffWorld preconfigured setups for dev testing

	//public static StuffWorldOriginal WaterBottom3Y()
	//{
	//	var world = new StuffWorldOriginal();
	//	for (int x = 0; x < world.World.Length; x++)
	//	{
	//		for (int y = 0; y < 3 && y < world.World[x].Length; y++)
	//		{
	//			world.SafeAddStuffIfEmpty(Stuffs.BASIC_WATER, x, y);
	//		}
	//	}
	//	return world;
	//}

	//public static StuffWorldOriginal WaterBottomHalf()
	//{
	//	var world = new StuffWorldOriginal();
	//	for (int x = 0; x < world.World.Length; x++)
	//	{
	//		for (int y = 0; y < world.World[x].Length / 2; y++)
	//		{
	//			world.SafeAddStuffIfEmpty(Stuffs.BASIC_WATER, x, y);
	//		}
	//	}
	//	return world;
	//}

	//public static StuffWorldOriginal SandAlmostEverywhere()
	//{
	//	var world = new StuffWorldOriginal();

	//	var thirdWidth = world.World.Length / 3;
	//	var totalHeight = world.World[0].Length;
	//	var fifthHeight = totalHeight / 5;

	//	Point bottomLeft = new Point(thirdWidth, totalHeight - fifthHeight);
	//	Point topRight = new Point(thirdWidth + thirdWidth, totalHeight);

	//	for (int x = 0; x < world.World.Length; x++)
	//	{
	//		for (int y = 0; y < world.World[x].Length / 2; y++)
	//		{
	//			if (x >= bottomLeft.X && x <= topRight.X && y >= bottomLeft.Y && y <= topRight.Y)
	//			{
	//				continue;
	//			}
	//			world.SafeAddStuffIfEmpty(Stuffs.BASIC_SAND, x, y);
	//		}
	//	}
	//	return world;
	//}
	#endregion

	public static DrawableWorld GetDevStuffWorld_002() => new();

	public static DrawableWorld WaterBottomHalf()
	{
		var world = new DrawableWorld();
		for (int x = 0; x < world.World.Length; x++)
		{
			for (int y = 0; y < world.World[x].Length / 2; y++)
			{
				world.SafeAddStuffIfEmpty(Stuffs.BASIC_WATER, x, y);
			}
		}
		return world;
	}

}
