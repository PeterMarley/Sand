using System;
using Sprite = FlatRedBall.Sprite;
using static Sand.Constants;
using Sand.Game;

namespace Sand.Stuff;

public class StuffBasic
{
	private readonly Guid _id = Guid.NewGuid();
	protected Random _random = new();
	protected Sprite _sprite;
	protected Phase _phase;

	public Guid Id => _id;
	public bool MovedThisUpdate { get; set; }
	public string Name { get; init; }
	public string Notes { get; init; }
	public int Version { get; init; }

	//protected abstract float _FreezingPoint { get; set; }
	//protected abstract float _BoilingPoint { get; set; }
	//protected abstract bool _DoesSublimate { get; set; }

	public StuffBasic(Phase phase)
	{
		_phase = phase;
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
		_phase = phase;

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

	//TODO actually stuffworld should be passed in here, not the underlying data structure
	private void ApplyGravityPhaseSolid(StuffBasic[][] world, int xIndex, int yIndex)
	{
		//-----------------------------------------------------------------
		//Check 2 spots below left and right, if all are filled then move on
		//-----------------------------------------------------------------

		// if bottom row outside array range just continue as this Stuff cant fall anywhere
		var rowBelowIndex = yIndex - 1;
		if (rowBelowIndex < 0) return;

		// check directly below
		if (world.Move(new (xIndex, yIndex), new (xIndex, rowBelowIndex)))
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
				if (colLeftIndex >= 0 && world.Move(new (xIndex, yIndex), new (colLeftIndex, rowBelowIndex)))
				{
					break;
				}
			}
			else
			{
				// check below and right
				if (colRightIndex < STUFF_WIDTH && world.Move(new(xIndex, yIndex), new(colRightIndex, rowBelowIndex)))
				{
					break;
				}
			}
			leftSide = !leftSide;
		}
	}

	private void ApplyGravityPhaseLiquid(StuffBasic[][] world, int xIndex, int yIndex)
	{
		//-----------------------------------------------------------------
		//Check 2 spots below left and right
		//-----------------------------------------------------------------

		// if bottom row outside array range just continue as this Stuff cant fall anywhere
		var rowBelowIndex = yIndex - 1;
		if (rowBelowIndex < 0) return;

		// check directly below
		if (world.Move(new(xIndex, yIndex), new(xIndex, rowBelowIndex)))
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
				if (colLeftIndex >= 0 && world.Move(new(xIndex, yIndex), new(colLeftIndex, rowBelowIndex)))
				{
					break;
				}
			}
			else
			{
				// check below and right
				if (colRightIndex < STUFF_WIDTH && world.Move(new(xIndex, yIndex), new(colRightIndex, rowBelowIndex)))
				{
					break;
				}
			}
			leftSide = !leftSide;
		}

		colLeftIndex--;
		colRightIndex++;
		leftSide = leftFirst;

		//-----------------------------------------------------------------
		//Check 2 spots directly left and right - represent fluidic flow
		//-----------------------------------------------------------------

		// check direct lateral movements
		for (var lateralGravAttempts = 2; lateralGravAttempts > 0; lateralGravAttempts--)
		{
			if (leftSide)
			{
				// check left
				if (colLeftIndex >= 0 && world.Move(new(xIndex, yIndex), new(colLeftIndex, yIndex)))
				{
					break;
				}
			}
			else
			{
				// check right
				if (colRightIndex < STUFF_WIDTH && world.Move(new(xIndex, yIndex), new(colRightIndex, yIndex)))
				{
					break;
				}
			}
			leftSide = !leftSide;
		}
	}

	public void ApplyGravity(StuffBasic[][] world, int xIndex, int yIndex)
	{
		switch (_phase)
		{
			case Phase.Solid:
				ApplyGravityPhaseSolid(world, xIndex, yIndex); 
				break;
			case Phase.Liquid:
				ApplyGravityPhaseLiquid(world, xIndex, yIndex);
				break;
			case Phase.Gas: 
			default:
				Logger.Instance.LogInfo($"Phase {_phase} not handled");
				break;
		}
	}

}
