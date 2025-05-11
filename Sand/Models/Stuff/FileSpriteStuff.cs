using FlatRedBall;
using System;
using Sprite = FlatRedBall.Sprite;
using static Sand.Constants;

namespace Sand;

public class FileSpriteStuff : AbstractStuff
{
	protected Sprite _sprite;

	//public SpriteStuff(Phase phase) : base(phase) { }

	public FileSpriteStuff(StuffDescriptor descriptor) : base(descriptor)
	{
		try
		{
			_sprite = (Sprite)SpriteService.Instance.GetType()
				.GetMethod(descriptor.SpriteSource)
				.Invoke(SpriteService.Instance, null);
		}
		catch (Exception ex)
		{
			Logger.Instance.LogError(ex, "Failed to automatically get sprite using MaterialDescriptor.SpriteSource and reflection. Using random debug sprite.");
			_sprite = SpriteService.Instance.GetRandomDebugSprite();
			throw;
		}
	}

	~FileSpriteStuff()
	{
		_sprite.Visible = false;
		SpriteManager.RemoveSprite(_sprite);
	}

	public override AbstractStuff SetPosition(int x, int y)
	{
		_sprite.X = x * STUFF_SCALE + _sprite.Width / 2;
		_sprite.Y = y * STUFF_SCALE + _sprite.Height / 2;
		SpriteManager.ManualUpdate(_sprite);
		return this;
	}
}
