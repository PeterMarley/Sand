using System;
using static Sand.Constants;
using Microsoft.Xna.Framework;

namespace Sand;

/// <summary>
/// Describes a world "pixel", isnt a pixel really, but its a pixel scaled up as per configuration <see cref="STUFF_SCALE"/>.
/// 
/// Finaliser removes from SpriteManage so fugeddaboudit
/// </summary>
public class Stuff
{
	public static Random _random = new();

	public bool Dormant { get; set; } = false;
	public string Name { get; init; }
	public string Notes { get; init; }
	public Phase Phase { get; private set; }
	public int Version { get; init; }
	public Color Color { get; private set; }

	/// <summary>This many updates have occured since this Stuff Moved</summary>
	public int NotMovedCount { get; set; }

	private StuffDescriptor descriptor;

	public Stuff(Phase phase)
	{
		Phase = phase;
	}

	public Stuff(StuffDescriptor descriptor)
	{
		this.descriptor = descriptor;

		Name = descriptor.Name;
		Notes = descriptor.Notes;
		Version = descriptor.Version;

		Color = descriptor.Colors[_random.Next(descriptor.Colors.Length)];

		if (!Enum.TryParse(descriptor.Phase, true, out Phase phase))
		{
			Logger.Instance.LogWarning($"failed to parse the phase string from material descriptor: descriptor.Phase={descriptor.Phase}");
			phase = Phase.Solid;
		}
		Phase = phase;
	}
	
}
