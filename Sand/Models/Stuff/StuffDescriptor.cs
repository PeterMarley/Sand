using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Sand.Models.Stuff;
public struct StuffDescriptor
{
	public StuffDescriptor() 
	{
		//if (ColorsSource != null && ColorsSource.Count > 0)
		//{ 
		//	Colors = new Color[ColorsSource.Count];
		//	for (var i = 0; i < ColorsSource.Count; i++)
		//	{
		//		var rgba = ColorsSource[i];

		//		var c = new Color
		//		{
		//			R = rgba[0],
		//			G = rgba[1],
		//			B = rgba[2],
		//			A = rgba.Length >= 3 ? rgba[3] : (byte)1
		//		};

		//		Colors[i] = c;
		//	}
		//}

		//for (int i = 0; i < ColorsSource.Count; i++)
		//{	
		//	var csRaw = ColorsSource[i];
		//	ColorsSourceParsed = new Color(csRaw[0], csRaw[1], csRaw[2], csRaw[3]);
		//}

	}
	public string Name { get; set; }
	public int Version { get; set; }
	public string Phase { get; set; }
	public string SpriteSource { get; set; }
	public Color[] Colors { get; set; } = [Color.Magenta];
	public List<byte[]> ColorsSource = [];
	//public List<byte[]> ColorsSourceParsed = [];
	public string ColorRotation { get; set; } = "OnCreate";
	public string Notes { get; set; }
}
