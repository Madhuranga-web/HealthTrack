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
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108011527_InitialCreate'
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

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108011527_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] nvarchar(450) NOT NULL,
        [FullName] nvarchar(100) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedUtc] datetime2 NOT NULL,
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

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108011527_InitialCreate'
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

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108011527_InitialCreate'
)
BEGIN
    CREATE TABLE [ActivityLogs] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [Action] nvarchar(120) NOT NULL,
        [CreatedUtc] datetime2 NOT NULL,
        CONSTRAINT [PK_ActivityLogs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ActivityLogs_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108011527_InitialCreate'
)
BEGIN
    CREATE TABLE [Appointments] (
        [Id] int NOT NULL IDENTITY,
        [ClientId] nvarchar(450) NOT NULL,
        [TrainerId] nvarchar(450) NOT NULL,
        [StartsUtc] datetime2 NOT NULL,
        [DurationMinutes] int NOT NULL,
        [Purpose] nvarchar(300) NOT NULL,
        [Status] int NOT NULL,
        [CreatedUtc] datetime2 NOT NULL,
        CONSTRAINT [PK_Appointments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Appointments_AspNetUsers_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Appointments_AspNetUsers_TrainerId] FOREIGN KEY ([TrainerId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108011527_InitialCreate'
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

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108011527_InitialCreate'
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

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108011527_InitialCreate'
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

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108011527_InitialCreate'
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

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108011527_InitialCreate'
)
BEGIN
    CREATE TABLE [ClientProfiles] (
        [UserId] nvarchar(450) NOT NULL,
        [HeightCm] decimal(9,2) NULL,
        [Goals] nvarchar(500) NULL,
        CONSTRAINT [PK_ClientProfiles] PRIMARY KEY ([UserId]),
        CONSTRAINT [FK_ClientProfiles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108011527_InitialCreate'
)
BEGIN
    CREATE TABLE [DietPlans] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(120) NOT NULL,
        [DailyCalorieTarget] int NOT NULL,
        [TrainerId] nvarchar(450) NOT NULL,
        [ClientId] nvarchar(450) NOT NULL,
        [CreatedUtc] datetime2 NOT NULL,
        CONSTRAINT [PK_DietPlans] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DietPlans_AspNetUsers_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_DietPlans_AspNetUsers_TrainerId] FOREIGN KEY ([TrainerId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108011527_InitialCreate'
)
BEGIN
    CREATE TABLE [HealthMetrics] (
        [Id] int NOT NULL IDENTITY,
        [ClientId] nvarchar(450) NOT NULL,
        [WeightKg] decimal(9,2) NOT NULL,
        [HeightCm] decimal(9,2) NOT NULL,
        [Bmi] decimal(9,2) NOT NULL,
        [RecordedUtc] datetime2 NOT NULL,
        CONSTRAINT [PK_HealthMetrics] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_HealthMetrics_AspNetUsers_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108011527_InitialCreate'
)
BEGIN
    CREATE TABLE [MealLogs] (
        [Id] int NOT NULL IDENTITY,
        [ClientId] nvarchar(450) NOT NULL,
        [Food] nvarchar(120) NOT NULL,
        [CaloriesConsumed] int NOT NULL,
        [ProteinGrams] decimal(9,2) NOT NULL,
        [LoggedUtc] datetime2 NOT NULL,
        CONSTRAINT [PK_MealLogs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_MealLogs_AspNetUsers_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108011527_InitialCreate'
)
BEGIN
    CREATE TABLE [Notifications] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [Message] nvarchar(300) NOT NULL,
        [IsRead] bit NOT NULL,
        [CreatedUtc] datetime2 NOT NULL,
        CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Notifications_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108011527_InitialCreate'
)
BEGIN
    CREATE TABLE [TrainerAssignments] (
        [Id] int NOT NULL IDENTITY,
        [TrainerId] nvarchar(450) NOT NULL,
        [ClientId] nvarchar(450) NOT NULL,
        [AssignedUtc] datetime2 NOT NULL,
        CONSTRAINT [PK_TrainerAssignments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TrainerAssignments_AspNetUsers_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_TrainerAssignments_AspNetUsers_TrainerId] FOREIGN KEY ([TrainerId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108011527_InitialCreate'
)
BEGIN
    CREATE TABLE [TrainerFeedback] (
        [Id] int NOT NULL IDENTITY,
        [TrainerId] nvarchar(450) NOT NULL,
        [ClientId] nvarchar(450) NOT NULL,
        [Message] nvarchar(1000) NOT NULL,
        [CreatedUtc] datetime2 NOT NULL,
        CONSTRAINT [PK_TrainerFeedback] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TrainerFeedback_AspNetUsers_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_TrainerFeedback_AspNetUsers_TrainerId] FOREIGN KEY ([TrainerId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108011527_InitialCreate'
)
BEGIN
    CREATE TABLE [TrainerProfiles] (
        [UserId] nvarchar(450) NOT NULL,
        [Specialty] nvarchar(120) NOT NULL,
        CONSTRAINT [PK_TrainerProfiles] PRIMARY KEY ([UserId]),
        CONSTRAINT [FK_TrainerProfiles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108011527_InitialCreate'
)
BEGIN
    CREATE TABLE [WorkoutPlans] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(120) NOT NULL,
        [Description] nvarchar(1000) NOT NULL,
        [TrainerId] nvarchar(450) NOT NULL,
        [ClientId] nvarchar(450) NOT NULL,
        [CreatedUtc] datetime2 NOT NULL,
        CONSTRAINT [PK_WorkoutPlans] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_WorkoutPlans_AspNetUsers_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_WorkoutPlans_AspNetUsers_TrainerId] FOREIGN KEY ([TrainerId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108011527_InitialCreate'
)
BEGIN
    CREATE TABLE [DietPlanMeals] (
        [Id] int NOT NULL IDENTITY,
        [DietPlanId] int NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [FoodDescription] nvarchar(500) NOT NULL,
        [CalorieTarget] int NOT NULL,
        [ProteinGrams] decimal(9,2) NOT NULL,
        [ScheduledTime] time NOT NULL,
        CONSTRAINT [PK_DietPlanMeals] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DietPlanMeals_DietPlans_DietPlanId] FOREIGN KEY ([DietPlanId]) REFERENCES [DietPlans] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108011527_InitialCreate'
)
BEGIN
    CREATE TABLE [WorkoutExercises] (
        [Id] int NOT NULL IDENTITY,
        [WorkoutPlanId] int NOT NULL,
        [Name] nvarchar(120) NOT NULL,
        [Sets] int NOT NULL,
        [Repetitions] int NOT NULL,
        [DurationMinutes] int NOT NULL,
        CONSTRAINT [PK_WorkoutExercises] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_WorkoutExercises_WorkoutPlans_WorkoutPlanId] FOREIGN KEY ([WorkoutPlanId]) REFERENCES [WorkoutPlans] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108011527_InitialCreate'
)
BEGIN
    CREATE TABLE [WorkoutLogs] (
        [Id] int NOT NULL IDENTITY,
        [ClientId] nvarchar(450) NOT NULL,
        [WorkoutPlanId] int NOT NULL,
        [CompletedExercise] nvarchar(120) NOT NULL,
        [DurationMinutes] int NOT NULL,
        [CaloriesBurned] int NOT NULL,
        [LoggedUtc] datetime2 NOT NULL,
        CONSTRAINT [PK_WorkoutLogs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_WorkoutLogs_AspNetUsers_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_WorkoutLogs_WorkoutPlans_WorkoutPlanId] FOREIGN KEY ([WorkoutPlanId]) REFERENCES [WorkoutPlans] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108011527_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ActivityLogs_UserId_CreatedUtc] ON [ActivityLogs] ([UserId], [CreatedUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108011527_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Appointments_ClientId] ON [Appointments] ([ClientId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108011527_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Appointments_TrainerId_StartsUtc_Status] ON [Appointments] ([TrainerId], [StartsUtc], [Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108011527_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108011527_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108011527_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108011527_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108011527_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108011527_InitialCreate'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108011527_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108011527_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_DietPlanMeals_DietPlanId] ON [DietPlanMeals] ([DietPlanId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108011527_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_DietPlans_ClientId] ON [DietPlans] ([ClientId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108011527_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_DietPlans_TrainerId] ON [DietPlans] ([TrainerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108011527_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_HealthMetrics_ClientId_RecordedUtc] ON [HealthMetrics] ([ClientId], [RecordedUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108011527_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_MealLogs_ClientId] ON [MealLogs] ([ClientId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108011527_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Notifications_UserId_IsRead_CreatedUtc] ON [Notifications] ([UserId], [IsRead], [CreatedUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108011527_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_TrainerAssignments_ClientId] ON [TrainerAssignments] ([ClientId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108011527_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_TrainerAssignments_TrainerId_ClientId] ON [TrainerAssignments] ([TrainerId], [ClientId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108011527_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_TrainerFeedback_ClientId] ON [TrainerFeedback] ([ClientId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108011527_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_TrainerFeedback_TrainerId] ON [TrainerFeedback] ([TrainerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108011527_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_WorkoutExercises_WorkoutPlanId] ON [WorkoutExercises] ([WorkoutPlanId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108011527_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_WorkoutLogs_ClientId] ON [WorkoutLogs] ([ClientId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108011527_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_WorkoutLogs_WorkoutPlanId] ON [WorkoutLogs] ([WorkoutPlanId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108011527_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_WorkoutPlans_ClientId] ON [WorkoutPlans] ([ClientId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108011527_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_WorkoutPlans_TrainerId] ON [WorkoutPlans] ([TrainerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108011527_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261108011527_InitialCreate', N'10.0.14');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108011946_AddSystemSettings'
)
BEGIN
    CREATE TABLE [SystemSettings] (
        [Key] nvarchar(80) NOT NULL,
        [Value] nvarchar(250) NOT NULL,
        CONSTRAINT [PK_SystemSettings] PRIMARY KEY ([Key])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108011946_AddSystemSettings'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261108011946_AddSystemSettings', N'10.0.14');
END;

COMMIT;
GO
