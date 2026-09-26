using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LigaMXCore.Models;

[Table("Municipio")]
public partial class Municipio
{
    [Key]
    public int MunicipioId { get; set; }

    [Required(ErrorMessage = "El nombre del municipio es obligatorio.")]
    [StringLength(80, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 80 caracteres.")]
    [RegularExpression(@"^[A-Za-zÁÉÍÓÚáéíóúÑñÜü\s\.\-']+$", ErrorMessage = "El nombre solo puede contener letras, espacios, puntos, guiones y apóstrofos.")]
    [Column("Municipio")]
    public string MunicipioNombre { get; set; } = null!;

    [Required(ErrorMessage = "El estado es obligatorio.")]
    public int EstadoId { get; set; }

    [InverseProperty("Municipio")]
    public virtual ICollection<Equipo> Equipos { get; set; } = new List<Equipo>();

    [InverseProperty("Municipio")]
    public virtual ICollection<Estadio> Estadios { get; set; } = new List<Estadio>();

    [ForeignKey("EstadoId")]
    [InverseProperty("Municipios")]
    public virtual Estado Estado { get; set; } = null!;
}
