using FlatRedBall;
using Microsoft.Xna.Framework.Graphics;
using System;
using Sprite = FlatRedBall.Sprite;
using static Sand.Constants;
using Sand.Game;
namespace Sand;

public class Stuff : IStuff
{
	#region Id
	private Guid _id = Guid.NewGuid();
	public Guid Id => _id;
	#endregion Id

	private readonly Sprite _sprite;

	//public float Weight { get; private set; } = 1f;

	public Stuff(float weight = 1f)
	{
		//Weight = weight;
		_sprite = SpriteService.Instance.GetNextStuffSprite();
		_sprite.TextureScale = STUFF_SCALE/*1*//*Constants.WINDOW_SCALE*/;
	}

	public IStuff SetPosition(int x, int y)
	{
		_sprite.X = (x * STUFF_SCALE) /*+ (_sprite.Width / 2)*/;
		_sprite.Y = (y * STUFF_SCALE) /*+ (_sprite.Height / 2)*/;
		return this;
	}
}