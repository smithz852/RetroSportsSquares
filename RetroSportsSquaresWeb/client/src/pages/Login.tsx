import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { Link, useLocation } from "wouter";
import { motion } from "framer-motion";

const loginSchema = z.object({
  email: z.string().email("Invalid email address"),
  password: z.string().min(6, "Password must be at least 6 characters"),
});

type LoginFormValues = z.infer<typeof loginSchema>;

export default function Login() {
  const [, setLocation] = useLocation();
  const form = useForm<LoginFormValues>({
    resolver: zodResolver(loginSchema),
    defaultValues: {
      email: "",
      password: "",
    },
  });

  const onSubmit = (data: LoginFormValues) => {
    console.log("Login submitted:", data);
    // For now, just redirect to options as we're in stub mode
    setLocation("/options");
  };

  return (
    <div className="flex items-center justify-center min-h-[80vh] p-4">
      <motion.div
        initial={{ opacity: 0, scale: 0.9 }}
        animate={{ opacity: 1, scale: 1 }}
        className="w-full max-w-md"
      >
        <Card className="bg-black border-4 border-red-600 rounded-none shadow-[0_0_15px_rgba(255,0,0,0.3)]">
          <CardHeader className="border-b-4 border-red-600 pb-6">
            <CardTitle className="text-3xl text-red-600 font-pixel text-center uppercase tracking-tighter">
              Identity Verification
            </CardTitle>
          </CardHeader>
          <CardContent className="pt-8">
            <Form {...form}>
              <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
                <FormField
                  control={form.control}
                  name="email"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel className="text-red-500 font-pixel text-xs uppercase">Email Address</FormLabel>
                      <FormControl>
                        <Input 
                          {...field} 
                          type="email" 
                          placeholder="PLAYER@ARCADE.COM"
                          className="bg-black border-2 border-red-900 text-red-500 font-mono rounded-none focus:border-red-600 focus:ring-0 placeholder:text-red-900"
                          data-testid="input-email"
                        />
                      </FormControl>
                      <FormMessage className="text-red-700 text-xs font-mono" />
                    </FormItem>
                  )}
                />
                
                <FormField
                  control={form.control}
                  name="password"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel className="text-red-500 font-pixel text-xs uppercase">Access Code</FormLabel>
                      <FormControl>
                        <Input 
                          {...field} 
                          type="password" 
                          placeholder="******"
                          className="bg-black border-2 border-red-900 text-red-500 font-mono rounded-none focus:border-red-600 focus:ring-0 placeholder:text-red-900"
                          data-testid="input-password"
                        />
                      </FormControl>
                      <FormMessage className="text-red-700 text-xs font-mono" />
                    </FormItem>
                  )}
                />

                <Button 
                  type="submit" 
                  className="w-full bg-red-600 text-black font-pixel py-6 rounded-none hover:bg-red-500 active:translate-y-1 transition-all uppercase"
                  data-testid="button-login"
                >
                  Authorize Access
                </Button>
              </form>
            </Form>
            
            <div className="mt-8 text-center">
              <Link href="/">
                <a className="text-red-900 hover:text-red-500 font-pixel text-[10px] uppercase transition-colors">
                  &lt; System Reset
                </a>
              </Link>
            </div>
          </CardContent>
        </Card>
      </motion.div>
    </div>
  );
}
