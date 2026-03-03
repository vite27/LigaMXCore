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

    public int JornadaId { get; set; }

    public int PartidoId { get; set; }

    public int EstadioId { get; set; }

    public int? GolLocal { get; set; }

    public int? GolVisita { get; set; }

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
