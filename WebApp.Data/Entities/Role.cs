namespace WebApp.Data.Entities;

public partial class Role
{
    public string Id { get; set; } = null!;

    public string? Description { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
