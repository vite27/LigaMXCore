using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LigaMXCore.Models;

[Table("Estadio")]
public partial class Estadio
{
    [Key]
    public int EstadioId { get; set; }

    [Required(ErrorMessage = "El nombre del estadio es obligatorio.")]
    [StringLength(80, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 80 caracteres.")]
    [Column("Estadio")]
    public string EstadioNombre { get; set; } = null!;

    [StringLength(50, MinimumLength = 2, ErrorMessage = "El alias debe tener entre 2 y 50 caracteres.")]
    [RegularExpression(@"^[A-Za-zÁÉÍÓÚáéíóúÑñÜü0-9\s/.,'-]+$", ErrorMessage = "El alias contiene caracteres no permitidos.")]
    public string? Alias { get; set; }

    [StringLength(200, ErrorMessage = "La dirección no puede exceder 200 caracteres.")]
    public string? Direccion { get; set; }

    [RegularExpression(@"^\d{5}$", ErrorMessage = "El código postal debe tener 5 dígitos.")]
    public string? CodigoPostal { get; set; }

    [Required(ErrorMessage = "El municipio es obligatorio.")]
    public int MunicipioId { get; set; }

    [InverseProperty("Estadio")]
    public virtual ICollection<JornadaPartido> JornadaPartidos { get; set; } = new List<JornadaPartido>();

    [ForeignKey("MunicipioId")]
    [InverseProperty("Estadios")]
    public virtual Municipio Municipio { get; set; } = null!;
}
