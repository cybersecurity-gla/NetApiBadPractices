using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BadApiExample.Models;

[Table("Persons")]
public class Person
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    [Column(TypeName = "nvarchar(100)")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    [Column(TypeName = "nvarchar(255)")]
    public string Email { get; set; } = string.Empty;

    [Range(1, 120)]
    public int Age { get; set; }

    [StringLength(20)]
    [Column(TypeName = "nvarchar(20)")]
    public string Phone { get; set; } = string.Empty;

    [StringLength(500)]
    [Column(TypeName = "nvarchar(500)")]
    public string Address { get; set; } = string.Empty;

    [Required]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedDate { get; set; }

    [Required]
    public bool IsActive { get; set; } = true;

    // Soft delete support
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedDate { get; set; }
}