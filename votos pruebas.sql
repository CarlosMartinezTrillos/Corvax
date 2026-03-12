SELECT * FROM Users;
GO

-- Estructura de Users
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Users';

-- Estructura de CommunityVotes (o como se llame tu tabla de votos)
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Communities';

-- Un par de filas de muestra para ver datos reales
SELECT TOP 3 * FROM Users;
SELECT TOP 4 * FROM Communities;
GO

UPDATE Communities SET UpVotes = 10001 WHERE Id = 4;
GO

-- Encontrar la tabla de votos de comunidad
SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME LIKE '%ommunity%'
   OR TABLE_NAME LIKE '%ote%'
   OR TABLE_NAME LIKE '%follow%'
ORDER BY TABLE_NAME;
GO


SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Communities';
GO

-- Ver las 3 filas que ya existen (los 3 usuarios reales)
SELECT TOP 5 * FROM Communities; -- cambia el nombre si es diferente

-- Estructura de Votes
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Votes';

-- Muestra de filas reales
SELECT TOP 5 * FROM Votes;
GO

SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Votes';

SELECT TOP 3 * FROM Votes;
GO

INSERT INTO Votes (UserId, PostId, CommentId, VoteType, CreatedAt, CommunityId)
SELECT
    b.Id,
    NULL,
    NULL,
    1,
    DATEADD(SECOND, -b.rn, GETDATE()),
    4
FROM #bots b
WHERE b.rn <= 10000
AND NOT EXISTS (
    SELECT 1 FROM Votes
    WHERE UserId = b.Id AND CommunityId = 4
);
GO

SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Votes';

SELECT TOP 4 * FROM Votes;
GO