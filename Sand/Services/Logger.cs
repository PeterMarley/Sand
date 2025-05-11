using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using static Sand.Constants;

namespace Sand;

public class Logger : IDisposable
{

	[DllImport("kernel32.dll")]
	static extern bool AllocConsole();

	#region File Log
	private ConcurrentQueue<string> _logStrings = [];
	private StreamWriter _logFileWriter;
	#endregion

	#region Singleton
	private static Logger _instance = new();
	public static Logger Instance
	{
		get
		{
			return _instance;
		}
	}
	#endregion

	private const string LOG_TEMPLATE__INFO = "[{0}] (INFO) {1} {2}";
	private const string LOG_TEMPLATE__WARN = "[{0}] (WARN) {1} {2}";
	private const string LOG_TEMPLATE__ERROR = "[{0}] (ERROR) {1} {2}\nExMessage=\"{3}\"\nExStackTrace=\n{4}";
	private const string DATE_FORMAT = "yy-MM-dd_HH:mm:ss.ffff";

	private Logger()
	{
		try
		{
#pragma warning disable CS0162 // Unreachable code detected
			if (LOG_TO_CONSOLE)
			{
				AllocConsole();
			}
#pragma warning restore CS0162 // Unreachable code detected

			var path = AppContext.BaseDirectory;
			var filename = $"SandLog_{DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified).ToString("yy-MM-dd_HH-mm-ss-ffff")}.log";
			var logDir = Path.Combine(path, "SandLogs");
			if (!Directory.Exists(logDir))
			{ 
				Directory.CreateDirectory(logDir);
			}
			var combined = Regex.Replace(Path.Combine(logDir, filename), "[^a-zA-Z0-9\\\\.:\\-_]", string.Empty);
			var filestream = File.OpenWrite(combined);
			_logFileWriter = new StreamWriter(filestream);
	}
		catch (Exception ex)
		{
			LogInfo($"Failed to create file log = {ex.Message}");
			throw;
		}
		
		LogInfo("Logger initialised");
	}

	#region Public API

	public void LogInfo(string message, [CallerMemberName] string caller = "", [CallerFilePath] string filePath = "")
	{
		//string loggedCaller;
		//if (filePath.Length > 0)
		//{
		//	loggedCaller = filePath.Split(System.IO.Path.DirectorySeparatorChar).LastOrDefault();
		//}
		//else
		//{
		//	loggedCaller = caller;
		//}
		//var logString = string.Format(LOG_TEMPLATE__INFO, GetDateStr(), loggedCaller, message);
		//LogInternal(logString);
		//if (LOG_TO_FILE)
		//{
		//	_logStrings.Enqueue(logString);
		//	if (_logStrings.Count > 5)
		//	{
		//		DrumpStringsToFile();
		//	}
		//}
	}
	public void LogWarning(string message, [CallerMemberName] string caller = "", [CallerFilePath] string filePath = "")
	{
		//string loggedCaller;
		//if (filePath.Length > 0)
		//{
		//	loggedCaller = filePath.Split(System.IO.Path.DirectorySeparatorChar).LastOrDefault();
		//}
		//else
		//{
		//	loggedCaller = caller;
		//}
		//var logString = string.Format(LOG_TEMPLATE__WARN, GetDateStr(), loggedCaller, message);
		//LogInternal(logString);
		//if (LOG_TO_FILE)
		//{
		//	_logStrings.Enqueue(logString);
		//	if (_logStrings.Count > 5)
		//	{
		//		DrumpStringsToFile();
		//	}
		//}
	}
	public void LogError(Exception ex, string message, [CallerMemberName] string caller = "", [CallerFilePath] string filePath = "")
	{
		//string loggedCaller = $"{(filePath.Length > 0 ? filePath.Split(System.IO.Path.DirectorySeparatorChar).LastOrDefault() : string.Empty)} {caller}";

		//var logString = string.Format(LOG_TEMPLATE__ERROR,
		//	GetDateStr(),
		//	loggedCaller,
		//	message,
		//	ex.Message,
		//	ex.StackTrace
		//);

		//LogInternal(logString);
		//if (LOG_TO_FILE)
		//{
		//	_logStrings.Enqueue(logString);
		//	if (_logStrings.Count > 5)
		//	{
		//		DrumpStringsToFile();
		//	}
		//}
	}
	public void Dispose()
	{
		if (LOG_TO_FILE)
		{
			DrumpStringsToFile();
		}
		_logFileWriter.Dispose();
	}
	
	#endregion

	#region Private API

	private bool _currentlyDumpingStrings = false;
	private void DrumpStringsToFile() 
	{
		//if (_currentlyDumpingStrings) return;

		//_currentlyDumpingStrings = true;
		//var sb = new StringBuilder();

		//var iTerminate = _logStrings.Count;
		//for (var i = 0; i < iTerminate; i++)
		//{
		//	_logStrings.TryDequeue(out string str);
		//	if (str != null)
		//	{
		//		sb.AppendLine(str);
		//	}
		//}
		//_logFileWriter.Write(sb.ToString());

		//_logFileWriter.Flush();
		//sb.Clear();
		//_currentlyDumpingStrings = false;
	}
	private static string GetDateStr() => DateTime.Now.ToString(DATE_FORMAT);
	private static void LogInternal(string message)
	{
		System.Diagnostics.Debug.WriteLine(message);
		Console.WriteLine(message);
	}

	#endregion




}
