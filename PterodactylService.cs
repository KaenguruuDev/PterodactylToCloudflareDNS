using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PterodactylToCloudflareDNS.PterodactylApiJson;

namespace PterodactylToCloudflareDNS;

public static partial class PterodactylService
{
	private static string? _apiKey;
	private static string? _apiUrl;
	private static readonly List<Server> Servers = [];

	public static async Task Run(Dictionary<string, string?> configuration)
	{
		configuration.TryGetValue("PTERODACTYLAPIKEY", out _apiKey);
		configuration.TryGetValue("PTERODACTYLAPIURL", out _apiUrl);

		await Logging.Log(LogSeverity.Info, "Pterodactyl", "Initialize");

		var success = await TryPullApiData();
		if (!success)
		{
			await Logging.Log(LogSeverity.Error, "Pterodactyl", "Could not pull data from API. Terminating...");
			return;
		}

		await Logging.Log(LogSeverity.Info, "Pterodactyl", "Configuration is valid. Monitoring...");

		var app = ConfigureApi();
		_ = app.RunAsync();
		await MonitorForChanges();
	}

	private static async Task<bool> TryPullApiData()
	{
		try
		{
			var data = await GetDataFromApi();
			return data != null;
		}
		catch (Exception e)
		{
			await Logging.Log(LogSeverity.Error, "Pterodactyl", $"Error pulling data from API: {e.Message}");
		}

		return false;
	}

	private static async Task<RootObject?> GetDataFromApi()
	{
		var response =
			await Api.Get(new Uri(new Uri(_apiUrl ?? "INVALID API URL"), "api/application/servers").AbsoluteUri,
				_apiKey);

		return response is null
			? null
			: JsonConvert.DeserializeObject<RootObject>(await response.Content.ReadAsStringAsync());
	}

	private static async Task MonitorForChanges()
	{
		while (true)
		{
			var rootObject = await GetDataFromApi();
			Servers.Clear();

			foreach (var server in rootObject?.Data ?? [])
			{
				var description = server.Attributes?.Description?.Replace("\n", "") ?? string.Empty;
				if (string.IsNullOrEmpty(description) || !description.Contains("DNS_CONFIG"))
					continue;

				var validDnsConfig = DnsConfigValidatorRegex().IsMatch(description);
				var match = DnsConfigParseRegex().Match(description);

				if (!validDnsConfig || !match.Success || match.Groups.Count != 5)
				{
					await Logging.Log(LogSeverity.Warning, "Pterodactyl",
						$"DNS_CONFIG: {(validDnsConfig ? match.Groups[0].Value : description)} could not be parsed.");
					return;
				}

				var ipAddress = match.Groups[1].Value.Replace(" ", "");
				int port;
				port = int.TryParse(match.Groups[2].Value, out port) ? port : 0;
				var subdomain = match.Groups[3].Value.Replace(" ", "");
				var domain = match.Groups[4].Value.Replace(" ", "");

				Servers.Add(new Server(ipAddress, port, domain, subdomain));
				await Logging.Log(LogSeverity.Info, "Pterodactyl",
					$"Server: {ipAddress}:{port} -> {subdomain}.{domain}");
			}

			await Task.Delay(TimeSpan.FromSeconds(30));
		}
	}

	private static WebApplication ConfigureApi()
	{
		var builder = WebApplication.CreateBuilder();
		builder.Logging.ClearProviders();

		var app = builder.Build();
		app.Urls.Add("http://localhost:5000");

		app.MapGet("/servers", (HttpContext context) =>
		{
			var authHeader = context.Request.Headers.Authorization.ToString();
			return authHeader != $"Bearer {_apiKey}"
				? "Unauthorized"
				: JsonConvert.SerializeObject(Servers);
		});

		return app;
	}

	[GeneratedRegex(@"(DNS_CONFIG\(.+\))")]
	private static partial Regex DnsConfigValidatorRegex();

	[GeneratedRegex(@"DNS_CONFIG\(((?:[0-9]{3}\.?)+), ?([0-9]{4,}), ?([^,]+),(.+)\)")]
	private static partial Regex DnsConfigParseRegex();
}