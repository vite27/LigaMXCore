using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LigaMXCore.Models;

[Table("EstatusPartido")]
public partial class EstatusPartido
{
    [Key]
    public int EstatusPartidoId { get; set; }

    [Column("EstatusPartido")]
    public string EstatusPartidoNombre { get; set; } = null!;

    [InverseProperty("EstatusPartido")]
    public virtual ICollection<JornadaPartido> JornadaPartidos { get; set; } = new List<JornadaPartido>();
}
