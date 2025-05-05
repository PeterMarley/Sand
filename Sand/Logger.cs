using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Sand;

public class Logger
{

	[DllImport("kernel32.dll")]
	static extern bool AllocConsole();

	#region File Log
	private bool _pushLogsToFile;
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
	private const string LOG_TEMPLATE__ERROR = "[{0}] (ERROR) {1} {2}\nExMessage=\"{3}\"\nExStackTrace=\n{4}";
	private const string DATE_FORMAT = "yy-MM-dd HH:mm:ss:ffff";

	private Logger()
	{
		AllocConsole();

		try
		{
			var path = AppContext.BaseDirectory;
			var filename = $"SandLog_{DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified).ToString("yy-MM-dd_HH-mm-ss-fff")}.txt";
			var logDir = Path.Combine(path, "SandLogs");
			if (!Directory.Exists(logDir))
			{ 
				Directory.CreateDirectory(logDir);
			}
			var combined = Regex.Replace(Path.Combine(logDir, filename), "[^a-zA-Z0-9\\\\.:\\-_]", string.Empty);
			var filestream = File.OpenWrite(combined);
			_logFileWriter = new StreamWriter(filestream);
			_pushLogsToFile = true;
		}
		catch (Exception ex)
		{
			LogInfo($"Failed to create file log = {ex.Message}");
			_pushLogsToFile = false;
		}
		
		LogInfo("Logger initialised");
	}

	public void LogInfo(string message, [CallerMemberName] string caller = "", [CallerFilePath] string filePath = "")
	{
		string loggedCaller;
		if (filePath.Length > 0)
		{
			loggedCaller = filePath.Split(System.IO.Path.DirectorySeparatorChar).LastOrDefault();
		}
		else
		{
			loggedCaller = caller;
		}
		var logString = string.Format(LOG_TEMPLATE__INFO, GetDateStr(), loggedCaller, message);
		LogInternal(logString);
		if (_pushLogsToFile)
		{
			_logStrings.Enqueue(logString);
			if (_logStrings.Count > 5)
			{
				DrumpStringsToFile();
			}
		}
	}
	public void LogError(Exception ex, string message, [CallerMemberName] string caller = "", [CallerFilePath] string filePath = "")
	{
		string loggedCaller = $"{(filePath.Length > 0 ? filePath.Split(System.IO.Path.DirectorySeparatorChar).LastOrDefault() : string.Empty)} {caller}";
		
		var logString = string.Format(LOG_TEMPLATE__ERROR,
			GetDateStr(),
			loggedCaller,
			message,
			ex.Message,
			ex.StackTrace
		);

		LogInternal(logString);
		if (_pushLogsToFile)
		{
			_logStrings.Enqueue(logString);
			if (_logStrings.Count > 5)
			{
				DrumpStringsToFile();
			}
		}
	}

	private bool _currentlyDumpingStrings = false;
	private void DrumpStringsToFile() 
	{
		if (_currentlyDumpingStrings) return;

		_currentlyDumpingStrings = true;
		var sb = new StringBuilder();

		var iTerminate = _logStrings.Count;
		for (var i = 0; i < iTerminate; i++)
		{
			_logStrings.TryDequeue(out string str);
			if (str != null)
			{
				sb.AppendLine(str);
			}
		}
		_logFileWriter.Write(sb.ToString());
		_logFileWriter.Flush();
		sb.Clear();
		_currentlyDumpingStrings = false;
	}
	private static string GetDateStr() => DateTime.Now.ToString();
	
	private static void LogInternal(string message)
	{
		System.Diagnostics.Debug.WriteLine(message);
		Console.WriteLine(message);
	}

	public void FinaliseBeforeClose(Exception ex)
	{
		if (_pushLogsToFile)
		{
			DrumpStringsToFile();
		}
	}


}
