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
				using (var sandGame = new SandGame())
				{
					var byEditor = args.Contains("LaunchedByEditor");

					if (byEditor)
					{
						try
						{
							sandGame.Run();
						}
						catch (Exception e)
						{
							System.IO.File.WriteAllText("CrashInfo.txt", e.ToString());
							throw;
						}
					}
					else
					{
						sandGame.Run();
					}

				}
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
