using System;
using Sprite = FlatRedBall.Sprite;
using static Sand.Config.Constants;
using FlatRedBall;
using Sand.Services;
using Sand.Config;
using Sand.Stuff.StuffDescriptors;

namespace Sand.Models.Stuff;

/// <summary>
/// Describes a world "pixel", isnt a pixel really, but its a pixel scaled up as per configuration <see cref="STUFF_SCALE"/>.
/// 
/// Finaliser removes from SpriteManage so fugeddaboudit
/// </summary>
public abstract class AbstractStuff
{
	protected Random _random = new();

	public Guid Id { get; init; } = Guid.NewGuid();
	public bool MovedThisUpdate { get; set; }
	public string Name { get; init; }
	public string Notes { get; init; }
	public Phase Phase { get; private set; }
	public int Version { get; init; }

	//protected abstract float _FreezingPoint { get; set; }
	//protected abstract float _BoilingPoint { get; set; }
	//protected abstract bool _DoesSublimate { get; set; }

	public AbstractStuff(Phase phase)
	{
		Phase = phase;
	}

	public AbstractStuff(StuffDescriptor descriptor)
	{
		Name = descriptor.Name;
		Notes = descriptor.Notes;
		Version = descriptor.Version;

		if (!Enum.TryParse(descriptor.Phase, true, out Phase phase))
		{
			Logger.Instance.LogWarning($"failed to parse the phase string from material descriptor: descriptor.Phase={descriptor.Phase}");
			phase = Phase.Solid;
		}
		Phase = phase;
	}

	public abstract AbstractStuff SetPosition(int x, int y);


	////TODO actually these methods shoudl all be in world -- NOT "actually stuffworld should be passed in here, not the underlying data structure"
	//private void ApplyGravityPhaseSolid(StuffBasic[][] world, int xIndex, int yIndex)
	//{
	//	//-----------------------------------------------------------------
	//	//Check 2 spots below left and right, if all are filled then move on
	//	//-----------------------------------------------------------------

	//	// if bottom row outside array range just continue as this Stuff cant fall anywhere
	//	var rowBelowIndex = yIndex - 1;
	//	if (rowBelowIndex < 0) return;

	//	// check directly below
	//	if (world.MoveSolid(new (xIndex, yIndex), new (xIndex, rowBelowIndex)))
	//	{
	//		return;
	//	}

	//	// check below and left (but alterate sides randomly)
	//	bool leftSide = this._random.Next(2) == 1;
	//	int colLeftIndex = xIndex - 1;
	//	int colRightIndex = xIndex + 1;

	//	for (var lateralGravAttempts = 2; lateralGravAttempts > 0; lateralGravAttempts--)
	//	{
	//		if (leftSide)
	//		{
	//			// check below and left
	//			if (colLeftIndex >= 0 && world.MoveSolid(new (xIndex, yIndex), new (colLeftIndex, rowBelowIndex)))
	//			{
	//				break;
	//			}
	//		}
	//		else
	//		{
	//			// check below and right
	//			if (colRightIndex < STUFF_WIDTH && world.MoveSolid(new(xIndex, yIndex), new(colRightIndex, rowBelowIndex)))
	//			{
	//				break;
	//			}
	//		}
	//		leftSide = !leftSide;
	//	}
	//}

	//private void ApplyGravityPhaseLiquid(StuffBasic[][] world, int xIndex, int yIndex)
	//{
	//	//-----------------------------------------------------------------
	//	//Check 2 spots below left and right
	//	//-----------------------------------------------------------------

	//	// if bottom row outside array range just continue as this Stuff cant fall anywhere
	//	var rowBelowIndex = yIndex - 1;
	//	if (rowBelowIndex < 0) return;

	//	// check directly below
	//	if (world.MoveLiquid(new(xIndex, yIndex), new(xIndex, rowBelowIndex)))
	//	{
	//		return;
	//	}
	//	bool leftFirst = this._random.Next(2) == 1;

	//	// check below and left (but alterate sides randomly)
	//	bool leftSide = leftFirst;
	//	int colLeftIndex = xIndex - 1;
	//	int colRightIndex = xIndex + 1;

	//	for (var lateralGravAttempts = 2; lateralGravAttempts > 0; lateralGravAttempts--)
	//	{
	//		if (leftSide)
	//		{
	//			// check below and left
	//			if (colLeftIndex >= 0 && world.MoveLiquid(new(xIndex, yIndex), new(colLeftIndex, rowBelowIndex)))
	//			{
	//				break;
	//			}
	//		}
	//		else
	//		{
	//			// check below and right
	//			if (colRightIndex < STUFF_WIDTH && world.MoveLiquid(new(xIndex, yIndex), new(colRightIndex, rowBelowIndex)))
	//			{
	//				break;
	//			}
	//		}
	//		leftSide = !leftSide;
	//	}

	//	colLeftIndex--;
	//	colRightIndex++;
	//	leftSide = leftFirst;

	//	//-----------------------------------------------------------------
	//	//Check 2 spots directly left and right - represent fluidic flow
	//	//-----------------------------------------------------------------

	//	// check direct lateral movements
	//	for (var lateralFlowAttempts = 2; lateralFlowAttempts > 0; lateralFlowAttempts--)
	//	{
	//		if (leftSide)
	//		{
	//			// check left
	//			if (colLeftIndex >= 0 && world.MoveLiquid(new(xIndex, yIndex), new(colLeftIndex, yIndex)))
	//			{
	//				break;
	//			}
	//		}
	//		else
	//		{
	//			// check right
	//			if (colRightIndex < STUFF_WIDTH && world.MoveLiquid(new(xIndex, yIndex), new(colRightIndex, yIndex)))
	//			{
	//				break;
	//			}
	//		}
	//		leftSide = !leftSide;
	//	}
	//}

	//public void ApplyGravity(StuffBasic[][] world, int xIndex, int yIndex)
	//{
	//	switch (Phase)
	//	{
	//		case Phase.Solid:
	//			ApplyGravityPhaseSolid(world, xIndex, yIndex); 
	//			break;
	//		case Phase.Liquid:
	//			ApplyGravityPhaseLiquid(world, xIndex, yIndex);
	//			break;
	//		case Phase.Gas: 
	//		default:
	//			Logger.Instance.LogInfo($"Phase {Phase} not handled");
	//			break;
	//	}
	//}

}
