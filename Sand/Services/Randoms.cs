using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sand.Services;

public class Randoms
{
	#region Singleton
	private static Randoms _instance;
	public static Randoms Instance => _instance ??= new();
	private Randoms() { Refresh(); }
	#endregion

	private Random _random = new ();
	/// <summary>
	/// This array can be used to get a random order of -1, 0 and 1 
	/// for chosing random left or right directions
	/// </summary>
	public int[] Ind_leftRightMid { get; private set; } = [-1, 0, 1];

	/// <summary>
	/// Regenerate all random sequences for this Update run through
	/// </summary>
	public void Refresh() 
	{
		Ind_leftRightMid = Ind_leftRightMid.OrderBy(x => _random.Next(2)).ToArray();
	}
}
