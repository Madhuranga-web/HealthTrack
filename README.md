# HealthTrack

HealthTrack is an ASP.NET Core MVC coursework application for client fitness tracking, trainer planning, and administrator oversight.

**Selected environment:** Visual Studio 2026, .NET 10 LTS, SQL Server LocalDB or Express. The coursework says ASP.NET MVC and Visual Studio 2017 or higher. This implementation uses ASP.NET Core MVC and **does not claim Visual Studio 2017 compatibility**. Ask the lecturer to confirm ASP.NET Core MVC is accepted before submission. Do not infer that a generic "2017 or higher" clause guarantees compatibility with .NET 10.

## Quick start (Windows)

1. Install Visual Studio 2026 with ASP.NET and web development and .NET 10 SDK. Install SQL Server Express LocalDB or SQL Server Express.
2. Open `HealthTrack.sln` and restore NuGet packages.
3. Check `HealthTrack/appsettings.json`. The default connection uses `(localdb)\MSSQLLocalDB`; change it for SQL Server Express.
4. In a terminal at the solution root run `dotnet tool install --global dotnet-ef --version 10.0.14` then `dotnet ef database update --project HealthTrack --startup-project HealthTrack`.
5. To create fictional demo accounts, set a strong temporary password in the **current local process** and enable seeding:
   ```powershell
   $env:SeedPassword = Read-Host 'Demo password'
   $env:SeedDemoData = 'true'
   dotnet run --project HealthTrack
   ```
   Choose at least 10 characters with uppercase and lowercase letters, a digit, and a symbol. The password is not saved in source. Remove both environment variables after setup. For Visual Studio launching, set them in the debug environment or initialize once from the terminal, then start Visual Studio normally.
6. Sign in as `admin@healthtrack.test`, either trainer (`trainer@healthtrack.test`, `trainer2@healthtrack.test`), or one of four clients (`client@healthtrack.test` through `client4@healthtrack.test`) with that local seed password. These accounts and data are fictional. Do not use the demo database for real health records; application-level encryption at rest is not implemented. Public registration always creates a Client.
7. Run `dotnet test HealthTrack.sln` for automated tests.

The Development startup path invokes `Database.MigrateAsync()` and then the repeatable demo seed only when `SeedDemoData=true`. For a normal run, apply migrations before starting. The seeder is idempotent for its sample records. Do not enable it in production.

## Features

- Identity registration, login, logout, role policies, active account checks, profile editing, server validation and antiforgery tokens.
- Admin accounts, activation, trainer assignments, appointments, activity counts and reports.
- Trainer client progress, workout and diet plan management, feedback, appointment decisions and reports.
- Client health metrics with BMI, plans, workout and meal logs, progress charts, appointments and notifications.
- Database-backed event notifications with read state.

**Reminder limitation:** Scheduled workout, meal and appointment reminders are not implemented. Notifications are created by actual in-app events; there is no background scheduler. The application must remain running to serve the UI.

**Time handling:** Appointment form values are UTC. The interface labels this explicitly. Only approved appointments block a time slot. Requests remain pending until a trainer approves them.

For the full database procedure, see [database setup](HealthTrack/Documentation/Database_Setup_Guide.md). See [installation and troubleshooting](HealthTrack/Documentation/Installation.md), [user manual](HealthTrack/Documentation/UserManual.md), [technical draft](HealthTrack/Documentation/TechnicalReport.md), [functional requirement map](HealthTrack/Documentation/FunctionalRequirements.md), [test checklist](HealthTrack/Documentation/Testing.md), and [viva guide](HealthTrack/Documentation/Viva.md).

## Academic integrity

This source is a learning draft. The attached brief requires original, understood work and proper acknowledgment of AI assistance. Review, adapt and understand every part of the implementation; use the reflection template in the technical report to describe your own contribution and disclose AI use under your university's rules.
