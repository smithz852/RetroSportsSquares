import { InfoModal } from "@/components/InfoModal";
import { useAuth, useMarkWelcomeSeen } from "@/hooks/use-auth";
import { WELCOME_PAGES } from "@/content/welcomeContent";

export function WelcomeModal() {
  const { user, isLoading } = useAuth();
  const { mutate: markWelcomeSeen } = useMarkWelcomeSeen();

  if (isLoading || !user || user.hasSeenWelcome) return null;

  return (
    <InfoModal
      open={true}
      onClose={() => markWelcomeSeen()}
      title="WELCOME TO RETRO SPORTS SQUARES"
      pages={WELCOME_PAGES}
    />
  );
}
