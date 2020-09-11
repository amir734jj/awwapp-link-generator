using System;
using System.ComponentModel.DataAnnotations;

namespace App.Models
{
    public class AwwAppLink
    {
        [Key]
        public string Id { get; set; }
        
        public string Link { get; set; }
        
        public bool Used { get; set; }
        
        public DateTimeOffset CreatedOn { get; set; } = DateTimeOffset.Now;
    }
}