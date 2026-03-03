using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class Users
{
    public long Id { get; set; }

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? FullName { get; set; }

    public string? Phone { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
    public long? AvatarFileId { get; set; }

    public virtual MediaFiles? AvatarFile { get; set; }

    public virtual ICollection<Bookings> Bookings { get; set; } = new List<Bookings>();

    public virtual Carts? Carts { get; set; }

    public virtual ICollection<FeedbackResponses> FeedbackResponses { get; set; } = new List<FeedbackResponses>();

    public virtual ICollection<ProductReviews> ProductReviews { get; set; } = new List<ProductReviews>();

    public virtual ICollection<ProviderFeedbacks> ProviderFeedbacks { get; set; } = new List<ProviderFeedbacks>();

    public virtual ICollection<Providers> Providers { get; set; } = new List<Providers>();

    public virtual ICollection<Roles> Role { get; set; } = new List<Roles>();
}
