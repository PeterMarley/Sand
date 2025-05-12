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

	public int DormantChecks { get; private set; }
	private bool _dormant = false;
	public bool Dormant //{ get; set; }
	{ 
		get 
		{
			DormantChecks++;
			return _dormant;
		}
		set 
		{
			DormantChecks = 0;
			_dormant = value;
		}
	}
	public string Name { get; init; }
	public string Notes { get; init; }
	public Phase Phase { get; private set; }
	public int Version { get; init; }

	private Color _color;
	public Color Color 
	{
		get => Dormant ? (Phase == Phase.Liquid ? Color.Magenta : Color.Gold) : _color;
		private set => _color = value;
	}

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
