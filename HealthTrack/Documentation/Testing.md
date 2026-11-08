# Verification checklist

## Automated tests

Run `dotnet test HealthTrack.sln`. Tests cover BMI bounds and calculation, booking windows, overlapping Approved appointments, trainer assignment, client workout ownership, trainer progress access, plan numeric validation and role authorization attributes. Results should be recorded after running on your machine.

## Manual end-to-end checks on Windows LocalDB

- [ ] Restore, migrate and seed with no hardcoded password.
- [ ] Register a new client and confirm the Client role and dashboard.
- [ ] Reject a weak password and invalid email.
- [ ] Sign in as Admin, Trainer and Client; verify each dashboard and denied cross-role routes.
- [ ] Admin create client/trainer, search accounts, deactivate and reactivate one, assign and unassign trainer, update dashboard message.
- [ ] Deactivated account cannot log in; an existing session is rejected on next request.
- [ ] Trainer sees assigned client only; direct URL to another client is forbidden.
- [ ] Trainer create, edit and delete workout and diet plans; add/edit/remove an exercise and meal; check client notification.
- [ ] Client sees only own assigned plans; logs a workout and a meal; invalid values are rejected.
- [ ] Client records height and weight; verify BMI formula and real chart values; test empty chart state on new client.
- [ ] Client books assigned trainer in future UTC; cannot use another trainer or past date.
- [ ] Trainer approves, rejects, reschedules and cancels appointments; overlap of Approved slots is blocked.
- [ ] Client cancellation changes status; booking history remains visible.
- [ ] Trainer sends feedback; client sees notification and marks it read.
- [ ] Admin appointments, engagement chart and report counts reflect newly inserted data.
- [ ] Trainer report counts reflect only assigned clients.
- [ ] Try modifying another client's record ID; confirm no data leaks.
- [ ] Verify antiforgery token rejection on POST without token, HTTPS redirect and friendly error page.
- [ ] Check narrow mobile viewport and keyboard navigation.

Record actual dates, results and screenshots yourself. Do not claim unchecked rows passed.
