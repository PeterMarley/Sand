using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Point = System.Drawing.Point;
using static Sand.Constants;
using FlatRedBall.Math.Geometry;

namespace Sand.Services;

public class CoordManager
{
	/// <summary>Convert a <see cref="Vector2">Vector2 World Coordinate</see>  into a <see cref="Sand.SandCoordinate"/></summary>
	public static SandCoordinate Convert(Vector2 worldCoord)
	{
		int? chunkIndex = null;
		Point? stuffPosition = null;
		var stuffCells = SandGame.World.StuffCells;
		//======================================================
		// GET THE CHUNK INDEX OF THIS WORLD COORDINDATE
		//======================================================

		for (int i = 0; i < stuffCells.Length; i++)
		{
			var chunk = stuffCells[i];
			if (worldCoord.X >= chunk.Left && worldCoord.X <= chunk.Right)
			{
				if (worldCoord.Y >= chunk.Bottom && worldCoord.Y <= chunk.Top)
				{
					chunkIndex = i;
					break;
				}
			}
		}

		//======================================================
		// GET THE STUFF RELATIVE COORDINATE INSIDE THE CELL
		//======================================================

		if (chunkIndex.HasValue)
		{
			var offset = stuffCells[chunkIndex.Value].Offset;

			// get absolute coordinate or cell origin (bottom left)
			Point cellOrigin = new(offset * RESOLUTION_X, 0);

			// get the internal cell stuff coord of the world coord arg
			stuffPosition = new Point(
				(int)((worldCoord.X - cellOrigin.X) / STUFF_TO_PIXEL_SCALE),
				(int)((worldCoord.Y - cellOrigin.Y) / STUFF_TO_PIXEL_SCALE)
			);
		}

		//======================================================
		// BUNDLE AND RETURN
		//======================================================

		return new SandCoordinate()
		{
			ChunkIndex = chunkIndex ?? throw new InvalidOperationException("chunkIndex not found"),
			StuffPosition = stuffPosition ?? throw new InvalidOperationException("stuffPosition not found")
		};
	}
	/// <summary>Get two opposing corners, describes all corners and edges</summary>
	public static (SandCoordinate bottomLeft, SandCoordinate topRight) Corners(AxisAlignedRectangle rectangle)
	{

		var adjX = rectangle.Width / 2;
		var adjY = rectangle.Height / 2;

		return new(
			Convert(new Vector2(rectangle.X - adjX, rectangle.Y - adjY)),
			Convert(new Vector2(rectangle.X + adjX, rectangle.Y + adjY))
		);
	}
}
