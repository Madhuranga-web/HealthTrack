# Technical report draft — personalize before submission

## 1. Introduction and requirements

HealthTrack supports three roles in a local fitness management workflow. Clients record measurements, workouts and meals and request consultations. Trainers provide assigned plans and feedback and decide appointments. Administrators manage accounts and assignments and inspect engagement. The implementation uses ASP.NET Core MVC with C#, EF Core, SQL Server, Identity, Razor Views, Bootstrap 5 and Chart.js. The target is Visual Studio 2026 and .NET 10 LTS. Obtain lecturer confirmation for ASP.NET Core MVC under the ASP.NET MVC coursework wording. No claim is made for Visual Studio 2017.

## 2. Logical solution

Identity stores password hashes and role memberships. Registration writes a Client role and profile only. Admins create Trainer and Client accounts. Trainer assignments are the authorization boundary for trainer operations. Client queries filter by the authenticated Identity user ID. Workouts and diets distinguish plans from user logs. Booking requests start Pending; trainers approve, reject, reschedule or cancel. Approved appointments are checked for trainer and client overlap. A serializable transaction surrounds trainer decisions and rescheduling. Notifications are stored in the database as event records. BMI is calculated on the server from metric inputs. Chart.js receives query results; an empty dataset displays an empty state. Reports use database counts.

## 3. MVC architecture

`Controllers` receive HTTP requests, verify roles and record ownership, validate input, and choose views. `ViewModels` define input forms. `Models` define the database entities. `AppDbContext` configures EF relationships, indices and precision. `HealthService` centralizes BMI, assignment and appointment checks plus activity and notification writes. Razor views render server data; bundled Bootstrap and Chart.js handle presentation. Identity provides password hashing, cookie authentication and role membership. Dependency injection constructs EF, Identity and services in `Program.cs`.

Render the source diagrams in this folder: `architecture.mmd`, `er.mmd`, `use-cases.mmd`, `classes.mmd`, `appointment-sequence.mmd`, `workout-sequence.mmd`.

## 4. Entities and relationships

| Entity | Main properties and purpose |
| --- | --- |
| ApplicationUser | Identity ID, email, full name, active status, creation time. |
| ClientProfile / TrainerProfile | Role-specific height and goals / specialty, keyed by user ID. |
| TrainerAssignment | Unique trainer/client pair; determines trainer record access. |
| WorkoutPlan / WorkoutExercise | Trainer recommendation for one client; one-to-many exercise details. |
| WorkoutLog | Client-recorded completion, minutes and calories. |
| DietPlan / DietPlanMeal | Recommended daily and per-meal targets, schedule and protein. |
| MealLog | Actual client food, calories and protein. |
| HealthMetric | Client height, weight, calculated BMI and timestamp. |
| Appointment | Client, trainer, UTC start, duration, purpose and status. |
| Notification | Message, read flag, recipient and timestamp. |
| TrainerFeedback | Trainer message to assigned client. |
| ActivityLog | Recorded user actions used by admin engagement chart. |
| SystemSetting | Administrator-managed dashboard message. |

Identity and app entities use primary and foreign keys. The trainer assignment pair is unique. Health metric, notification, appointment and activity queries have supporting indexes. EF maps decimal metric fields to precision 9,2. The `InitialCreate` and `AddSystemSettings` migrations are the schema source of truth.

## 5. Controllers and actions

| Controller | Main actions |
| --- | --- |
| Account | Register, Login, Logout, Profile, ChangePassword |
| Home | Index, Denied, Error |
| Admin | Index, Users, Create, Toggle, Assignments, Assign, Unassign, Appointments, Reports, Settings |
| Trainer | Index, Clients, Progress, Feedback, Reports |
| Client | Index, Progress, Metric, Workouts, LogWorkout, Diet, LogMeal, Profile, Goals |
| Plans | Workouts, Workout, AddExercise, EditExercise, DeleteExercise, DeleteWorkout, Diets, Diet, AddMeal, EditMeal, DeleteMeal, DeleteDiet |
| Appointments | Index, Book, Decide, Cancel, Reschedule |
| Notifications | Index, Read |

## 6. Key classes and methods

`HealthService.CalculateBmi(weightKg,heightCm)` validates bounds and calculates kg/m² to one decimal. `IsAssigned(trainerId,clientId)` checks the assignment table. `ValidBooking(start,minutes,now)` checks the booking window and duration. `HasConflict` and `HasClientConflict` check overlap against Approved bookings. `Notify` and `Activity` insert event records. The Development startup path applies migrations before `DemoSeeder.RunAsync` creates fictional users and records when explicitly enabled. `AppDbContext.OnModelCreating` configures relationships and indexes. Form classes in `ViewModels/Forms.cs` carry data annotations. `ApplicationUser` extends IdentityUser. Entity properties are documented in the table above and in `Models/Entities.cs`.

## 7. Validation and security

Data annotations cover required, email, numeric range and string length fields. Controller checks enforce assignment, ownership, booking date and conflicts on the server. A global antiforgery filter protects unsafe HTTP actions. Identity hashes passwords. Registration cannot choose roles. Admin creation permits Client or Trainer only. Role policies protect controller endpoints. A cookie validation event rejects inactive accounts. EF uses parameterized SQL. App settings hold a local connection template, not a secret. HTTPS redirection and production exception handling are configured.

## 8. Testing approach and limitations

`HealthTrack.Tests` tests BMI bounds, booking validity and conflicts, assignment checks, client and trainer access checks including assignment removal, plan input ranges, role attributes, the 22-table SQL Server model, and repeatable fictional seeding with Identity password hashing on relational SQLite. The manual checklist is in `Testing.md`. The Linux workspace built the solution and passed 14 tests; a temporary SQL Server container could not unpack within disk limits. Run the migration and full browser flow against Windows LocalDB or SQL Server Express before submission. Scheduled reminders are not implemented. The current UI uses UTC appointment input. Health measurements are not encrypted at rest by the application; this fictional local demo should not hold real medical records. Notifications are event-driven and visible while the app is running. Admin reporting uses counts, popular logged routines and recent activity rather than a downloadable report file.

## 9. Future improvements

Add a Windows service or durable scheduler for reminders, email verification and password reset, an appointment calendar with local timezone conversion, richer nutritional analysis, report export, and additional integration tests against SQL Server.

## 10. References

- Microsoft, ASP.NET Core MVC documentation: https://learn.microsoft.com/aspnet/core/mvc/
- Microsoft, ASP.NET Core Identity documentation: https://learn.microsoft.com/aspnet/core/security/authentication/identity
- Microsoft, EF Core documentation: https://learn.microsoft.com/ef/core/
- Chart.js documentation: https://www.chartjs.org/docs/latest/
- Bootstrap 5 documentation: https://getbootstrap.com/docs/5.3/
- CS6004ES Coursework 2 (2025/26), supplied assessment brief.

## Individual reflection template — write in your own words

- My role and concrete contributions: [Describe your tasks and commits.]
- A technical decision I made and why: [Explain a real decision you can defend.]
- A problem I encountered and how I diagnosed it: [Give evidence.]
- What testing I personally ran and what it showed: [Use actual results.]
- What I learned and would change next time: [Personal reflection.]

## AI assistance disclosure template

I used [tool and version] for [specific activities]. I reviewed and changed [specific generated files or ideas]. I personally verified [commands, flows, dates and results]. I understand and can explain [key design decisions]. I followed [the university's current AI guidance and citation format]. Replace every bracketed item with accurate information; do not present generated material as solely your own work.
