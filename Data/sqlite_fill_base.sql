
INSERT INTO Usuario (Username, Password) VALUES 
('admin','admin');

INSERT INTO Participante (Nombres, ApellidoPaterno, ApellidoMaterno) VALUES
('Héctor Miguel', 'Rodríguez' ,'Vite');

INSERT INTO Pais (Pais) VALUES
('México');

INSERT INTO EstatusJornada (EstatusJornada) VALUES
('Pendiente'),
('En Curso'),
('Finalizada');

INSERT INTO EstatusPartido (EstatusPartido) VALUES
('Pendiente'),
('En Curso'),
('Finalizado');

INSERT INTO TipoResultado (TipoResultado) VALUES
('Victoria Local'),
('Victoria Visita'),
('Empate');

INSERT INTO Estado (Estado, PaisId) VALUES
('Aguascalientes', 1),
('Baja California', 1),
('Baja California Sur', 1),
('Campeche', 1),
('Chiapas', 1),
('Chihuahua', 1),
('Ciudad de México', 1),
('Coahuila', 1),
('Colima', 1),
('Durango', 1),
('Guanajuato', 1),
('Guerrero', 1),
('Hidalgo', 1),
('Jalisco', 1),
('México', 1),
('Michoacán', 1),
('Morelos', 1),
('Nayarit', 1),
('Nuevo León', 1),
('Oaxaca', 1),
('Puebla', 1),
('Querétaro', 1),
('Quintana Roo', 1),
('San Luis Potosí', 1),
('Sinaloa', 1),
('Sonora', 1),
('Tabasco', 1),
('Tamaulipas', 1),
('Tlaxcala', 1),
('Veracruz', 1),
('Yucatán', 1),
('Zacatecas', 1);

INSERT INTO Temporada (Temporada, Comentarios) VALUES
('Clausura 2026', '60vo torneo corto');

INSERT INTO Municipio (Municipio, EstadoId) VALUES
('Aguascalientes',       (SELECT EstadoId FROM Estado WHERE Estado = 'Aguascalientes')),
('Ciudad Juárez',        (SELECT EstadoId FROM Estado WHERE Estado = 'Chihuahua')),
('Ciudad de México',     (SELECT EstadoId FROM Estado WHERE Estado = 'Ciudad de México')),
('Guadalupe',            (SELECT EstadoId FROM Estado WHERE Estado = 'Nuevo León')),
('León',                 (SELECT EstadoId FROM Estado WHERE Estado = 'Guanajuato')),
('Mazatlán',             (SELECT EstadoId FROM Estado WHERE Estado = 'Sinaloa')),
('Pachuca',              (SELECT EstadoId FROM Estado WHERE Estado = 'Hidalgo')),
('Puebla',               (SELECT EstadoId FROM Estado WHERE Estado = 'Puebla')),
('Querétaro',            (SELECT EstadoId FROM Estado WHERE Estado = 'Querétaro')),
('San Luis Potosí',      (SELECT EstadoId FROM Estado WHERE Estado = 'San Luis Potosí')),
('San Nicolás de los Garza', (SELECT EstadoId FROM Estado WHERE Estado = 'Nuevo León')),
('Tijuana',              (SELECT EstadoId FROM Estado WHERE Estado = 'Baja California')),
('Toluca',               (SELECT EstadoId FROM Estado WHERE Estado = 'México')),
('Torreón',              (SELECT EstadoId FROM Estado WHERE Estado = 'Coahuila')),
('Zapopan',              (SELECT EstadoId FROM Estado WHERE Estado = 'Jalisco'));

-- Agregado municipio necesario para Estadio Jalisco
INSERT INTO Municipio (Municipio, EstadoId) VALUES
('Guadalajara', (SELECT EstadoId FROM Estado WHERE Estado = 'Jalisco'));

INSERT INTO Estadio (Estadio, Alias, Direccion, CodigoPostal, MunicipioId) VALUES

-- América
('Estadio Banorte',
 'Estadio Azteca / El Coloso de Santa Úrsula',
 'Calzada de Tlalpan 3465, Col. Santa Úrsula Coapa',
 '04650',
 (SELECT MunicipioId FROM Municipio WHERE Municipio = 'Ciudad de México')),

-- Atlas
('Estadio Jalisco',
 'El Viejo Parque',
 'Calle 7 Colinas 1772, Col. Independencia',
 '44290',
 (SELECT MunicipioId FROM Municipio WHERE Municipio = 'Guadalajara')),

