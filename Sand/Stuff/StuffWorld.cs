using Sand.Stuff;
using System;
using System.Drawing;
using System.Text;
using static Sand.Constants;

namespace Sand.Stuff;

public class StuffWorld
{
	#region ctor
	/// <summary>
	/// Outer array is X, inner array is Y.
	/// <br/><em>[0, 0]</em> represents bottom left of viewport.
	/// <br/><em>[0, yMax]</em> represents top left.
	/// <br/><em>[xMax, yMax]</em> represents top right.
	/// </summary>
	private StuffBasic[][] _world;
	private StringBuilder _stringBuilder = new();
	private readonly Random _random = new();
	public StuffWorld()
	{
		_world = new StuffBasic[STUFF_WIDTH][];
		for (int x = 0; x < _world.Length; x++)
		{
			_world[x] = new StuffBasic[STUFF_HEIGHT];
		}
	}
	#endregion

	#region Public API

	public void AddStuffTopMiddle(StuffBasic stuff)
	{
		var middle = (STUFF_WIDTH / 2) - 1;
		var top = STUFF_HEIGHT - 1;
		AddStuffIfEmpty(stuff, middle, top);
	}

	public void AddStuffIfEmpty(StuffBasic stuff, int x, int y)
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

}
