using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LigaMXCore.Models;

public partial class Jornada
{
    [Key]
    public int JornadaId { get; set; }

    public int Orden { get; set; }

    public string JornadaNombre { get; set; } = null!;

    public int TemporadaId { get; set; }

    [InverseProperty("Jornada")]
    public virtual ICollection<JornadaPartido> JornadaPartidos { get; set; } = new List<JornadaPartido>();

    [InverseProperty("Jornada")]
    public virtual ICollection<JornadaPronostico> JornadaPronosticos { get; set; } = new List<JornadaPronostico>();

    [ForeignKey("TemporadaId")]
    [InverseProperty("Jornada")]
    public virtual Temporada Temporada { get; set; } = null!;
}
