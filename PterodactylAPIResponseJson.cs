using Newtonsoft.Json;

namespace PterodactylToCloudflareDNS;

public class RootObject
{
    [JsonProperty("object")]
    public string? Object { get; set; }
    public List<ServerData>? Data { get; set; }
    public Meta? Meta { get; set; }
}

public class ServerData
{
    [JsonProperty("object")]
    public string? Object { get; set; }
    public Attributes? Attributes { get; set; }
}

public class Attributes
{
    public int Id { get; set; }
    public string? ExternalId { get; set; }
    public string? Uuid { get; set; }
    public string? Identifier { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public object? Status { get; set; }
    public bool Suspended { get; set; }
    public Limits? Limits { get; set; }
    public FeatureLimits? FeatureLimits { get; set; }
    public int User { get; set; }
    public int Node { get; set; }
    public int Allocation { get; set; }
    public int Nest { get; set; }
    public int Egg { get; set; }
    public Container? Container { get; set; }
    public string? UpdatedAt { get; set; }
    public string? CreatedAt { get; set; }
}

public class Limits
{
    public int Memory { get; set; }
    public int Swap { get; set; }
    public int Disk { get; set; }
    public int Io { get; set; }
    public int Cpu { get; set; }
    public object? Threads { get; set; }
    public bool OomDisabled { get; set; }
}

public class FeatureLimits
{
    public int Databases { get; set; }
    public int Allocations { get; set; }
    public int Backups { get; set; }
}

public class Container
{
    public string? StartupCommand { get; set; }
    public string? Image { get; set; }
    public int Installed { get; set; }
    public Dictionary<string, string>? Environment { get; set; }
}

public class Meta
{
    public Pagination? Pagination { get; set; }
}

public class Pagination
{
    public int Total { get; set; }
    public int Count { get; set; }
    public int PerPage { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public Links? Links { get; set; }
}

public class Links
{
    //! I DON'T CARE
}
