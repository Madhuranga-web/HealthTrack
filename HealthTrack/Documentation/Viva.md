# Viva demonstration guide

1. Start with `HealthTrack.sln`, identify `Program.cs`, models, controllers, views and EF migration. State Visual Studio 2026/.NET 10 choice and lecturer confirmation status.
2. Register a new client; show Identity hashed credential storage concept and role assignment in `AccountController`.
3. Sign in as Admin. Show dashboard counts and chart, create a trainer, activate/deactivate, assign a trainer to the new client, and open reports.
4. Sign in as Trainer. Show assigned roster, create a workout plan with an exercise and a diet plan with recommended calories; open client progress and send feedback.
5. Sign in as Client. Show both plans, log a workout and meal, record height/weight and explain BMI (`weight / height²` in metres). Show chart changes and notification read state.
6. Book a UTC appointment. Sign in as Trainer and approve or reschedule it. Attempt an overlapping approved booking to explain conflict handling.
7. Show one invalid form and one forbidden cross-role/client access attempt. Explain antiforgery, role policies, record-level assignment checks and Identity passwords.
8. Run `dotnet test HealthTrack.sln`, discuss what tests cover and what still requires Windows SQL Server verification.
9. Acknowledge that scheduled reminders are not implemented; event notifications are database-backed. Explain your own edits, contributions and AI assistance according to the course rules.
