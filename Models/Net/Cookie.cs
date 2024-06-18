using Models.Abstractions;

namespace Models.Net;

public class Cookie : IEntity<string> {
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string YoulaAuth { get; set; }
    public string YoulaAuthRefresh { get; set; }
    public string? YoulaAuthRefreshSwitchUser { get; set; }
    public string? CtoBundle { get; set; }
    public string Uid { get; set; }
    public string? DomainSid { get; set; }
    public string SessId { get; set; }
    
    // Custom
    public bool IsShadowBanned { get; set; }
    public bool IsInvalidToken { get; set; }
}