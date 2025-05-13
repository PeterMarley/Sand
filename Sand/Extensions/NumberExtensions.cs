using FlatRedBall;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Sand.Constants;
namespace Sand.Extensions;

public static class NumberExtensions
{
	public static (int top, int right, int bottom, int left) ToStuffCoord(Sprite sprite)
	{
		int spriteGridLeft;// = (int)((Player.Sprite.Left + 1000) / STUFF_SCALE);// - (Player.StuffWidth / 2));// + (RESOLUTION_X / 2);
		int spriteGridRight;// = (int)((Player.Sprite.Right + 1000) / STUFF_SCALE);// - (Player.StuffWidth / 2));// + (RESOLUTION_X / 2);
		int spriteGridTop;// = (int)((Player.Sprite.Top + 1000) / STUFF_SCALE);// - (Player.StuffHeight / 2));// + (RESOLUTION_X / 2);
		int spriteGridBottom;// = (int)((Player.Sprite.Bottom + 1000) / STUFF_SCALE);// - (Player.StuffHeight / 2));// + (RESOLUTION_X / 2);
		var offsetX = sprite.Width / 2;
		var offsetY = sprite.Height / 2;
		//var spriteX = (int)((sprite.X + 1000) / STUFF_SCALE);// - (Player.StuffWidth / 2));// + (RESOLUTION_X / 2);
		//var spriteY = (int)((sprite.Y + 1000) / STUFF_SCALE);// - (Player.StuffWidth / 2));// + (RESOLUTION_X / 2);
		var x = sprite.X;
		var y = sprite.Y;
		spriteGridLeft = (int)((x - offsetX + (RESOLUTION_X / 2)) / STUFF_SCALE);// - (Player.StuffWidth / 2));// + (RESOLUTION_X / 2);
		spriteGridRight = (int)((x + offsetX + (RESOLUTION_X / 2)) / STUFF_SCALE);// - (Player.StuffWidth / 2));// + (RESOLUTION_X / 2);
		spriteGridTop = (int)((y + offsetY + (RESOLUTION_Y / 2)) / STUFF_SCALE);// - (Player.StuffHeight / 2));// + (RESOLUTION_X / 2);
		spriteGridBottom = (int)((y - offsetY + (RESOLUTION_Y / 2)) / STUFF_SCALE);// - (Player.StuffHeight / 2));// + (RESOLUTION_X / 2);


		return (spriteGridTop, spriteGridRight, spriteGridBottom, spriteGridLeft);
	}

	// ie get the x and y of a stuff, and get its sprite coord
	//public static (float top, float right, float bottom, float left) ToWorldCoords(int xStuff, int yStuff)
	//{
	//	int spriteGridLeft;// = (int)((Player.Sprite.Left + 1000) / STUFF_SCALE);// - (Player.StuffWidth / 2));// + (RESOLUTION_X / 2);
	//	int spriteGridRight;// = (int)((Player.Sprite.Right + 1000) / STUFF_SCALE);// - (Player.StuffWidth / 2));// + (RESOLUTION_X / 2);
	//	int spriteGridTop;// = (int)((Player.Sprite.Top + 1000) / STUFF_SCALE);// - (Player.StuffHeight / 2));// + (RESOLUTION_X / 2);
	//	int spriteGridBottom;// = (int)((Player.Sprite.Bottom + 1000) / STUFF_SCALE);// - (Player.StuffHeight / 2));// + (RESOLUTION_X / 2);
	//	var offsetX = sprite.Width / 2;
	//	var offsetY = sprite.Height / 2;
	//	//var spriteX = (int)((sprite.X + 1000) / STUFF_SCALE);// - (Player.StuffWidth / 2));// + (RESOLUTION_X / 2);
	//	//var spriteY = (int)((sprite.Y + 1000) / STUFF_SCALE);// - (Player.StuffWidth / 2));// + (RESOLUTION_X / 2);
	//	var x = sprite.X;
	//	var y = sprite.Y;
	//	spriteGridLeft = (int)((x - offsetX + (RESOLUTION_X / 2)) / STUFF_SCALE);// - (Player.StuffWidth / 2));// + (RESOLUTION_X / 2);
	//	spriteGridRight = (int)((x + offsetX + (RESOLUTION_X / 2)) / STUFF_SCALE);// - (Player.StuffWidth / 2));// + (RESOLUTION_X / 2);
	//	spriteGridTop = (int)((y + offsetY + (RESOLUTION_Y / 2)) / STUFF_SCALE);// - (Player.StuffHeight / 2));// + (RESOLUTION_X / 2);
	//	spriteGridBottom = (int)((y - offsetY + (RESOLUTION_Y / 2)) / STUFF_SCALE);// - (Player.StuffHeight / 2));// + (RESOLUTION_X / 2);


	//	return (spriteGridTop, spriteGridRight, spriteGridBottom, spriteGridLeft);
	//}
}
