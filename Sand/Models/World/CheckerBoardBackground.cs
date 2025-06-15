using FlatRedBall;
using FlatRedBall.Graphics;
using Microsoft.Xna.Framework.Graphics;
using static Sand.Constants;

namespace Sand;

public class CheckerBoardBackground : IDrawableBatch
{
	private Sprite _sprite;
	public CheckerBoardBackground()
	{

		var texture = FlatRedBallServices.Load<Texture2D>("Content/TextureImages/Checkerboard_BG_8x8.png");

		_sprite = SpriteManager.AddSprite(texture);
		_sprite.TextureAddressMode = TextureAddressMode.Wrap;
		_sprite.TextureScale = 1;
		_sprite.Width = RESOLUTION_X;
		_sprite.Height = RESOLUTION_Y;
		_sprite.Z = Z_IND_BACKGROUND;
		_sprite.Visible = SHOW_BACKGROUND;

		_sprite.X = 0;
		_sprite.Y = 0;
		_sprite.LeftTextureCoordinate = 0;
		_sprite.RightTextureCoordinate = RESOLUTION_X;
		_sprite.TopTextureCoordinate = 0;
		_sprite.BottomTextureCoordinate = RESOLUTION_Y;

		SpriteManager.AddDrawableBatch(this);
	}

	public float X => 0;
	public float Y => 0;
	public float Z => 0;
	public bool UpdateEveryFrame => true;
	public void Destroy() { }
	public void Draw(Camera camera) { }
	public void Update() { }
}

