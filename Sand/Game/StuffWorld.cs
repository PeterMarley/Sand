using FlatRedBall.Math.Statistics;
using System;
using System.Reflection;
using System.Text;
using static Sand.Constants;

namespace Sand;

public class StuffWorld
{
	#region ctor
	/// <summary>
	/// Outer array is X, inner array is Y.<br/><em>[0, 0]</em> represents bottom left of viewport. <em>[xMax, yMax]</em> represents top right
	/// </summary>
	public IStuff[][] _world;
	private StringBuilder _stringBuilder;

	public StuffWorld()
	{
		_stringBuilder = new StringBuilder();
		_world = new Stuff[WIDTH][];
		for (int x = 0; x < _world.Length; x++)
		{
			_world[x] = new Stuff[HEIGHT];
		}
	}
	#endregion
	public void AddStuffTopMiddle()
	{
		var middle = ((WIDTH/* * STUFF_SCALE*/) / 2) - 1;// i think this will get optimised, but leaving it here as i want to remember im doub
		var top = (HEIGHT/* * STUFF_SCALE*/) - 1;
		var stuff = new Stuff().SetPosition(middle, top);
		_world[middle][top] = stuff;
	}

	private bool _break = false;
	private void ApplyGravity(IStuff stuff, int xIndex, int yIndex) 
	{
		//---------------------------------------------------
		//Check 3 spots below, if all are filled then move on
		//---------------------------------------------------

		// if below bottom row just continue as this Stuff cant fall
		var rowBelowIndex = yIndex - 1;
		if (rowBelowIndex < 0) return;

		// check below
		//var stuffBelow = _world[xIndex][rowBelowIndex];
		//// if not stuff directly below fall to here and finish
		//if (stuffBelow == null)
		//{
		//	// update StuffWorld
		//	_world[xIndex][yIndex] = null;
		//	_world[xIndex][rowBelowIndex] = stuff;
		//	// update Stuff sprite position
		//	stuff.SetPosition(xIndex, rowBelowIndex);
		//	return;
		//}
		if (GravitateStuffTo(_world, stuff, xIndex, yIndex, xIndex, rowBelowIndex))
		{
			return;
		}


		_break = true;
		State.BreakLatch = true;
		//// check below and left
		//var colLeftIndex = xIndex - 1;
		//// if col index safe
		//if (colLeftIndex >= 0)
		//{
		//	// get stuff below and left
		//	var stuffBelowLeft = _world[colLeftIndex][rowBelowIndex];
		//	// if nothing below left fall to here and finish
		//	if (stuffBelowLeft == null)
		//	{
		//		_world[xIndex][yIndex] = null;
		//		_world[colLeftIndex][rowBelowIndex] = stuff;
		//		// update Stuff sprite position
		//		stuff.SetPosition(colLeftIndex, rowBelowIndex);
		//		return;
		//	}
		//}

		var random = new Random();
		var r = random.Next(2);
		bool left = r == 1;
		//Logger.Instance.LogInfo($"r is {r}, b is {left}");
		var colLeftIndex = xIndex - 1;
		var colRightIndex = xIndex + 1;
		var successfullyGravitated = false;
		Action a, b;
		//if (left)
		//{
			a = new Action(() => {
				if (colLeftIndex >= 0 && GravitateStuffTo(_world, stuff, xIndex, yIndex, colLeftIndex, rowBelowIndex))
				{
					successfullyGravitated = true;
					//return;
				}
			});

			b = new Action(() => {
				if (colRightIndex < WIDTH && GravitateStuffTo(_world, stuff, xIndex, yIndex, colRightIndex, rowBelowIndex))
				{
					successfullyGravitated = true;
					return;
				}
			});

			// check below and right
			// if col index safe

		//}
		//else
		//{
		//	// check below and right
		//	// if col index safe

		//	a = new Action(() => {
		//		if (colRightIndex < HEIGHT && GravitateStuffTo(_world, stuff, xIndex, yIndex, colRightIndex, rowBelowIndex))
		//		{
		//			successfullyGravitated = true;
		//			return;
		//		}
		//	});

		//	b = new Action(() => {
		//		if (colLeftIndex >= 0 && GravitateStuffTo(_world, stuff, xIndex, yIndex, colLeftIndex, rowBelowIndex))
		//		{
		//			successfullyGravitated = true;
		//			//return;
		//		}
		//	});

		//	//if (colRightIndex < HEIGHT && GravitateStuffTo(_world, stuff, xIndex, yIndex, colRightIndex, rowBelowIndex))
		//	//{
		//	//	return;
		//	//}

		//	//if (colLeftIndex >= 0 && GravitateStuffTo(_world, stuff, xIndex, yIndex, colLeftIndex, rowBelowIndex))
		//	//{
		//	//	return;
		//	//}

		//	//for (var attempts = 0; !successfullyGravitated && attempts < 3; attempts++)
		//	//{
		//	//	if (left) a();
		//	//	else b();
		//	//	left = !left;
		//	//}
		//}

		for (var lateralGravitationAttempts = 0; !successfullyGravitated && lateralGravitationAttempts < 3; lateralGravitationAttempts++)
		{
			if (left)
			{
				a();
			}
			else
			{
				b();
			}
			left = !left;
		}

		//// check below and left
		//var colLeftIndex = xIndex - 1;
		//if (colLeftIndex >= 0 && GravitateStuffTo(_world, stuff, xIndex, yIndex, colLeftIndex, rowBelowIndex))
		//{
		//	return;
		//}

		//// check below and right
		//var colRightIndex = xIndex + 1;
		//// if col index safe
		//if (colRightIndex < HEIGHT)
		//{
		//	// get stuff below and right
		//	var stuffBelowRight = _world[colRightIndex][rowBelowIndex];
		//	// if nothing below right fall to here and finish
		//	if (stuffBelowRight == null)
		//	{
		//		_world[xIndex][yIndex] = null;
		//		_world[colRightIndex][rowBelowIndex] = stuff;
		//		// update Stuff sprite position
		//		stuff.SetPosition(colRightIndex, rowBelowIndex);
		//		return;
		//	}
		//}

		//// check below and right
		//var colRightIndex = xIndex + 1;
		//// if col index safe
		//if (colRightIndex < HEIGHT && GravitateStuffTo(_world, stuff, xIndex, yIndex, colRightIndex, rowBelowIndex))
		//{
		//	return;
		//}

		bool GravitateStuffTo(IStuff[][] world, IStuff stuffSource, int xFrom, int yFrom, int xTo, int yTo)
		{
			// check below
			var stuffTarget = _world[xTo][yTo];
			// if not stuff directly below fall to here and finish
			if (stuffTarget == null)
			{
				// update StuffWorld
				_world[xFrom][yFrom] = null;
				_world[xTo][yTo] = stuffSource;
				// update Stuff sprite position
				stuffSource.SetPosition(xTo, yTo);
				return true;
			}
			return false;
		}
	}

