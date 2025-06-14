namespace Sand;

public readonly struct PlayerCollisionResult(
	bool allowUp,
	bool allowRight,
	bool allowDown,
	bool allowLeft,
	bool inLiquid,
	int moveFactor,
	int gravMagnitude,
	int moveLeftOffsetY,
	int moveRightOffsetY
)
{
	public readonly bool AllowUp = allowUp;
	public readonly bool AllowRight = allowRight;
	public readonly bool AllowDown = allowDown;
	public readonly bool AllowLeft = allowLeft;

	public readonly bool InLiquid = inLiquid;

	public readonly int MoveFactor = moveFactor;
	public readonly int GravMagnitude = gravMagnitude;

	public readonly int MoveLeftOffsetY = moveLeftOffsetY;
	public readonly int MoveRightOffsetY = moveRightOffsetY;
}