-- Atlético San Luis
('Estadio Libertad Financiera',
 'El Alfonso Lastras',
 'Av. Salvador Nava Martínez s/n, Col. Fraccionamiento Lomas',
 '78250',
 (SELECT MunicipioId FROM Municipio WHERE Municipio = 'San Luis Potosí')),

-- Cruz Azul / Puebla
('Estadio Cuauhtémoc',
 'El Coloso de Maravillas',
 'Calzada Ignacio Zaragoza 666, Col. Unidad Deportiva',
 '72220',
 (SELECT MunicipioId FROM Municipio WHERE Municipio = 'Puebla')),

-- Guadalajara (Chivas)
('Estadio Akron',
 'El Gigante de Zapopan / Estadio Chivas',
 'Circuito JVC 2800, Col. El Bajío',
 '45014',
 (SELECT MunicipioId FROM Municipio WHERE Municipio = 'Zapopan')),

-- FC Juárez
('Estadio Olímpico Benito Juárez',
 'El Olímpico',
 'Av. 16 de Septiembre s/n, Col. División del Norte',
 '32310',
 (SELECT MunicipioId FROM Municipio WHERE Municipio = 'Ciudad Juárez')),

-- León
('Estadio León',
 'El Nou Camp Mexicano',
 'Av. Adolfo López Mateos s/n, Col. La Martinica',
 '37500',
 (SELECT MunicipioId FROM Municipio WHERE Municipio = 'León')),

-- Mazatlán FC
('Estadio El Encanto',
 NULL,
 'Av. Insurgentes s/n, Col. Estadio',
 '82017',
 (SELECT MunicipioId FROM Municipio WHERE Municipio = 'Mazatlán')),

-- Monterrey (Rayados)
('Estadio BBVA',
 'El Gigante de Acero',
 'Av. Pablo Livas 2011, Col. La Pastora',
 '67140',
 (SELECT MunicipioId FROM Municipio WHERE Municipio = 'Guadalupe')),

-- Necaxa
('Estadio Victoria',
 'El Coloso de la Colonia Héroes',
 'Av. Manuel Zavala Madrigal 101, Col. Héroes',
 '20190',
 (SELECT MunicipioId FROM Municipio WHERE Municipio = 'Aguascalientes')),

-- Pachuca
('Estadio Hidalgo',
 'El Huracán',
 'Acceso al Estadio s/n, Col. Ex-Hacienda de Coscotitlán',
 '42064',
 (SELECT MunicipioId FROM Municipio WHERE Municipio = 'Pachuca')),

-- Pumas UNAM
('Estadio Olímpico Universitario',
 'El Estadio de los Pumas',
 'Av. Insurgentes Sur s/n, Col. Ciudad Universitaria',
 '04510',
 (SELECT MunicipioId FROM Municipio WHERE Municipio = 'Ciudad de México')),

-- Querétaro
('Estadio La Corregidora',
 'El Corregidora',
 'Av. de las Torres s/n, Col. Álamos 3ra Sección',
 '76160',
 (SELECT MunicipioId FROM Municipio WHERE Municipio = 'Querétaro')),

-- Santos Laguna
('Estadio TSM Corona',
 'El Templo del Desierto',
 'Calzada Territorio Santos Modelo 1, Col. Todos Los Santos',
 '27014',
 (SELECT MunicipioId FROM Municipio WHERE Municipio = 'Torreón')),

-- Tigres UANL
('Estadio Universitario',
 'El Volcán',
 'Av. Pedro de Alba s/n, Col. Ciudad Universitaria',
 '66451',
 (SELECT MunicipioId FROM Municipio WHERE Municipio = 'San Nicolás de los Garza')),

-- Tijuana (Xolos)
('Estadio Caliente',
 'El Nido de los Xolos',
 'Blvd. Agua Caliente 12027, Col. Hipódromo',
 '22020',
 (SELECT MunicipioId FROM Municipio WHERE Municipio = 'Tijuana')),

-- Toluca
('Estadio Nemesio Díez',
 'La Bombonera / El Infierno',
 'Av. Constituyentes Poniente 1000, Col. La Merced',
 '50080',
 (SELECT MunicipioId FROM Municipio WHERE Municipio = 'Toluca'));

INSERT INTO Equipo (Equipo, Alias, MunicipioId) VALUES

