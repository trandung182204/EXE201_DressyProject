using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class Roles
{
    public int Id { get; set; }

    public string? RoleName { get; set; }

    public virtual ICollection<Users> User { get; set; } = new List<Users>();
}
