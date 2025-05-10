using FlatRedBall;
using Microsoft.Xna.Framework;
using Sand.Config;
using Sand.Models.Stuff;
using Sand.Services;
using Sand.Stuff;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Sand.Config.Constants;
using Point = System.Drawing.Point;
namespace Sand.Models.StuffWorld;

public class StuffWorld
{

	#region ctor
	/// <summary>
	/// Outer array is X, inner array is Y.
	/// <br/><em>[0, 0]</em> represents bottom left of viewport.
	/// <br/><em>[0, yMax]</em> represents top left.
	/// <br/><em>[xMax, yMax]</em> represents top right.
	/// </summary>
	public AbstractStuff[][] World { get; private set; }
	//private readonly List<(Point topLeft, Point bottomRight)> _subGrids = [];
	private List<(Point bottomLeft, Point topRight)> _subGrids = [];


	private StringBuilder _stringBuilder = new();
	private readonly Random _random = new();

	public Vector2 Dimensions { get; private set; }


	public StuffWorld()
	{
		World = new AbstractStuff[STUFF_WIDTH][];
		for (int x = 0; x < World.Length; x++)
		{
			World[x] = new AbstractStuff[STUFF_HEIGHT];
		}
		Dimensions = new Vector2(World.Length, World[0].Length);

		//===============================================================
		// Create SUB GRIDS - These are updated one per update, spliting
		// overall updates across 6 actions
		//===============================================================

		/*
			// divisors to describe the number of grids along each axis

			var xDivisor = 4;
			var yDivisor = 2;

			the above would create a grid like so:

						xxxx
						xxxx
		 */
		var gridsAlongX = 3;
		var gridsAlongY = 2;

		var subWidth = STUFF_WIDTH / gridsAlongX;
		var subHeight = STUFF_HEIGHT / gridsAlongY;

		////private readonly List<(Point topLeft, Point bottomRight)> _subGrids = [];
		//for (int xCoord = 0; xCoord < gridsAlongX; xCoord++)
		//{
		//	for (int yCoord = 0; yCoord < gridsAlongY; yCoord++)
		//	{
		//		var y = (int)(yCoord * subHeight);
		//		var x = (int)(xCoord * subWidth);
		//		_subGrids.Add((new(x, y + subHeight), new(x + subWidth, y)));
		//	}
		//}

		//private readonly List<(Point bottomLeft, Point topRight)> _subGrids = [];
		for (int xCoord = 0; xCoord < gridsAlongX; xCoord++)
		{
			for (int yCoord = 0; yCoord < gridsAlongY; yCoord++)
			{
				var y = (int)(yCoord * subHeight);
				var x = (int)(xCoord * subWidth);
				_subGrids.Add((new(x, y), new(x + subWidth, y + subHeight)));
			}
		}

		_subGrids.Sort((a, b) => _random.Next(-1, 2));

		updateCounter = _subGrids.Count - 1;
	}
	#endregion

	#region Public API

	#region Adding Stuff

	public void AddStuffTopMiddle(string stuffType)
	{
		var middle = STUFF_WIDTH / 2 - 1;
		var top = STUFF_HEIGHT - 1;
		SafeAddStuffIfEmpty(stuffType, middle, top);
	}

