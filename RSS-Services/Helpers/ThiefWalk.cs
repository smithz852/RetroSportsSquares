using System.Collections.Generic;
using System.Linq;

namespace RSS_Services.Helpers
{
    public enum ThiefEventType
    {
        // Victim's whole pot moves to the shooter; live side also transfers squares.
        Elimination,
        // Shooter won their own arrow's period mid-game: their pot becomes a bounty.
        SelfHit,
        // A winner picks up a pending self-hit bounty.
        BountyCollect,
    }

    // Period == null means the event resolves at game end (an arrow still in
    // flight, or a bounty nobody claimed). Actor is the player acting/receiving:
    // Elimination → Actor = shooter, Target = victim; SelfHit → Actor = owner;
    // BountyCollect → Actor = collector, Target = bounty owner.
    public sealed record ThiefEvent(ThiefEventType Type, int? Period, string ActorId, string? TargetId);

    // Replays the resolved period sequence and derives every arrow event, in the
    // exact order it occurs. Pure structure, no amounts — the settlement engine,
    // the live processor (square transfers), and settlement flagging all consume
    // this one walk, so the rules can never drift between them.
    //
    // Rules (user-final, July 2026):
    // - A null period with a prior winner arms an arrow; the shooter is the most
    //   recent SURVIVING winner. Consecutive nulls keep one arrow, same shooter.
    // - Winner while armed, winner != shooter → elimination. Order of operations
    //   on any winning period: credit the win, collect any bounty, then the arrow.
    // - Winner while armed, winner == shooter → self-hit bomb (mid-game only):
    //   pot becomes a bounty for the next winner. On the FINAL period it's a dud.
    // - Arrow still in flight at game end → shooter eliminates the earliest
    //   surviving winner; if that is the shooter (sole survivor) it's a dud.
    // - A bounty unclaimed at game end goes to the earliest surviving winner
    //   (silently back to its owner when they're the only one left).
    public static class ThiefWalk
    {
        public static List<ThiefEvent> Analyze(IReadOnlyDictionary<int, string?> periodWinners, int periodCount)
        {
            var events = new List<ThiefEvent>();
            string? lastWinner = null;   // most recent surviving winner = shooter when armed
            var armed = false;
            string? bountyOwner = null;
            var eliminated = new HashSet<string>();
            var winnersInOrder = new List<string>();

            var allResolved = true;
            for (int period = 1; period <= periodCount; period++)
            {
                if (!periodWinners.TryGetValue(period, out var winnerId))
                {
                    // Unresolved tail — mid-game analysis stops here.
                    allResolved = false;
                    break;
                }

                if (winnerId == null)
                {
                    if (lastWinner != null) armed = true;
                    continue;
                }

                // Defensive for arbitrary sequences (property tests): a live game
                // can never record an eliminated player as a later winner (they own
                // no squares), but if a sequence does, treat them as back in play.
                eliminated.Remove(winnerId);
                if (!winnersInOrder.Contains(winnerId)) winnersInOrder.Add(winnerId);

                if (bountyOwner != null)
                {
                    events.Add(new ThiefEvent(ThiefEventType.BountyCollect, period, winnerId, bountyOwner));
                    bountyOwner = null;
                }

                if (armed)
                {
                    armed = false;
                    if (winnerId == lastWinner)
                    {
                        if (period < periodCount)
                        {
                            events.Add(new ThiefEvent(ThiefEventType.SelfHit, period, winnerId, null));
                            bountyOwner = winnerId;
                        }
                        // Final-period self-hit: plain dud, shooter keeps the pot.
                    }
                    else
                    {
                        events.Add(new ThiefEvent(ThiefEventType.Elimination, period, lastWinner!, winnerId));
                        eliminated.Add(winnerId);
                        continue; // shooter remains the most recent surviving winner
                    }
                }

                lastWinner = winnerId;
            }

            if (allResolved)
            {
                if (armed && lastWinner != null)
                {
                    var target = winnersInOrder.FirstOrDefault(w => !eliminated.Contains(w));
                    if (target != null && target != lastWinner)
                    {
                        events.Add(new ThiefEvent(ThiefEventType.Elimination, null, lastWinner, target));
                        eliminated.Add(target);
                    }
                    // target == shooter → sole surviving winner → dud
                }

                if (bountyOwner != null)
                {
                    var recipient = winnersInOrder.FirstOrDefault(w => !eliminated.Contains(w)) ?? bountyOwner;
                    events.Add(new ThiefEvent(ThiefEventType.BountyCollect, null, recipient, bountyOwner));
                }
            }

            return events;
        }
    }
}
