using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LigaMXCore.Models;

public partial class Pais
{
    [Key]
    public int PaisId { get; set; }

    [Required(ErrorMessage = "El nombre del país es obligatorio.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres.")]
    [RegularExpression(@"^[A-Za-zÁÉÍÓÚáéíóúÑñÜü\s\.\-']+$", ErrorMessage = "El nombre solo puede contener letras, espacios, puntos, guiones y apóstrofos.")]
    [Column("Pais")]
    public string PaisNombre { get; set; } = null!;

    [InverseProperty("Pais")]
    public virtual ICollection<Estado> Estados { get; set; } = new List<Estado>();
}
