using FlatRedBall;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Sand;

public class SpriteService
{
	#region singleton
	private static SpriteService _instance;
	public static SpriteService Instance => _instance ??= new();
	#endregion

	private readonly Texture2D _white = FlatRedBallServices.Load<Texture2D>("Content/TextureImages/WhitePixel.png");
	private readonly Texture2D _cyan = FlatRedBallServices.Load<Texture2D>("Content/TextureImages/CyanPixel.png");
	private readonly Texture2D _red = FlatRedBallServices.Load<Texture2D>("Content/TextureImages/RedPixel.png");
	private readonly Texture2D[] _textureArray;

	private readonly Texture2D _sand01 = FlatRedBallServices.Load<Texture2D>("Content/TextureImages/SandPixel_01_e3d292.png");
	private readonly Texture2D _sand02 = FlatRedBallServices.Load<Texture2D>("Content/TextureImages/SandPixel_02_daca7b.png");
	private readonly Texture2D _sand03 = FlatRedBallServices.Load<Texture2D>("Content/TextureImages/SandPixel_03_c8b969.png");
	private readonly Texture2D _sand04 = FlatRedBallServices.Load<Texture2D>("Content/TextureImages/SandPixel_04_bfae5f.png");
	private readonly Texture2D _sand05 = FlatRedBallServices.Load<Texture2D>("Content/TextureImages/SandPixel_05_a6974b.png");
	private readonly Texture2D _sand06 = FlatRedBallServices.Load<Texture2D>("Content/TextureImages/SandPixel_06_91823e.png");
	private readonly Texture2D _sand07 = FlatRedBallServices.Load<Texture2D>("Content/TextureImages/SandPixel_07_7f7236.png");
	private readonly Texture2D[] _sandTextures;

	private readonly Texture2D _water = FlatRedBallServices.Load<Texture2D>("Content/TextureImages/WaterPixel_01_2c4847.png");

	private Random _random = new();

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
	public Texture2D RandomTextureFrom(Texture2D[] textures)
	{
		return textures[_random.Next(0, textures.Length)];
	}
	private SpriteService()
	{
		_textureArray = [_white, _cyan, _red];
		_sandTextures = [_sand01, _sand02, _sand03, _sand04, _sand05, _sand06, _sand07];
	}

	//#region Sprite Gets

	////======================================================================
	//// Do not rename these are they are referred to by the Material YAMLs
	//// and invoked by reflection targeting the method names
	////======================================================================

	//public Sprite GetRandomDebugSprite()
	//{
	//	var t = _textureArray[_NextColourIndex()];
	//	var sprite = SpriteManager.AddSprite(t);
	//	sprite.TextureScale = Constants.STUFF_SCALE;
	//	return sprite;
	//}

	//public Sprite GetRandomSandStuffSprite()
	//{
	//	var sprite = SpriteManager.AddManualSprite(RandomTextureFrom(_sandTextures));
	//	sprite.TextureScale = Constants.STUFF_SCALE;
	//	return sprite;
	//}

	//public Sprite GetWaterStuffSprite()
	//{
	//	var sprite = SpriteManager.AddSprite(_water);
	//	sprite.TextureScale = Constants.STUFF_SCALE;
	//	return sprite;
	//}

	//#endregion
}
