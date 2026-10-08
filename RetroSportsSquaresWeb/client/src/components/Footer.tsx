import { Link } from "wouter";

const linkClass =
  "font-['VT323'] text-lg text-gray-400 hover:text-primary transition-colors focus-visible:outline-none focus-visible:underline focus-visible:text-primary";

const headingClass =
  "font-['Press_Start_2P'] text-[10px] tracking-wider text-primary mb-3";

const FOOTER_GROUPS: { heading: string; links: { href: string; label: string }[] }[] = [
  {
    heading: "PLAY",
    links: [
      { href: "/", label: "Home" },
      { href: "/options", label: "Game Options" },
    ],
  },
  {
    heading: "ACCOUNT",
    links: [
      { href: "/player-dashboard", label: "Player Dashboard" },
      { href: "/settings", label: "Settings" },
    ],
  },
  {
    heading: "HELP",
    links: [{ href: "/support", label: "Support" }],
  },
  {
    heading: "SITE",
    links: [{ href: "/sitemap", label: "Sitemap" }],
  },
];

export function Footer() {
  return (
    <footer role="contentinfo" className="border-t-4 border-primary bg-black mt-12">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-10">
        <nav aria-label="Footer" className="grid grid-cols-2 sm:grid-cols-4 gap-8">
          {FOOTER_GROUPS.map((group) => (
            <div key={group.heading}>
              <h2 className={headingClass}>{group.heading}</h2>
              <ul className="space-y-2">
                {group.links.map((link) => (
                  <li key={link.href}>
                    <Link href={link.href} className={linkClass}>
                      {link.label}
                    </Link>
                  </li>
                ))}
              </ul>
            </div>
          ))}
        </nav>

        <p className="font-['VT323'] text-gray-600 text-sm mt-10 pt-6 border-t-2 border-primary/20">
          &copy; {new Date().getFullYear()} Sports Squares. All rights reserved.
        </p>
      </div>
    </footer>
  );
}
