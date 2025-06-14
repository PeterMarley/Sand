using Microsoft.Xna.Framework;

namespace Sand;

public readonly struct WorldCoords(
	Vector2 mouseRelative,
	Vector2 cameraCentre,
	Vector2 cameraBottomLeft,
	Vector2 cameraOffset,
	Vector2 cameraTopRight,
	Vector2 mouseAbsolute)
{
	public readonly Vector2 MouseRelative = mouseRelative;
	public readonly Vector2 MouseAbsolute = mouseAbsolute;

	public readonly Vector2 CameraCentre = cameraCentre;
	public readonly Vector2 CameraBottomLeft = cameraBottomLeft;
	public readonly Vector2 CameraOffset = cameraOffset;
	public readonly Vector2 CameraTopRight = cameraTopRight;
}