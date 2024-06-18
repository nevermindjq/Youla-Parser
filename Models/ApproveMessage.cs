using Models.Abstractions;

namespace Models;

public class ApproveMessage : IEntity<string> {
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public int MessageId { get; set; }
    public long AdminId { get; set; }
    public long UserId { get; set; }
}