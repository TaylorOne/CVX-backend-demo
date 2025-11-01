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

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250206010029_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250206010029_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] nvarchar(450) NOT NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250206010029_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250206010029_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250206010029_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250206010029_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250206010029_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] nvarchar(450) NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250206010029_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250206010029_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250206010029_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250206010029_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250206010029_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250206010029_InitialCreate'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250206010029_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250206010029_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250206010029_InitialCreate', N'8.0.12');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250315235646_ApplicationUser'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [AvatarUrl] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250315235646_ApplicationUser'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [Bio] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250315235646_ApplicationUser'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [CreatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250315235646_ApplicationUser'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [FirstName] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250315235646_ApplicationUser'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [LastName] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250315235646_ApplicationUser'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [LinkedIn] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250315235646_ApplicationUser'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250315235646_ApplicationUser', N'8.0.12');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250316182947_RoleAssignments_Industries_Skills'
)
BEGIN
    CREATE TABLE [Industries] (
        [Id] int NOT NULL IDENTITY,
        [Industry] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Industries] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250316182947_RoleAssignments_Industries_Skills'
)
BEGIN
    CREATE TABLE [RoleAssignments] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [Role] int NOT NULL,
        [ScopeId] int NOT NULL,
        [ScopeType] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [InviteStatus] int NOT NULL,
        CONSTRAINT [PK_RoleAssignments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RoleAssignments_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250316182947_RoleAssignments_Industries_Skills'
)
BEGIN
    CREATE TABLE [Skills] (
        [Id] int NOT NULL IDENTITY,
        [Skill] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Skills] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250316182947_RoleAssignments_Industries_Skills'
)
BEGIN
    CREATE TABLE [ApplicationUserIndustries] (
        [IndustriesId] int NOT NULL,
        [UsersId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_ApplicationUserIndustries] PRIMARY KEY ([IndustriesId], [UsersId]),
        CONSTRAINT [FK_ApplicationUserIndustries_AspNetUsers_UsersId] FOREIGN KEY ([UsersId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ApplicationUserIndustries_Industries_IndustriesId] FOREIGN KEY ([IndustriesId]) REFERENCES [Industries] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250316182947_RoleAssignments_Industries_Skills'
)
BEGIN
    CREATE TABLE [ApplicationUserSkills] (
        [SkillsId] int NOT NULL,
        [UsersId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_ApplicationUserSkills] PRIMARY KEY ([SkillsId], [UsersId]),
        CONSTRAINT [FK_ApplicationUserSkills_AspNetUsers_UsersId] FOREIGN KEY ([UsersId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ApplicationUserSkills_Skills_SkillsId] FOREIGN KEY ([SkillsId]) REFERENCES [Skills] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250316182947_RoleAssignments_Industries_Skills'
)
BEGIN
    CREATE INDEX [IX_ApplicationUserIndustries_UsersId] ON [ApplicationUserIndustries] ([UsersId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250316182947_RoleAssignments_Industries_Skills'
)
BEGIN
    CREATE INDEX [IX_ApplicationUserSkills_UsersId] ON [ApplicationUserSkills] ([UsersId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250316182947_RoleAssignments_Industries_Skills'
)
BEGIN
    CREATE INDEX [IX_RoleAssignments_UserId] ON [RoleAssignments] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250316182947_RoleAssignments_Industries_Skills'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250316182947_RoleAssignments_Industries_Skills', N'8.0.12');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250316185943_MakeAppUserFieldsNullable'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUsers]') AND [c].[name] = N'LinkedIn');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUsers] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [AspNetUsers] ALTER COLUMN [LinkedIn] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250316185943_MakeAppUserFieldsNullable'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUsers]') AND [c].[name] = N'LastName');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUsers] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [AspNetUsers] ALTER COLUMN [LastName] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250316185943_MakeAppUserFieldsNullable'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUsers]') AND [c].[name] = N'FirstName');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUsers] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [AspNetUsers] ALTER COLUMN [FirstName] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250316185943_MakeAppUserFieldsNullable'
)
BEGIN
    DECLARE @var3 sysname;
    SELECT @var3 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUsers]') AND [c].[name] = N'Bio');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUsers] DROP CONSTRAINT [' + @var3 + '];');
    ALTER TABLE [AspNetUsers] ALTER COLUMN [Bio] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250316185943_MakeAppUserFieldsNullable'
)
BEGIN
    DECLARE @var4 sysname;
    SELECT @var4 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUsers]') AND [c].[name] = N'AvatarUrl');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUsers] DROP CONSTRAINT [' + @var4 + '];');
    ALTER TABLE [AspNetUsers] ALTER COLUMN [AvatarUrl] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250316185943_MakeAppUserFieldsNullable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250316185943_MakeAppUserFieldsNullable', N'8.0.12');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250406135236_ChangeIndustriesToExperience'
)
BEGIN
    DROP TABLE [ApplicationUserIndustries];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250406135236_ChangeIndustriesToExperience'
)
BEGIN
    DROP TABLE [Industries];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250406135236_ChangeIndustriesToExperience'
)
BEGIN
    CREATE TABLE [Experience] (
        [Id] int NOT NULL IDENTITY,
        [Sector] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Experience] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250406135236_ChangeIndustriesToExperience'
)
BEGIN
    CREATE TABLE [ApplicationUserExperience] (
        [SectorsId] int NOT NULL,
        [UsersId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_ApplicationUserExperience] PRIMARY KEY ([SectorsId], [UsersId]),
        CONSTRAINT [FK_ApplicationUserExperience_AspNetUsers_UsersId] FOREIGN KEY ([UsersId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ApplicationUserExperience_Experience_SectorsId] FOREIGN KEY ([SectorsId]) REFERENCES [Experience] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250406135236_ChangeIndustriesToExperience'
)
BEGIN
    CREATE INDEX [IX_ApplicationUserExperience_UsersId] ON [ApplicationUserExperience] ([UsersId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250406135236_ChangeIndustriesToExperience'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250406135236_ChangeIndustriesToExperience', N'8.0.12');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250409001639_CollaborativesStakingTiers'
)
BEGIN
    ALTER TABLE [ApplicationUserExperience] DROP CONSTRAINT [FK_ApplicationUserExperience_Experience_SectorsId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250409001639_CollaborativesStakingTiers'
)
BEGIN
    ALTER TABLE [Experience] DROP CONSTRAINT [PK_Experience];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250409001639_CollaborativesStakingTiers'
)
BEGIN
    EXEC sp_rename N'[Experience]', N'Sectors';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250409001639_CollaborativesStakingTiers'
)
BEGIN
    ALTER TABLE [Sectors] ADD CONSTRAINT [PK_Sectors] PRIMARY KEY ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250409001639_CollaborativesStakingTiers'
)
BEGIN
    CREATE TABLE [Collaboratives] (
        [Id] int NOT NULL IDENTITY,
        [ParentCollaborative] int NULL,
        [Name] nvarchar(max) NULL,
        [Description] nvarchar(max) NULL,
        [RevenueShare] decimal(18,2) NOT NULL,
        [IndirectCosts] decimal(18,2) NOT NULL,
        [CollabLeaderCompensation] decimal(18,2) NOT NULL,
        [PayoutFrequency] nvarchar(max) NULL,
        [CreatorId] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [ApprovalStatus] int NOT NULL,
        CONSTRAINT [PK_Collaboratives] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250409001639_CollaborativesStakingTiers'
)
BEGIN
    CREATE TABLE [CollaborativeExperience] (
        [CollaborativesId] int NOT NULL,
        [SectorsId] int NOT NULL,
        CONSTRAINT [PK_CollaborativeExperience] PRIMARY KEY ([CollaborativesId], [SectorsId]),
        CONSTRAINT [FK_CollaborativeExperience_Collaboratives_CollaborativesId] FOREIGN KEY ([CollaborativesId]) REFERENCES [Collaboratives] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_CollaborativeExperience_Sectors_SectorsId] FOREIGN KEY ([SectorsId]) REFERENCES [Sectors] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250409001639_CollaborativesStakingTiers'
)
BEGIN
    CREATE TABLE [CollaborativeSkills] (
        [CollaborativesId] int NOT NULL,
        [SkillsId] int NOT NULL,
        CONSTRAINT [PK_CollaborativeSkills] PRIMARY KEY ([CollaborativesId], [SkillsId]),
        CONSTRAINT [FK_CollaborativeSkills_Collaboratives_CollaborativesId] FOREIGN KEY ([CollaborativesId]) REFERENCES [Collaboratives] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_CollaborativeSkills_Skills_SkillsId] FOREIGN KEY ([SkillsId]) REFERENCES [Skills] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250409001639_CollaborativesStakingTiers'
)
BEGIN
    CREATE TABLE [StakingTiers] (
        [Id] int NOT NULL IDENTITY,
        [Tier] nvarchar(max) NOT NULL,
        [ExchangeRate] decimal(18,2) NOT NULL,
        [CollaborativeId] int NULL,
        CONSTRAINT [PK_StakingTiers] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_StakingTiers_Collaboratives_CollaborativeId] FOREIGN KEY ([CollaborativeId]) REFERENCES [Collaboratives] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250409001639_CollaborativesStakingTiers'
)
BEGIN
    CREATE INDEX [IX_CollaborativeExperience_SectorsId] ON [CollaborativeExperience] ([SectorsId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250409001639_CollaborativesStakingTiers'
)
BEGIN
    CREATE INDEX [IX_CollaborativeSkills_SkillsId] ON [CollaborativeSkills] ([SkillsId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250409001639_CollaborativesStakingTiers'
)
BEGIN
    CREATE INDEX [IX_StakingTiers_CollaborativeId] ON [StakingTiers] ([CollaborativeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250409001639_CollaborativesStakingTiers'
)
BEGIN
    ALTER TABLE [ApplicationUserExperience] ADD CONSTRAINT [FK_ApplicationUserExperience_Sectors_SectorsId] FOREIGN KEY ([SectorsId]) REFERENCES [Sectors] ([Id]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250409001639_CollaborativesStakingTiers'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250409001639_CollaborativesStakingTiers', N'8.0.12');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250409105411_SkillGroups'
)
BEGIN
    ALTER TABLE [Skills] ADD [SkillGroupsId] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250409105411_SkillGroups'
)
BEGIN
    CREATE TABLE [SkillGroups] (
        [Id] int NOT NULL IDENTITY,
        [GroupName] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_SkillGroups] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250409105411_SkillGroups'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250409105411_SkillGroups', N'8.0.12');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250413122533_CollaborativeAddWebsiteUrl'
)
BEGIN
    ALTER TABLE [Collaboratives] ADD [WebsiteUrl] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250413122533_CollaborativeAddWebsiteUrl'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250413122533_CollaborativeAddWebsiteUrl', N'8.0.12');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250420120611_FixCollabStakingTierRelationship'
)
BEGIN
    ALTER TABLE [StakingTiers] DROP CONSTRAINT [FK_StakingTiers_Collaboratives_CollaborativeId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250420120611_FixCollabStakingTierRelationship'
)
BEGIN
    ALTER TABLE [StakingTiers] ADD CONSTRAINT [FK_StakingTiers_Collaboratives_CollaborativeId] FOREIGN KEY ([CollaborativeId]) REFERENCES [Collaboratives] ([Id]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250420120611_FixCollabStakingTierRelationship'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250420120611_FixCollabStakingTierRelationship', N'8.0.12');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250422214208_NetworkMemberStatus'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [NetworkMemberStatus] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250422214208_NetworkMemberStatus'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250422214208_NetworkMemberStatus', N'8.0.12');
END;
GO

COMMIT;
GO

