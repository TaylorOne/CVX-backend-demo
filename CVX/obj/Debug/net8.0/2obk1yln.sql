BEGIN TRANSACTION;
GO

DROP TABLE [CollaborativeMemberRoles];
GO

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StakingTiers]') AND [c].[name] = N'ExchangeRate');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [StakingTiers] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [StakingTiers] ALTER COLUMN [ExchangeRate] decimal(18,2) NOT NULL;
GO

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Collaboratives]') AND [c].[name] = N'RevenueShare');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Collaboratives] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [Collaboratives] ALTER COLUMN [RevenueShare] decimal(18,2) NOT NULL;
GO

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Collaboratives]') AND [c].[name] = N'IndirectCosts');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Collaboratives] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [Collaboratives] ALTER COLUMN [IndirectCosts] decimal(18,2) NOT NULL;
GO

DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Collaboratives]') AND [c].[name] = N'CollabLeaderCompensation');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Collaboratives] DROP CONSTRAINT [' + @var3 + '];');
ALTER TABLE [Collaboratives] ALTER COLUMN [CollabLeaderCompensation] decimal(18,2) NOT NULL;
GO

CREATE TABLE [RoleAssignments] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [InviteStatus] int NOT NULL,
    [Role] int NOT NULL,
    [ScopeId] int NOT NULL,
    [ScopeType] int NOT NULL,
    CONSTRAINT [PK_RoleAssignments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RoleAssignments_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_RoleAssignments_UserId] ON [RoleAssignments] ([UserId]);
GO

DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'20250504194146_ChangeRoleAssignmentsAndAddPrecisionToDecimals';
GO

COMMIT;
GO

