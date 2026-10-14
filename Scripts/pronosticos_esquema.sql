-- Endurece JornadaPronostico / JornadaPronosticoDetalle (tablas vacias al aplicar).
-- Agrega FK Detalle->JornadaPartido, UNIQUEs y ON DELETE CASCADE cabecera->detalle.
PRAGMA foreign_keys=OFF;
BEGIN;
DROP TABLE JornadaPronosticoDetalle;
DROP TABLE JornadaPronostico;
CREATE TABLE JornadaPronostico (
    JornadaPronosticoId INTEGER PRIMARY KEY,
    JornadaId INTEGER NOT NULL,
    ParticipanteId INTEGER NOT NULL,
    FOREIGN KEY(JornadaId) REFERENCES Jornada(JornadaId),
    FOREIGN KEY(ParticipanteId) REFERENCES Participante(ParticipanteId),
    UNIQUE(JornadaId, ParticipanteId)
);
CREATE TABLE JornadaPronosticoDetalle (
    JornadaPronosticoDetalleId INTEGER PRIMARY KEY,
    JornadaPronosticoId INTEGER NOT NULL,
    JornadaPartidoId INTEGER NOT NULL,
    GolLocal INTEGER NOT NULL,
    GolVisita INTEGER NOT NULL,
    Puntos INTEGER NULL,
    TipoResultadoId INTEGER NOT NULL,
    FOREIGN KEY(JornadaPronosticoId) REFERENCES JornadaPronostico(JornadaPronosticoId) ON DELETE CASCADE,
    FOREIGN KEY(JornadaPartidoId) REFERENCES JornadaPartido(JornadaPartidoId),
    FOREIGN KEY(TipoResultadoId) REFERENCES TipoResultado(TipoResultadoId),
    UNIQUE(JornadaPronosticoId, JornadaPartidoId)
);
COMMIT;
PRAGMA foreign_keys=ON;
