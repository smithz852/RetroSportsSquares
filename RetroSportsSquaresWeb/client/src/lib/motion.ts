import type { Target, Transition } from "framer-motion";

// Plain `Target` (no nested transition), since the same value is used for both
// the `initial` prop (which rejects a transition) and the `animate` prop.
type MotionTarget = Target;

// When the user prefers reduced motion, skip straight to the final state instead
// of animating into it (WCAG 2.3.3). `reduced` is framer-motion's useReducedMotion()
// return value, which is `null` until the media query resolves on mount.
export function entranceProps(
  reduced: boolean | null,
  initial: MotionTarget,
  animate: MotionTarget,
  transition?: Transition
) {
  if (reduced) {
    return { initial: animate, animate };
  }
  return { initial, animate, transition };
}

// Drops whileHover/whileTap entirely under reduced motion instead of playing the
// scale/transform animation on interaction.
export function hoverTapProps(
  reduced: boolean | null,
  whileHover?: MotionTarget,
  whileTap?: MotionTarget
) {
  if (reduced) {
    return {};
  }
  return { whileHover, whileTap };
}