('América',         'Las Águilas',              (SELECT MunicipioId FROM Municipio WHERE Municipio = 'Ciudad de México')),
('Atlas',           'Los Zorros',               (SELECT MunicipioId FROM Municipio WHERE Municipio = 'Guadalajara')),
('Atlético San Luis','Los Tuneros',             (SELECT MunicipioId FROM Municipio WHERE Municipio = 'San Luis Potosí')),
('Cruz Azul',       'La Máquina Cementera',     (SELECT MunicipioId FROM Municipio WHERE Municipio = 'Puebla')),
('Deportivo Toluca','Los Diablos Rojos',         (SELECT MunicipioId FROM Municipio WHERE Municipio = 'Toluca')),
('FC Juárez',       'Los Bravos',               (SELECT MunicipioId FROM Municipio WHERE Municipio = 'Ciudad Juárez')),
('Guadalajara',     'Las Chivas / El Rebaño Sagrado', (SELECT MunicipioId FROM Municipio WHERE Municipio = 'Zapopan')),
('León',            'La Fiera',                 (SELECT MunicipioId FROM Municipio WHERE Municipio = 'León')),
('Mazatlán FC',     'Los Cañoneros',            (SELECT MunicipioId FROM Municipio WHERE Municipio = 'Mazatlán')),
('Monterrey',       'Los Rayados',              (SELECT MunicipioId FROM Municipio WHERE Municipio = 'Guadalupe')),
('Necaxa',          'Los Rayos',                (SELECT MunicipioId FROM Municipio WHERE Municipio = 'Aguascalientes')),
('Pachuca',         'Los Tuzos',                (SELECT MunicipioId FROM Municipio WHERE Municipio = 'Pachuca')),
('Puebla',          'La Franja',                (SELECT MunicipioId FROM Municipio WHERE Municipio = 'Puebla')),
('Pumas UNAM',      'Los Universitarios',       (SELECT MunicipioId FROM Municipio WHERE Municipio = 'Ciudad de México')),
('Querétaro',       'Los Gallos Blancos',       (SELECT MunicipioId FROM Municipio WHERE Municipio = 'Querétaro')),
('Santos Laguna',   'Los Guerreros',            (SELECT MunicipioId FROM Municipio WHERE Municipio = 'Torreón')),
('Tigres UANL',     'Los Felinos',              (SELECT MunicipioId FROM Municipio WHERE Municipio = 'San Nicolás de los Garza')),
('Tijuana',         'Los Xolos',                (SELECT MunicipioId FROM Municipio WHERE Municipio = 'Tijuana'));


INSERT INTO Partido (EquipoLocalId, EquipoVisitaId) VALUES
(1,2),(1,3),(1,4),(1,5),(1,6),(1,7),(1,8),(1,9),(1,10),(1,11),(1,12),(1,13),(1,14),(1,15),(1,16),(1,17),(1,18),
(2,3),(2,4),(2,5),(2,6),(2,7),(2,8),(2,9),(2,10),(2,11),(2,12),(2,13),(2,14),(2,15),(2,16),(2,17),(2,18),
(3,4),(3,5),(3,6),(3,7),(3,8),(3,9),(3,10),(3,11),(3,12),(3,13),(3,14),(3,15),(3,16),(3,17),(3,18),
(4,5),(4,6),(4,7),(4,8),(4,9),(4,10),(4,11),(4,12),(4,13),(4,14),(4,15),(4,16),(4,17),(4,18),
(5,6),(5,7),(5,8),(5,9),(5,10),(5,11),(5,12),(5,13),(5,14),(5,15),(5,16),(5,17),(5,18),
(6,7),(6,8),(6,9),(6,10),(6,11),(6,12),(6,13),(6,14),(6,15),(6,16),(6,17),(6,18),
(7,8),(7,9),(7,10),(7,11),(7,12),(7,13),(7,14),(7,15),(7,16),(7,17),(7,18),
(8,9),(8,10),(8,11),(8,12),(8,13),(8,14),(8,15),(8,16),(8,17),(8,18),
(9,10),(9,11),(9,12),(9,13),(9,14),(9,15),(9,16),(9,17),(9,18),
(10,11),(10,12),(10,13),(10,14),(10,15),(10,16),(10,17),(10,18),
(11,12),(11,13),(11,14),(11,15),(11,16),(11,17),(11,18),
(12,13),(12,14),(12,15),(12,16),(12,17),(12,18),
(13,14),(13,15),(13,16),(13,17),(13,18),
(14,15),(14,16),(14,17),(14,18),
(15,16),(15,17),(15,18),
(16,17),(16,18),
(17,18);