-- ═══════════════════════════════════════════════════════════════
-- STEP 1 — Tabla auxiliar de números (1 al 10000)
-- ═══════════════════════════════════════════════════════════════

IF OBJECT_ID('tempdb..#nums') IS NOT NULL DROP TABLE #nums;

SELECT TOP 10000
    ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS n
INTO #nums
FROM master..spt_values a
CROSS JOIN master..spt_values b;
GO

INSERT INTO Users (Username, Email, PasswordHash, Role, CreatedAt, IsActive)
SELECT
    'corvax_bot_' + CAST(n AS VARCHAR(10)),
    'bot_' + CAST(n AS VARCHAR(10)) + '@corvax.bot',
    'BOT_ACCOUNT_DISABLED',
    'Bot',
    DATEADD(SECOND, -n, GETDATE()),
    0
FROM #nums
WHERE NOT EXISTS (
    SELECT 1 FROM Users
    WHERE Username = 'corvax_bot_' + CAST(n AS VARCHAR(10))
);
GO

-- ═══════════════════════════════════════════════════════════════
-- STEP 3 — Capturar IDs de los bots
-- ═══════════════════════════════════════════════════════════════

IF OBJECT_ID('tempdb..#bots') IS NOT NULL DROP TABLE #bots;

SELECT
    Id,
    ROW_NUMBER() OVER (ORDER BY Id) AS rn
INTO #bots
FROM Users
WHERE Username LIKE 'corvax_bot_%';
GO

-- ═══════════════════════════════════════════════════════════════
-- STEP 4 — Insertar votos
-- ═══════════════════════════════════════════════════════════════

-- ── Comunidad 1 — 777 upvotes (Easter egg: Reliquia de Corvax 🎖)
INSERT INTO Votes (UserId, PostId, CommentId, VoteType, CreatedAt, CommunityId)
SELECT b.Id, NULL, NULL, 1, DATEADD(SECOND, -b.rn, GETDATE()), 1
FROM #bots b
WHERE b.rn <= 777
AND NOT EXISTS (
    SELECT 1 FROM Votes WHERE UserId = b.Id AND CommunityId = 1
);
GO

-- ── Comunidad 2 — 2940 upvotes (Diamante — Bandada Sombría 💠)
INSERT INTO Votes (UserId, PostId, CommentId, VoteType, CreatedAt, CommunityId)
SELECT b.Id, NULL, NULL, 1, DATEADD(SECOND, -b.rn, GETDATE()), 2
FROM #bots b
WHERE b.rn <= 2940
AND NOT EXISTS (
    SELECT 1 FROM Votes WHERE UserId = b.Id AND CommunityId = 2
);

-- ── Comunidad 3 — 4340 upvotes (Ébano — Guardián de Corvax 🟣)
INSERT INTO Votes (UserId, PostId, CommentId, VoteType, CreatedAt, CommunityId)
SELECT b.Id, NULL, NULL, 1, DATEADD(SECOND, -b.rn, GETDATE()), 3
FROM #bots b
WHERE b.rn <= 4340
AND NOT EXISTS (
    SELECT 1 FROM Votes WHERE UserId = b.Id AND CommunityId = 3
);

-- ── Comunidad 4 — 10000 upvotes (Artefacto Ancestral 👑)
INSERT INTO Votes (UserId, PostId, CommentId, VoteType, CreatedAt, CommunityId)
SELECT b.Id, NULL, NULL, 1, DATEADD(SECOND, -b.rn, GETDATE()), 4
FROM #bots b
WHERE b.rn <= 10000
AND NOT EXISTS (
    SELECT 1 FROM Votes WHERE UserId = b.Id AND CommunityId = 4
);
GO

-- ═══════════════════════════════════════════════════════════════
-- STEP 5 — Sincronizar contadores en Communities
-- ═══════════════════════════════════════════════════════════════

UPDATE c
SET c.UpVotes = v.total
FROM Communities c
INNER JOIN (
    SELECT CommunityId, COUNT(*) AS total
    FROM Votes
    WHERE CommunityId IN (1,2,3,4)
      AND VoteType = 1
    GROUP BY CommunityId
) v ON c.Id = v.CommunityId;
GO

SELECT
    c.Id,
    c.Name,
    c.UpVotes,
    c.DownVotes,
    COUNT(v.Id) AS VotosEnTablaVotes
FROM Communities c
LEFT JOIN Votes v ON v.CommunityId = c.Id AND v.VoteType = 1
WHERE c.Id IN (1,2,3,4)
GROUP BY c.Id, c.Name, c.UpVotes, c.DownVotes
ORDER BY c.Id;
GO

