using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LigaMXCore.Models;

[Table("JornadaPronosticoDetalle")]
public partial class JornadaPronosticoDetalle
{
    [Key]
    public int JornadaPronosticoDetalleId { get; set; }

    public int JornadaPronosticoId { get; set; }

    public int JornadaPartidoId { get; set; }

    public int GolLocal { get; set; }

    public int GolVisita { get; set; }

    public int? Puntos { get; set; }

    public int TipoResultadoId { get; set; }

    [ForeignKey("JornadaPronosticoId")]
    [InverseProperty("JornadaPronosticoDetalles")]
    public virtual JornadaPronostico JornadaPronostico { get; set; } = null!;

    [ForeignKey("TipoResultadoId")]
    [InverseProperty("JornadaPronosticoDetalles")]
    public virtual TipoResultado TipoResultado { get; set; } = null!;
}
