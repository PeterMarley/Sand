using System;
using System.Linq;

namespace Sand
{
	/// <summary>
	/// The main class.
	/// </summary>
	public static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{

			try
			{
				using var sandGame = new SandGame();
				sandGame.Run();
			}
			catch (Exception ex)
			{
				Logger.Instance.LogError(ex, "caught in Main()");
			}
			finally 
			{
				Logger.Instance.Dispose();
			}
		}
	}
}
