using FlatRedBall;
using Sand.Services;
using Sand.Stuff;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
	private AbstractStuff[][] _world;
	private StringBuilder _stringBuilder = new();
	private readonly Random _random = new();

	public StuffWorld()
	{
		_world = new AbstractStuff[STUFF_WIDTH][];
		for (int x = 0; x < _world.Length; x++)
		{
			_world[x] = new AbstractStuff[STUFF_HEIGHT];
		}
	}
	#endregion

	#region Public API

	#region Adding Stuff

	public void AddStuffTopMiddle(string stuffType)
	{
		var middle = (STUFF_WIDTH / 2) - 1;
		var top = STUFF_HEIGHT - 1;
		SafeAddStuffIfEmpty(stuffType, middle, top);
	}

	public void SafeAddStuffIfEmpty(string stuffType, int x, int y)
	{
		if (_world.IsValidIndex(x,y) && _world[x][y] == null)
		{
			var stuff = StuffFactory.Instance.Get(stuffType);
			_world[x][y] = stuff.SetPosition(x, y);
		}
	}

	public void SafeAddStuffIfEmpty_InSquare(string stuffType, int x, int y, int length)
	{
		for (int i = x - length; i < x + length; i++)
		{
			for (int j = y - length; j < y + length; j++)
			{
				SafeAddStuffIfEmpty(stuffType, i, j);
			}
		}
	}

	#endregion

	#region Moving Stuff

	//TODO actually these methods shoudl all be in world -- NOT "actually stuffworld should be passed in here, not the underlying data structure"

	public void ApplyGravity(int xIndex, int yIndex)
	{
		var stuff = _world[xIndex][yIndex];
		if (stuff == null) return;
		switch (stuff.Phase)
		{
			case Phase.Solid:
				ApplyGravityPhaseSolid(_world, xIndex, yIndex);
				break;
			case Phase.Liquid:
				ApplyGravityPhaseLiquid(_world, xIndex, yIndex);
				break;
			case Phase.Gas:
			default:
				Logger.Instance.LogInfo($"Phase {stuff.Phase} not handled");
				break;
		}

		void ApplyGravityPhaseSolid(AbstractStuff[][] world, int xIndex, int yIndex)
		{
			//-----------------------------------------------------------------
			//Check 2 spots below left and right, if all are filled then move on
			//-----------------------------------------------------------------

			// if bottom row outside array range just continue as this Stuff cant fall anywhere
			var rowBelowIndex = yIndex - 1;
			if (rowBelowIndex < 0) return;

			// check directly below
			if (Move(new(xIndex, yIndex), new(xIndex, rowBelowIndex)))
			{
				return;
			}

			// check below and left (but alterate sides randomly)
			bool leftSide = this._random.Next(2) == 1;
			int colLeftIndex = xIndex - 1;
			int colRightIndex = xIndex + 1;

			for (var lateralGravAttempts = 2; lateralGravAttempts > 0; lateralGravAttempts--)
			{
				if (leftSide)
				{
					// check below and left
					if (colLeftIndex >= 0 && Move(new(xIndex, yIndex), new(colLeftIndex, rowBelowIndex)))
					{
						break;
					}
				}
				else
				{
					// check below and right
					if (colRightIndex < STUFF_WIDTH && Move(new(xIndex, yIndex), new(colRightIndex, rowBelowIndex)))
					{
						break;
					}
				}
				leftSide = !leftSide;
			}
		}

		void ApplyGravityPhaseLiquid(AbstractStuff[][] world, int xIndex, int yIndex)
		{
			//-----------------------------------------------------------------
			//Check 2 spots below left and right
			//-----------------------------------------------------------------

			// if bottom row outside array range just continue as this Stuff cant fall anywhere
			var rowBelowIndex = yIndex - 1;
			if (rowBelowIndex < 0) return;

			// check directly below
			if (Move(new(xIndex, yIndex), new(xIndex, rowBelowIndex)))
			{
				return;
			}
			bool leftFirst = this._random.Next(2) == 1;

			// check below and left (but alterate sides randomly)
			bool leftSide = leftFirst;
			int colLeftIndex = xIndex - 1;
			int colRightIndex = xIndex + 1;

			for (var lateralGravAttempts = 2; lateralGravAttempts > 0; lateralGravAttempts--)
			{
				if (leftSide)
				{
					// check below and left
					if (colLeftIndex >= 0 && Move(new(xIndex, yIndex), new(colLeftIndex, rowBelowIndex)))
					{
						break;
					}
				}
				else
				{
					// check below and right
					if (colRightIndex < STUFF_WIDTH && Move(new(xIndex, yIndex), new(colRightIndex, rowBelowIndex)))
					{
						break;
					}
				}
				leftSide = !leftSide;
			}

			if (TimeManager.CurrentFrame % 1 == 0)
			{
				colLeftIndex--;
				colRightIndex++;
				leftSide = leftFirst;

				//-----------------------------------------------------------------
				//Check 2 spots directly left and right - represent fluidic flow
				//-----------------------------------------------------------------

				// check direct lateral movements
				for (var lateralFlowAttempts = 2; lateralFlowAttempts > 0; lateralFlowAttempts--)
				{
					if (leftSide)
					{
						// check left
						if (colLeftIndex >= 0 && Move(new(xIndex, yIndex), new(colLeftIndex, yIndex)))
						{
							break;
						}
					}
					else
					{
						// check right
						if (colRightIndex < STUFF_WIDTH && Move(new(xIndex, yIndex), new(colRightIndex, yIndex)))
						{
							break;
						}
					}
					leftSide = !leftSide;
				}
			}
		}
	}

	public bool Move(Point from, Point to)
	{
		if (from is { X: >= 0 and < STUFF_WIDTH, Y: >= 0 and < STUFF_HEIGHT } &&
			to is { X: >= 0 and < STUFF_WIDTH, Y: >= 0 and < STUFF_HEIGHT })
		{
			var stuffAtSource = _world[from.X][from.Y];
			
			if (stuffAtSource == null) return true;

			switch (stuffAtSource.Phase)
			{
				case Phase.Solid:
					MoveSolid(from, to);
					break;
				case Phase.Liquid:
					//if (TimeManager.CurrentFrame % 2 == 0)
					//{
						MoveLiquid(from, to);
					//}
					break;
				default: throw new NotImplementedException($"{stuffAtSource.Phase} phase movement not implemented");
			}
		}

		return false;

		bool MoveSolid(Point from, Point to)
		{
			// check for stuff at target
			var didMove = false;

			var sourceHasStuff = _world.TryGetStuff(from.X, from.Y, out AbstractStuff stuffSource);
			var targetHasStuff = _world.TryGetStuff(to.X, to.Y, out AbstractStuff stuffTarget);

			// if not stuff at target fall to here and finish
			if ((!targetHasStuff || stuffTarget is not {Phase: Phase.Solid }))
			{
				
				_world[to.X][to.Y] = stuffSource.SetPosition(to.X, to.Y); // taretSource effectively removed but we have a ref above
				_world[from.X][from.Y] = null;
				stuffSource.MovedThisUpdate = true;
				didMove = true;
			}
			//else
			//{
				//=======================================================================
				// LIQUID DISPLACEMENT
				//=======================================================================

				// if stuff at the target, but target NOT solid (we know that source IS solid), then
				// fall here - liquid, gas displacement means the thing pushed out of the way has
				// to go up
				if (targetHasStuff && stuffTarget is not { Phase: Phase.Solid })
				{
					// up upwards in Y-axis (checkling one left and right also) from displaced liqud
					// until empty stuff found - staying within the water column.
					// Once we fall out of bounds of array just stop looping as the target is already
					// garbage awaiting collection.
					var hasDisplaced = false;
					var waterColumnX = to.X;
					for (int cursorY = to.Y; !hasDisplaced && _world.IsValidIndex(waterColumnX, cursorY); cursorY++)
					{

						///////////////////////////////////////////////////////
						// lets try randomly going left, on or right of yCoord in water column droping
						// there if empty, so 1/3 itll land left, on or right of cursorY
						///////////////////////////////////////////////////////
						for (var i = 0; !hasDisplaced && i < Randoms.Instance.Ind_leftRightMid.Length; i++)
						{
							var adjCursorX = waterColumnX + Randoms.Instance.Ind_leftRightMid[i];
							if (_world.IsValidIndex(adjCursorX, cursorY) && _world[adjCursorX][cursorY] == null)
							{
								// move this displaced liquid to here
								_world[adjCursorX][cursorY] = stuffTarget.SetPosition(adjCursorX, cursorY); ;
								hasDisplaced = true;// BREAKS both loops
							}
						}
					}

					// no empty spot found so "destroy" the displayers stuff (for now)
					// TODO this will need updated when screen can move
				}
			//}

			return didMove;
		}

		bool MoveLiquid(Point from, Point to)
		{
			// check for stuff at target

			if (from is { X: >= 0 and < STUFF_WIDTH, Y: >= 0 and < STUFF_HEIGHT } &&
				to is { X: >= 0 and < STUFF_WIDTH, Y: >= 0 and < STUFF_HEIGHT })
			{

				var stuffSource = _world[from.X][from.Y];
				var stuffTarget = _world[to.X][to.Y];
				// if not stuff at target fall to here and finish
				if (stuffTarget == null)
				{
					// update world
					_world[to.X][to.Y] = stuffSource;
					_world[from.X][from.Y] = null;

					if (stuffSource != null)
					{
						stuffSource.MovedThisUpdate = true;
					}

					return true;
				}
			}


			return false;
		}
	}

	#endregion

	#region Game Loop Methods called by SandGame

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

					ApplyGravity(xIndex, yIndex);
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

	#region StuffWorld preconfigured setups for dev testing
	public void PrepareWaterBottom3Y()
	{
		for (int x = 0; x < _world.Length; x++)
		{
			for (int y = 0; y < 3 && y < _world[x].Length; y++)
			{
				SafeAddStuffIfEmpty(Stuffs.BASIC_WATER, x, y);
			}
		}
	}

	public void PrepareWaterBottomHalf()
	{
		for (int x = 0; x < _world.Length; x++)
		{
			for (int y = 0; y < _world[x].Length / 2; y++)
			{
				SafeAddStuffIfEmpty(Stuffs.BASIC_WATER, x, y);
			}
		}
	}
	#endregion

	#endregion

}
