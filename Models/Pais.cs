using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LigaMXCore.Models;

public partial class Pais
{
    [Key]
    public int PaisId { get; set; }

    [Column("Pais")]
    public string PaisNombre { get; set; } = null!;

    [InverseProperty("Pais")]
    public virtual ICollection<Estado> Estados { get; set; } = new List<Estado>();
}
