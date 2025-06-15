using System;

namespace Sand;

public class FastRandomBoolGenerator
{

	private static FastRandomBoolGenerator _instance;
	public readonly static FastRandomBoolGenerator Instance = _instance ??= new();

	private readonly Random _random = new ();
	private int _bitBuffer;
	private int _bitsRemaining;

	public bool Next()
	{
		if (_bitsRemaining == 0)
		{
			_bitBuffer = _random.Next();
			_bitsRemaining = 32;
		}

		bool result = (_bitBuffer & 1) == 1;
		_bitBuffer >>= 1;
		_bitsRemaining--;
		return result;
	}
}