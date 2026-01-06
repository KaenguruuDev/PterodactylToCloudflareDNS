using System.Diagnostics;
using System.Text.RegularExpressions;

namespace PterodactylToCloudflareDNS;

public enum LogSeverity
{
	Debug,
	Info,
	Warning,
	Error,
	Critical
}

public static class Logging
{
	private static readonly FileStream FileStream;

	static Logging()
	{
		if (!Directory.Exists("logs"))
			Directory.CreateDirectory("logs");

		if (File.Exists("logs/latest.log"))
			File.Move("logs/latest.log", $"logs/{GetLogFileName(new FileInfo("logs/latest.log").LastWriteTime)}.log");

		var maxLogCount = 15;
		foreach (var file in Directory.GetFiles("logs").Order())
		{
			if (maxLogCount-- <= 0)
				File.Delete(file);
		}

		FileStream = new FileStream("logs/latest.log", FileMode.Append, FileAccess.Write, FileShare.Read);

		AppDomain.CurrentDomain.ProcessExit += (_, _) => OnExit();

		Log(LogSeverity.Info, "Logging", $"{DateTimeOffset.Now:dd/MM/yyyy HH:mm:ss}");
		Log(LogSeverity.Info, "Logging", "Initialize");
	}

	public static void Log(LogSeverity severity, string source, string message, Exception? exception = null)
	{
		_ = LogMsg(severity, source, message, exception);
	}

	[Obsolete("Prefer explicit LogDebug, LogInfo, ... methods")]
	public static async Task LogAsync(LogSeverity severity, string source, string message, Exception? exception = null)
		=> await LogMsg(severity, source, message, exception);

	private static async Task LogMsg(LogSeverity severity, string source, string message, Exception? exception = null)
	{
		var log = $"{GetLogTimeStamp() + " [" + severity + "] " + source,-30}{message}";

		await using (StreamWriter writer = new(FileStream, leaveOpen: true))
		{
			await writer.WriteLineAsync(log);
			if (exception != null)
				await writer.WriteLineAsync($"---- BEGIN TRACE ----\n{exception}\n---- END   TRACE ----");
		}

		if (severity == LogSeverity.Debug && !Program.IsDebug)
			return;
		
		Console.ForegroundColor = severity switch
		{
			LogSeverity.Debug => ConsoleColor.Green,
			LogSeverity.Critical or LogSeverity.Error => ConsoleColor.Red,
			LogSeverity.Warning => ConsoleColor.Yellow,
			_ => ConsoleColor.White
		};

		Console.WriteLine(log);
		Console.ResetColor();
	}

	public static string GetLogTimeStamp()
		=> $"{DateTimeOffset.Now.Hour:D2}:{DateTimeOffset.Now.Minute:D2}:{DateTimeOffset.Now.Second:D2}";

	private static string GetLogFileName(DateTimeOffset time)
		=> $"{time.Year:D4}-{time.Month:D2}-{time.Day:D2}T{time.AddHours(1).Hour:D2}-{time.Minute:D2}-{time.Second:D2}";

	private static void OnExit()
		=> FileStream.Close();

#pragma warning disable CS0618 // Type or member is obsolete
	public static async Task LogInfo(string source, string message, Exception? exception = null)
		=> await LogAsync(LogSeverity.Info, source, message, exception);

	public static async Task LogDebug(string source, string message, Exception? exception = null)
		=> await LogAsync(LogSeverity.Debug, source, message, exception);

	public static async Task LogWarning(string source, string message, Exception? exception = null)
		=> await LogAsync(LogSeverity.Warning, source, message, exception);

	public static async Task LogError(string source, string message, Exception? exception = null)
		=> await LogAsync(LogSeverity.Error, source, message, exception);

	public static async Task LogCritical(string source, string message, Exception? exception = null)
		=> await LogAsync(LogSeverity.Critical, source, message, exception);
#pragma warning restore CS0618 // Type or member is obsolete
}