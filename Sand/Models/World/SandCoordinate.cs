using System.Drawing;

namespace Sand;

public readonly struct SandCoordinate
{
	public int ChunkIndex { get; init; }
	public Point StuffPosition { get; init; }
}
