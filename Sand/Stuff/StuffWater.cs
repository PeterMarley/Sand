using Sand.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sand.Stuff;

public class StuffWater : Stuff
{
	public StuffWater(Phase phase = Phase.Liquid) : base(phase)
	{
		base._sprite = SpriteService.Instance.GetWaterSprite();
	}

	protected override float _FreezingPoint { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
	protected override float _BoilingPoint { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
	protected override bool _DoesSublimate { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
}
