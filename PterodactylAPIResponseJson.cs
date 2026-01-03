using Newtonsoft.Json;

namespace PterodactylToCloudflareDNS.PterodactylApiJson;

// Example success single server: {"success":true,"data":{"server_uuid":"c404670d-1e35-43dc-902e-9e42bf155ffc","server_name":"test","enabled":true,"ip":"127.0.0.1","port":2000,"subdomain":"test","domain":"kaenguruu.dev","full_domain":"test.kaenguruu.dev","updated_at":"2026-01-03 02:52:36"}}
public class SingleServerQueryResponse
{
	[JsonProperty("success")]
	public bool Success { get; set; }

	[JsonProperty("data")]
	public ServerData? Data { get; set; }
}

public class ServerData
{
	[JsonProperty("server_uuid")]
	public string ServerUuid { get; set; } = string.Empty;

	[JsonProperty("server_name")]
	public string ServerName { get; set; } = string.Empty;

	[JsonProperty("enabled")]
	public bool Enabled { get; set; }

	[JsonProperty("ip")]
	public string Ip { get; set; } = string.Empty;

	[JsonProperty("port")]
	public int Port { get; set; }

	[JsonProperty("subdomain")]
	public string Subdomain { get; set; } = string.Empty;

	[JsonProperty("domain")]
	public string Domain { get; set; } = string.Empty;

	[JsonProperty("full_domain")]
	public string FullDomain { get; set; } = string.Empty;

	[JsonProperty("updated_at")]
	public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.MinValue;
}

public class MultiServerQueryResponse
{
	[JsonProperty("object")]
	public string Object { get; set; } = string.Empty;

	[JsonProperty("data")]
	public MultiServerData[] Data { get; set; } = Array.Empty<MultiServerData>();
}

public class MultiServerData
{
	[JsonProperty("object")]
	public string Object { get; set; } = string.Empty;

	[JsonProperty("attributes")]
	public MultiServerAttributes Attributes { get; set; } = new MultiServerAttributes();
}

public class MultiServerAttributes
{
	[JsonProperty("uuid")]
	public string Uuid { get; set; } = string.Empty;

	[JsonProperty("identifier")]
	public string Identifier { get; set; } = string.Empty;
}