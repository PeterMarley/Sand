using FlatRedBall.Math.Statistics;
using Sand.Stuff;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
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
	private IStuff[][] _world;
	private StringBuilder _stringBuilder = new();
	private readonly Random _random = new();
	public StuffWorld()
	{
		_world = new IStuff[STUFF_WIDTH][];
		for (int x = 0; x < _world.Length; x++)
		{
			_world[x] = new IStuff[STUFF_HEIGHT];
		}
	}
	#endregion

	#region Public API

	public void AddStuffTopMiddle(IStuff stuff)
	{
		var middle = (STUFF_WIDTH / 2) - 1;
		var top = STUFF_HEIGHT - 1;
		AddStuffIfEmpty(stuff, middle, top);
	}

	//public void AddStuffTo(int x, int y)
	//{
	//	AddStuffTo(new StuffSand(), x, y);
	//}

	public void AddStuffIfEmpty(IStuff stuff, int x, int y)
	{
		if (_world[x][y] == null)
		{
			_world[x][y] = stuff.SetPosition(x, y);
		}
	}

	public void Update()
	{
		var p = new Point();
		var ltr = true;

		try
		{
			for (var yIndex = 0; yIndex < STUFF_HEIGHT; yIndex++)
			{
				for (var xIndexSource = 0; xIndexSource < STUFF_WIDTH; xIndexSource++)
				{
					//==========================================================||
					// we adjust the x index depending if we're going
					//	left to right (ltr) => 0 to last index
					//	right to left (rtl) => last index to 0
					//==========================================================||

					int xIndex = ltr ? xIndexSource : STUFF_WIDTH - 1 - xIndexSource;

					p.X = xIndex;
					p.Y = yIndex;

					// get stuff here
					var targetStuff = _world[xIndex][yIndex];
					// if nothing here then move on to next Stuff
					if (targetStuff == null || targetStuff.MovedThisUpdate) continue;

					targetStuff.ApplyGravity(_world, xIndex, yIndex);
				}

				// flip the direction of the next horizontal traversal - for reasons
				ltr = !ltr;
			}
		}
		catch (Exception applyGravEx)
		{
			Logger.Instance.LogError(applyGravEx, $"(x,y)=({p.X},{p.Y}), (maxX, maxY)=({STUFF_WIDTH - 1},{STUFF_HEIGHT - 1})");
		}
	}

	public void Print()
	{
#pragma warning disable CS0162 // Unreachable code detected
#pragma warning disable IDE0079 // Remove unnecessary suppression
		if (!PRINT_STUFF_WORLD) return;
#pragma warning restore IDE0079 // Remove unnecessary suppression

		_stringBuilder.AppendLine("\n\n\n\n\n\n\n\n\n\n\n\n");


		foreach (var i in _world)
		{
			foreach (var j in i)
			{
				_stringBuilder.Append($" {(j == null ? "--" : j.Id.ToString()[..2])} ");
			}
			_stringBuilder.AppendLine();
		}

		Logger.Instance.LogInfo(_stringBuilder.ToString());
		_stringBuilder.Clear();
#pragma warning restore CS0162 // Unreachable code detected
	}

	public void Draw()
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

					targetStuff.SetPosition(xIndex, yIndex);
					targetStuff.MovedThisUpdate = false;

				}
			}
		}
		catch (Exception applyGravEx)
		{
			Logger.Instance.LogError(applyGravEx, $"(x,y)=({x},{y}), (maxX, maxY)=({STUFF_WIDTH - 1},{STUFF_HEIGHT - 1})");
		}
	}
	
	#endregion

	//private void ApplyGravity(IStuff stuff, int xIndex, int yIndex)
	//{
	//	//---------------------------------------------------
	//	//Check 3 spots below, if all are filled then move on
	//	//---------------------------------------------------

	//	// if bottom row outside array range just continue as this Stuff cant fall anywhere
	//	var rowBelowIndex = yIndex - 1;
	//	if (rowBelowIndex < 0) return;

	//	// check directly below
	//	if (GravitateStuffTo(stuff, xIndex, yIndex, xIndex, rowBelowIndex))
	//	{
	//		return;
	//	}

	//	// check below and left (but alterate sides randomly)
	//	bool leftSide = _random.Next(2) == 1;
	//	var colLeftIndex = xIndex - 1;
	//	var colRightIndex = xIndex + 1;

	//	for (var lateralGravAttempts = 2; lateralGravAttempts > 0; lateralGravAttempts--)
	//	{
	//		if (leftSide)
	//		{
	//			// check below and left
	//			if (colLeftIndex >= 0 && GravitateStuffTo(stuff, xIndex, yIndex, colLeftIndex, rowBelowIndex))
	//			{
	//				break;
	//			}
	//		}
	//		else
	//		{
	//			// check below and right
	//			if (colRightIndex < STUFF_WIDTH && GravitateStuffTo(stuff, xIndex, yIndex, colRightIndex, rowBelowIndex))
	//			{
	//				break;
	//			}
	//		}
	//		leftSide = _random.Next(2) == 1;
	//	}

	//	bool GravitateStuffTo(IStuff stuffSource, int xFrom, int yFrom, int xTo, int yTo)
	//	{
	//		// check for stuff at target
	//		var stuffTarget = _world[xTo][yTo];
	//		// if not stuff at target fall to here and finish
	//		if (stuffTarget == null)
	//		{
	//			// update world
	//			_world[xFrom][yFrom] = null;
	//			_world[xTo][yTo] = stuffSource;
	//			return true;
	//		}
	//		return false;
	//	}
	//}

}
