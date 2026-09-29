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

    [Required(ErrorMessage = "El nombre del estatus de jornada es obligatorio.")]
    [StringLength(30, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 30 caracteres.")]
    [RegularExpression(@"^[A-Za-zÁÉÍÓÚáéíóúÑñÜü\s]+$", ErrorMessage = "El nombre solo puede contener letras y espacios.")]
    [Column("EstatusJornada")]
    public string EstatusJornadaNombre { get; set; } = null!;

    [InverseProperty("EstatusJornada")]
    public virtual ICollection<Jornada> Jornadas { get; set; } = new List<Jornada>();
}
