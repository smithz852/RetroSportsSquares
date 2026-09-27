import type { ReactNode } from "react";
import type { InfoModalPage } from "@/components/InfoModal";
import type { PayoutMode } from "@shared/schema";

// Placeholder copy, condensed from GAME_MODES.md — wording to be refined by product owner.
export const REFRESH_NOTE =
  "Live scores refresh on a roughly 10-minute cycle, not instantly play-by-play — if the board looks a beat behind the real game, that's expected, not a bug.";

const GENERAL_INTRO_PAGE: InfoModalPage = {
  heading: "HOW PAYOUTS WORK",
  body: (
    <>
      <p>
        The pool is everything wagered — price per square times squares claimed — split evenly
        across the periods of the game.
      </p>
      <p>
        A period that lands on an unclaimed square has no winner, and each mode gives that
        moment its own twist. Coins move only when the game ends; everything shown mid-game is
        provisional. Every coin wagered is paid back out — if no period all game has a winner,
        everyone simply gets their wager back.
      </p>
      <p className="text-yellow-400">{REFRESH_NOTE}</p>
    </>
  ),
};

type ModeContent = { title: string; body: ReactNode };

const MODE_CONTENT: Record<PayoutMode, ModeContent> = {
  Default: {
    title: "DEFAULT",
    body: (
      <>
        <p className="italic text-red-400">The classic. Steady payouts, no surprises.</p>
        <p>Each period pays an equal share of the pool to its winner.</p>
        <p>
          A period with no winner pays nothing — its share is returned to all players at the end
          of the game, in proportion to what each player wagered.
        </p>
      </>
    ),
  },
  Fair: {
    title: "FAIR",
    body: (
      <>
        <p className="italic text-red-400">Missed periods make every win worth more.</p>
        <p>The pool splits across the periods that were actually won, not all periods.</p>
        <p>
          Every unclaimed period raises the payout of every winning period. Win twice, hold two
          shares. Nothing is refunded at the end — the winners absorb it all.
        </p>
      </>
    ),
  },
  Push: {
    title: "PUSH",
    body: (
      <>
        <p className="italic text-red-400">Missed periods roll their coins onto the next prize.</p>
        <p>
          Each period pays its equal share, but an unclaimed period pushes its share onto the
          next period's prize. Consecutive misses stack — the pot keeps growing until someone
          claims it.
        </p>
        <p>
          If the pot is still riding when the game ends, it goes to the earliest winner of the
          game.
        </p>
      </>
    ),
  },
  Thief: {
    title: "THIEF",
    body: (
      <>
        <p className="italic text-red-400">
          A missed period arms an arrow. The next winner takes it in the back.
        </p>
        <p className="text-yellow-400">Needs games with 3+ periods.</p>
        <p>
          Payouts use the FAIR split. A period with no winner arms an arrow, fired by the most
          recent winner (the shooter). The next player to win a period is eliminated — the
          shooter takes their entire winnings and all of their squares.
        </p>
        <p>
          Self-hit: winning the period right after your own arrow knocks your winnings loose as a
          bounty the next winner collects, instead of eliminating you. An arrow still armed at the
          buzzer targets the earliest surviving winner — a dud if that player is the shooter.
        </p>
      </>
    ),
  },
  Destruction: {
    title: "DESTRUCTION",
    body: (
      <>
        <p className="italic text-red-400">A missed period drops a bomb on the last winner's coins.</p>
        <p className="text-yellow-400">Needs games with 3+ periods.</p>
        <p>
          Each period pays its equal share, like Default. A period with no winner is a bomb — the
          most recent winner loses everything they've collected, and it becomes loot for the next
          player to win a period. Nobody is eliminated — squares and future wins are unaffected.
        </p>
        <p>
          Salvage rights: the bomb periods' own shares are collected at game end by the game's
          first winner, even if that player was bombed along the way.
        </p>
      </>
    ),
  },
};

export function getGameRulesPages(mode: PayoutMode | null | undefined): InfoModalPage[] {
  const content = MODE_CONTENT[mode ?? "Default"] ?? MODE_CONTENT.Default;
  return [GENERAL_INTRO_PAGE, { heading: content.title, body: content.body }];
}
