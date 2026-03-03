using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LigaMXCore.Models;

[Table("Participante")]
public partial class Participante
{
    [Key]
    public int ParticipanteId { get; set; }

    public string Nombres { get; set; } = null!;

    public string ApellidoPaterno { get; set; } = null!;

    public string ApellidoMaterno { get; set; } = null!;

    [InverseProperty("Participante")]
    public virtual ICollection<JornadaPronostico> JornadaPronosticos { get; set; } = new List<JornadaPronostico>();
}
