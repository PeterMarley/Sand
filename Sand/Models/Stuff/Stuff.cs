using System;
using static Sand.Constants;
using Microsoft.Xna.Framework;
using Sand.Models.Stuff;
using FlatRedBall;

namespace Sand;

/// <summary>
/// Describes a world "pixel", isnt a pixel really, but its a pixel scaled up as per configuration <see cref="STUFF_SCALE"/>.
/// 
/// Finaliser removes from SpriteManage so fugeddaboudit
/// </summary>
public class Stuff
{
	public int DormantChecks { get; private set; }

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
	public bool Dormant //{ get; set; }
	{ 
		get 
		{
			//DormantChecks++;
			return _dormant;
		}
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

	private Color DormancyColor
	{
		get
		{
			switch (Phase)
			{
				case Phase.Solid:
					return Color.Green;
				case Phase.Powder:
					return Color.Yellow;
				case Phase.Liquid:
					return Color.Magenta;
				case Phase.Gas:
					return Color.Cyan;
				default:
					return Color.White;				

			}
		}
	}

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