-- ═══════════════════════════════════════════════════════════════
-- STEP 7 — Limpieza
-- ═══════════════════════════════════════════════════════════════

DROP TABLE #nums;
DROP TABLE #bots;
GO

SELECT TOP 2 Id, Title, CommunityId FROM Posts WHERE CommunityId = 1 ORDER BY Id;
SELECT TOP 2 Id, Title, CommunityId FROM Posts WHERE CommunityId = 2 ORDER BY Id;
SELECT TOP 2 Id, Title, CommunityId FROM Posts WHERE CommunityId = 3 ORDER BY Id;
SELECT TOP 2 Id, Title, CommunityId FROM Posts WHERE CommunityId = 4 ORDER BY Id;
GO

-- ═══════════════════════════════════════════════════════════════
-- STEP 1 — Reusar los bots ya creados
-- ═══════════════════════════════════════════════════════════════

IF OBJECT_ID('tempdb..#bots') IS NOT NULL DROP TABLE #bots;

SELECT
    Id,
    ROW_NUMBER() OVER (ORDER BY Id) AS rn
INTO #bots
FROM Users
WHERE Username LIKE 'corvax_bot_%';

-- ═══════════════════════════════════════════════════════════════
-- STEP 2 — Insertar votos por post
-- ═══════════════════════════════════════════════════════════════

-- ── Post 4 — La Guerra de Malvinas — 69 upvotes 😄
INSERT INTO Votes (UserId, PostId, CommentId, VoteType, CreatedAt, CommunityId)
SELECT b.Id, 4, NULL, 1, DATEADD(SECOND, -b.rn, GETDATE()), NULL
FROM #bots b
WHERE b.rn <= 69
AND NOT EXISTS (
    SELECT 1 FROM Votes WHERE UserId = b.Id AND PostId = 4
);

-- ── Post 5 — La Guerra de Corea — 420 upvotes 🟣
INSERT INTO Votes (UserId, PostId, CommentId, VoteType, CreatedAt, CommunityId)
SELECT b.Id, 5, NULL, 1, DATEADD(SECOND, -b.rn, GETDATE()), NULL
FROM #bots b
WHERE b.rn <= 420
AND NOT EXISTS (
    SELECT 1 FROM Votes WHERE UserId = b.Id AND PostId = 5
);

-- ── Post 9 — La OTAN tras Ucrania — 404 upvotes 🐛
INSERT INTO Votes (UserId, PostId, CommentId, VoteType, CreatedAt, CommunityId)
SELECT b.Id, 9, NULL, 1, DATEADD(SECOND, -b.rn, GETDATE()), NULL
FROM #bots b
WHERE b.rn <= 404
AND NOT EXISTS (
    SELECT 1 FROM Votes WHERE UserId = b.Id AND PostId = 9
);

-- ── Post 10 — Iran vs Israel — 1337 upvotes 💻
INSERT INTO Votes (UserId, PostId, CommentId, VoteType, CreatedAt, CommunityId)
SELECT b.Id, 10, NULL, 1, DATEADD(SECOND, -b.rn, GETDATE()), NULL
FROM #bots b
WHERE b.rn <= 1337
AND NOT EXISTS (
    SELECT 1 FROM Votes WHERE UserId = b.Id AND PostId = 10
);

-- ── Post 7 — El F-35 — 666 upvotes 💀
INSERT INTO Votes (UserId, PostId, CommentId, VoteType, CreatedAt, CommunityId)
SELECT b.Id, 7, NULL, 1, DATEADD(SECOND, -b.rn, GETDATE()), NULL
FROM #bots b
WHERE b.rn <= 666
AND NOT EXISTS (
    SELECT 1 FROM Votes WHERE UserId = b.Id AND PostId = 7
);

-- ── Post 8 — Drones Bayraktar — 2100 upvotes 💠
INSERT INTO Votes (UserId, PostId, CommentId, VoteType, CreatedAt, CommunityId)
SELECT b.Id, 8, NULL, 1, DATEADD(SECOND, -b.rn, GETDATE()), NULL
FROM #bots b
WHERE b.rn <= 2100
AND NOT EXISTS (
    SELECT 1 FROM Votes WHERE UserId = b.Id AND PostId = 8
);

-- ── Post 1 — Conflicto en Gaza — 7777 upvotes 👑
INSERT INTO Votes (UserId, PostId, CommentId, VoteType, CreatedAt, CommunityId)
SELECT b.Id, 1, NULL, 1, DATEADD(SECOND, -b.rn, GETDATE()), NULL
FROM #bots b
WHERE b.rn <= 7777
AND NOT EXISTS (
    SELECT 1 FROM Votes WHERE UserId = b.Id AND PostId = 1
);

