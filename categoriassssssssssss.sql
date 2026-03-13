CREATE TABLE SavedPosts (
    Id      INT IDENTITY(1,1) PRIMARY KEY,
    UserId  INT NOT NULL,
    PostId  INT NOT NULL,
    SavedAt DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_SavedPosts_User
        FOREIGN KEY (UserId) REFERENCES Users(Id),

    CONSTRAINT FK_SavedPosts_Post
        FOREIGN KEY (PostId) REFERENCES Posts(Id) ON DELETE CASCADE,

    CONSTRAINT UQ_SavedPosts_UserPost
        UNIQUE (UserId, PostId)
);
GO