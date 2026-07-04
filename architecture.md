# RetroSportsSquares — Architecture Map

A real-time "sports squares" web app. Players join a game tied to a real sporting
event, claim squares on a 10×10 grid, and win payouts each period based on the last
digit of each team's score. Live scores and game state are pushed to clients over
SignalR. Backend is a .NET 8 solution; frontend is a React + Vite SPA.

---

## 1. Top-Level Layout

```
RetroSportsSquares/
├── RSS.sln                     # .NET solution (3 projects)
├── RSS/                        # RSS-API — ASP.NET Core Web API (entry point)
├── RSS-Services/               # Business logic / service layer
├── RSS-DB/                     # EF Core data layer (entities, DbContext, migrations)
└── RetroSportsSquaresWeb/      # React + Vite frontend SPA
    ├── client/src/             # React app
    └── shared/                 # Shared TS types + API endpoint map
```

**Dependency direction:** `RSS` (API) → `RSS-Services` → `RSS-DB`.
The API layer wires everything up via DI; services hold logic; DB holds entities.

---

## 2. Backend — .NET 8

### 2.1 `RSS` (RSS-API) — Web API host / entry point

The composition root. `Program.cs` configures everything:

- **Database:** MySQL via Pomelo EF Core provider; migrations live in `RSS_DB`.
- **Identity + Auth:** ASP.NET Core Identity (`ApplicationUser` + `IdentityRole`) with
  **JWT bearer** tokens. Notable custom behavior in `Program.cs`:
  - SignalR handshakes pass the JWT via `access_token` query string for `/hubs` paths.
  - `OnTokenValidated` enforces a **security stamp** check (cached 30s in `IMemoryCache`)
    so tokens can be invalidated (e.g. on password/email change).
- **CORS:** allows `http://localhost:5173` (Vite dev server) with credentials.
- **Rate limiting:** IP-based (10/min) and email-based (3/hr) partitioned limiters on
  auth-sensitive paths (`/auth/forgot-password`, `/auth/reset-password`,
  `/user/request-email-change`).
- **Email:** Resend provider (`ZlEmailProvider` / `IEmailService`) + `TokenService`.
- **SignalR hub:** `GameHub` mapped at `/hubs/game`.
- **Background services (hosted):** six long-running workers (see §2.4).
- **Seeding:** `DevDataSeeder` (dev only) and `AdminRoleSeeder` (creates Admin role/user).

**Controllers** (`RSS/Controllers/`) — thin HTTP layer, delegate to services:

| Controller | Route prefix | Responsibility |
|---|---|---|
| `AuthController` | `/Auth` | login, signup, `me`, forgot/reset password, logout, JWT issuance |
| `SquareGamesController` | `/SquareGames` | game lifecycle: create, join, leave, start, delete, square selections, turn management, gameboard/score reads |
| `AvailableSportsGamesController` | `/AvailableSportsGames` | list real sport events available to build a game on; sports & leagues catalog |
| `PlayerDashboardController` | `/PlayerDashboard` | a player's stats, current games, past games |
| `AdminController` | `/Admin` | `[Authorize(Roles="Admin")]` — summary, current/past games, player stats, users |
| `UserController` | `/User` | display name, gamer tag, email-change request/confirm |

**Other `RSS` pieces:**
- `Hubs/GameHub.cs` — SignalR hub; clients call `JoinGame`/`LeaveGame` to join a
  per-game group (`game-{gameId}`).
- `Hubs/GameHubNotifier.cs` (`IGameHubNotifier`) — server-side broadcaster. Services
  call this to push events to a game's group (see §2.5 for event list).
- `Middleware/EmailExtractionMiddleware.cs` — runs after auth; pulls email from JWT
  claims (used for rate-limit partition key).
- `Helpers/MapperHelpers.cs` — entity → DTO mapping for API responses.
- `SportsDataAutomation/` — the background workers (see §2.4).

### 2.2 `RSS-Services` — business logic

Service classes (registered scoped/typed in `Program.cs`):

| Service | Role |
|---|---|
| `AvailableGamesServices` | CRUD + queries for `SquareGames`; score/winner/payout computation |
| `SquareServices` | board generation, square selection, square limits, stale-game closing, outside-number reveal |
| `GamePlayerServices` | join/leave, host checks, **turn-based selection** flow (begin, advance, skip, status) |
| `SportsGameServices` | **external sports API integration** (api-sports.io) via typed `HttpClient`; fetch/update live game data |
| `PlayerDashboardService` | player-facing dashboard aggregations |
| `AdminDashboardService` | admin dashboard aggregations (summaries, user/player stats) |
| `UserServices` | profile updates |
| `GeneralServices` | shared helpers |
| `TokenService` | email-confirmation / reset token generation |
| `IGameHubNotifier` | interface implemented in `RSS` (inverts the API dependency so services can push SignalR events) |

