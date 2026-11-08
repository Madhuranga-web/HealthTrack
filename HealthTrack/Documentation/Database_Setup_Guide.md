# HealthTrack database setup guide

## Supported local setup

Use Windows, Visual Studio 2026, the .NET 10 SDK, and either SQL Server Express LocalDB or SQL Server Express. Install the ASP.NET and web development workload. SQL Server Management Studio (SSMS) is optional but useful for inspecting tables and running the generated SQL script. The application uses `Microsoft.EntityFrameworkCore.SqlServer` and ASP.NET Core Identity.

The default connection in `HealthTrack/appsettings.json` is:

```text
Server=(localdb)\MSSQLLocalDB;Database=HealthTrack;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True
```

`Trusted_Connection=True` means Windows integrated authentication. There is no SQL username or password in source control. LocalDB is available only on Windows. For SQL Server Express, use a local instance such as:

```text
Server=.\SQLEXPRESS;Database=HealthTrack;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True
```

Prefer a development environment variable (`ConnectionStrings__DefaultConnection`) or Visual Studio user secrets to override the connection without editing source. The project has a `UserSecretsId`, so the Visual Studio **Manage User Secrets** command works. Never commit an environment-specific `appsettings.*.json` or credentials.

## Method A: EF Core migrations (recommended)

1. Install LocalDB or SQL Server Express and verify the SQL Server service/instance is running.
2. Open `HealthTrack.sln` in Visual Studio 2026; restore NuGet packages.
3. From the solution root run:

   ```powershell
   dotnet restore HealthTrack.sln
   dotnet build HealthTrack.sln
   dotnet tool install --global dotnet-ef --version 10.0.14
   dotnet ef database update --project HealthTrack --startup-project HealthTrack
   ```

4. EF Core creates the `HealthTrack` database if the Windows account has permission, creates all 22 application and Identity tables, and records `InitialCreate` and `AddSystemSettings` in `__EFMigrationsHistory`. On later runs it applies only pending migrations. It does not recreate existing tables or erase data.
5. Start the application. The default connection uses LocalDB. In Visual Studio, set `HealthTrack` as startup project and launch the `https` profile.

For SQL Server Express use the alternative connection string above. If your account cannot create databases, ask the SQL Server administrator to create an empty `HealthTrack` database and grant your Windows account `db_owner` during local setup, then rerun `dotnet ef database update`.

## Method B: generated SQL script in SSMS

`HealthTrack_Database_Setup.sql` is generated from the checked-in EF migrations using `dotnet ef migrations script --idempotent`. It includes the EF migration history logic. This is an alternative to Method A for creating the schema:

1. In SSMS connect to the chosen LocalDB or SQL Server Express instance using **Windows Authentication**. LocalDB server name: `(localdb)\MSSQLLocalDB`.
2. Create an empty database named `HealthTrack` in SSMS if it does not exist. The EF SQL script creates schema inside a selected database; it does not create the database itself.
3. Open `HealthTrack_Database_Setup.sql`, select `HealthTrack` in the SSMS database dropdown, and execute. Check the Messages pane for errors.
4. Refresh Tables and check `dbo.__EFMigrationsHistory` and the 22 tables listed below.

**Choose Method A or Method B for schema creation. Do not run both as separate schema creation procedures on the same database.** If you later enable the application demo seed, its startup `MigrateAsync()` checks the history table and performs no schema change when Method B was applied successfully.

## Fictional demo data

Demo seeding runs only when the application is in Development and `SeedDemoData=true`. The startup path applies pending EF migrations, then calls `DemoSeeder`. Provide a strong password via an environment variable or user secrets. Identity requires at least 10 characters, uppercase and lowercase letters, a digit, and a symbol. For a temporary PowerShell session:

```powershell
$env:HEALTHTRACK_SeedPassword = Read-Host 'Choose a local demo password'
$env:HEALTHTRACK_SeedDemoData = 'true'
dotnet run --project HealthTrack
Remove-Item Env:\HEALTHTRACK_SeedPassword, Env:\HEALTHTRACK_SeedDemoData
```

The prefixed environment variables are read by `Program.cs`. Alternatively, set `SeedPassword` and `SeedDemoData` in Visual Studio user secrets. Do not put the password in a committed file or command history. Seeding does not change passwords on accounts that already exist. Change an existing demo password through the app or reset the local database if needed.

The seed creates one administrator (`admin@healthtrack.test`), two trainers (`trainer@healthtrack.test`, `trainer2@healthtrack.test`), and four clients (`client@healthtrack.test` through `client4@healthtrack.test`). All names, emails and health data are fictional. It creates four assignments, four workout plans, four diet plans, eight workout logs, eight meal logs, eight health metrics, four appointments and six notifications. It checks each identifiable record before inserting and can be run repeatedly without duplicating these records. The initial admin is created only by this controlled local seed.

## Table map

