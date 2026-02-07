import { Switch, Route } from "wouter";
import { queryClient } from "./lib/queryClient";
import { QueryClientProvider } from "@tanstack/react-query";
import { Toaster } from "@/components/ui/toaster";
import { TooltipProvider } from "@/components/ui/tooltip";
import { Navbar } from "@/components/Navbar";
import NotFound from "@/pages/not-found";
import Home from "@/pages/Home";
import GameOptions from "@/pages/GameOptions";
import Login from "@/pages/Login";
import GameBoard from "@/pages/GameBoard";
import Dashboard from "@/pages/Dashboard";
import Signup from "@/pages/SignUp";

function Router() {
  return (
    <div className="min-h-screen bg-black text-foreground font-sans selection:bg-red-900 selection:text-white pb-20">
      <Navbar />
      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <Switch>
          <Route path="/" component={Home} />
          <Route path="/login" component={Login} />
           <Route path="/signup" component={Signup} />
          <Route path="/options" component={GameOptions} />
          <Route path="/arena/:type/:leagueId" component={Dashboard} />
          <Route path="/game/:id" component={GameBoard} />
          <Route component={NotFound} />
        </Switch>
      </main>
    </div>
  );
}

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <TooltipProvider>
        <Router />
        <Toaster />
      </TooltipProvider>
    </QueryClientProvider>
  );
}

export default App;
