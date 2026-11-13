import { useCallback, useEffect, useRef, useState } from "react";

const ANNOUNCEMENT_LIFETIME_MS = 5000;

interface Announcement {
  id: number;
  text: string;
}

// A visually-hidden role="status" region for non-interruptive live updates
// (e.g. SignalR gameplay events). Each announce() call appends a new, uniquely
// keyed entry rather than overwriting a single string — screen readers only
// re-announce aria-live content on a DOM mutation, so repeating the exact same
// text back-to-back (e.g. two SquareSelected events in a row) would otherwise
// go silent on the second one.
export function useLiveAnnouncer() {
  const [announcements, setAnnouncements] = useState<Announcement[]>([]);
  const nextId = useRef(0);
  const timeoutsRef = useRef<Set<ReturnType<typeof setTimeout>>>(new Set());

  const announce = useCallback((text: string) => {
    const id = nextId.current++;
    setAnnouncements((prev) => [...prev, { id, text }]);
    const timeout = setTimeout(() => {
      timeoutsRef.current.delete(timeout);
      setAnnouncements((prev) => prev.filter((a) => a.id !== id));
    }, ANNOUNCEMENT_LIFETIME_MS);
    timeoutsRef.current.add(timeout);
  }, []);

  useEffect(() => {
    return () => {
      timeoutsRef.current.forEach(clearTimeout);
      timeoutsRef.current.clear();
    };
  }, []);

  const liveRegion = (
    <div role="status" aria-live="polite" aria-atomic="false" className="sr-only">
      {announcements.map((a) => (
        <div key={a.id}>{a.text}</div>
      ))}
    </div>
  );

  return { announce, liveRegion };
}
