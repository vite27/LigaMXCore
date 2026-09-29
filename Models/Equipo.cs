using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LigaMXCore.Models;

[Table("Equipo")]
public partial class Equipo
{
    [Key]
    public int EquipoId { get; set; }

    [Required(ErrorMessage = "El nombre del equipo es obligatorio.")]
    [StringLength(60, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 60 caracteres.")]
    [Column("Equipo")]
    public string EquipoNombre { get; set; } = null!;

    [StringLength(50, MinimumLength = 2, ErrorMessage = "El alias debe tener entre 2 y 50 caracteres.")]
    [RegularExpression(@"^[A-Za-zÁÉÍÓÚáéíóúÑñÜü0-9\s/.,'-]+$", ErrorMessage = "El alias contiene caracteres no permitidos.")]
    public string? Alias { get; set; }

    [Required(ErrorMessage = "El municipio es obligatorio.")]
    public int MunicipioId { get; set; }

    [StringLength(255, ErrorMessage = "La ruta del logo no puede exceder 255 caracteres.")]
    public string? EquipoLogo { get; set; }

    [ForeignKey("MunicipioId")]
    [InverseProperty("Equipos")]
    public virtual Municipio Municipio { get; set; } = null!;

    [InverseProperty("EquipoLocal")]
    public virtual ICollection<Partido> PartidoEquipoLocals { get; set; } = new List<Partido>();

    [InverseProperty("EquipoVisita")]
    public virtual ICollection<Partido> PartidoEquipoVisita { get; set; } = new List<Partido>();
}