**Helpers/** — per-sport mappers translating the external API's JSON into domain data:
`BasketballMapperHelper`, `FootballMapperHelper`, `SoccerMapperHelper`, plus
`TimeHelpers` (heavy use of Pacific Standard Time for daily scheduling).

`DTOs/` in both `RSS/DTOs` and `RSS-Services/DTOs` carry request/response shapes.

### 2.3 `RSS-DB` — data layer

- **`Context/AppDbContext`** extends `IdentityDbContext<ApplicationUser>`. DbSets:
  `SquareGames`, `DailySportsGames`, `GamePlayers`, `GameSquares`.
  `SquareGames.PeriodWinners` (`Dictionary<int,string?>`) is JSON-serialized to a
  `longtext` column with a custom `ValueComparer`.
- **`Context/AppDbContextFactory`** — design-time factory for EF migrations.
- **`Migrations/`** — EF Core migrations (initial create + snapshot).

**Entities (domain model):**

```
ApplicationUser (Identity user; DisplayName, GamerTag)
   └──< GamePlayer  (a user's membership in one game; IsHost, TurnOrder, HasHadTurn)

DailySportsGames  (a real sporting event pulled from the sports API)
   • ApiGameId, HomeTeam/AwayTeam, GameStartTime, SportType, League/LeagueId,
     Status, current + per-period scores, InUse
   └──< SquareGames  (a squares game built on top of one DailySportsGames event)
          • GameName, GameType, isOpen, PlayerCount, PricePerSquare,
            SquareSelectionLimit, IsPublic, IsTurnBased + turn fields,
            TopNumbers/LeftNumbers (the randomized 0–9 axis digits),
            PeriodCount, IsCompleted, PeriodWinners{period→userId}
          ├──< GamePlayer   (players in this game)
          └──< GameSquares  (the 100 grid cells)
                 • SquareValue, HomeDigit/AwayDigit, RowIndex/ColumnIndex,
                   nullable GamePlayerId (who claimed it)
```

### 2.4 Background automation (hosted services)

Six `BackgroundService` workers registered in `Program.cs`, all under
`RSS/SportsDataAutomation/`:

- **Load workers** (`FootballAutomation`, `BasketballAutomation`, `SoccerAutomation`)
  extend `BaseSportsAutomation`. Once per day at a per-sport Pacific-time hour they
  fetch the day's games into `DailySportsGames`, and continuously close stale games.
- **Refetch workers** (`FootballRefetchAutomation`, `BasketballRefetchAutomation`,
  `SoccerRefetchAutomation`) extend `BaseRefetchAutomation`. They poll the sports API
  for in-progress games, update scores, **resolve per-period winners**, and push
  `ScoreUpdated` SignalR events. Handles terminal statuses (`FT`, `AOT`, `Final/OT`,
  `Postponed`).

> External API: **api-sports.io** (key + host header configured on the typed
> `HttpClient` in `Program.cs`).

### 2.5 Real-time events (SignalR)

Server → client events broadcast to group `game-{gameId}` via `IGameHubNotifier`:

`TurnAdvanced`, `PlayerJoined`, `PlayerLeft`, `SelectionsStarted`, `SquareSelected`,
`GameStarted`, `GameDeleted`, `ScoreUpdated`.

The frontend (`use-game-hub.ts`) listens for each and invalidates the matching React
Query cache key (turnStatus, boardSquares, OutsideSquares, gameScoreData) or redirects.

---

## 3. Frontend — `RetroSportsSquaresWeb`

React 18 + TypeScript + Vite SPA. (The `package.json` also lists an Express/Drizzle
stack, but the shipped app is the client SPA talking to the .NET API.)

**Stack:** Vite • wouter (routing) • TanStack React Query (server state) •
`@microsoft/signalr` • Radix UI + Tailwind (shadcn-style `components/ui/`) •
react-hook-form + zod • framer-motion • recharts. Retro-styled theme.

### 3.1 Structure

```
client/src/
├── App.tsx              # wouter <Switch> route table + providers
├── main.tsx             # React root
├── pages/               # route-level screens
├── components/          # shared components (+ ui/ = shadcn primitives)
├── hooks/               # data + realtime hooks
└── lib/                 # queryClient, auth-utils, utils
shared/
├── routes.ts            # API_BASE_URL + endpoints map (single source of API paths)
├── schema.ts            # TS interfaces mirroring backend DTOs
└── models/auth.ts
```

### 3.2 Routes (`App.tsx`, via wouter)

| Path | Page | Notes |
|---|---|---|
| `/` | `Home` | landing / lobby |
| `/login`, `/signup` | `Login`, `SignUp` | auth |
| `/options` | `GameOptions` | pick sport |
| `/leagues/:sport` | `LeagueOptions` | pick league |
| `/arena/:type/:leagueId` | `Dashboard` | choose a real event & create/browse games |
| `/game/:id` | `GameBoard` | **the core screen** — grid, selections, live scores |
| `/player-dashboard` | `PlayerDashboard` | player stats & history |
| `/admin` | `AdminDashboard` | wrapped in `AdminRoute` guard |
| `/settings` | `Settings` | profile, email/password change |
| `/confirm-email-change`, `/reset-password` | token-landing pages |

### 3.3 Hooks (data + realtime)

- `use-auth.ts` — auth state, login/logout, current user.
- `use-games.ts` — available games / create game.
- `use-gameplay.ts` — board, square selection, turn actions.
- `use-game-hub.ts` — SignalR connection; maps server events → query invalidation.
- `use-dashboard.ts` / `use-admin-dashboard.ts` — dashboard queries.
- `lib/queryClient.ts` — React Query client + fetch wrapper (JWT from `localStorage`).

### 3.4 Backend contract

`shared/routes.ts` is the canonical list of every backend endpoint the frontend calls,
grouped by `games`, `selections`, `dashboard`, `admin`, `auth`, `user`.
`API_BASE_URL` defaults to `https://localhost:7187` (override via `VITE_API_URL`).
JWT is stored in `localStorage` and sent as a bearer header (and as `access_token`
query param on the SignalR handshake).

---

## 4. Key Flows

**Create a game**
1. User picks sport → league → a real event (`DailySportsGames`).
2. `POST /SquareGames/CreateGame` (transactional): creates `SquareGames`, adds the host
   as a `GamePlayer`, marks the event `InUse`, and generates the 100 `GameSquares`.

**Join & select squares**
1. `POST /SquareGames/join/{id}` → `PlayerJoined` broadcast.
2. **Open mode:** players claim any free square (`POST .../SquareSelections/{id}`) →
   `SquareSelected` broadcast.
   **Turn-based mode:** host `begin-selections`; server tracks `CurrentTurnUserId` +
   turn timeout; selecting auto-advances the turn (`skip-player` to force) →
   `TurnAdvanced` broadcast.

**Start & play**
1. Host `POST /SquareGames/start/{id}` closes selections; axis digits
   (`TopNumbers`/`LeftNumbers`) are revealed → `GameStarted`.
2. Refetch background workers poll live scores, update `DailySportsGames`, resolve each
   period's winner into `SquareGames.PeriodWinners`, and push `ScoreUpdated`.
3. `GameBoard` shows live scores, winners, and per-period payouts.

---

## 5. Conventions & Notes

- **Layering:** controllers stay thin; logic lives in `RSS-Services`; EF entities/queries
  behind `AppDbContext`. Services push realtime events through `IGameHubNotifier`
  (interface in Services, implemented in the API layer to keep the dependency inward).
- **Time:** daily scheduling and "today" logic are all in **Pacific Standard Time**
  (`TimeHelpers`, background workers).
- **Auth invalidation:** security-stamp check on every token validation (30s cache)
  lets password/email changes revoke existing JWTs.
- **DTO mapping:** `RSS/Helpers/MapperHelpers` (API) and per-sport mappers in
  `RSS-Services/Helpers` (external API → domain).
- **EF migrations** (run from the `RSS` API dir; see comments in `AppDbContext.cs`):
  ```
  dotnet ef migrations add <Name> --project ../RSS-DB --startup-project .
  dotnet ef database update      --project ../RSS-DB --startup-project .
  ```
- **Config/secrets:** connection string, `Jwt:*`, and `Resend:ApiKey` live in
  `RSS/appsettings.json`. Note the sports-API key is currently hard-coded in
  `Program.cs`.
```
