import { Link } from "wouter";
import { RetroButton } from "@/components/RetroButton";
import { ArrowLeft } from "lucide-react";

const TERMS_SECTIONS: { heading: string; body: string[] }[] = [
  {
    heading: "No Cash Value",
    body: [
      "Coins and in-game wagers used on Sports Squares have no cash value. Coins cannot be purchased with real money, redeemed for cash or any other item of value, cashed out, or transferred between accounts.",
      "Sports Squares is a free-to-play game of skill and chance using virtual currency only. No real-money prizes are awarded at any time.",
    ],
  },
  {
    heading: "Eligibility",
    body: [
      "You must be 18 years of age or older to create an account and use Sports Squares.",
    ],
  },
  {
    heading: "Account Responsibilities",
    body: [
      "You are responsible for maintaining the confidentiality of your account credentials and for all activity under your account.",
    ],
  },
  {
    heading: "Acceptable Use",
    body: [
      "You agree not to misuse the service, attempt to circumvent game rules, or interfere with other players' experience.",
    ],
  },
  {
    heading: "Changes to These Terms",
    body: [
      "We may update these Terms from time to time. Continued use of Sports Squares after a change constitutes acceptance of the revised Terms.",
    ],
  },
];

export default function Terms() {
  return (
    <div className="space-y-6 max-w-2xl">
      <div className="border-b-2 border-primary/30 pb-4">
        <h1 className="text-xl md:text-2xl font-['Press_Start_2P'] text-primary text-shadow-retro mb-1">
          TERMS OF SERVICE
        </h1>
        <p className="font-['VT323'] text-gray-400 text-xl tracking-widest">
          DRAFT &mdash; PENDING LEGAL REVIEW
        </p>
      </div>

      <div className="border-4 border-primary box-shadow-retro bg-black p-6 space-y-8">
        <p className="font-['VT323'] text-lg text-gray-400">
          This page is a placeholder outlining the key terms Sports Squares intends to apply. It has not yet
          been reviewed by legal counsel and should not be treated as final.
        </p>
        {TERMS_SECTIONS.map((section) => (
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
