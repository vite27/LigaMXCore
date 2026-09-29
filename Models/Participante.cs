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

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 50 caracteres.")]
    [RegularExpression(@"^[A-Za-zÁÉÍÓÚáéíóúÑñÜü\s]+$", ErrorMessage = "El nombre solo puede contener letras y espacios.")]
    public string Nombres { get; set; } = null!;

    [Required(ErrorMessage = "El apellido paterno es obligatorio.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "El apellido debe tener entre 2 y 50 caracteres.")]
    [RegularExpression(@"^[A-Za-zÁÉÍÓÚáéíóúÑñÜü\s]+$", ErrorMessage = "El apellido solo puede contener letras y espacios.")]
    public string ApellidoPaterno { get; set; } = null!;

    [Required(ErrorMessage = "El apellido materno es obligatorio.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "El apellido debe tener entre 2 y 50 caracteres.")]
    [RegularExpression(@"^[A-Za-zÁÉÍÓÚáéíóúÑñÜü\s]+$", ErrorMessage = "El apellido solo puede contener letras y espacios.")]
    public string ApellidoMaterno { get; set; } = null!;

    [InverseProperty("Participante")]
    public virtual ICollection<JornadaPronostico> JornadaPronosticos { get; set; } = new List<JornadaPronostico>();
}
