using System.Collections.Generic;

namespace Sand.Models;

public struct StuffDescriptor
{
	public string Name { get; set; }
	public int Version { get; set; }
	public string Phase { get; set; }
	public string SpriteSource { get; set; }
	public byte[] ColorRgba { get; set; }
	public string Notes { get; set; }
}
