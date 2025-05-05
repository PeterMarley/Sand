using System;
namespace Sand;
public interface IStuff
{
	Guid Id { get; }
	IStuff SetPosition(int x, int y);
}
