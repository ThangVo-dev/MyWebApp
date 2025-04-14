using System;
using System.Collections.Generic;

namespace WebApp.Data.Entities;

public partial class Product
{
    public string Id { get; set; } = null!;

    public string? Name { get; set; }

    public DateOnly? CreateAdd { get; set; }

    public string? CategoryId { get; set; }

    public virtual CategoryProduct? Category { get; set; }
}
