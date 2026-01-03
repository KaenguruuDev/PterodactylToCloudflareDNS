namespace PterodactylToCloudflareDNS;

internal abstract class Program
{
	public static bool IsDebug;

	private static async Task Main(string[] args)
	{
		if (!await CheckParameters(args))
			return;

		var configuration = await CheckConfiguration(args);
		if (configuration is null)
			return;

		await Logging.LogInfo("Init", $"Starting PTCF Service");

		IsDebug = args.Any(a => a == "debug");
		if (IsDebug)
			await Logging.LogWarning("Init",
				"PTCF started in debug mode. Sensitive information will be logged. Do not use in production.");

#pragma warning disable
		CloudflareService.Run(configuration);
#pragma warning restore

		await Task.Delay(-1);
	}

	private static async Task<Dictionary<string, string?>?> CheckConfiguration(string[] args)
	{
		var configuration = new Dictionary<string, string?>
		{
			["CLOUDFLAREAPIKEY"] = null,
			["CLOUDFLAREZONEID"] = null,
			["PTERODACTYL_CLIENT_API_KEY"] = null,
			["PTERODACTYL_APPLICATION_API_KEY"] = null,
			["PTERODACTYLAPIURL"] = null
		};

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

		if (configuration.All(c => c.Value != null))
			return configuration;

		await Logging.LogError("Init",
			$"Missing configuration values for: {configuration.Aggregate("", (s, pair) => $"{s}, {pair.Key}")[2..]}");
		return null;
	}

	private static async Task<bool> CheckParameters(string[] args)
	{
		if (args.Length < 1)
		{
			await Logging.LogError("Init", "Missing configuration file or application mode.");
			return false;
		}

		if (File.Exists(args[0]))
			return true;
		await Logging.LogError("Init", "File not found: " + args[0]);

		return false;
	}
}