import { ButtonHTMLAttributes, forwardRef } from "react";
import { cn } from "@/lib/utils";
import { motion } from "framer-motion";

interface RetroButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: "primary" | "secondary" | "outline";
  size?: "sm" | "md" | "lg";
}

export const RetroButton = forwardRef<HTMLButtonElement, RetroButtonProps>(
  ({ className, variant = "primary", size = "md", children, ...props }, ref) => {
    const { onAnimationStart, onAnimationEnd, onAnimationIteration, onDragStart, onDragEnd, onDrag, ...motionProps } = props;
    
    const variants = {
      primary: "bg-primary text-primary-foreground border-2 border-primary hover:bg-red-600",
      secondary: "bg-secondary text-secondary-foreground border-2 border-primary hover:bg-zinc-800",
      outline: "bg-transparent text-primary border-2 border-primary hover:bg-primary/10",
    };

    const sizes = {
      sm: "px-3 py-1 text-xs font-['Press_Start_2P']",
      md: "px-6 py-3 text-sm font-['Press_Start_2P']",
      lg: "px-8 py-4 text-base font-['Press_Start_2P']",
    };

    return (
      <motion.button
        ref={ref}
        whileHover={{ scale: 1.02 }}
        whileTap={{ scale: 0.95 }}
        className={cn(
          "relative uppercase transition-colors duration-100",
          "focus:outline-none focus:ring-2 focus:ring-white focus:ring-offset-2 focus:ring-offset-black",
          "box-shadow-retro active:translate-y-1 active:shadow-none",
          variants[variant],
          sizes[size],
          className
        )}
        {...motionProps}
      >
        {children}
      </motion.button>
    );
  }
);

RetroButton.displayName = "RetroButton";
