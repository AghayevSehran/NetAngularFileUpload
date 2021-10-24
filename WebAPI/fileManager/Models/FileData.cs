using System;
using System.Collections.Generic;

#nullable disable

namespace fileManager.Models
{
    public partial class FileRequest
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Extension { get; set; }
        public string Path { get; set; }
        public string MimeType { get; internal set; }
    }
}
