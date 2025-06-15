namespace PterodactylToCloudflareDNS;

internal abstract class Program
{
	private static bool _isPterodactyl;
	public static bool IsDebug;

	// Usage: ./MTCF path/to/.env pterodactyl/cloudflare
	private static async Task Main(string[] args)
	{
		if (!await CheckParameters(args))
			return;

		var configuration = await CheckConfiguration(args);
		if (configuration is null)
			return;

		await Logging.Log(LogSeverity.Info, "Init",
			$"Starting {(_isPterodactyl ? "Pterodactyl" : "Cloudflare")} Service");

		IsDebug = args.Any(a => a == "debug");
		if (IsDebug)
			await Logging.Log(LogSeverity.Warning, "Init",
				"PTCF started in debug mode. Sensitive information will be logged. Do not use in production.");

#pragma warning disable
		if (_isPterodactyl)
			PterodactylService.Run(configuration);
		else
			CloudflareService.Run(configuration);
#pragma warning restore

		await Task.Delay(-1);
	}

	private static async Task<Dictionary<string, string?>?> CheckConfiguration(string[] args)
	{
		var configuration = new Dictionary<string, string?>();

		if (args[1] == "cloudflare")
		{
			configuration["CLOUDFLAREAPIKEY"] = null;
			configuration["CLOUDFLAREZONEID"] = null;
			configuration["PTERODACTYLAPIKEY"] = null;
			configuration["PTERODACTYLAPIURL"] = null;
			_isPterodactyl = false;
		}
		else
		{
			configuration["PTERODACTYLAPIKEY"] = null;
			configuration["PTERODACTYLAPIURL"] = null;
			_isPterodactyl = true;
		}

		var lines = await File.ReadAllLinesAsync(args[0]);
		foreach (var line in lines)
		{
			var identifier = line.Split("=").FirstOrDefault();
			var value = line.Split("=").LastOrDefault();

			if (identifier == null || value == null)
				continue;

			if (!configuration.ContainsKey(identifier))
				continue;

			configuration[identifier] = value;
		}

		if (configuration.All(c => c.Value != null)) return configuration;

		await Logging.Log(LogSeverity.Error, "Init",
			$"Missing configuration values for: {configuration.Aggregate("", (s, pair) => $"{s}, {pair.Key}")[2..]}");
		return null;
	}

	private static async Task<bool> CheckParameters(string[] args)
	{
		if (args.Length < 2)
		{
			await Logging.Log(LogSeverity.Error, "Init", "Missing configuration file or application mode.");
			return false;
		}

		if (!File.Exists(args[0]))
		{
			await Logging.Log(LogSeverity.Error, "Init", "File not found: " + args[0]);
			return false;
		}

		if (args[1] != "cloudflare" && args[1] != "pterodactyl")
		{
			await Logging.Log(LogSeverity.Error, "Init",
				$"Mode '{args[1]}' not supported. Use 'pterodactyl' or 'cloudflare'");
			return false;
		}

		return true;
	}
}