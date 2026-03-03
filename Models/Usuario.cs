using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LigaMXCore.Models;

[Table("Usuario")]
public partial class Usuario
{
    [Key]
    public int UsuarioId { get; set; }

    [Column("UserName")]
    public string UsuarioNombre { get; set; } = null!;

    public string Password { get; set; } = null!;
}
