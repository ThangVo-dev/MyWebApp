﻿namespace WebApp.Data.Entities;

public partial class User
{
    public string Id { get; set; } = null!;

    public string? FullName { get; set; }

    public DateTime? CreateTime { get; set; }

    public string? RoleId { get; set; }

    public string? UserName { get; set; }

    public string? Email { get; set; }

    public string? PasswordHash { get; set; }

    public string? PhoneNumber { get; set; }

    public virtual Role? Role { get; set; }
}
