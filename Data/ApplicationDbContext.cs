using System;
using System.Collections.Generic;
using LigaMXCore.Models;
using Microsoft.EntityFrameworkCore;

namespace LigaMXCore.Data;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Equipo> Equipos { get; set; }

    public virtual DbSet<Estadio> Estadios { get; set; }

    public virtual DbSet<Estado> Estados { get; set; }

    public virtual DbSet<EstatusJornada> EstatusJornada { get; set; }

    public virtual DbSet<EstatusPartido> EstatusPartido { get; set; }

    public virtual DbSet<JornadaPartido> JornadaPartidos { get; set; }

    public virtual DbSet<JornadaPronostico> JornadaPronosticos { get; set; }

    public virtual DbSet<JornadaPronosticoDetalle> JornadaPronosticoDetalles { get; set; }

    public virtual DbSet<Jornada> Jornada { get; set; }

    public virtual DbSet<Municipio> Municipios { get; set; }

    public virtual DbSet<Pais> Pais { get; set; }

    public virtual DbSet<Participante> Participante { get; set; }

    public virtual DbSet<Partido> Partidos { get; set; }

    public virtual DbSet<Temporada> Temporada { get; set; }

    public virtual DbSet<TipoResultado> TipoResultado { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)

        => optionsBuilder.UseSqlite("Data Source=liga.db");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Equipo>(entity =>
        {
            entity.Property(e => e.EquipoId).ValueGeneratedOnAdd();

            entity.HasOne(d => d.Municipio).WithMany(p => p.Equipos).OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Estadio>(entity =>
        {
            entity.Property(e => e.EstadioId).ValueGeneratedOnAdd();

            entity.HasOne(d => d.Municipio).WithMany(p => p.Estadios).OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Estado>(entity =>
        {
            entity.Property(e => e.EstadoId).ValueGeneratedOnAdd();

            entity.HasOne(d => d.Pais).WithMany(p => p.Estados).OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<EstatusJornada>(entity =>
        {
            entity.Property(e => e.EstatusJornadaId).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<EstatusPartido>(entity =>
        {
            entity.Property(e => e.EstatusPartidoId).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<JornadaPartido>(entity =>
        {
            entity.Property(e => e.JornadaPartidoId).ValueGeneratedOnAdd();

            entity.HasOne(d => d.Estadio).WithMany(p => p.JornadaPartidos).OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.EstatusPartido).WithMany(p => p.JornadaPartidos).OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Jornada).WithMany(p => p.JornadaPartidos).OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Partido).WithMany(p => p.JornadaPartidos).OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.TipoResultado).WithMany(p => p.JornadaPartidos).OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<JornadaPronostico>(entity =>
        {
            entity.Property(e => e.JornadaPronosticoId).ValueGeneratedOnAdd();

            entity.HasOne(d => d.Jornada).WithMany(p => p.JornadaPronosticos).OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Participante).WithMany(p => p.JornadaPronosticos).OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<JornadaPronosticoDetalle>(entity =>
        {
            entity.Property(e => e.JornadaPronosticoDetalleId).ValueGeneratedOnAdd();

            entity.HasOne(d => d.JornadaPronostico).WithMany(p => p.JornadaPronosticoDetalles).OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.TipoResultado).WithMany(p => p.JornadaPronosticoDetalles).OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Jornada>(entity =>
        {
            entity.Property(e => e.JornadaId).ValueGeneratedOnAdd();

            entity.HasOne(d => d.Temporada).WithMany(p => p.Jornada).OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Municipio>(entity =>
        {
            entity.Property(e => e.MunicipioId).ValueGeneratedOnAdd();

            entity.HasOne(d => d.Estado).WithMany(p => p.Municipios).OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Pais>(entity =>
        {
            entity.Property(e => e.PaisId).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<Participante>(entity =>
        {
            entity.Property(e => e.ParticipanteId).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<Partido>(entity =>
        {
            entity.Property(e => e.PartidoId).ValueGeneratedOnAdd();

            entity.HasOne(d => d.EquipoLocal).WithMany(p => p.PartidoEquipoLocals).OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.EquipoVisita).WithMany(p => p.PartidoEquipoVisita).OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Temporada>(entity =>
        {
            entity.Property(e => e.TemporadaId).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<TipoResultado>(entity =>
        {
            entity.Property(e => e.TipoResultadoId).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.Property(e => e.UsuarioId).ValueGeneratedOnAdd();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
