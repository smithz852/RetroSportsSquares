# Data Breach Notification Runbook

Incident-response procedure for a security incident involving user data, written to satisfy
California Civil Code §1798.82 (the state's breach-notification statute). This repo is public,
so this document is deliberately written at the *procedure* level — it does not name real
infrastructure resource IDs, Key Vault secret names, API keys, or personal contact addresses.
Anyone executing this runbook for real should have those specifics from the project's private
configuration (Azure Key Vault / App Service configuration / local notes), not from this file.

## 1. Scope

Applies to any incident where there is reason to believe an unauthorized party accessed or
acquired data from `AspNetUsers` (the Identity-backed user table) or any system with access to
it (the MySQL database itself, the API's hosting environment, or a third-party service holding
a copy of user data, e.g. the email provider).

This app's actual personal-information surface is narrow. As of this writing, `ApplicationUser`
stores: `Email`, a one-way hashed `PasswordHash` (never plaintext), `DisplayName`, `GamerTag`,
`CoinBalance` (virtual, no real-money value), `AgeVerified`/`AgeVerifiedAt` (boolean + timestamp
only — raw date of birth is deliberately never persisted), `AcceptedTermsAt`/`TermsVersion`, and
`RecapEmailsOptedOut`/`UnsubscribeToken`. There is no SSN, driver's license, financial account,
or health data anywhere in the schema.

**Legal trigger decision (documented in advance, not improvised during an incident):** §1798.82
is triggered by unauthorized acquisition of an email address *in combination with* a password
or credential that would permit account access. This app's password field is always a salted
hash, not plaintext — a strict reading of the statute could argue a hash-only exposure doesn't
meet the "unencrypted" bar. **This runbook rejects that reading on purpose.** Treat any confirmed
unauthorized access to the `AspNetUsers` table, or to the underlying database, as a reportable
breach by default. Erring conservative is cheaper than litigating the definition under pressure,
and users reasonably expect to be told either way.

This app does not currently collect user state/region. Per-incident scoping should **default to
treating every affected account as a CA resident** unless there's a specific reason to believe
otherwise — safer than trying to retrofit geo-detection just for this purpose.

**Application-log retention exception (documented, accepted):** `ILogger` output across the
backend (`TokenService`, `GameNotificationService`, `AuthController`) was audited and corrected
2026-11 to log `UserId` instead of plaintext email wherever both were available, specifically so
that a deleted account's email doesn't keep circulating through console/Azure Log Stream output
indefinitely after `AccountDeletionService` anonymizes the `AspNetUsers` row. This is a forward-
looking fix only — it does not retroactively scrub log lines emitted before the change shipped.
Since this app had no production deployment at the time the fix landed, no pre-fix log history
with real user emails exists to retroactively scrub in the first place. If that ever stops being
true (e.g. a pre-launch log export is retained somewhere), treat any plaintext email surviving in
it past that user's account deletion as the same accepted-but-bounded retention exception CCPA
§1798.105(d) allows for operational/security logs — not a breach — but note the exception here
rather than assuming it silently. The support-request form (`SupportService`) still logs a
submitted email as-is; see [[project_launch_readiness]] for why that's tracked separately (it's
not always tied to a logged-in `ApplicationUser`, so there's no account-deletion event to key a
fix off of).

## 2. Roles

Single-maintainer project today — no formal team structure needed. Whoever is running this
runbook is the incident commander by default: they own the decision to contain, the decision to
notify, and the decision to escalate to the CA Attorney General if the threshold below is met.

Escalation/contact details (who to loop in, what inbox gets used for outbound notices) are kept
in the project's private notes, not in this file, since this repo is public.

## 3. Detection

No centralized monitoring exists yet (tracked separately — see Section 7). Until that lands,
realistic detection sources for this stack are:

- Azure App Service HTTP/diagnostic logs (manual review)
- MySQL slow-query / connection logs
- Unexpected behavior reported by a user (e.g. "I'm logged into an account I don't recognize")
- A GitHub Dependabot/security advisory against a dependency this app uses
- Unusual outbound email volume/bounce patterns from the Resend dashboard (could indicate the
  email pipeline or API key was used by someone else)
- Unexpected database size/row-count changes, or queries you didn't run

## 4. Immediate containment checklist

Work top-down; not every step applies to every incident.

- [ ] Rotate the JWT signing key (invalidates every outstanding token immediately — forces
      re-login app-wide; this is more aggressive than the existing per-user security-stamp
      rotation and should be reserved for a confirmed breach, not routine password resets)
- [ ] Rotate database credentials
- [ ] Rotate the email provider API key (Resend) if the breach could involve that service
- [ ] Rotate the sports-data API key if that service/credential was in scope
- [ ] If a specific set of accounts is known to be compromised (not the whole table), force a
      password reset on just those accounts rather than a global key rotation
- [ ] Take a snapshot/backup of current DB state before making further changes, for later
      forensic review
