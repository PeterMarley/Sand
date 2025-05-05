using System;
namespace Sand.Stuff;
public interface IStuff
{
	Guid Id { get; }
	bool MovedThisUpdate { get; set; }
	IStuff SetPosition(int x, int y);
	void ApplyGravity(IStuff[][] world, int xDest, int yDest);
}
