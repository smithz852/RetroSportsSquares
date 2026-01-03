# Sports Squares

## Overview

Sports Squares is a retro arcade-themed web application for managing sports betting pool games. Users can create games, select squares on a 10x10 grid, and track scores during football and basketball games. The application features a distinctive 8-bit visual aesthetic with pixelated fonts, red-on-black color schemes, and arcade-style UI components.

## User Preferences

Preferred communication style: Simple, everyday language.

## System Architecture

### Frontend Architecture
- **Framework**: React 18 with TypeScript
- **Routing**: Wouter (lightweight React router)
- **State Management**: TanStack React Query for server state caching and synchronization
- **Styling**: Tailwind CSS with custom retro arcade theme (Press Start 2P and VT323 fonts)
- **UI Components**: shadcn/ui component library built on Radix UI primitives
- **Animations**: Framer Motion for retro-style transitions and effects
- **Build Tool**: Vite with HMR support

### Backend Architecture
- **Runtime**: Node.js with Express.js
- **Language**: TypeScript with ESM modules
- **API Design**: RESTful endpoints defined in `shared/routes.ts` with Zod validation
- **Build**: esbuild for production bundling with selective dependency bundling

### Data Storage
- **Database**: PostgreSQL with Drizzle ORM
- **Schema Location**: `shared/schema.ts` defines games, square selections, users, and sessions tables
- **Migrations**: Drizzle Kit for database schema management (`db:push` command)

### Authentication
- **Provider**: Replit Auth (OpenID Connect integration)
- **Session Management**: PostgreSQL-backed sessions via connect-pg-simple
- **User Storage**: Mandatory users and sessions tables for Replit Auth compatibility

### Project Structure
```
client/           # React frontend application
  src/
    components/   # UI components including retro-styled custom components
    hooks/        # React Query hooks for API calls
    pages/        # Route page components
    lib/          # Utility functions and query client
server/           # Express backend
  replit_integrations/auth/  # Replit Auth integration
shared/           # Shared types, schemas, and API route definitions
  models/         # Data models (auth)
  schema.ts       # Drizzle database schema
  routes.ts       # API route contracts with Zod validation
```

### Key Design Patterns
- **Type-Safe API Contracts**: Routes defined with Zod schemas in `shared/routes.ts` ensure type safety between frontend and backend
- **Storage Abstraction**: `IStorage` interface in `server/storage.ts` enables potential database swapping
- **Component Composition**: shadcn/ui components customized with retro theme variants

## External Dependencies

### Database
- **PostgreSQL**: Primary database, connection via `DATABASE_URL` environment variable
- **Drizzle ORM**: Type-safe database queries and schema management

### Authentication
- **Replit Auth**: OpenID Connect provider for user authentication
- **Required Environment Variables**: `ISSUER_URL`, `REPL_ID`, `SESSION_SECRET`, `DATABASE_URL`

### UI Libraries
- **Radix UI**: Accessible primitive components (dialogs, dropdowns, tooltips, etc.)
- **Lucide React**: Icon library
- **Framer Motion**: Animation library

### Development Tools
- **Vite**: Development server with hot module replacement
- **Replit Plugins**: Dev banner, cartographer, and runtime error overlay for Replit environment