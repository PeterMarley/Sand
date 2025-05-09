using System;
using Sprite = FlatRedBall.Sprite;
using static Sand.Constants;
using Sand.Game;
using FlatRedBall;

namespace Sand.Stuff;

/// <summary>
/// Describes a world "pixel", isnt a pixel really, but its a pixel scaled up as per configuration <see cref="Constants.STUFF_SCALE"/>.
/// 
/// Finaliser removes from SpriteManage so fugeddaboudit
/// </summary>
public class StuffBasic
{
	protected Random _random = new();
	protected Sprite _sprite;

	public Guid Id { get; init; } = Guid.NewGuid();
	public bool MovedThisUpdate { get; set; }
	public string Name { get; init; }
	public string Notes { get; init; }
	public Phase Phase { get; private set; }
	public int Version { get; init; }

	//protected abstract float _FreezingPoint { get; set; }
	//protected abstract float _BoilingPoint { get; set; }
	//protected abstract bool _DoesSublimate { get; set; }

	public StuffBasic(Phase phase)
	{
		Phase = phase;
	}

	/// <summary>
	/// Finaliser, ensures garbage collector remove sprite from scene
	/// </summary>
	~StuffBasic()
	{
		this._sprite.Visible = false;
		SpriteManager.RemoveSprite(this._sprite);
	}

	public StuffBasic(StuffDescriptor descriptor)
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

		try
		{
			_sprite = (Sprite) (SpriteService.Instance.GetType()
				.GetMethod(descriptor.SpriteSource)
				.Invoke(SpriteService.Instance, null));
		}
		catch (Exception ex)
		{
			Logger.Instance.LogError(ex, "Failed to automatically get sprite using MaterialDescriptor.SpriteSource and reflection. Using random debug sprite.");
			_sprite = SpriteService.Instance.GetRandomDebugSprite();
			throw;
		}
	}

	public StuffBasic SetPosition(int x, int y)
	{
		_sprite.X = x * STUFF_SCALE + _sprite.Width / 2;
		_sprite.Y = y * STUFF_SCALE + _sprite.Height / 2;
		return this;
	}


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
