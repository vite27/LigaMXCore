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

    [Column("Equipo")]
    public string EquipoNombre { get; set; } = null!;

    public string? Alias { get; set; }

    public int MunicipioId { get; set; }

    public string? EquipoLogo { get; set; }

    [ForeignKey("MunicipioId")]
    [InverseProperty("Equipos")]
    public virtual Municipio Municipio { get; set; } = null!;

    [InverseProperty("EquipoLocal")]
    public virtual ICollection<Partido> PartidoEquipoLocals { get; set; } = new List<Partido>();

    [InverseProperty("EquipoVisita")]
    public virtual ICollection<Partido> PartidoEquipoVisita { get; set; } = new List<Partido>();
}
