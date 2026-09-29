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

    [Range(1, int.MaxValue, ErrorMessage = "El orden debe ser un número positivo.")]
    public int Orden { get; set; }

    [Required(ErrorMessage = "El nombre de la jornada es obligatorio.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 50 caracteres.")]
    [Column("Jornada")]
    public string JornadaNombre { get; set; } = null!;

    [Required(ErrorMessage = "La temporada es obligatoria.")]
    public int TemporadaId { get; set; }

    [Required(ErrorMessage = "El estatus es obligatorio.")]
    public int EstatusJornadaId { get; set; }

    [InverseProperty("Jornada")]
    public virtual ICollection<JornadaPartido> JornadaPartidos { get; set; } = new List<JornadaPartido>();

    [InverseProperty("Jornada")]
    public virtual ICollection<JornadaPronostico> JornadaPronosticos { get; set; } = new List<JornadaPronostico>();

    [ForeignKey("TemporadaId")]
    [InverseProperty("Jornada")]
    public virtual Temporada Temporada { get; set; } = null!;

    [ForeignKey("EstatusJornadaId")]
    [InverseProperty("Jornadas")]
    public virtual EstatusJornada EstatusJornada { get; set; } = null!;
}
