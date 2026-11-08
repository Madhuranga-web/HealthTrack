# Installation and troubleshooting

## Requirements

- Windows with Visual Studio 2026 and the ASP.NET and web development workload
- .NET 10 SDK
- SQL Server Express LocalDB or SQL Server Express
- Internet access for the initial NuGet restore; the bundled CSS and JavaScript work offline thereafter

The development choice is Visual Studio 2026 with .NET 10. Request lecturer confirmation that ASP.NET Core MVC satisfies the coursework's ASP.NET MVC wording. The solution is not compatible with Visual Studio 2017.

## Setup

1. Extract the source ZIP and open `HealthTrack.sln` in Visual Studio 2026.
2. Restore packages (`Build > Restore NuGet Packages` or `dotnet restore HealthTrack.sln`).
3. Install LocalDB if needed. For SQL Express, replace the server in `ConnectionStrings:DefaultConnection` in `HealthTrack/appsettings.json`, for example `Server=.\\SQLEXPRESS;Database=HealthTrack;Trusted_Connection=True;TrustServerCertificate=True`.
4. Install EF CLI: `dotnet tool install --global dotnet-ef --version 10.0.14`.
5. From the solution root run `dotnet ef database update --project HealthTrack --startup-project HealthTrack`. This applies `InitialCreate` and `AddSystemSettings`.
6. Set `SeedPassword` to a strong local password and `SeedDemoData=true` in your launch environment. Run the app once in Development. See the PowerShell example in the root README. Disable the flag after seeding.
7. Set `HealthTrack` as startup project and press F5. If Visual Studio asks to trust the local HTTPS certificate, accept it.
8. Use the seven fictional `@healthtrack.test` accounts listed in README. Change demo passwords or delete the accounts before using a non-demo database.

Do not commit passwords or a local database. Prefer a process environment variable or Visual Studio user secrets for local settings. If using `dotnet user-secrets`, initialize a secrets ID for the web project first.

## Common errors

| Symptom | Check |
| --- | --- |
| LocalDB connection fails | Install LocalDB; verify `(localdb)\\MSSQLLocalDB` is running or use SQL Express connection string. |
| Login fails after seeding | Check the seed password and that the app ran in Development with `SeedDemoData=true`. Identity requires uppercase, lowercase, digit, symbol, and 10 characters. |
| Database table missing | Run `dotnet ef database update` for the same connection string the app uses. |
| HTTPS warning | Trust the Visual Studio development certificate locally. |
| NuGet restore fails | Check network and configured NuGet.org access; rerun `dotnet restore`. |
| Access denied | Sign in with the proper role; trainers only see assigned clients and clients only see their own records. |
| No chart appears | First create measurements or activity. Charts intentionally show empty states for no data. |

For full database tables, migration script and SSMS checks, see [Database_Setup_Guide.md](Database_Setup_Guide.md).
