-- Odds-board "Won" column scenarios (4-period football game).
--
-- HOW TO USE
--   1. Create a football game (Chiefs/Bills) with 3 players (A, B, C) who each hold
--      some squares, e.g. via the UI. Pick the payout mode in the create dialog, or
--      set it below with the MODE line.
--   2. Run the lookup query to get the game id and the three user ids, then paste
--      them into the SET lines.
--   3. Run ONE scenario's steps in order, refreshing the game board after each step.
--      Each step overwrites PeriodWinners, so you can also jump around.
--
--   mysql -uroot RetroSportsSquares < winnings-scenarios.sql   (or paste blocks into a client)
--
-- CAVEATS: this writes PeriodWinners directly, so it bypasses GameResultProcessor.
-- No emails are sent, no Thief square transfers happen, no settlement / wallet
-- credits run, and IsCompleted stays 0. The Won column only reads PeriodWinners,
-- so it still shows exactly what the engine computes.
-- P below = price per square x squares claimed (the pool). 4 periods.

-- ---------------------------------------------------------------- lookup
-- Games (newest first) and who is in them:
SELECT sg.Id AS game_id, sg.PayoutMode, sg.PeriodCount, sg.PeriodWinners
FROM SquareGames sg ORDER BY sg.CreatedAt DESC LIMIT 5;

-- Players in a game, with the name the board shows (GamerTag ?? DisplayName):
-- SELECT gp.ApplicationUserId AS user_id, COALESCE(u.GamerTag, u.DisplayName) AS board_name
-- FROM GamePlayers gp JOIN AspNetUsers u ON u.Id = gp.ApplicationUserId
-- WHERE gp.GameId = '<game id>';

SET @g = '<game id>';
SET @a = '<user id A>';
SET @b = '<user id B>';
SET @c = '<user id C>';

-- ---------------------------------------------------------------- reset
-- Run before each scenario (optionally pick the mode).
-- UPDATE SquareGames SET PayoutMode = 'Fair' WHERE Id = @g;   -- Default|Fair|Push|Destruction|Thief
UPDATE SquareGames SET PeriodWinners = '{}', IsCompleted = 0, SettlementCompleted = 0 WHERE Id = @g;

-- ================================================================ FAIR
-- Share = P / (4 - unclaimed periods so far); only rises as periods go unclaimed.
-- (mode must be Fair)
UPDATE SquareGames SET PeriodWinners = JSON_OBJECT('1', @a) WHERE Id = @g;
--   -> A: P/4
UPDATE SquareGames SET PeriodWinners = JSON_OBJECT('1', @a, '2', NULL) WHERE Id = @g;
--   -> A: P/3   (Q2 unclaimed, share rose)
UPDATE SquareGames SET PeriodWinners = JSON_OBJECT('1', @a, '2', NULL, '3', @b) WHERE Id = @g;
--   -> A: P/3, B: P/3
UPDATE SquareGames SET PeriodWinners = JSON_OBJECT('1', @a, '2', NULL, '3', @b, '4', @c) WHERE Id = @g;
--   -> A, B, C: P/3 each = final settlement (sums to P)

-- ================================================================ PUSH
-- Unclaimed period's share rides onto the next winner. (mode must be Push)
UPDATE SquareGames SET PeriodWinners = JSON_OBJECT('1', @a) WHERE Id = @g;
--   -> A: P/4
UPDATE SquareGames SET PeriodWinners = JSON_OBJECT('1', @a, '2', NULL) WHERE Id = @g;
--   -> A: P/4; pot banner shows P/2 riding on the next period; nobody's row changes
UPDATE SquareGames SET PeriodWinners = JSON_OBJECT('1', @a, '2', NULL, '3', @b) WHERE Id = @g;
--   -> A: P/4, B: P/2 (own P/4 + carried P/4)
UPDATE SquareGames SET PeriodWinners = JSON_OBJECT('1', @a, '2', NULL, '3', @b, '4', @a) WHERE Id = @g;
--   -> A: P/2, B: P/2 (final)

-- ================================================================ DESTRUCTION
-- Unclaimed period = bomb: last winner loses their pot, next winner collects it;
-- bomb-period shares salvage to the FIRST winner at the end. (mode must be Destruction)
UPDATE SquareGames SET PeriodWinners = JSON_OBJECT('1', @a) WHERE Id = @g;
--   -> A: P/4
UPDATE SquareGames SET PeriodWinners = JSON_OBJECT('1', @a, '2', NULL) WHERE Id = @g;
--   -> A: 0 (bombed); loot banner shows P/4 for the next winner
UPDATE SquareGames SET PeriodWinners = JSON_OBJECT('1', @a, '2', NULL, '3', @b) WHERE Id = @g;
--   -> A: 0, B: P/2 (own P/4 + looted P/4)
UPDATE SquareGames SET PeriodWinners = JSON_OBJECT('1', @a, '2', NULL, '3', @b, '4', @a) WHERE Id = @g;
--   -> A: P/2 (Q4 P/4 + salvage P/4), B: P/2 (final)

-- ================================================================ THIEF
-- Unclaimed period arms an arrow; the next DIFFERENT winner is eliminated and their
-- whole pot goes to the shooter. Fair base share. (mode must be Thief)
UPDATE SquareGames SET PeriodWinners = JSON_OBJECT('1', @a) WHERE Id = @g;
--   -> A: P/4
UPDATE SquareGames SET PeriodWinners = JSON_OBJECT('1', @a, '2', NULL) WHERE Id = @g;
--   -> A: P/3; arrow armed, nothing stolen yet
UPDATE SquareGames SET PeriodWinners = JSON_OBJECT('1', @a, '2', NULL, '3', @b) WHERE Id = @g;
--   -> A: 2P/3 (stole B's pot), B: 0 (eliminated)
UPDATE SquareGames SET PeriodWinners = JSON_OBJECT('1', @a, '2', NULL, '3', @b, '4', @c) WHERE Id = @g;
--   -> A: 2P/3, B: 0, C: P/3 (final)
-- Variant: self-hit. Same start, but Q3 goes to A again -> A's pot becomes a bounty
-- for the next winner:
-- UPDATE SquareGames SET PeriodWinners = JSON_OBJECT('1', @a, '2', NULL, '3', @a) WHERE Id = @g;
--   -> A: 0 (A won Q1 + Q3 = 2P/3, and the self-hit parks that whole pot as a bounty)
-- UPDATE SquareGames SET PeriodWinners = JSON_OBJECT('1', @a, '2', NULL, '3', @a, '4', @b) WHERE Id = @g;
--   -> A: 0, B: P (own P/3 + collected bounty 2P/3) = final

-- ================================================================ DEFAULT (control)
-- Flat P/4 per won period; nothing else moves. (mode must be Default)
UPDATE SquareGames SET PeriodWinners = JSON_OBJECT('1', @a, '2', NULL, '3', @b) WHERE Id = @g;
--   -> A: P/4, B: P/4 (Q2's P/4 is refunded at settlement, not shown as winnings)
