---
name: verify
description: Build/launch/drive recipe for verifying RetroSportsSquares changes end-to-end (API + SignalR).
---

# Verifying RetroSportsSquares

## Build & launch the API

```powershell
# Backend only (RSS.sln also triggers the frontend Vite build — slow and noisy)
cd RSS
dotnet build RSS-API.csproj

# Run (background). Dev profile listens on http://localhost:5161 (NOT https:7187 —
# that's only the https launch profile; dotnet run picks the http one first).
dotnet run --no-build
```

- DB: local MySQL, connection string in `RSS/appsettings.json` (root / see file). `mysql.exe` is at `C:\Program Files\MySQL\MySQL Server 8.3\bin\mysql.exe` for direct row pokes.
- Migrations: from `RSS` dir, `dotnet ef migrations add <Name> --project ../RSS-DB --startup-project .` then `dotnet ef database update ...`. Note: `RSS-DB/Migrations/` is gitignored (only the snapshot is tracked).
- Dev seeding: `DevDataSeeder` seeds one basketball DailySportsGames (Lakers/Warriors, league 12). **No users are seeded** — create them via `POST /auth/signup` `{email, password, name, gamerTag}` (password needs upper/lower/digit/symbol, e.g. `Test123$abc`).
- Admin: role is granted at startup to the user matching `Admin:SeedEmail` config. To make a test user admin: sign them up first, then restart the API with `$env:Admin__SeedEmail = "<their email>"`.

## Driving the surface

- REST: login via `POST /auth/login` → `{token, user}`; Bearer token auth. Create game: `POST /SquareGames/CreateGame` (get `dailySportsGameId` from `GET /AvailableSportsGames/GetAvailable/basketball/league/12/GameOptions`); response field is `gameId`, not `id`.
- SignalR: drive with a Node script using the frontend's own `@microsoft/signalr` (resolve via `createRequire` pointing at `RetroSportsSquaresWeb/package.json`). Connect to `http://localhost:5161/hubs/game` with `accessTokenFactory`, `invoke("JoinGame", gameId)`, listen for events.
- Frontend: `npm run dev` in `RetroSportsSquaresWeb` (port 5173). No Playwright/browser automation available on this machine — UI verification is manual. NOTE: frontend default `API_BASE_URL` is `https://localhost:7187`, so for manual UI testing run the API with the https profile or set `VITE_API_URL`.
- `npm run check` has a pre-existing failure in `GameOptions.tsx` (TS2802, unrelated iterator flag issue) — don't attribute it to your change.

## Gotchas

- The chat send rate limiter (`chat-send` policy, 5/10s per user) counts *all* POSTs to the endpoint including ones later rejected by model validation.
- Test-user/game cleanup: host can `DELETE /SquareGames/{gameId}` while the game is open.
