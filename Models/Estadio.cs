using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LigaMXCore.Models;

[Table("Estadio")]
public partial class Estadio
{
    [Key]
    public int EstadioId { get; set; }

    [Column("Estadio")]
    public string EstadioNombre { get; set; } = null!;

    public string? Alias { get; set; }

    public string? Direccion { get; set; }

    public string? CodigoPostal { get; set; }

    public int MunicipioId { get; set; }

    [InverseProperty("Estadio")]
    public virtual ICollection<JornadaPartido> JornadaPartidos { get; set; } = new List<JornadaPartido>();

    [ForeignKey("MunicipioId")]
    [InverseProperty("Estadios")]
    public virtual Municipio Municipio { get; set; } = null!;
}
