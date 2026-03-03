CREATE TABLE CommunityRankHistory (
    Id INT PRIMARY KEY IDENTITY,
    CommunityId INT,
    RankId INT,
    AchievedAt DATETIME,
    LostAt DATETIME NULL
);
GO