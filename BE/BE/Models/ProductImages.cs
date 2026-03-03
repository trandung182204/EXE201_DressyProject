using System;

namespace BE.Models;

public partial class ProductImages
{
    public long Id { get; set; }
    public long? ProductId { get; set; }

    // CHANGED
    public long? ImageFileId { get; set; }

    public virtual Products? Product { get; set; }

    // (optional) navigation
    public virtual MediaFiles? ImageFile { get; set; }
}
