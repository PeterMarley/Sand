namespace Sand;

public readonly struct Edges<T>(
	T top,
	T right,
	T bottom,
	T left)
{
	public readonly T Top = top;
	public readonly T Right = right;
	public readonly T Bottom = bottom;
	public readonly T Left = left;
}