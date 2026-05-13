"use client";

import { usePathname, useRouter } from "next/navigation";
import { useEffect, type ReactNode } from "react";

import { getDashboardRouteForArea, getStoredAccessArea } from "@/lib/access-area";
import { clearAuthSession, getAuthToken, isTokenValid } from "@/lib/auth";

type AuthGuardProps = {
  children: ReactNode;
};

function isAuthRoute(pathname: string) {
  return pathname === "/auth";
}

export function AuthGuard({ children }: AuthGuardProps) {
  const pathname = usePathname();
  const router = useRouter();
  const isClient = typeof window !== "undefined";
  const token = isClient ? getAuthToken() : null;
  const validSession = isTokenValid(token);
  const authRoute = isAuthRoute(pathname);
  const shouldRedirectToAuth = !validSession && !authRoute;
  const shouldRedirectToDashboard = validSession && authRoute;

  useEffect(() => {
    if (!validSession) {
      clearAuthSession();
    }

    if (shouldRedirectToAuth) {
      router.replace("/auth");
      return;
    }

    if (shouldRedirectToDashboard) {
      router.replace(getDashboardRouteForArea(getStoredAccessArea()));
    }
  }, [router, shouldRedirectToAuth, shouldRedirectToDashboard, validSession]);

  if (!isClient || shouldRedirectToAuth || shouldRedirectToDashboard) {
    return null;
  }

  return <>{children}</>;
}
