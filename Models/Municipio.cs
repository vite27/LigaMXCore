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

    [Column("Municipio")]
    public string MunicipioNombre { get; set; } = null!;

    public int EstadoId { get; set; }

    [InverseProperty("Municipio")]
    public virtual ICollection<Equipo> Equipos { get; set; } = new List<Equipo>();

    [InverseProperty("Municipio")]
    public virtual ICollection<Estadio> Estadios { get; set; } = new List<Estadio>();

    [ForeignKey("EstadoId")]
    [InverseProperty("Municipios")]
    public virtual Estado Estado { get; set; } = null!;
}
