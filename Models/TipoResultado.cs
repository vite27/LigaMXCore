using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LigaMXCore.Models;

[Table("TipoResultado")]
public partial class TipoResultado
{
    [Key]
    public int TipoResultadoId { get; set; }

    [Column("TipoResultado")]
    public string TipoResultadoNombre { get; set; } = null!;

    [InverseProperty("TipoResultado")]
    public virtual ICollection<JornadaPartido> JornadaPartidos { get; set; } = new List<JornadaPartido>();

    [InverseProperty("TipoResultado")]
    public virtual ICollection<JornadaPronosticoDetalle> JornadaPronosticoDetalles { get; set; } = new List<JornadaPronosticoDetalle>();
}
