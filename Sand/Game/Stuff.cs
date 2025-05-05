using FlatRedBall;
using Microsoft.Xna.Framework.Graphics;
using System;
using Sprite = FlatRedBall.Sprite;
using static Sand.Constants;
using Sand.Game;
namespace Sand;

public class Stuff : IStuff
{
	#region Color
	//private static readonly Texture2D _SINGLE_PIXEL_WHITE = FlatRedBallServices.Load<Texture2D>("Content/TextureImages/WhitePixel.png");
	//private static readonly Texture2D _SINGLE_PIXEL_CYAN = FlatRedBallServices.Load<Texture2D>("Content/TextureImages/CyanPixel.png");
	//private static readonly Texture2D _SINGLE_PIXEL_RED = FlatRedBallServices.Load<Texture2D>("Content/TextureImages/RedPixel.png");
	//private static readonly Texture2D _SINGLE_PIXEL_BLUE = new Texture2D(FlatRedBallServices.GraphicsDevice, WIDTH * STUFF_SCALE, HEIGHT * STUFF_SCALE);
	//private static readonly Texture2D[] _P_COLOUR_TEXTURES = [_SINGLE_PIXEL_WHITE, _SINGLE_PIXEL_CYAN, _SINGLE_PIXEL_RED];
	//private static int _lastUsedColourIndex = 0;
	//private static int _NextColourIndex()
	//{
	//		_lastUsedColourIndex++;
	//		if (_lastUsedColourIndex > _P_COLOUR_TEXTURES.Length - 1)
	//		{
	//			_lastUsedColourIndex = 0;
	//		}
	//		return _lastUsedColourIndex;
	//}
	#endregion Color

	#region Id
	private Guid _id = Guid.NewGuid();
	public Guid Id => _id;
	#endregion Id

	private readonly Sprite _sprite;

	public Stuff()
	{
		_sprite = SpriteService.Instance.GetNextStuffSprite();
		_sprite.TextureScale = STUFF_SCALE/*1*//*Constants.WINDOW_SCALE*/;
	}

	public IStuff SetPosition(int x, int y)
	{
		_sprite.X = (x * STUFF_SCALE) /*+ (_sprite.Width / 2)*/;
		_sprite.Y = (y * STUFF_SCALE) /*+ (_sprite.Height / 2)*/;
		return this;
	}

	//public Tuple<int,int> GetPosition()
	//{
	//	return new ((int)(_sprite.X /*/ Constants.WINDOW_SCALE*/),(int)(_sprite.Y /*/ Constants.WINDOW_SCALE*/));
	//}
}