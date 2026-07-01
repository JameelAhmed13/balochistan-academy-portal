using BalochiAcademy.Domain.Common;

namespace BalochiAcademy.Domain.Entities;

public class Institute : AuditableEntity
{
    public string  Name     { get; set; } = string.Empty;
    public string? Code     { get; set; }
    public string? Address  { get; set; }
    public bool    IsActive { get; set; } = true;

    public ICollection<User> Users { get; set; } = [];
}
