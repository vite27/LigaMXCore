using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LigaMXCore.Models;

public partial class Temporada
{
    [Key]
    public int TemporadaId { get; set; }

    [Required(ErrorMessage = "El nombre de la temporada es obligatorio.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 50 caracteres.")]
    [RegularExpression(@"^[A-Za-z0-9ÁÉÍÓÚáéíóúÑñÜü\s\-]+$", ErrorMessage = "El nombre solo puede contener letras, números, espacios y guiones.")]
    [Column("Temporada")]
    public string TemporadaNombre { get; set; } = null!;

    [StringLength(500, ErrorMessage = "Los comentarios no pueden exceder 500 caracteres.")]
    public string? Comentarios { get; set; }

    [InverseProperty("Temporada")]
    public virtual ICollection<Jornada> Jornada { get; set; } = new List<Jornada>();
}