- [ ] If the vector was a specific code path (e.g. an injection bug), patch and deploy before
      reopening access, not after

## 5. Assessment / scoping checklist

- [ ] Which table(s)/column(s) were actually exposed? (Cross-reference against Section 1 — does
      it include email + password/credential, or only non-triggering fields like `DisplayName`?)
- [ ] How many distinct user rows are affected?
- [ ] Is the affected count over **500**? If yes, the CA Attorney General must also receive a
      copy of the notice (see Section 6).
- [ ] What is the actual timeline — when did the exposure start, when was it detected, when was
      it contained? (Needed both for the notice content and to support the "without unreasonable
      delay" standard if ever questioned.)
- [ ] Was any data actually *acquired* by a third party, or was this exposure without evidence of
      acquisition (e.g. a misconfigured-but-unused endpoint)? The statute is triggered by
      acquisition, not mere exposure — but if there's no way to rule acquisition out, treat it as
      if it happened.

## 6. Notification

### 6.1 Who gets notified

Every affected user, individually, by email — acceptable under CA law because users already have
a pre-existing email relationship with this app (account email, used for password reset and
recap emails today).

### 6.2 Required content (per Civ. Code §1798.82(d))

The notice must include:
- What happened (plain-language description of the incident)
- What categories of information were involved
- The date or date range of the breach
- Whether notification was delayed for a law-enforcement investigation (and if so, why)
- A statement of what the app is doing in response (e.g. forced password reset, key rotation)
- What the user should do (e.g. "we've already logged you out everywhere; please choose a new
  password" — or, if passwords weren't involved, no forced action needed)

Credit-monitoring-agency contact info is only required if SSN/driver's license data was involved
— not applicable to this app's current schema, but keep the clause in the template below in case
the schema ever grows to include that kind of data.

### 6.3 User notification template

```
Subject: Important security notice about your [App Name] account

We're writing to let you know about a security incident that may have affected your account.

What happened: [plain description — e.g. "We discovered unauthorized access to our user
database on [date]."]

What information was involved: [e.g. "your email address and a securely hashed version of your
password — not your actual password, which we never store in readable form."]

When this happened: [date or date range], discovered on [date].

What we've done: [e.g. "We've logged out all active sessions and rotated our security keys.
Please choose a new password the next time you sign in."]

What you should do: [e.g. "No action is required" OR "Please reset your password using the link
below" / "We recommend changing your password on any other site where you reused it."]

If you have questions, contact us at [support contact].

— [App Name] Team
```

### 6.4 CA Attorney General notification (only if >500 CA residents affected)

Submit a sample copy of the user notice above via the AG's reporting portal:
https://oag.ca.gov/privacy/databreach/reporting

No separate template needed — the same user-facing notice is the AG submission, per the
statute's "sample copy" requirement.

## 7. Incident timeline log (fill in per-incident)

| Event | Date/time (UTC) |
|---|---|
| Detected at | |
| Incident commander notified | |
| Containment action taken | |
| Scope/assessment completed | |
| Decision: reportable under §1798.82? | |
| User notification sent | |
| AG notification sent (if applicable) | |
| Post-incident review completed | |

## 8. Post-incident

- [ ] Confirm all affected users were force-logged-out and/or reset, as applicable
- [ ] Root-cause the vector and patch it (not just rotate keys — fix the hole)
- [ ] Write a short internal retro: what was the vector, what detected it (or what should have
      detected it sooner), what changes would shorten the timeline next time
- [ ] Update this runbook if the incident revealed a gap in the procedure itself

## 9. Known gap — detection tooling (planned, not yet implemented)

There is currently no centralized monitoring/alerting (Section 3 lists manual-only detection
sources). The planned fix is Azure Monitor + Application Insights, since the app already runs on
Azure App Service — this is largely a configuration/connection-string integration rather than new
application code, and would add automatic alerting on things like authentication-failure spikes
and abnormal request volume. Not yet scheduled; tracked as a follow-up, not a blocker to this
runbook existing.

**Pre-scoped decisions for when this gets built** (so the eventual `Program.cs` change is a
drop-in, not a redesign):
- SDK: `Azure.Monitor.OpenTelemetry.AspNetCore` (the current Microsoft-recommended distro for
  .NET 8), not the older `Microsoft.ApplicationInsights.AspNetCore` package. It auto-instruments
  ASP.NET Core requests, outbound HTTP calls, and EF Core SQL queries with no extra code — the
  EF Core piece is useful here specifically, since abnormal database query volume is one of the
  detection signals named in Section 3.
- Connection string: config-driven, following the same pattern already used for `Jwt:Key` /
  `Resend:ApiKey` / `ApiSports:Key` — placeholder in `appsettings.json`, real value via Key Vault
  in Production. Never hard-code it.
- Actual alert rules (auth-failure-rate thresholds, request-volume anomalies, etc.) are
  configured in the Azure Monitor dashboard after the resource exists — not something that can
  be pre-built in code ahead of time.
