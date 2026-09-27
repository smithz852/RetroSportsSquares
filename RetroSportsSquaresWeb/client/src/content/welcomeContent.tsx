import type { InfoModalPage } from "@/components/InfoModal";
import { REFRESH_NOTE } from "@/content/gameModeRules";

// Placeholder copy — wording to be refined by product owner.
export const WELCOME_PAGES: InfoModalPage[] = [
  {
    heading: "WELCOME TO Retro Sports Square",
    body: (
      <>
        <p>
          Retro Sports Squares turns real sports games into an exciting competition! Claim squares,
          generate the board, and win when the final score of a period matches your
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
          At the end of each period, the last digit of each team's score points to a square on the grid.
          Whoever owns that square wins that period!
        </p>
      </>
    ),
   },
    {
        heading: "FINDING GAMES",
        body: (
            <>
                <p>
                    Your journey starts on the <span className="text-yellow-400">Arena</span> page!
                </p>
                <p>You'll be presented first with a list of active sports for the day, which upon selection, will present you with a list of active leagues for that sport.
                For a sport or league to be an option, it must have a planned or live game for the day. If there are neither, you won't have any sports to choose from.
                </p>
                <p>After you've made your selction, you'll reach the lobby where you can join, search, or create a new game.</p>
                <p>Private games can be joined from the lobby as well using the <span className="text-yellow-400">Enter Code</span> button.</p>
            </>
        ),
    },
  {
    heading: "GAME MODES & COINS",
    body: (
      <>
        <p>
          We have <span className="text-yellow-400">five game modes to choose from</span> —
          Standard, Fair, Push, Thief, or Destruction — each with its own twist on what happens
          when a period lands on an unclaimed square.
        </p>
        <p>
          Look for the mode badge above the board, and hit the{" "}
          <span className="text-yellow-400">RULES</span> button any time you need a refresher.
            </p>
            <p>Public games use your coin wallet; private games play for bragging rights only.</p>
            <p>Aside from winning, <span className="text-yellow-400">you get 15 coin per day upon login</span>. </p>
      </>
    ),
   },
    {
        heading: "PLAYER DASHBOARD",
        body: (
            <>
                <p>
                    Your <span className="text-yellow-400">Profile</span> button gets you access to the Player Dashboard.
                </p>
                <p>
                    Here you can track your stats, gained through playing in public games, view and navigate to your current matches, and check your past matches.
                </p>
                <p>Your settings are also available on this page for any account related changes.</p>
            </>
        ),
    },
  {
    heading: "LIVE SCORES",
    body: (
      <>
        <p>{REFRESH_NOTE}</p>
        <p>GOOD LUCK, AND HAVE FUN!</p>
      </>
    ),
  },
];
