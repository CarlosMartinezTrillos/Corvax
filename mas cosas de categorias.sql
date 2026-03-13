CREATE TABLE CommunityCategories (
    CommunityId INT NOT NULL,
    CategoryId  INT NOT NULL,
    IsMain      BIT DEFAULT(0) NOT NULL,
    PRIMARY KEY (CommunityId, CategoryId),
    FOREIGN KEY (CommunityId) REFERENCES Communities(Id),
    FOREIGN KEY (CategoryId)  REFERENCES Categories(Id)
);
GO

SELECT Id, Name, ColorHex FROM Categories ORDER BY Name;
GO

-- ══════════════════════════════════════════════════════════
--  CORVAX — Categorías Globales nuevas (sin repetir las 7 existentes)
--  Existentes: Actualidad, Aéreo, Análisis, Geopolítica,
--              Historia, Naval, Tecnología
-- ══════════════════════════════════════════════════════════

INSERT INTO Categories (Name, ColorHex, Description, CommunityId) VALUES

-- ── POLÍTICA & GEOPOLÍTICA ────────────────────────────────
('Política',               '#b91c1c', 'Sistemas políticos, partidos y gobiernos',               NULL),
('Elecciones',             '#f97316', 'Procesos electorales y democracia',                      NULL),
('Diplomacia',             '#fb923c', 'Negociaciones y relaciones entre estados',               NULL),

-- ── MILITAR & SEGURIDAD ───────────────────────────────────
('Conflictos',             '#7f1d1d', 'Guerras, conflictos armados y zonas de tensión',         NULL),
('Estrategia Militar',     '#991b1b', 'Doctrina, táctica y operaciones militares',              NULL),
('Armamento',              '#c2410c', 'Tecnología bélica, armas y sistemas de defensa',         NULL),
('Seguridad Nacional',     '#9a3412', 'Inteligencia, contraterrorismo y defensa interior',      NULL),
('Historia Militar',       '#78350f', 'Guerras históricas, batallas y figuras militares',       NULL),
('Terrestre',              '#854d0e', 'Operaciones, vehículos y doctrina del ejército de tierra',NULL),

-- ── TECNOLOGÍA & CIENCIA ──────────────────────────────────
('Inteligencia Artificial','#2563eb', 'IA, machine learning y automatización',                  NULL),
('Ciberseguridad',         '#1d4ed8', 'Hacking, privacidad y seguridad digital',                NULL),
('Ciencia',                '#0ea5e9', 'Descubrimientos científicos y divulgación',              NULL),
('Espacio',                '#0284c7', 'Astronomía, exploración espacial y cosmología',          NULL),
('Energía',                '#0369a1', 'Energías renovables, nuclear y recursos energéticos',    NULL),

-- ── ECONOMÍA & FINANZAS ───────────────────────────────────
('Economía',               '#22c55e', 'Macroeconomía, mercados y tendencias globales',          NULL),
('Finanzas',               '#16a34a', 'Inversiones, bolsa y mercados financieros',              NULL),
('Criptomonedas',          '#15803d', 'Bitcoin, blockchain y economía descentralizada',         NULL),
('Negocios',               '#166534', 'Empresas, startups y mundo corporativo',                 NULL),
('Comercio',               '#14532d', 'Comercio internacional, aranceles y logística',          NULL),

-- ── SOCIEDAD & CULTURA ────────────────────────────────────
('Sociedad',               '#a855f7', 'Fenómenos sociales, tendencias y vida urbana',           NULL),
('Cultura',                '#9333ea', 'Arte, música, cine y expresión cultural',                NULL),
('Filosofía',              '#6d28d9', 'Pensamiento, ética y corrientes filosóficas',            NULL),
('Religión',               '#5b21b6', 'Religiones del mundo, espiritualidad y teología',        NULL),
('Educación',              '#4c1d95', 'Sistema educativo, pedagogía y conocimiento',            NULL),

-- ── MEDIO AMBIENTE ────────────────────────────────────────
('Medio Ambiente',         '#10b981', 'Ecología, cambio climático y sostenibilidad',            NULL),
('Clima',                  '#059669', 'Meteorología, fenómenos climáticos y calentamiento',     NULL),
('Naturaleza',             '#047857', 'Biodiversidad, fauna, flora y ecosistemas',              NULL),

-- ── SALUD & CIENCIAS SOCIALES ─────────────────────────────
('Salud',                  '#06b6d4', 'Medicina, pandemias y salud pública',                    NULL),
('Psicología',             '#0891b2', 'Comportamiento humano, mente y salud mental',            NULL),
('Demografía',             '#0e7490', 'Población, migraciones y estadísticas sociales',         NULL),

-- ── DEPORTE & ENTRETENIMIENTO ─────────────────────────────
('Deportes',               '#f59e0b', 'Fútbol, olimpiadas y competencias deportivas',           NULL),
('Esports',                '#d97706', 'Videojuegos competitivos y cultura gamer',               NULL),
('Entretenimiento',        '#b45309', 'Cine, series, música y cultura pop',                     NULL),

-- ── DEBATE & OPINIÓN ──────────────────────────────────────
('Debate',                 '#ec4899', 'Discusiones abiertas y confrontación de ideas',          NULL),
('Opinión',                '#db2777', 'Análisis, editoriales y puntos de vista',                NULL),
('Humor',                  '#be185d', 'Memes, sátira política y contenido cómico',              NULL),
('Pregunta',               '#9f1239', 'Consultas, encuestas y preguntas a la comunidad',        NULL),

-- ── REGIONAL ─────────────────────────────────────────────
('Latinoamérica',          '#84cc16', 'Noticias y análisis del continente latinoamericano',     NULL),
('Europa',                 '#65a30d', 'Política, sociedad y cultura europea',                   NULL),
('Asia',                   '#4d7c0f', 'Geopolítica, economía y cultura asiática',               NULL),
('África',                 '#3f6212', 'Conflictos, desarrollo y cultura africana',              NULL),
('Medio Oriente',          '#365314', 'Geopolítica, religión y conflictos en Oriente Medio',   NULL);
GO

SELECT Id, Name, Slug, MainCategoryId FROM Communities;
GO