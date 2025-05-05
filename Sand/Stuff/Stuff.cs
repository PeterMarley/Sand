using FlatRedBall;
using Microsoft.Xna.Framework.Graphics;
using System;
using Sprite = FlatRedBall.Sprite;
using static Sand.Constants;
using Point = System.Drawing.Point;
namespace Sand.Stuff;



public abstract class Stuff : IStuff
{
	private readonly Guid _id = Guid.NewGuid();
	public Guid Id => _id;

	public bool MovedThisUpdate { get; set; }

	protected Random _random = new();

	protected Sprite _sprite;
	protected Phase _phase;

	protected abstract float _FreezingPoint { get; set; }
	protected abstract float _BoilingPoint { get; set; }
	protected abstract bool _DoesSublimate { get; set; }


	public Stuff(Phase phase)
	{
		_phase = phase;
	}

	public IStuff SetPosition(int x, int y)
	{
		_sprite.X = x * STUFF_SCALE + _sprite.Width / 2;
		_sprite.Y = y * STUFF_SCALE + _sprite.Height / 2;
		return this;
	}

	private void ApplyGravityPhaseSolid(IStuff[][] world, int xIndex, int yIndex)
	{
		//---------------------------------------------------
		//Check 2 spots below left and right, if all are filled then move on
		//---------------------------------------------------

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

	private void ApplyGravityPhaseLiquid(IStuff[][] world, int xIndex, int yIndex)
	{
		//---------------------------------------------------
		//Check 2 spots below left and right, if all are filled then move on
		//---------------------------------------------------

		// if bottom row outside array range just continue as this Stuff cant fall anywhere
		var rowBelowIndex = yIndex - 1;
		if (rowBelowIndex < 0) return;

		// check directly below
		if (world.Move(new(xIndex, yIndex), new(xIndex, rowBelowIndex)))
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

		// check direct lateral movements
		for (var lateralGravAttempts = 2; lateralGravAttempts > 0; lateralGravAttempts--)
		{
			if (leftSide)
			{
				// check below and left
				if (colLeftIndex >= 0 && world.Move(new(xIndex, yIndex), new(colLeftIndex, xIndex)))
				{
					break;
				}
			}
			else
			{
				// check below and right
				if (colRightIndex < STUFF_WIDTH && world.Move(new(xIndex, yIndex), new(colRightIndex, xIndex)))
				{
					break;
				}
			}
			leftSide = !leftSide;
		}
	}

	public void ApplyGravity(IStuff[][] world, int xIndex, int yIndex)
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
