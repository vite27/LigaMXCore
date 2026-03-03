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

    [Column("Temporada")]
    public string TemporadaNombre { get; set; } = null!;

    public string? Comentarios { get; set; }

    [InverseProperty("Temporada")]
    public virtual ICollection<Jornada> Jornada { get; set; } = new List<Jornada>();
}
