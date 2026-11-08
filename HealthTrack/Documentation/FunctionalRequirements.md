# Functional requirement map

| Requirement | Implemented path | Notes |
| --- | --- | --- |
| Registration and login | `AccountController`, Identity | Public registration creates Client only; admin seed creates initial Admin. |
| Client dashboard | `ClientController.Index` | BMI, assigned plan counts, next approved appointment, unread messages. |
| Trainer dashboard | `TrainerController.Index` | Assigned client and pending booking counts. |
| Admin dashboard | `AdminController.Index` | Accounts, activity chart, appointment count. |
| Workout plans and logs | `PlansController` and `ClientController` | Trainer plan and exercise CRUD; client completion logs. |
| Diet plans and logs | `PlansController` and `ClientController` | Recommended targets separated from actual meals. |
| Appointments | `AppointmentsController` | UTC input, trainer and client overlap checks, statuses and history. |
| Progress and analytics | `ClientController.Progress`, `TrainerController.Progress`, reports | Database-driven Chart.js data. |
| Notifications | `NotificationsController` | Event notices and read state; no scheduled reminders. |
| Security | Identity, policies, `HealthService.IsAssigned`, ownership queries | Cookie checks reject inactive accounts; antiforgery protects POST. |
| Settings | `AdminController.Settings` | Dashboard message for client/trainer. |
