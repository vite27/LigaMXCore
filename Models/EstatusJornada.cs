using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LigaMXCore.Models;

public partial class EstatusJornada
{
    [Key]
    public int EstatusJornadaId { get; set; }

    [Column("EstatusJornada")]
    public string EstatusJornadaNombre { get; set; } = null!;
}