	public void Update()
	{
		int x = 0;
		int y = 0;
		try
		{
			for (var xIndex = 0; xIndex < _world.Length; xIndex++)
			{
				for (var yIndex = 0; yIndex < _world[xIndex].Length; yIndex++)
				{
					// get stuff here
					var targetStuff = _world[xIndex][yIndex];
					// if nothing here then move on to next Stuff
					if (targetStuff == null) continue;

					x = xIndex;
					y = yIndex;

					ApplyGravity(targetStuff, xIndex, yIndex);

				}
			}
		}
		catch (Exception applyGravEx)
		{
			Logger.Instance.LogError(applyGravEx, $"(x,y)=({x},{y}), (maxX, maxY)=({WIDTH - 1},{HEIGHT - 1})");
		}

		//int x = 0;
		//int y = 0;
		//try
		//{
		//	for (var xIndex = 0; xIndex < _world.Length; xIndex++)
		//	{
		//		for (var yIndex = _world[xIndex].Length - 1; yIndex >= 0; yIndex--)
		//		{
		//			// get stuff here
		//			var targetStuff = _world[xIndex][yIndex];
		//			// if nothing here then move on to next Stuff
		//			if (targetStuff == null) continue;

		//			x = xIndex;
		//			y = yIndex;

		//			ApplyGravity(targetStuff, xIndex, yIndex);

		//		}
		//	}
		//}
		//catch (Exception applyGravEx)
		//{
		//	Logger.Instance.LogError(applyGravEx, $"(x,y)=({x},{y}), (maxX, maxY)=({WIDTH - 1},{HEIGHT - 1})");
		//}
	}

	public void Print()
	{

		//var yAxisLabel = 1;
		//var xAxisLabel = 1;
		_stringBuilder.AppendLine("\n\n\n\n\n\n\n\n\n\n\n\n");
		_stringBuilder.Append("    ");

		for (int i = 0; i < _world.Length; i++)
		{
			//_stringBuilder.Append($" {xAxisLabel++:D2} ");
		}

		_stringBuilder.AppendLine();

		foreach (var i in _world)
		{
			//_stringBuilder.Append($" {yAxisLabel++} ");
			foreach (var j in i)
			{
				_stringBuilder.Append($" {(j == null ? "--" : j.Id.ToString()[..2])} ");
			}
			_stringBuilder.Append('\n');
		}

		Logger.Instance.LogInfo(_stringBuilder.ToString());
		_stringBuilder.Clear();
	}
}