-- ── Post 2 — Guerra en Ucrania — 10500 upvotes 🏺
INSERT INTO Votes (UserId, PostId, CommentId, VoteType, CreatedAt, CommunityId)
SELECT b.Id, 2, NULL, 1, DATEADD(SECOND, -b.rn, GETDATE()), NULL
FROM #bots b
WHERE b.rn <= 10000  -- tenemos 10000 bots, tope real
AND NOT EXISTS (
    SELECT 1 FROM Votes WHERE UserId = b.Id AND PostId = 2
);

-- ═══════════════════════════════════════════════════════════════
-- STEP 3 — Verificar conteos reales desde Votes
-- ═══════════════════════════════════════════════════════════════

SELECT
    v.PostId,
    p.Title,
    COUNT(*) AS UpVotes
FROM Votes v
INNER JOIN Posts p ON p.Id = v.PostId
WHERE v.PostId IN (1, 2, 4, 5, 7, 8, 9, 10)
  AND v.VoteType = 1
GROUP BY v.PostId, p.Title
ORDER BY v.PostId;

-- ═══════════════════════════════════════════════════════════════
-- STEP 4 — Limpieza
-- ═══════════════════════════════════════════════════════════════

DROP TABLE #bots;
GO


-- ═══════════════════════════════════════════════════════════════
-- STEP 1 — Reusar los bots ya creados
-- ═══════════════════════════════════════════════════════════════

IF OBJECT_ID('tempdb..#bots') IS NOT NULL DROP TABLE #bots;

SELECT
    Id,
    ROW_NUMBER() OVER (ORDER BY Id) AS rn
INTO #bots
FROM Users
WHERE Username LIKE 'corvax_bot_%';

-- ═══════════════════════════════════════════════════════════════
-- STEP 2 — Insertar downvotes por post
-- ═══════════════════════════════════════════════════════════════

-- ── Post 4 — La Guerra de Malvinas — 13 downvotes 👁
INSERT INTO Votes (UserId, PostId, CommentId, VoteType, CreatedAt, CommunityId)
SELECT b.Id, 4, NULL, -1, DATEADD(SECOND, -b.rn, GETDATE()), NULL
FROM #bots b
WHERE b.rn BETWEEN 5001 AND 5013
AND NOT EXISTS (
    SELECT 1 FROM Votes WHERE UserId = b.Id AND PostId = 4 AND VoteType = -1
);

-- ── Post 5 — La Guerra de Corea — 280 downvotes 🟤
INSERT INTO Votes (UserId, PostId, CommentId, VoteType, CreatedAt, CommunityId)
SELECT b.Id, 5, NULL, -1, DATEADD(SECOND, -b.rn, GETDATE()), NULL
FROM #bots b
WHERE b.rn BETWEEN 5001 AND 5280
AND NOT EXISTS (
    SELECT 1 FROM Votes WHERE UserId = b.Id AND PostId = 5 AND VoteType = -1
);

-- ── Post 9 — La OTAN tras Ucrania — 66 downvotes 🔥
INSERT INTO Votes (UserId, PostId, CommentId, VoteType, CreatedAt, CommunityId)
SELECT b.Id, 9, NULL, -1, DATEADD(SECOND, -b.rn, GETDATE()), NULL
FROM #bots b
WHERE b.rn BETWEEN 5001 AND 5066
AND NOT EXISTS (
    SELECT 1 FROM Votes WHERE UserId = b.Id AND PostId = 9 AND VoteType = -1
);

-- ── Post 10 — Iran vs Israel — 777 downvotes ⚙
INSERT INTO Votes (UserId, PostId, CommentId, VoteType, CreatedAt, CommunityId)
SELECT b.Id, 10, NULL, -1, DATEADD(SECOND, -b.rn, GETDATE()), NULL
FROM #bots b
WHERE b.rn BETWEEN 5001 AND 5777
AND NOT EXISTS (
    SELECT 1 FROM Votes WHERE UserId = b.Id AND PostId = 10 AND VoteType = -1
);

-- ── Post 7 — El F-35 — 404 downvotes 👻
INSERT INTO Votes (UserId, PostId, CommentId, VoteType, CreatedAt, CommunityId)
SELECT b.Id, 7, NULL, -1, DATEADD(SECOND, -b.rn, GETDATE()), NULL
FROM #bots b
WHERE b.rn BETWEEN 5001 AND 5404
AND NOT EXISTS (
    SELECT 1 FROM Votes WHERE UserId = b.Id AND PostId = 7 AND VoteType = -1
);

