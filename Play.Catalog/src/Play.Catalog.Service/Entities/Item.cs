﻿using Play.Common;
using System.ComponentModel.DataAnnotations;

namespace Play.Catalog.Service.Entities
{
    public class Item : IEntity
    {
        public Guid Id { get; set; }
        [Required]
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
    }
}
