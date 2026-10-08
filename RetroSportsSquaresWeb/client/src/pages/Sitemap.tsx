import { Link } from "wouter";
import { RetroButton } from "@/components/RetroButton";
import { ArrowLeft } from "lucide-react";

const SITEMAP_SECTIONS: { heading: string; links: { href: string; label: string }[] }[] = [
  {
    heading: "PLAY",
    links: [
      { href: "/", label: "Home" },
      { href: "/options", label: "Browse Games" },
    ],
  },
  {
    heading: "ACCOUNT",
    links: [
      { href: "/login", label: "Login" },
      { href: "/signup", label: "Sign Up" },
      { href: "/player-dashboard", label: "Player Dashboard" },
      { href: "/settings", label: "Settings" },
    ],
  },
  {
    heading: "HELP",
    links: [{ href: "/support", label: "Support" }],
  },
];

export default function Sitemap() {
  return (
    <div className="space-y-6 max-w-2xl">
      <div className="border-b-2 border-primary/30 pb-4">
        <h1 className="text-xl md:text-2xl font-['Press_Start_2P'] text-primary text-shadow-retro mb-1">
          SITEMAP
        </h1>
        <p className="font-['VT323'] text-gray-400 text-xl tracking-widest">
          EVERY PAGE ON THE SITE
        </p>
      </div>

      <div className="border-4 border-primary box-shadow-retro bg-black p-6 space-y-8">
        {SITEMAP_SECTIONS.map((section) => (
          <section key={section.heading} aria-labelledby={`sitemap-${section.heading}`}>
            <h2
              id={`sitemap-${section.heading}`}
              className="font-['Press_Start_2P'] text-sm text-primary mb-3"
            >
              {section.heading}
            </h2>
            <ul className="space-y-2">
              {section.links.map((link) => (
                <li key={link.href}>
                  <Link
                    href={link.href}
                    className="font-['VT323'] text-xl text-gray-300 hover:text-primary transition-colors focus-visible:outline-none focus-visible:underline focus-visible:text-primary"
                  >
                    {link.label}
                  </Link>
                </li>
              ))}
            </ul>
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