-- ── Post 8 — Drones Bayraktar — 1337 downvotes 💻
INSERT INTO Votes (UserId, PostId, CommentId, VoteType, CreatedAt, CommunityId)
SELECT b.Id, 8, NULL, -1, DATEADD(SECOND, -b.rn, GETDATE()), NULL
FROM #bots b
WHERE b.rn BETWEEN 5001 AND 6337
AND NOT EXISTS (
    SELECT 1 FROM Votes WHERE UserId = b.Id AND PostId = 8 AND VoteType = -1
);

-- ── Post 1 — Conflicto en Gaza — 560 downvotes ⚠
INSERT INTO Votes (UserId, PostId, CommentId, VoteType, CreatedAt, CommunityId)
SELECT b.Id, 1, NULL, -1, DATEADD(SECOND, -b.rn, GETDATE()), NULL
FROM #bots b
WHERE b.rn BETWEEN 5001 AND 5560
AND NOT EXISTS (
    SELECT 1 FROM Votes WHERE UserId = b.Id AND PostId = 1 AND VoteType = -1
);

-- ── Post 2 — Guerra en Ucrania — 999 downvotes ⏳
INSERT INTO Votes (UserId, PostId, CommentId, VoteType, CreatedAt, CommunityId)
SELECT b.Id, 2, NULL, -1, DATEADD(SECOND, -b.rn, GETDATE()), NULL
FROM #bots b
WHERE b.rn BETWEEN 5001 AND 5999
AND NOT EXISTS (
    SELECT 1 FROM Votes WHERE UserId = b.Id AND PostId = 2 AND VoteType = -1
);

-- ═══════════════════════════════════════════════════════════════
-- STEP 3 — Verificar conteos reales desde Votes
-- ═══════════════════════════════════════════════════════════════

SELECT
    v.PostId,
    p.Title,
    SUM(CASE WHEN v.VoteType =  1 THEN 1 ELSE 0 END) AS UpVotes,
    SUM(CASE WHEN v.VoteType = -1 THEN 1 ELSE 0 END) AS DownVotes
FROM Votes v
INNER JOIN Posts p ON p.Id = v.PostId
WHERE v.PostId IN (1, 2, 4, 5, 7, 8, 9, 10)
GROUP BY v.PostId, p.Title
ORDER BY v.PostId;

-- ═══════════════════════════════════════════════════════════════
-- STEP 4 — Limpieza
-- ═══════════════════════════════════════════════════════════════

DROP TABLE #bots;
GO


IF OBJECT_ID('tempdb..#bots') IS NOT NULL DROP TABLE #bots;

SELECT
    Id,
    ROW_NUMBER() OVER (ORDER BY Id) AS rn
INTO #bots
FROM Users
WHERE Username LIKE 'corvax_bot_%';

-- ── Post 1 — Conflicto en Gaza — 560 downvotes
-- Usamos bots desde 8001 — ninguno votó upvote en post 1 (solo llegamos a 7777)
INSERT INTO Votes (UserId, PostId, CommentId, VoteType, CreatedAt, CommunityId)
SELECT b.Id, 1, NULL, -1, DATEADD(SECOND, -b.rn, GETDATE()), NULL
FROM #bots b
WHERE b.rn BETWEEN 8001 AND 8560
AND NOT EXISTS (
    SELECT 1 FROM Votes WHERE UserId = b.Id AND PostId = 1
);

-- ── Post 2 — Guerra en Ucrania — 999 downvotes
-- Post 2 tuvo 10000 upvotes, todos los bots ya votaron — necesitamos eliminar
-- los upvotes de 999 bots del final y reemplazarlos por downvotes
DELETE FROM Votes
WHERE PostId = 2
  AND VoteType = 1
  AND UserId IN (
      SELECT TOP 999 Id FROM #bots ORDER BY rn DESC
  );

INSERT INTO Votes (UserId, PostId, CommentId, VoteType, CreatedAt, CommunityId)
SELECT b.Id, 2, NULL, -1, DATEADD(SECOND, -b.rn, GETDATE()), NULL
FROM #bots b
WHERE b.rn BETWEEN 9002 AND 10000
AND NOT EXISTS (
    SELECT 1 FROM Votes WHERE UserId = b.Id AND PostId = 2
);

-- Verificar los dos posts corregidos
SELECT
    v.PostId,
    p.Title,
    SUM(CASE WHEN v.VoteType =  1 THEN 1 ELSE 0 END) AS UpVotes,
    SUM(CASE WHEN v.VoteType = -1 THEN 1 ELSE 0 END) AS DownVotes
FROM Votes v
INNER JOIN Posts p ON p.Id = v.PostId
WHERE v.PostId IN (1, 2)
GROUP BY v.PostId, p.Title;

DROP TABLE #bots;
GO