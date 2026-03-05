CREATE UNIQUE INDEX IX_UserCommunityVote
ON Votes(UserId, CommunityId)
WHERE CommunityId IS NOT NULL;
GO

SELECT * FROM Communities;
GO