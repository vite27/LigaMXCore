CREATE TABLE Usuario (
    UsuarioId INTEGER PRIMARY KEY,
    Username TEXT NOT NULL,
    Password TEXT NOT NULL
);

CREATE TABLE Participante (
    ParticipanteId INTEGER PRIMARY KEY,
    Nombres TEXT NOT NULL,
    ApellidoPaterno TEXT NOT NULL,
    ApellidoMaterno TEXT NOT NULL
);

CREATE TABLE Pais (
    PaisId INTEGER PRIMARY KEY,
    Pais TEXT NOT NULL
);

CREATE TABLE Estado (
    EstadoId INTEGER PRIMARY KEY,
    Estado TEXT NOT NULL,
    PaisId INTEGER NOT NULL,
	FOREIGN KEY(PaisId) REFERENCES Pais(PaisId)
);

CREATE TABLE Municipio (
    MunicipioId INTEGER PRIMARY KEY,
    Municipio TEXT NOT NULL,
    EstadoId INTEGER NOT NULL,
	FOREIGN KEY(EstadoId) REFERENCES Estado(EstadoId)
);

CREATE TABLE EstatusJornada (
    EstatusJornadaId INTEGER PRIMARY KEY,
    EstatusJornada TEXT NOT NULL
);

CREATE TABLE EstatusPartido (
    EstatusPartidoId INTEGER PRIMARY KEY,
    EstatusPartido TEXT NOT NULL
);

CREATE TABLE Estadio (
    EstadioId INTEGER PRIMARY KEY,
    Estadio TEXT NOT NULL,
    Alias TEXT NULL,
    Direccion TEXT NULL,
    CodigoPostal TEXT NULL,
    MunicipioId INTEGER NOT NULL,
	FOREIGN KEY(MunicipioId) REFERENCES Municipio(MunicipioId)
);

CREATE TABLE Equipo (
    EquipoId INTEGER PRIMARY KEY,
    Equipo TEXT NOT NULL,
    Alias TEXT NULL,
    MunicipioId INTEGER NOT NULL,
    EquipoLogo TEXT NULL,
	FOREIGN KEY(MunicipioId) REFERENCES Municipio(MunicipioId)
);

CREATE TABLE Temporada (
    TemporadaId INTEGER PRIMARY KEY,
    Temporada TEXT NOT NULL,
    Comentarios TEXT NULL
);

CREATE TABLE TipoResultado (
    TipoResultadoId INTEGER PRIMARY KEY,
    TipoResultado TEXT NOT NULL
);

CREATE TABLE Jornada (
    JornadaId INTEGER PRIMARY KEY,
    Orden INTEGER NOT NULL,
    Jornada TEXT NOT NULL,
    TemporadaId INTEGER NOT NULL,
	FOREIGN KEY(TemporadaId) REFERENCES Temporada(TemporadaId)
);

CREATE TABLE Partido (
    PartidoId INTEGER PRIMARY KEY,
    EquipoLocalId INTEGER NOT NULL,
    EquipoVisitaId INTEGER NOT NULL,
	FOREIGN KEY(EquipoLocalId) REFERENCES Equipo(EquipoId),
	FOREIGN KEY(EquipoVisitaId) REFERENCES Equipo(EquipoId)
);

CREATE TABLE JornadaPartido (
    JornadaPartidoId INTEGER PRIMARY KEY,
    JornadaId INTEGER NOT NULL,
    PartidoId INTEGER NOT NULL,
    EstadioId INTEGER NOT NULL,
	GolLocal INTEGER NULL,
	GolVisita INTEGER NULL,
	EstatusPartidoId INTEGER NOT NULL,
	TipoResultadoId INTEGER NOT NULL,
	FOREIGN KEY(JornadaId) REFERENCES Jornada(JornadaId),
	FOREIGN KEY(PartidoId) REFERENCES Partido(PartidoId),
	FOREIGN KEY(EstadioId) REFERENCES Estadio(EstadioId),
	FOREIGN KEY(EstatusPartidoId) REFERENCES EstatusPartido(EstatusPartidoId),
	FOREIGN KEY(TipoResultadoId) REFERENCES TipoResultado(TipoResultadoId)
);

CREATE TABLE JornadaPronostico (
    JornadaPronosticoId INTEGER PRIMARY KEY,
    JornadaId INTEGER NOT NULL,
    ParticipanteId INTEGER NOT NULL,
	FOREIGN KEY(JornadaId) REFERENCES Jornada(JornadaId),
	FOREIGN KEY(ParticipanteId) REFERENCES Participante(ParticipanteId)
);

CREATE TABLE JornadaPronosticoDetalle (
    JornadaPronosticoDetalleId INTEGER PRIMARY KEY,
    JornadaPronosticoId INTEGER NOT NULL,
    JornadaPartidoId INTEGER NOT NULL,
	GolLocal INTEGER NOT NULL,
	GolVisita INTEGER NOT NULL,
	Puntos INTEGER NULL,
	TipoResultadoId INTEGER NOT NULL,
	FOREIGN KEY(JornadaPronosticoId) REFERENCES JornadaPronostico(JornadaPronosticoId),
	FOREIGN KEY(TipoResultadoId) REFERENCES TipoResultado(TipoResultadoId)
);


