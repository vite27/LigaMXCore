
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
