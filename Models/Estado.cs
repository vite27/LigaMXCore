using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LigaMXCore.Models;

[Table("Estado")]
public partial class Estado
{
    [Key]
    public int EstadoId { get; set; }

    [Column("Estado")]
    public string EstadoNombre { get; set; } = null!;

    public int PaisId { get; set; }

    [InverseProperty("Estado")]
    public virtual ICollection<Municipio> Municipios { get; set; } = new List<Municipio>();

    [ForeignKey("PaisId")]
    [InverseProperty("Estados")]
    public virtual Pais Pais { get; set; } = null!;
}
