using Microsoft.Xna.Framework;

namespace Sand;

public readonly struct WorldCoords(
	Vector2 mouseRelative,
	Vector2 camCentre,
	Vector2 camBottomLeft,
	Vector2 camOffset,
	Vector2 camTopRight,
	Vector2 mouseAbsolute)
{
	public readonly Vector2 MouseRelative = mouseRelative;
	public readonly Vector2 MouseAbsolute = mouseAbsolute;

	public readonly Vector2 CameraCentre = camCentre;
	public readonly Vector2 CameraBottomLeft = camBottomLeft;
	public readonly Vector2 CameraOffset = camOffset;
	public readonly Vector2 CameraTopRight = camTopRight;
}
