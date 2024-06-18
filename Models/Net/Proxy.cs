using Models.Abstractions;

namespace Models.Net;

public class Proxy : IEntity<long> {
    public long Id { get; set; }
    public string Host { get; set; }
    public int Port { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public bool InUse { get; set; }
}