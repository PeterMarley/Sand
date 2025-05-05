using FlatRedBall;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Sand.Constants;

namespace Sand.Game;

public class SpriteService
{
	#region singleton
	private static SpriteService _instance;
	public static SpriteService Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new SpriteService();
			}
			return _instance;
		}
	}
	#endregion

	private readonly Texture2D _white = FlatRedBallServices.Load<Texture2D>("Content/TextureImages/WhitePixel.png");
	private readonly Texture2D _cyan = FlatRedBallServices.Load<Texture2D>("Content/TextureImages/CyanPixel.png");
	private readonly Texture2D _red = FlatRedBallServices.Load<Texture2D>("Content/TextureImages/RedPixel.png");
	private readonly Texture2D[] _textureArray;

	private int _lastUsedColourIndex = 0;
	private int _NextColourIndex()
	{
		_lastUsedColourIndex++;
		if (_lastUsedColourIndex > _textureArray.Length - 1)
		{
			_lastUsedColourIndex = 0;
		}
		return _lastUsedColourIndex;
	}
	private SpriteService()
	{
		_textureArray = [_white, _cyan, _red];
	}

	public Sprite GetNextStuffSprite() 
	{
		var t = _textureArray[_NextColourIndex()];
		var sprite = SpriteManager.AddSprite(t);
		return sprite;
	}

	//public Texture2D Blue => _blue;
}
