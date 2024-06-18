using Models.Abstractions;

namespace Models;

public class User : IEntity<long> {
    public long Id { get; set; }
    public string Username { get; set; }
    public bool IsAdmin { get; set; }
    public bool IsApproved { get; set; }
}