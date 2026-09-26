using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LigaMXCore.Models;

[Table("Estado")]
public partial class Estado
{
    [Key]
    public int EstadoId { get; set; }

    [Required(ErrorMessage = "El nombre del estado es obligatorio.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres.")]
    [RegularExpression(@"^[A-Za-zÁÉÍÓÚáéíóúÑñÜü\s\.\-']+$", ErrorMessage = "El nombre solo puede contener letras, espacios, puntos, guiones y apóstrofos.")]
    [Column("Estado")]
    public string EstadoNombre { get; set; } = null!;

    [Required(ErrorMessage = "El país es obligatorio.")]
    public int PaisId { get; set; }

    [InverseProperty("Estado")]
    public virtual ICollection<Municipio> Municipios { get; set; } = new List<Municipio>();

    [ForeignKey("PaisId")]
    [InverseProperty("Estados")]
    public virtual Pais Pais { get; set; } = null!;
}
