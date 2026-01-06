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

		// Check for email test mode
		if (args.Any(a => a == "test-email"))
		{
			await RunEmailTest();
			return;
		}

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
			["PTERODACTYLAPIURL"] = null,
			["SMTP_HOST"] = null,
			["SMTP_PORT"] = "587",
			["SMTP_USERNAME"] = null,
			["SMTP_PASSWORD"] = null,
			["FROM_ADDRESS"] = null,
			["EMAIL_SUBSCRIBERS"] = null,
			["COMPANY_NAME"] = "PTCF Service",
			["BRAND_COLOR"] = "#3b82f6",
			["COMPANY_LOGO_URL"] = ""
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

		// Initialize email provider (optional configuration)
		var subscribers = configuration["EMAIL_SUBSCRIBERS"]?.Split(',') ?? [];
		var smtpPort = int.TryParse(configuration["SMTP_PORT"], out var port) ? port : 587;

		EmailProvider.Initialize(
			configuration["COMPANY_NAME"] ?? "PTCF Service",
			configuration["BRAND_COLOR"] ?? "#3b82f6",
			configuration["COMPANY_LOGO_URL"] ?? "",
			configuration["SMTP_HOST"] ?? "",
			smtpPort,
			configuration["SMTP_USERNAME"] ?? "",
			configuration["SMTP_PASSWORD"] ?? "",
			configuration["FROM_ADDRESS"] ?? "",
			subscribers
		);

		// Only require Cloudflare and Pterodactyl configuration (skip if in email test mode)
		var isEmailTest = args.Any(a => a == "test-email");
		if (!isEmailTest)
		{
			var requiredKeys = new[] { "CLOUDFLAREAPIKEY", "CLOUDFLAREZONEID", "PTERODACTYL_CLIENT_API_KEY",
				"PTERODACTYL_APPLICATION_API_KEY", "PTERODACTYLAPIURL" };

			if (!requiredKeys.All(key => configuration[key] != null))
			{
				await Logging.LogError("Init",
					$"Missing configuration values for: {string.Join(", ", requiredKeys.Where(key => configuration[key] == null))}");
				return null;
			}
		}

		return configuration;
	}

	private static async Task RunEmailTest()
	{
		await Logging.LogInfo("EmailTest", "Starting email template test mode...");
		await Logging.LogInfo("EmailTest", "Sending test emails for all templates...");

		var testDomain = "test-server.example.com";
		var testError = "This is a test error message to demonstrate the error template.";

		try
		{
			// Test 1: New DNS Records Created
			await Logging.LogInfo("EmailTest", "Sending: New DNS Records Created");
			await EmailProvider.SendEmail(
				$"[TEST] Records Created for {testDomain}",
				EmailTemplates.NewDnsRecordsCreated(testDomain, "192.168.1.100",
					"_minecraft._tcp.test", 0, 5, 25565, testDomain)
			);
			await Task.Delay(500);

			// Test 2: DNS Records Updated
			await Logging.LogInfo("EmailTest", "Sending: DNS Records Updated");
			await EmailProvider.SendEmail(
				$"[TEST] Records Updated for {testDomain}",
				EmailTemplates.DnsRecordsUpdated(testDomain, "192.168.1.200",
					"_minecraft._tcp.test", 0, 5, 25566, testDomain)
			);
			await Task.Delay(500);

			// Test 3: DNS Records Deleted
			await Logging.LogInfo("EmailTest", "Sending: DNS Records Deleted");
			await EmailProvider.SendEmail(
				$"[TEST] Records Deleted for {testDomain}",
				EmailTemplates.DnsRecordsDeleted(testDomain, "192.168.1.100",
					"_minecraft._tcp.test", 0, 5, 25565, testDomain, "Server no longer exists (test)")
			);
			await Task.Delay(500);

			// Test 4: A Record Creation Failed
			await Logging.LogInfo("EmailTest", "Sending: A Record Creation Failed");
			await EmailProvider.SendEmail(
				"[TEST] A Record Creation Failed",
				EmailTemplates.DnsRecordCreationFailed(testDomain, "A", testError)
			);
			await Task.Delay(500);

			// Test 5: SRV Record Creation Failed
			await Logging.LogInfo("EmailTest", "Sending: SRV Record Creation Failed");
			await EmailProvider.SendEmail(
				"[TEST] SRV Record Creation Failed",
				EmailTemplates.DnsRecordCreationFailed(testDomain, "SRV", testError)
			);
			await Task.Delay(500);

			// Test 6: A Record Update Failed
			await Logging.LogInfo("EmailTest", "Sending: A Record Update Failed");
			await EmailProvider.SendEmail(
				"[TEST] A Record Update Failed",
				EmailTemplates.DnsRecordUpdateFailed(testDomain, "A", testError)
			);
			await Task.Delay(500);

			// Test 7: SRV Record Update Failed
			await Logging.LogInfo("EmailTest", "Sending: SRV Record Update Failed");
			await EmailProvider.SendEmail(
				"[TEST] SRV Record Update Failed",
				EmailTemplates.DnsRecordUpdateFailed(testDomain, "SRV", testError)
			);
			await Task.Delay(500);

			// Test 8: A Record Deletion Failed
			await Logging.LogInfo("EmailTest", "Sending: A Record Deletion Failed");
			await EmailProvider.SendEmail(
				"[TEST] A Record Deletion Failed",
				EmailTemplates.DnsRecordDeletionFailed(testDomain, "A", testError)
			);
			await Task.Delay(500);

			// Test 9: SRV Record Deletion Failed
			await Logging.LogInfo("EmailTest", "Sending: SRV Record Deletion Failed");
			await EmailProvider.SendEmail(
				"[TEST] SRV Record Deletion Failed",
				EmailTemplates.DnsRecordDeletionFailed(testDomain, "SRV", testError)
			);

			await Logging.LogInfo("EmailTest", "All test emails have been sent successfully!");
			await Logging.LogInfo("EmailTest", "Check your inbox to verify the templates.");
		}
		catch (Exception ex)
		{
			await Logging.LogError("EmailTest", $"Failed to send test emails: {ex.Message}");
			Environment.Exit(1);
		}
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