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

    [Required(ErrorMessage = "El nombre del estatus de partido es obligatorio.")]
    [StringLength(30, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 30 caracteres.")]
    [RegularExpression(@"^[A-Za-zÁÉÍÓÚáéíóúÑñÜü\s]+$", ErrorMessage = "El nombre solo puede contener letras y espacios.")]
    [Column("EstatusPartido")]
    public string EstatusPartidoNombre { get; set; } = null!;

    [InverseProperty("EstatusPartido")]
    public virtual ICollection<JornadaPartido> JornadaPartidos { get; set; } = new List<JornadaPartido>();
}
