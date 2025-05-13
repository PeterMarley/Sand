using AsepriteDotNet;
using FlatRedBall;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sprite = FlatRedBall.Sprite;
using static Sand.Constants;

namespace Sand;

public class Player
{
	public Texture2D Texture { get; private set; }
	public Sprite Sprite { get; private set; }
	public int StuffHeight => height / Constants.STUFF_SCALE;
	public int StuffWidth => width / Constants.STUFF_SCALE;

	private const int width = 100;
	private const int height = 100;

	public Player()
	{
		Texture = new Texture2D(FlatRedBallServices.GraphicsDevice, width, height);
		var a = new Color[width * height];
		Array.Fill(a, Color.OliveDrab);
		Texture.SetData(a);
		Sprite = SpriteManager.AddSprite(Texture);
		Sprite.X = RESOLUTION_X / 2;
		Sprite.Y = RESOLUTION_Y / 2;
		Sprite.Z = 1;
		Sprite.TextureScale = 1;
	}
}
