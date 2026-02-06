using System;

namespace BE.Models;

public partial class MediaFiles
{
    public long Id { get; set; }
    public string FileName { get; set; } = null!;
    public string MimeType { get; set; } = null!;
    public long FileSize { get; set; }
    public byte[] Data { get; set; } = null!;
    public DateTime? CreatedAt { get; set; }
}
