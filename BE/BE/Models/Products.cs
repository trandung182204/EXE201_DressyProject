using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE.Models;

public partial class Products
{
    public long Id { get; set; }

    public long? ProviderId { get; set; }
    
    [Column("provider_branch_id")]
    public long? ProviderBranchId { get; set; }

    public virtual ProviderBranches? ProviderBranch { get; set; }

    public long? CategoryId { get; set; }

    public string? Name { get; set; }

    public string? ProductType { get; set; }

    public string? Description { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Categories? Category { get; set; }

    public virtual ICollection<ProductImages> ProductImages { get; set; } = new List<ProductImages>();

    public virtual ProductRatingSummary? ProductRatingSummary { get; set; }

    public virtual ICollection<ProductReviews> ProductReviews { get; set; } = new List<ProductReviews>();

    public virtual ICollection<ProductVariants> ProductVariants { get; set; } = new List<ProductVariants>();

    public virtual Providers? Provider { get; set; }
}
