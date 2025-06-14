using System;
using static Sand.Constants;
using Microsoft.Xna.Framework;
using Sand.Models.Stuff;
using FlatRedBall;

namespace Sand;

/// <summary>
/// Describes a world "pixel", isnt a pixel really, but its a pixel scaled up as per configuration <see cref="STUFF_TO_PIXEL_SCALE"/>.
/// 
/// Finaliser removes from SpriteManage so fugeddaboudit
/// </summary>
public class Stuff
{
	public int DormantChecks { get; private set; }

	/// <summary>Checks dormancy, and if dormant <see cref="Stuff">Stuff</see> checked a certain number of times then becomes active</summary>
	public bool CheckDormancy() 
	{
		DormantChecks++;
		if (DormantChecks > 9 && TimeManager.CurrentFrame % DormantChecks > (DormantChecks - 1) / 2)
		{
			_dormant = false;
		}
		return _dormant;
	}

	private bool _dormant = false;
	public bool Dormant
	{ 
		get =>_dormant;
		set
		{
			if (value != _dormant)
			{
				DormantChecks = 0;
			}
			_dormant = value;
		}
	}

	public string Name { get; init; }
	public string Notes { get; init; }
	public Phase Phase { get; private set; }
	public int Version { get; init; }

	private Color DormancyColor => Phase switch
	{
		Phase.Solid => Color.Green,
		Phase.Powder => Color.Yellow,
		Phase.Liquid => Color.Magenta,
		Phase.Gas => Color.Cyan,
		_ => Color.White,
	};

	private int ColorIndex = 0;
	private Color _color;
	public Color Color 
	{
		get
		{
			switch (this.descriptor.ColorRotation)
			{
				case "OnCreate":
				default:
					return Dormant && SHOW_STUFF_DORMANCY_COLORS ? DormancyColor : _color;
				case "OnDraw":
					if (Dormant && SHOW_STUFF_DORMANCY_COLORS)
					{
						return DormancyColor;
					}
					ColorIndex++;
					if (ColorIndex >= this.descriptor.Colors.Length)
					{
						ColorIndex = 0;
					}
					
					return this.descriptor.Colors[ColorIndex];
			}
		}
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

		Color = descriptor.Colors[Randoms.Random.Next(descriptor.Colors.Length)];

		if (!Enum.TryParse(descriptor.Phase, true, out Phase phase))
		{
			Logger.Instance.LogWarning($"failed to parse the phase string from material descriptor: descriptor.Phase={descriptor.Phase}");
			phase = Phase.Solid;
		}
		Phase = phase;
	}
	
}
