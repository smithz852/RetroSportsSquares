import type { InfoModalPage } from "@/components/InfoModal";

// Placeholder copy — wording to be refined by product owner.
export const WELCOME_PAGES: InfoModalPage[] = [
  {
    heading: "WELCOME TO THE ARENA",
    body: (
      <>
        <p>
          Retro Sports Squares turns real sports games into a betting board — claim squares,
          get assigned random digits, and win when the final score of a period matches your
          square.
        </p>
        <p>No sports knowledge required. Just squares, digits, and a live scoreboard.</p>
      </>
    ),
  },
  {
    heading: "HOW SQUARES WORK",
    body: (
      <>
        <p>
          Every board is a 10x10 grid. Once selections close, each row and column gets a random
          digit 0-9 — <span className="text-yellow-400">you don't pick your numbers, the board
          assigns them.</span>
        </p>
        <p>
          At the end of each period, the last digit of each team's score points to a square.
          Whoever owns that square wins that period's share of the pot.
        </p>
      </>
    ),
  },
  {
    heading: "GAME MODES & COINS",
    body: (
      <>
        <p>
          Every game runs one of five <span className="text-yellow-400">payout modes</span> —
          Default, Fair, Push, Thief, or Destruction — each with its own twist on what happens
          when a period lands on an unclaimed square.
        </p>
        <p>
          Look for the mode badge above the board, and hit the{" "}
          <span className="text-yellow-400">RULES</span> button any time you need a refresher.
          Public games use your coin wallet; private games play for bragging rights only.
        </p>
      </>
    ),
  },
  {
    heading: "LIVE SCORES",
    body: (
      <>
        <p>
          Scores and the board refresh on a live feed roughly every{" "}
          <span className="text-yellow-400">10 minutes</span> — not instantly play-by-play. If a
          score looks a beat behind the real game, that's expected.
        </p>
        <p>GOOD LUCK, AND HAVE FUN!</p>
      </>
    ),
  },
];
