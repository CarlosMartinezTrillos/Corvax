CREATE TABLE CommunityRanks (
    Id INT PRIMARY KEY,
    Name NVARCHAR(100),
    OrderGroup NVARCHAR(100),
    Level INT,
    MinScore FLOAT,
    BorderColor NVARCHAR(20),
    GlowColor NVARCHAR(20),
    HasAnimatedBorder BIT,
    HasBackgroundEffect BIT,
    HasCrownIcon BIT,
    HasParticleEffect BIT
);
GO