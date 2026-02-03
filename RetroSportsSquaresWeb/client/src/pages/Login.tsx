import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { Link, useLocation } from "wouter";
import { motion } from "framer-motion";
import { useLogin } from "@/hooks/use-auth";
import { useToast } from "@/hooks/use-toast";
import { useEffect } from "react";

const loginSchema = z.object({
  email: z.string().email("Invalid email address"),
  password: z.string().min(1, "Password is required"),
});

export default function Login() {
  const [, setLocation] = useLocation();
  const { mutate: login, isPending, error } = useLogin();
  const { toast } = useToast();

  const form = useForm<z.infer<typeof loginSchema>>({
    resolver: zodResolver(loginSchema),
    defaultValues: {
      email: "",
      password: "",
    },
  });

  // Show error toast when login fails
  useEffect(() => {
    if (error) {
      toast({
        title: "ACCESS DENIED",
        description: error.message.toUpperCase(),
        variant: "destructive",
        className: "bg-black border-2 border-red-900 text-red-500 font-pixel text-[10px]",
      });
    }
  }, [error, toast]);

  const onSubmit = (values: z.infer<typeof loginSchema>) => {
    login(values, {
      onSuccess: () => setLocation("/options"),
    });
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

                <div className="flex flex-col gap-4">
                  <Button 
                    type="submit" 
                    disabled={isPending}
                    className="w-full bg-red-600 text-black font-pixel py-6 rounded-none hover:bg-red-500 active:translate-y-1 transition-all uppercase"
                    data-testid="button-login"
                  >
                    {isPending ? "AUTHENTICATING..." : "Authorize Access"}
                  </Button>
                  <Button 
                    type="button"
                    onClick={() => setLocation("/signup")}
                    variant="outline"
                    className="w-full border-2 border-red-600 text-red-600 font-pixel py-6 rounded-none hover:bg-red-600 hover:text-black uppercase"
                    data-testid="button-goto-signup"
                  >
                    Register Profile
                  </Button>
                </div>
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
