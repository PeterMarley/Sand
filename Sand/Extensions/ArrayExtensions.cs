using Sand.Stuff;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using static Sand.Constants;

namespace Sand;

public static class ArrayExtensions
{


	public static bool TryGetStuff(this StuffBasic[][] arr2d, int x, int y, [NotNullWhen(true)] out StuffBasic element)
	{
		if (arr2d.IsValidIndex(x, y) && arr2d[x][y] != null)
		{
			element = arr2d[x][y];
			return true;
		}
		else
		{
			element = null;
			return false;
		}
	}

	private static readonly Func<object[], int, bool> _ARE_THESE_INDICES_ALRIGHT_BAI = (arr, i) => i >= 0 && i < arr.Length;

	public static bool IsValidIndex(this StuffBasic[][] arr2d, int x, int y)
	{
		return _ARE_THESE_INDICES_ALRIGHT_BAI(arr2d, x) && _ARE_THESE_INDICES_ALRIGHT_BAI(arr2d[x], y);
	}

	public static bool IsValidIndex(this StuffBasic[] arr, int i)
	{
		return _ARE_THESE_INDICES_ALRIGHT_BAI(arr, i);
	}

	//public static void PrepareWaterBottom3Y(this StuffBasic[][] arr2d)
	//{
	//	for (int x = 0; x < arr2d.Length; x++)
	//	{
	//		for (int y = 0; y < 3 && y < arr2d[x].Length; y++)
	//		{
	//			arr2d[x][y] = StuffFactory.Instance.Get(Stuffs.BASIC_WATER);
	//		}
	//	}
	//}

}
