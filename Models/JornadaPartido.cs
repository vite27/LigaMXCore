using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LigaMXCore.Models;

[Table("JornadaPartido")]
public partial class JornadaPartido
{
    [Key]
    public int JornadaPartidoId { get; set; }

    [Required(ErrorMessage = "La jornada es obligatoria.")]
    public int JornadaId { get; set; }

    [Required(ErrorMessage = "El partido es obligatorio.")]
    public int PartidoId { get; set; }

    [Required(ErrorMessage = "El estadio es obligatorio.")]
    public int EstadioId { get; set; }

    public int? GolLocal { get; set; }

    public int? GolVisita { get; set; }

    [Required(ErrorMessage = "El estatus es obligatorio.")]
    public int EstatusPartidoId { get; set; }

    public int TipoResultadoId { get; set; }

    [ForeignKey("EstadioId")]
    [InverseProperty("JornadaPartidos")]
    public virtual Estadio Estadio { get; set; } = null!;

    [ForeignKey("EstatusPartidoId")]
    [InverseProperty("JornadaPartidos")]
    public virtual EstatusPartido EstatusPartido { get; set; } = null!;

    [ForeignKey("JornadaId")]
    [InverseProperty("JornadaPartidos")]
    public virtual Jornada Jornada { get; set; } = null!;

    [ForeignKey("PartidoId")]
    [InverseProperty("JornadaPartidos")]
    public virtual Partido Partido { get; set; } = null!;

    [ForeignKey("TipoResultadoId")]
    [InverseProperty("JornadaPartidos")]
    public virtual TipoResultado TipoResultado { get; set; } = null!;
}
