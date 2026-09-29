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

    [Required(ErrorMessage = "El nombre del tipo de resultado es obligatorio.")]
    [StringLength(30, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 30 caracteres.")]
    [RegularExpression(@"^[A-Za-zÁÉÍÓÚáéíóúÑñÜü\s]+$", ErrorMessage = "El nombre solo puede contener letras y espacios.")]
    [Column("TipoResultado")]
    public string TipoResultadoNombre { get; set; } = null!;

    [InverseProperty("TipoResultado")]
    public virtual ICollection<JornadaPartido> JornadaPartidos { get; set; } = new List<JornadaPartido>();

    [InverseProperty("TipoResultado")]
    public virtual ICollection<JornadaPronosticoDetalle> JornadaPronosticoDetalles { get; set; } = new List<JornadaPronosticoDetalle>();
}