	public void SafeAddStuffIfEmpty(string stuffType, int x, int y)
	{
		if (World.IsValidIndex(x, y) && World[x][y] == null)
		{
			var stuff = StuffFactory.Instance.Get(stuffType);
			World[x][y] = stuff;//.SetPosition(x, y);
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
		var stuff = World[xIndex][yIndex];
		if (stuff == null) return;
		switch (stuff.Phase)
		{
			case Phase.Solid:
				ApplyGravityPhaseSolid(World, xIndex, yIndex);
				break;
			case Phase.Liquid:
				ApplyGravityPhaseLiquid(World, xIndex, yIndex);
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
			if (Move(new(xIndex, yIndex), new(xIndex, rowBelowIndex - 2)) || Move(new(xIndex, yIndex), new(xIndex, rowBelowIndex - 1)))
			{
				return;
			}

			// check below and left (but alterate sides randomly)
			bool leftSide = _random.Next(2) == 1;
			int colLeftIndex = xIndex - 1;
			int colRightIndex = xIndex + 1;

			for (var lateralGravAttempts = 2; lateralGravAttempts > 0; lateralGravAttempts--)
			{
				if (leftSide)
				{
					// check below and left
					if (colLeftIndex >= 0 && Move(new(xIndex, yIndex), new(colLeftIndex, rowBelowIndex)))
					{
						return;
						//break;
					}
				}
				else
				{
					// check below and right
					if (colRightIndex < STUFF_WIDTH && Move(new(xIndex, yIndex), new(colRightIndex, rowBelowIndex)))
					{
						return;
						//break;
					}
				}
				leftSide = !leftSide;
			}

			// check directly below
			if (Move(new(xIndex, yIndex), new(xIndex, rowBelowIndex)))
			{
				return;
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
			if (Move(new(xIndex, yIndex), new(xIndex, rowBelowIndex - 2)) || Move(new(xIndex, yIndex), new(xIndex, rowBelowIndex - 1)))
			{
				return;
			}
			bool leftFirst = _random.Next(2) == 1;

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
		if (World.IsValidIndex(from) && World.IsValidIndex(to))
		//if (from is { X: >= 0 and < STUFF_WIDTH, Y: >= 0 and < STUFF_HEIGHT } &&
		//to is { X: >= 0 and < STUFF_WIDTH, Y: >= 0 and < STUFF_HEIGHT })
		{
			var stuffAtSource = World[from.X][from.Y];

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

			var sourceHasStuff = World.TryGetStuff(from.X, from.Y, out AbstractStuff stuffSource);
			var targetHasStuff = World.TryGetStuff(to.X, to.Y, out AbstractStuff stuffTarget);

			// if not stuff at target fall to here and finish
			if (!targetHasStuff || stuffTarget is not { Phase: Phase.Solid })
			{

				World[to.X][to.Y] = stuffSource;//.SetPosition(to.X, to.Y); // taretSource effectively removed but we have a ref above
				World[from.X][from.Y] = null;
				stuffSource.MovedThisUpdate = true;
				didMove = true;
			}

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
				for (int cursorY = to.Y; !hasDisplaced && World.IsValidIndex(waterColumnX, cursorY); cursorY++)
				{

					///////////////////////////////////////////////////////
					// lets try randomly going left, on or right of yCoord in water column droping
					// there if empty, so 1/3 itll land left, on or right of cursorY
					///////////////////////////////////////////////////////
					for (var i = 0; !hasDisplaced && i < Randoms.Instance.Ind_leftRightMid.Length; i++)
					{
						var adjCursorX = waterColumnX + Randoms.Instance.Ind_leftRightMid[i];
						if (World.IsValidIndex(adjCursorX, cursorY) && World[adjCursorX][cursorY] == null)
						{
							// move this displaced liquid to here
							World[adjCursorX][cursorY] = stuffTarget;//.SetPosition(adjCursorX, cursorY); ;
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
			if (World.IsValidIndex(from) && World.IsValidIndex(to))
			//if (from is { X: >= 0 and < STUFF_WIDTH, Y: >= 0 and < STUFF_HEIGHT } &&
			//	to is { X: >= 0 and < STUFF_WIDTH, Y: >= 0 and < STUFF_HEIGHT })
			{

				var stuffSource = World[from.X][from.Y];
				var stuffTarget = World[to.X][to.Y];
				// if not stuff at target fall to here and finish
				if (stuffTarget == null)
				{
					// update world
					World[to.X][to.Y] = stuffSource;
					World[from.X][from.Y] = null;

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

	[Obsolete("Use a more performant update method like UpdateInSixths()")]
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
					var targetStuff = World[xIndex][yIndex];
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

	private int updateCounter = 5;
	public void UpdateInSixths()
	{
		// get the subrid coords for a 6th of the World
		if (updateCounter < _subGrids.Count - 1)
		{
			updateCounter++;
		}
		else 
		{
			updateCounter = 0;
			
			_subGrids = _subGrids.OrderBy(((Point bottomLeft, Point topRight) points) => _random.Next(-1, 2)).ToList();
		}
		var subgrid = _subGrids[updateCounter];

		var p = new Point();
		var ltr = true;

		var width = subgrid.topRight.X - subgrid.bottomLeft.X;
		var height = subgrid.topRight.Y - subgrid.bottomLeft.Y;

		try
		{
			var xIndex = 0;
			for (var yIndex = subgrid.bottomLeft.Y /*+ updateCounter*/; yIndex < subgrid.topRight.Y; yIndex++/* += (updateCounter + 1)*/)
			{
				for (int i = 0; i < width; i++)
				{
					p.X = xIndex;
					p.Y = yIndex;

					// get stuff here
					var targetStuff = World[xIndex][yIndex];
					// if nothing here then move on to next Stuff
					if (targetStuff != null && !targetStuff.MovedThisUpdate)
					{
						ApplyGravity(xIndex, yIndex);
					}

					// at end in/decrement
					xIndex += ltr ? 1 : -1;
				}


				//var j = 0;

				//for (var xIndexSource = subgrid.bottomLeft.X; xIndexSource < subgrid.topRight.X; xIndexSource++)
				//{
				//	//==========================================================||
				//	// we adjust the x index depending if we're going
				//	//	left to right (ltr) => 0 to last index
				//	//	right to left (rtl) => last index to 0
				//	//==========================================================||

				//	int xIndex = ltr ? xIndexSource : xIndexSource + width - j;

				//	p.X = xIndex;
				//	p.Y = yIndex;

				//	// get stuff here
				//	var targetStuff = World[xIndex][yIndex];
				//	// if nothing here then move on to next Stuff
				//	if (targetStuff == null || targetStuff.MovedThisUpdate) continue;

				//	ApplyGravity(xIndex, yIndex);

				//	j++;
				//}

				// flip the direction of the next horizontal traversal - for reasons
				ltr = !ltr;
			}

			////for (var yIndex = subgrid.bottomLeft.Y; yIndex < subgrid.topRight.Y; yIndex++)
			////{
			////	for (var xIndexSource = subgrid.bottomLeft.X; xIndexSource < subgrid.topRight.X; xIndexSource++)
			////	{
			////		//==========================================================||
			////		// we adjust the x index depending if we're going
			////		//	left to right (ltr) => 0 to last index
			////		//	right to left (rtl) => last index to 0
			////		//==========================================================||

			////		int xIndex = ltr ? xIndexSource : STUFF_WIDTH - 1 - xIndexSource;

			////		p.X = xIndex;
			////		p.Y = yIndex;

			////		// get stuff here
			////		var targetStuff = World[xIndex][yIndex];
			////		// if nothing here then move on to next Stuff
			////		if (targetStuff == null || targetStuff.MovedThisUpdate) continue;

			////		ApplyGravity(xIndex, yIndex);
			////	}

			////	// flip the direction of the next horizontal traversal - for reasons
			////	ltr = !ltr;
			////}
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


		foreach (var i in World)
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
			for (var xIndex = 0; xIndex < World.Length; xIndex++)
			{
				for (var yIndex = 0; yIndex < World[xIndex].Length; yIndex++)
				{
					// get stuff here
					var targetStuff = World[xIndex][yIndex];
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

	//#region StuffWorld preconfigured setups for dev testing
	//public void PrepareWaterBottom3Y()
	//{
	//	for (int x = 0; x < _world.Length; x++)
	//	{
	//		for (int y = 0; y < 3 && y < _world[x].Length; y++)
	//		{
	//			SafeAddStuffIfEmpty(Stuffs.BASIC_WATER, x, y);
	//		}
	//	}
	//}

	//public void PrepareWaterBottomHalf()
	//{
	//	for (int x = 0; x < _world.Length; x++)
	//	{
	//		for (int y = 0; y < _world[x].Length / 2; y++)
	//		{
	//			SafeAddStuffIfEmpty(Stuffs.BASIC_WATER, x, y);
	//		}
	//	}
	//}
	//#endregion

	#endregion

}
