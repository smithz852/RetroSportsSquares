import { useState } from "react";
import { useLocation } from "wouter";
import { RetroButton } from "@/components/RetroButton";
import { Input } from "@/components/ui/input";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { useToast } from "@/hooks/use-toast";
import { useSignup } from "@/hooks/use-auth";
import { motion } from "framer-motion";

export default function Signup() {
  const [, setLocation] = useLocation();
  const { toast } = useToast();
  const { mutate: signup, isPending } = useSignup();
  const [formData, setFormData] = useState({
    name: "",
    email: "",
    password: "",
    confirmPassword: "",
  });

  const validatePassword = (password: string) => {
    const minLength = password.length >= 8;
    const hasUpperCase = /[A-Z]/.test(password);
    const hasNumber = /[0-9]/.test(password);
    const hasSpecialChar = /[!@#$%^&*(),.?":{}|<>]/.test(password);
    
    return minLength && hasUpperCase && hasNumber && hasSpecialChar;
  };
  
  const handlePasswordChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const password = e.target.value;
    setFormData({ ...formData, password });
    
    // Set custom validation message
    if (!validatePassword(password) && password.length > 0) {
      e.target.setCustomValidity("Password must be 8+ chars with uppercase, number & special char");
    } else {
      e.target.setCustomValidity("");
    }
  };

  const handleConfirmPasswordChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const confirmPassword = e.target.value;
    setFormData({ ...formData, confirmPassword });
    
    // Check if passwords match
    if (formData.password !== confirmPassword && confirmPassword.length > 0) {
      e.target.setCustomValidity("Passwords do not match");
    } else {
      e.target.setCustomValidity("");
    }
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    signup({
      name: formData.name,
      email: formData.email,
      password: formData.password,
    }, {
      onSuccess: () => {
        toast({
          title: "SUCCESS",
          description: "PROFILE CREATED! PLEASE LOGIN.",
          className: "bg-black border-2 border-primary text-primary font-pixel text-[10px]",
        });
        setLocation("/login");
      },
      onError: () => {
        toast({
          title: "ERROR",
          description: "FAILED TO CREATE PROFILE.",
          variant: "destructive",
          className: "bg-black border-2 border-red-900 text-red-500 font-pixel text-[10px]",
        });
      }
    });
  };

  const passwordPattern = "^(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#$%^&*(),.?\":{}|<>]).{8,}$";

  return (
    <div className="min-h-[80vh] flex items-center justify-center p-4">
      <motion.div
        initial={{ scale: 0.9, opacity: 0 }}
        animate={{ scale: 1, opacity: 1 }}
        className="w-full max-w-md"
      >
        <Card className="bg-black border-4 border-primary rounded-none shadow-[0_0_30px_rgba(255,0,0,0.2)]">
          <CardHeader className="border-b-4 border-primary bg-primary/10 p-6">
            <CardTitle className="text-xl text-primary font-pixel text-center uppercase">
              Create Profile
            </CardTitle>
          </CardHeader>
          <CardContent className="p-8">
            <form onSubmit={handleSubmit} className="space-y-6">
              <div className="space-y-2">
                <label className="text-primary font-pixel text-[10px] uppercase">Name</label>
                <Input
                  required
                  value={formData.name}
                  onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                  className="bg-black border-2 border-primary text-white font-pixel text-xs rounded-none h-12 focus-visible:ring-0 focus-visible:border-white"
                />
              </div>
              <div className="space-y-2">
                <label className="text-primary font-pixel text-[10px] uppercase">Email</label>
                <Input
                  required
                  type="email"
                  value={formData.email}
                  onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                  className="bg-black border-2 border-primary text-white font-pixel text-xs rounded-none h-12 focus-visible:ring-0 focus-visible:border-white"
                />
              </div>
              <div className="space-y-2">
                <label className="text-primary font-pixel text-[10px] uppercase">Password</label>
                <Input
                  required
                  type="password"
                  pattern={passwordPattern}
                  value={formData.password}
                  onChange={handlePasswordChange}
                  className="bg-black border-2 border-primary text-white font-pixel text-xs rounded-none h-12 focus-visible:ring-0 focus-visible:border-white"
                />
              </div>
              <div className="space-y-2">
                <label className="text-primary font-pixel text-[10px] uppercase">Confirm Password</label>
                <Input
                  required
                  type="password"
                  value={formData.confirmPassword}
                  onChange={handleConfirmPasswordChange}
                  className="bg-black border-2 border-primary text-white font-pixel text-xs rounded-none h-12 focus-visible:ring-0 focus-visible:border-white"
                />
              </div>
              
              <div className="pt-4 flex flex-col gap-4">
                <RetroButton type="submit" disabled={isPending} className="w-full py-6 text-lg">
                  {isPending ? "CREATING..." : "SIGN UP"}
                </RetroButton>
                <RetroButton 
                  variant="outline" 
                  onClick={() => setLocation("/login")}
                  className="w-full py-4 text-xs"
                >
                  RETURN TO LOGIN
                </RetroButton>
              </div>
            </form>
          </CardContent>
        </Card>
      </motion.div>
    </div>
  );
}