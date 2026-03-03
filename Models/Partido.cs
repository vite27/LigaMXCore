using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LigaMXCore.Models;

[Table("Partido")]
public partial class Partido
{
    [Key]
    public int PartidoId { get; set; }

    public int EquipoLocalId { get; set; }

    public int EquipoVisitaId { get; set; }

    [ForeignKey("EquipoLocalId")]
    [InverseProperty("PartidoEquipoLocals")]
    public virtual Equipo EquipoLocal { get; set; } = null!;

    [ForeignKey("EquipoVisitaId")]
    [InverseProperty("PartidoEquipoVisita")]
    public virtual Equipo EquipoVisita { get; set; } = null!;

    [InverseProperty("Partido")]
    public virtual ICollection<JornadaPartido> JornadaPartidos { get; set; } = new List<JornadaPartido>();
}
