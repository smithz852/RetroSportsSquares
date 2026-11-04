import { Link } from "wouter";
import { RetroButton } from "@/components/RetroButton";
import { ArrowLeft } from "lucide-react";

const PRIVACY_SECTIONS: { heading: string; body: string[] }[] = [
  {
    heading: "Information We Collect",
    body: [
      "We collect the information you provide at signup (name, gamer tag, email, password) and gameplay data needed to run Sports Squares (game participation, square selections, coin balance).",
    ],
  },
  {
    heading: "Age Verification",
    body: [
      "At signup we ask for your date of birth to confirm you are 18 or older. We do not store your date of birth — only a record that your age was verified, along with the date it was verified.",
    ],
  },
  {
    heading: "How We Use Your Information",
    body: [
      "Your information is used to operate your account, run gameplay, and send account-related emails (such as password resets and game results).",
    ],
  },
  {
    heading: "Your Rights",
    body: [
      "You may request deletion of your account and associated personal data at any time from Settings. See our account deletion process for details on what is removed versus retained for games already in progress.",
    ],
  },
  {
    heading: "Data Security",
    body: [
      "We take reasonable measures to protect your information, including never storing passwords in plain text.",
    ],
  },
];

export default function Privacy() {
  return (
    <div className="space-y-6 max-w-2xl">
      <div className="border-b-2 border-primary/30 pb-4">
        <h1 className="text-xl md:text-2xl font-['Press_Start_2P'] text-primary text-shadow-retro mb-1">
          PRIVACY POLICY
        </h1>
        <p className="font-['VT323'] text-gray-400 text-xl tracking-widest">
          DRAFT &mdash; PENDING LEGAL REVIEW
        </p>
      </div>

      <div className="border-4 border-primary box-shadow-retro bg-black p-6 space-y-8">
        <p className="font-['VT323'] text-lg text-gray-400">
          This page is a placeholder outlining the key privacy practices Sports Squares intends to apply. It has
          not yet been reviewed by legal counsel and should not be treated as final.
        </p>
        {PRIVACY_SECTIONS.map((section) => (
          <section key={section.heading}>
            <h2 className="font-['Press_Start_2P'] text-sm text-primary mb-3">
              {section.heading}
            </h2>
            {section.body.map((paragraph, i) => (
              <p key={i} className="font-['VT323'] text-xl text-gray-300 mb-2">
                {paragraph}
              </p>
            ))}
          </section>
        ))}
      </div>

      <div>
        <Link href="/">
          <RetroButton variant="outline" size="sm">
            <span className="flex items-center gap-2">
              <ArrowLeft className="w-3 h-3" />
              BACK HOME
            </span>
          </RetroButton>
        </Link>
      </div>
    </div>
  );
}
