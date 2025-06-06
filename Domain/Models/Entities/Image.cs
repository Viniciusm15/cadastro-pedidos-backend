﻿using Domain.Interfaces;

namespace Domain.Models.Entities
{
    public class Image : BaseEntity, ISoftDeletable
    {
        public required byte[] ImageData { get; set; }
        public required string ImageMimeType { get; set; }
        public string? Description { get; set; }

        public int EntityId { get; set; }
        public string EntityType { get; set; }
    }
}
