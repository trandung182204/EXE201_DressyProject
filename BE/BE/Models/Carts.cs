using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class Carts
{
    public long Id { get; set; }

    public long? CustomerId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<CartItems> CartItems { get; set; } = new List<CartItems>();

    public virtual Users? Customer { get; set; }
}
