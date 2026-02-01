using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class Categories
{
    public long Id { get; set; }

    public string? Name { get; set; }

    public long? ParentId { get; set; }

    public string? Description { get; set; }

    public string? Status { get; set; }

    public long? ProviderId { get; set; }
    public Providers? Provider { get; set; }

    public virtual ICollection<Categories> InverseParent { get; set; } = new List<Categories>();

    public virtual Categories? Parent { get; set; }

    public virtual ICollection<Products> Products { get; set; } = new List<Products>();
}