| Table | Purpose / key relationship |
| --- | --- |
| AspNetUsers | Identity users and application names/active flags; string primary key. |
| AspNetRoles | Admin, Trainer and Client role definitions. |
| AspNetUserRoles | Composite key linking users and roles. |
| AspNetUserClaims | Claims owned by a user. |
| AspNetUserLogins | External login keys owned by a user. |
| AspNetUserTokens | User token keys owned by a user. |
| AspNetRoleClaims | Claims owned by a role. |
| ClientProfiles | One profile per client user ID. |
| TrainerProfiles | One profile per trainer user ID. |
| TrainerAssignments | Trainer/client pair; unique index prevents duplicate assignment. |
| WorkoutPlans | Trainer recommendation for a client. |
| WorkoutExercises | Exercises belonging to one workout plan. |
| WorkoutLogs | Client-recorded completion linked to a workout plan. |
| DietPlans | Trainer daily calorie target for a client. |
| DietPlanMeals | Scheduled recommended meals belonging to a diet plan. |
| MealLogs | Client-recorded actual food, calories and protein. |
| HealthMetrics | Client weight, height, calculated BMI and timestamp. |
| Appointments | Client/trainer booking, UTC start, duration and status. |
| Notifications | Per-user message with read/unread state. |
| TrainerFeedback | Trainer comment for a client. |
| ActivityLogs | Recorded user actions for reporting. |
| SystemSettings | Administrator dashboard message. |
| __EFMigrationsHistory | EF Core records applied migration IDs; not an application model table. |

All application tables have primary keys; Identity configures composite keys where appropriate. Foreign keys link profiles, assignments, plans, logs, appointments, notifications and feedback to users or parent plans. The unique `(TrainerId, ClientId)` assignment index prevents duplicate links. Restrict delete behavior protects historical health and activity records; deleting a plan cascades to its exercises or recommended meals, while workout logs restrict plan deletion. Query indexes cover health history, appointments, notifications and activity. EF maps metric and nutrition decimal values with precision 9,2 and uses SQL Server `datetime2` and `time` for date and time fields. Form ranges are enforced by server validation; they are not SQL CHECK constraints.

See [Database_ER.mmd](Database_ER.mmd) for the database relationship diagram and [AppDbContext.cs](../Data/AppDbContext.cs) for exact mappings. The EF migrations remain the authoritative schema.

## Verify in SSMS

Connect to `HealthTrack` and run:

```sql
SELECT MigrationId FROM dbo.__EFMigrationsHistory ORDER BY MigrationId;
SELECT name FROM sys.tables WHERE is_ms_shipped = 0 ORDER BY name;
SELECT fk.name, OBJECT_NAME(fk.parent_object_id) AS ChildTable,
       OBJECT_NAME(fk.referenced_object_id) AS ParentTable
FROM sys.foreign_keys AS fk ORDER BY ChildTable, fk.name;
SELECT i.name, OBJECT_NAME(i.object_id) AS TableName, i.is_unique
FROM sys.indexes AS i
WHERE i.name = 'IX_TrainerAssignments_TrainerId_ClientId';
```

Expect two migration IDs, 22 model tables plus `__EFMigrationsHistory`, foreign keys and a unique trainer assignment index. After demo seed, check counts with `SELECT COUNT(*)` on `AspNetUsers`, `TrainerAssignments`, `WorkoutPlans`, `DietPlans`, `WorkoutLogs`, `MealLogs`, `HealthMetrics`, `Appointments` and `Notifications`. Identity `PasswordHash` should be populated; it must not equal the chosen password. Do not publish database backups or user secret values.

## Troubleshooting

| Symptom | Action |
| --- | --- |
| LocalDB instance not found | Install SQL Server Express LocalDB; check `(localdb)\MSSQLLocalDB` in SSMS, or switch to SQL Server Express. |
| Login failed for Windows user | Verify Windows Authentication and permissions for the selected instance/database. |
| Cannot create database | Create an empty `HealthTrack` database with an authorized SQL account, then run migrations. |
| Table missing | Check the actual connection string and `__EFMigrationsHistory`; rerun Method A against the intended database. |
| Seed password rejected | Use uppercase, lowercase, digit, symbol and at least 10 characters; ensure the app runs in Development. |
| Duplicate demo email with another role | Resolve that local test account manually; the seed stops rather than changing its role. |
| Tool command not found | Install `.NET 10 SDK` and `dotnet-ef` 10.0.14, then restart the terminal. |
| SQL connection from this Linux workspace fails | LocalDB is Windows-only; use Windows Visual Studio/SSMS for the final LocalDB run. |

## Verification completed in the development workspace

On the Linux development workspace, `dotnet build HealthTrack.sln` succeeded with zero warnings, and `dotnet test HealthTrack.sln` passed 14 tests. EF Core reported no pending model changes. The SQL Server provider generated the idempotent `HealthTrack_Database_Setup.sql` script from both checked-in migrations. Static inspection of that script found 22 model tables, 23 primary-key constraints including the migration history table, 26 foreign keys, and the unique assignment index.

A relational SQLite test created the same EF model, ran the fictional seed twice, checked the seven Identity users and their role links, verified password hashes, and confirmed that duplicate assignments and an invalid foreign key are rejected. This tests seed logic and relational integrity, but it is **not** a SQL Server migration test.

A temporary SQL Server 2022 verification container was attempted but could not unpack because the managed Linux workspace ran out of disk space. The temporary image was removed. No live SQL Server database was created here, and the LocalDB/SQL Server Express migration, SSMS checks, controller CRUD and Windows Visual Studio launch remain to be verified on Windows.
