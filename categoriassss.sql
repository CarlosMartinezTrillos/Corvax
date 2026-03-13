SELECT c.Id, c.Name, cat.Id as CatId, cat.Name as CatName, cat.ColorHex
FROM Communities c
LEFT JOIN Categories cat ON cat.CommunityId = c.Id
ORDER BY c.Id;
GO

-- ══════════════════════════════════════════════════════════
--  Poblar CommunityCategories
--  + actualizar MainCategoryId en Communities
-- ══════════════════════════════════════════════════════════

-- Historia Militar → Main: Historia, Extra: Naval
INSERT INTO CommunityCategories (CommunityId, CategoryId, IsMain) VALUES
(1, 2, 1),  -- Historia      = MAIN
(1, 6, 0);  -- Naval         = extra

-- Geopolitica → Main: Geopolitica, Extra: Analisis
INSERT INTO CommunityCategories (CommunityId, CategoryId, IsMain) VALUES
(2, 5, 1),  -- Geopolitica   = MAIN
(2, 3, 0);  -- Analisis      = extra

-- Tecnologia Militar → Main: Tecnologia, Extra: Aereo
INSERT INTO CommunityCategories (CommunityId, CategoryId, IsMain) VALUES
(3, 4, 1),  -- Tecnologia    = MAIN
(3, 7, 0);  -- Aereo         = extra

-- Conflictos Activos → Main: Actualidad (única)
INSERT INTO CommunityCategories (CommunityId, CategoryId, IsMain) VALUES
(4, 1, 1);  -- Actualidad    = MAIN

-- ── Actualizar MainCategoryId en Communities ──────────────
UPDATE Communities SET MainCategoryId = 2 WHERE Id = 1;  -- Historia
UPDATE Communities SET MainCategoryId = 5 WHERE Id = 2;  -- Geopolitica
UPDATE Communities SET MainCategoryId = 4 WHERE Id = 3;  -- Tecnologia
UPDATE Communities SET MainCategoryId = 1 WHERE Id = 4;  -- Actualidad

-- ── Verificar ─────────────────────────────────────────────
 SELECT cc.CommunityId, c.Name, cc.CategoryId, cat.Name, cc.IsMain
 FROM CommunityCategories cc
 JOIN Communities c ON c.Id = cc.CommunityId
 JOIN Categories cat ON cat.Id = cc.CategoryId
 ORDER BY cc.CommunityId, cc.IsMain DESC;

go