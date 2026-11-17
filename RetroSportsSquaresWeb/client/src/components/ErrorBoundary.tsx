import { Component, type ReactNode } from "react";
import { AlertTriangle } from "lucide-react";
import { RetroButton } from "@/components/RetroButton";

interface ErrorBoundaryProps {
  children: ReactNode;
}

interface ErrorBoundaryState {
  hasError: boolean;
}

export class ErrorBoundary extends Component<ErrorBoundaryProps, ErrorBoundaryState> {
  state: ErrorBoundaryState = { hasError: false };

  static getDerivedStateFromError() {
    return { hasError: true };
  }

  componentDidCatch(error: unknown, info: unknown) {
    console.error("Uncaught render error:", error, info);
  }

  render() {
    if (this.state.hasError) {
      return (
        <div className="min-h-[80vh] w-full flex flex-col items-center justify-center bg-black text-center px-4">
          <div className="mb-8 relative">
            <AlertTriangle className="h-32 w-32 text-red-600 animate-pulse motion-reduce:animate-none" />
            <div className="absolute inset-0 border-4 border-red-600 animate-ping motion-reduce:animate-none opacity-20" />
          </div>

          <h1 className="text-4xl md:text-6xl font-['Press_Start_2P'] text-red-600 mb-6 text-shadow-retro">
            SYSTEM ERROR
          </h1>

          <p className="text-xl md:text-2xl font-['VT323'] text-red-400 mb-12 max-w-lg">
            SOMETHING WENT WRONG. TRY RESTARTING THIS SCREEN.
          </p>

          <RetroButton size="lg" onClick={() => this.setState({ hasError: false })}>
            TRY AGAIN
          </RetroButton>
        </div>
      );
    }

    return this.props.children;
  }
}
