import type { ReactNode } from "react";
import { Circle, Handshake, Hand, BowArrow, Bomb, type LucideIcon } from "lucide-react";
import type { InfoModalPage } from "@/components/InfoModal";
import type { PayoutMode } from "@shared/schema";

// Placeholder copy, condensed from GAME_MODES.md — wording to be refined by product owner.
export const REFRESH_NOTE =
  "Right now, live scores update on a roughly 10-minute cycle rather than instant play-by-play — that's today's speed, and we'll be looking to speed it up in future updates as the platform grows.";

// The stored/compared value everywhere else in the app stays "Default" (DB + settlement
// logic) — this only controls what players see it called.
export function getPayoutModeLabel(mode: PayoutMode | null | undefined): string {
  const resolved = mode ?? "Default";
  return resolved === "Default" ? "STANDARD" : resolved.toUpperCase();
}

// Board-cell icon shown on unclaimed squares once a game has started, in place of "OPEN".
const PAYOUT_MODE_ICONS: Record<PayoutMode, LucideIcon> = {
  Default: Circle,
  Fair: Handshake,
  Push: Hand,
  Thief: BowArrow,
  Destruction: Bomb,
};

export function getPayoutModeIcon(mode: PayoutMode | null | undefined): LucideIcon {
  return PAYOUT_MODE_ICONS[mode ?? "Default"] ?? PAYOUT_MODE_ICONS.Default;
}

const GENERAL_INTRO_PAGE: InfoModalPage = {
  heading: "Rules",
  body: (
    <>
     <p>
        <span className="text-yellow-400">Selections</span>: Start selecting your squares by clicking on them. Each square costs the stated price. You can claim as many as you want or up to the limit set by the host.
        For turn based games, you must wait until the host initiates selections to claim squares. For free-for-all games, you can claim squares at any time until the host starts the game.
        Once you've made your picks in either case, click the <span className="text-yellow-400">Submit</span> button to lock in your selections. All submissions are final.
      </p>
      <p>
        <span className="text-yellow-400">Winning</span>: The winning square is determined by the last digit of each team's score at the end of each period. The number will correspond to the randomly assigned digits on the outside of the board, and where the lines intersect is the winning square.
      </p>
      <p>
        <span className="text-yellow-400">Unclaimed Squares</span>: A period that lands on an unclaimed square has no winner, and each mode gives that
        moment its own twist. Coins move only when the game ends; everything shown mid-game is
        provisional. Every coin wagered is paid back out if no period all game has a winner.
      </p>
      <p>
        <span className="text-yellow-400">Payouts</span>: The amount won each period is dermined by the total pool of coins and the game mode.
        The pool is everything wagered, price per square times squares claimed.
      </p>
      
      <p className="text-yellow-400">{REFRESH_NOTE}</p>
    </>
  ),
};

type ModeContent = { title: string; body: ReactNode };

const MODE_CONTENT: Record<PayoutMode, ModeContent> = {
  Default: {
    title: getPayoutModeLabel("Default"),
    body: (
      <>
        <p className="italic text-red-400">The classic. Steady payouts, no surprises.</p>
        <p>Each period pays an equal share of the pool to its winner.</p>
        <p>
          A period with no winner pays nothing — its share is returned to all players at the end
          of the game, in proportion to what each player wagered. 
        </p>
        <p>I.e., If you wagered 20 coins and the pool was 100 coins, you get 20% of the unclaimed period(s) back.</p>
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
          Landing on a Fair square for the period end raises the payout of every winning period. Nothing is refunded at the end — the winners absorb it all. 
        </p>
        <p>I.e., In a 4 period game, if 3 periods are won by players, and 1 period lands on a Fair square, the pool is split across the 3 winning periods.</p>
      </>
    ),
  },
  Push: {
    title: "PUSH",
    body: (
      <>
        <p className="italic text-red-400">Missed periods roll their coins onto the next prize.</p>
        <p>
          Each period pays its equal share, but landing on a Push square for a period pushes its share onto the
          next period's prize. Consecutive Push squares stack — the pot keeps growing until someone
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
          Payouts use the FAIR split. A period that ends on an Arrow square arms an arrow, fired by the most
          recent winner (the shooter). The next player to win a period is eliminated — the
          shooter takes their entire winnings and all of their squares. If there is no recent winner when an Arrow square wins, it's a dud.
        </p>
        <p>Landing on consecutive Arrow squares means the arrow stays in flight. 
          A winning Arrow square on the last period hits the earliest surviving winner of the game, except if the earliest winner is the shooter, in which case its a dud (see self-hit rules next).
          </p>
        <p>
          <span className="text-red-400">Self-hit</span>: winning the period right after your own arrow knocks your winnings loose, but does not eliminate you. The next player to win takes all the loose coins.
          If an Arrow square wins on the last period, and the shooter is the earliest surviving winner, its a dud since no one has a chance to collect the loose coins.
        </p>
      </>
    ),
  },
  Destruction: {
    title: "DESTRUCTION",
    body: (
      <>
        <p className="italic text-red-400">A Bomb square drops a bomb on the last winner's coins.</p>
        <p className="text-yellow-400">Needs games with 3+ periods.</p>
        <p>
          Each period pays its equal share, like Standard. A period with no winner is a bomb — the
          most recent winner loses everything they've collected, and it becomes loot for the next
          player to win a period. Nobody is eliminated, squares and future wins are unaffected.
        </p>
        <p>
          <span className="text-yellow-400">Salvage Rights</span>: the bomb periods' still own an equal share of the pot. Those shares are collected at game end by the game's
          first winner, even if that player was bombed along the way. The first winner also collects any unclaimed coins left at game end from previous bombs.
        </p>
      </>
    ),
  },
};

export function getGameRulesPages(mode: PayoutMode | null | undefined): InfoModalPage[] {
  const content = MODE_CONTENT[mode ?? "Default"] ?? MODE_CONTENT.Default;
  return [GENERAL_INTRO_PAGE, { heading: content.title, body: content.body }];
}

// Mode-only, no square-selection mechanics — for previewing a mode before a game
// (and its squares) exist, e.g. the CreateGameDialog picker.
export function getModeDetailPage(mode: PayoutMode | null | undefined): InfoModalPage[] {
  const content = MODE_CONTENT[mode ?? "Default"] ?? MODE_CONTENT.Default;
  return [{ heading: content.title, body: content.body }];
}
