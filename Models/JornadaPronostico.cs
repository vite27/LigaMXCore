using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LigaMXCore.Models;

[Table("JornadaPronostico")]
public partial class JornadaPronostico
{
    [Key]
    public int JornadaPronosticoId { get; set; }

    public int JornadaId { get; set; }

    public int ParticipanteId { get; set; }

    [ForeignKey("JornadaId")]
    [InverseProperty("JornadaPronosticos")]
    public virtual Jornada Jornada { get; set; } = null!;

    [InverseProperty("JornadaPronostico")]
    public virtual ICollection<JornadaPronosticoDetalle> JornadaPronosticoDetalles { get; set; } = new List<JornadaPronosticoDetalle>();

    [ForeignKey("ParticipanteId")]
    [InverseProperty("JornadaPronosticos")]
    public virtual Participante Participante { get; set; } = null!;
}
