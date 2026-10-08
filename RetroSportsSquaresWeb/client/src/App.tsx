import { Switch, Route } from "wouter";
import { queryClient } from "./lib/queryClient";
import { QueryClientProvider } from "@tanstack/react-query";
import { Toaster } from "@/components/ui/toaster";
import { TooltipProvider } from "@/components/ui/tooltip";
import { Navbar } from "@/components/Navbar";
import { Footer } from "@/components/Footer";
import { WelcomeModal } from "@/components/WelcomeModal";
import NotFound from "@/pages/not-found";
import Home from "@/pages/Home";
import GameOptions from "@/pages/GameOptions";
import Login from "@/pages/Login";
import GameBoard from "@/pages/GameBoard";
import Dashboard from "@/pages/Dashboard";
import PlayerDashboard from "@/pages/PlayerDashboard";
import LeagueOptions from "@/pages/LeagueOptions";
import Signup from "@/pages/SignUp";
import Settings from "@/pages/Settings";
import ConfirmEmailChange from "@/pages/ConfirmEmailChange";
import ResetPassword from "@/pages/ResetPassword";
import AdminDashboard from "@/pages/AdminDashboard";
import { AdminRoute } from "@/components/AdminRoute";
import Support from "@/pages/Support";
import Sitemap from "@/pages/Sitemap";

function Router() {
  return (
    <div className="min-h-screen bg-black text-foreground font-sans selection:bg-red-900 selection:text-white flex flex-col">
      <Navbar />
      <WelcomeModal />
      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 flex-1 w-full">
        <Switch>
          <Route path="/" component={Home} />
          <Route path="/login" component={Login} />
           <Route path="/signup" component={Signup} />
          <Route path="/options" component={GameOptions} />
          <Route path="/leagues/:sport" component={LeagueOptions} />
          <Route path="/arena/:type/:leagueId" component={Dashboard} />
          <Route path="/game/:id" component={GameBoard} />
          <Route path="/player-dashboard" component={PlayerDashboard} />
          <Route path="/admin">
            <AdminRoute>
              <AdminDashboard />
            </AdminRoute>
          </Route>
          <Route path="/settings" component={Settings} />
          <Route path="/support" component={Support} />
          <Route path="/sitemap" component={Sitemap} />
          <Route path="/confirm-email-change" component={ConfirmEmailChange} />
          <Route path="/reset-password" component={ResetPassword} />
          <Route component={NotFound} />
        </Switch>
      </main>
      <Footer />
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
