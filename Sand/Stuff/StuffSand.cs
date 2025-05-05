using Sand.Game;
using System;
using static Sand.Constants;

namespace Sand.Stuff;

public class StuffSand : Stuff
{
	public StuffSand(Phase phase = Phase.Solid) : base(phase)
	{
		base._sprite = SpriteService.Instance.GetRandomSandStuffSprite();
	}

	protected override float _FreezingPoint { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
	protected override float _BoilingPoint { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
	protected override bool _DoesSublimate { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
}