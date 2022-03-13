IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220226155247_AddedUserReviews')
BEGIN
    CREATE TABLE [Reviews] (
        [ReviewID] nvarchar(450) NOT NULL,
        [ReviewCreatorID] nvarchar(max) NULL,
        [AttachedPictureLinks] nvarchar(max) NULL,
        [ReviewSubjectName] nvarchar(max) NULL,
        [ReviewSubjectGenre] nvarchar(max) NULL,
        [ReviewText] nvarchar(max) NULL,
        [OwnerRating] int NOT NULL,
        CONSTRAINT [PK_Reviews] PRIMARY KEY ([ReviewID])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220226155247_AddedUserReviews')
BEGIN
    CREATE TABLE [Tags] (
        [TagID] nvarchar(450) NOT NULL,
        [TagName] nvarchar(max) NULL,
        CONSTRAINT [PK_Tags] PRIMARY KEY ([TagID])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220226155247_AddedUserReviews')
BEGIN
    CREATE TABLE [ReviewAndTagRelations] (
        [RelationID] nvarchar(450) NOT NULL,
        [ReviewID] nvarchar(450) NOT NULL,
        [TagID] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_ReviewAndTagRelations] PRIMARY KEY ([RelationID]),
        CONSTRAINT [FK_ReviewAndTagRelations_Reviews_ReviewID] FOREIGN KEY ([ReviewID]) REFERENCES [Reviews] ([ReviewID]) ON DELETE CASCADE,
        CONSTRAINT [FK_ReviewAndTagRelations_Tags_TagID] FOREIGN KEY ([TagID]) REFERENCES [Tags] ([TagID]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220226155247_AddedUserReviews')
BEGIN
    CREATE INDEX [IX_ReviewAndTagRelations_ReviewID] ON [ReviewAndTagRelations] ([ReviewID]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220226155247_AddedUserReviews')
BEGIN
    CREATE INDEX [IX_ReviewAndTagRelations_TagID] ON [ReviewAndTagRelations] ([TagID]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220226155247_AddedUserReviews')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20220226155247_AddedUserReviews', N'5.0.13');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220227131841_AddedReviewsCreationDate')
BEGIN
    ALTER TABLE [Reviews] ADD [CreationDate] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220227131841_AddedReviewsCreationDate')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20220227131841_AddedReviewsCreationDate', N'5.0.13');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220227205404_RatingsAndLikes')
BEGIN
    CREATE TABLE [ReviewLikes] (
        [LikeID] nvarchar(450) NOT NULL,
        [ReviewID] nvarchar(450) NULL,
        [UserID] nvarchar(max) NULL,
        CONSTRAINT [PK_ReviewLikes] PRIMARY KEY ([LikeID]),
        CONSTRAINT [FK_ReviewLikes_Reviews_ReviewID] FOREIGN KEY ([ReviewID]) REFERENCES [Reviews] ([ReviewID]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220227205404_RatingsAndLikes')
BEGIN
    CREATE TABLE [ReviewRatings] (
        [RatingID] nvarchar(450) NOT NULL,
        [ReviewID] nvarchar(450) NULL,
        [UserID] nvarchar(max) NULL,
        [RatingValue] int NOT NULL,
        CONSTRAINT [PK_ReviewRatings] PRIMARY KEY ([RatingID]),
        CONSTRAINT [FK_ReviewRatings_Reviews_ReviewID] FOREIGN KEY ([ReviewID]) REFERENCES [Reviews] ([ReviewID]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220227205404_RatingsAndLikes')
BEGIN
    CREATE INDEX [IX_ReviewLikes_ReviewID] ON [ReviewLikes] ([ReviewID]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220227205404_RatingsAndLikes')
BEGIN
    CREATE INDEX [IX_ReviewRatings_ReviewID] ON [ReviewRatings] ([ReviewID]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220227205404_RatingsAndLikes')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20220227205404_RatingsAndLikes', N'5.0.13');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220228184110_AddedReviewCreatorName')
BEGIN
    ALTER TABLE [Reviews] ADD [ReviewCreatorName] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220228184110_AddedReviewCreatorName')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20220228184110_AddedReviewCreatorName', N'5.0.13');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220311085117_AddedUserPreferences')
BEGIN
    CREATE TABLE [UserPreferences] (
        [PreferenceID] nvarchar(450) NOT NULL,
        [UserID] nvarchar(max) NULL,
        [IsDarkTheme] bit NOT NULL,
        [IsEnglishVersion] bit NOT NULL,
        CONSTRAINT [PK_UserPreferences] PRIMARY KEY ([PreferenceID])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220311085117_AddedUserPreferences')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20220311085117_AddedUserPreferences', N'5.0.13');
END;
GO

COMMIT;
GO

