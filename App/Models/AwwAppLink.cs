using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Marten.Schema;

namespace App.Models
{
    public class AwwAppLink
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Identity]
        public Guid Id { get; set; }
        
        [UniqueIndex(IndexType = UniqueIndexType.Computed)]
        public string Link { get; set; }
        
        public bool Used { get; set; }
        
        public DateTimeOffset CreatedOn { get; set; } = DateTimeOffset.Now;
    }
}