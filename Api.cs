using System.Net.Http.Headers;

namespace PterodactylToCloudflareDNS;

public static class Api
{
	private enum RequestMode
	{
		Get,
		Post,
		Put,
	}

	private static readonly HttpClient Client = new();

	private static CancellationToken GetCancellationToken() =>
		new CancellationTokenSource(TimeSpan.FromSeconds(15)).Token;

	public static async Task<HttpResponseMessage?> Get(string url, string? apiKey = null, bool throwOnFailure = false)
	{
		return await MakeRequest(RequestMode.Get, url, apiKey, null, throwOnFailure);
	}

	public static async Task<HttpResponseMessage?> Post(string url, StringContent json, string? apiKey = null,
		bool throwOnFailure = false)
	{
		return await MakeRequest(RequestMode.Post, url, apiKey, json, throwOnFailure);
	}

	public static async Task<HttpResponseMessage?> Put(string url, StringContent json, string? apiKey = null,
		bool throwOnFailure = false)
	{
		return await MakeRequest(RequestMode.Put, url, apiKey, json, throwOnFailure);
	}

	private static async Task<HttpResponseMessage?> MakeRequest(RequestMode mode, string url, string? apiKey = null,
		StringContent? json = null, bool throwOnFailure = false)
	{
		if (string.IsNullOrWhiteSpace(url))
			throw new ArgumentNullException(nameof(url));
		if (apiKey != null)
			Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

		var logSuffix = mode switch
		{
			RequestMode.Get => "GET",
			RequestMode.Post => "POST",
			RequestMode.Put => "PUT",
			_ => "UNKNOWN"
		};

		var token = GetCancellationToken();
		try
		{
			var response = mode switch
			{
				RequestMode.Get => await Client.GetAsync(url, token),
				RequestMode.Post => await Client.PostAsync(url, json, token),
				RequestMode.Put => await Client.PutAsync(url, json, token),
				_ => null
			};

			if (response is null)
				throw new ArgumentException("Unsupported request mode");

			if (throwOnFailure)
				response.EnsureSuccessStatusCode();

			return response;
		}
		catch (OperationCanceledException) when (!token.IsCancellationRequested)
		{
			await Logging.Log(LogSeverity.Error, $"API/{logSuffix}", $"Request to {url} timed out.");
		}
		catch (OperationCanceledException)
		{
			await Logging.Log(LogSeverity.Error, $"API/{logSuffix}", $"Request to {url} was cancelled manually.");
		}
		catch (HttpRequestException requestException)
		{
			await Logging.Log(LogSeverity.Debug, $"API/{logSuffix}", $"{requestException.Message}");
		}
		catch (Exception ex)
		{
			await Logging.Log(LogSeverity.Error, $"API/{logSuffix}", $"Request to {url} failed: {ex.Message}");
		}

		return null;
	}
}