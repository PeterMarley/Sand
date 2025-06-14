using System.Collections.Generic;
using System.Drawing;
using static Sand.Constants;

namespace Sand;

public class Chunk 
{
	private readonly int _width;
	private readonly int _height;
	private readonly Point _origin;

	/// <summary><code>
	/// /=====================>
	/// \=====================\
	/// /=====================/
	/// \=====================\
	/// >=====================/
	/// </code></summary>
	public List<Point> Points_BottomLeftToTopRight_AlternatingRowDirection { get; private init; }


	public Chunk(Point origin, int chunkWidth, int chunkHeight)
	{
		_width = chunkWidth;
		_height = chunkHeight;
		_origin = origin;


		//=======================================================
		// Pre-Generate useful lists of points
		//=======================================================

		var leftToRight = true;

		Points_BottomLeftToTopRight_AlternatingRowDirection = new List<Point>(chunkWidth * chunkHeight);
		for (int y = origin.Y; y < origin.Y + chunkHeight; y++)
		{
			// switching horizontal scan direction for each row
			if (leftToRight)
			{
				for (int x = origin.X; x < origin.X + chunkWidth + chunkWidth; x++)
				{
					if (x >= 0 && x < STUFF_CELL_WIDTH && y >= 0 && y < STUFF_CELL_HEIGHT)
					{
						Points_BottomLeftToTopRight_AlternatingRowDirection.Add(new Point(x, y));
					}
				}
			}
			else
			{
				for (int x = origin.X + chunkWidth; x >= origin.X; x--)
				{
					if (x >= 0 && x < STUFF_CELL_WIDTH && y >= 0 && y < STUFF_CELL_HEIGHT)
					{
						Points_BottomLeftToTopRight_AlternatingRowDirection.Add(new Point(x, y));
					}
				}
			}

		}
	}
}
