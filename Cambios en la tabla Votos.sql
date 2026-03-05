ALTER TABLE Votes
ADD CommunityId INT NULL;

ALTER TABLE Votes
ADD CONSTRAINT FK_Votes_Communities
FOREIGN KEY (CommunityId) REFERENCES Communities(Id);