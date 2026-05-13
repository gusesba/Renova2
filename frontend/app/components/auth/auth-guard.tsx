"use client";

import { usePathname, useRouter } from "next/navigation";
import { useEffect, useState, type ReactNode } from "react";

import { getDashboardRouteForArea, getStoredAccessArea } from "@/lib/access-area";
import { clearAuthSession, getAuthToken, isTokenValid, type LicenseStatus } from "@/lib/auth";
import { getLicenseStatus } from "@/services/auth-service";

type AuthGuardProps = {
  children: ReactNode;
};

type LicenseCheck = {
  pathname: string;
  status: LicenseStatus;
  token: string;
};

function isAuthRoute(pathname: string) {
  return pathname === "/auth";
}

function isLicenseRoute(pathname: string) {
  return pathname === "/entrar-em-contato" || pathname === "/permissao-expirada";
}

export function AuthGuard({ children }: AuthGuardProps) {
  const pathname = usePathname();
  const router = useRouter();
  const [licenseCheck, setLicenseCheck] = useState<LicenseCheck | null>(null);
  const isClient = typeof window !== "undefined";
  const token = isClient ? getAuthToken() : null;
  const validSession = isTokenValid(token);
  const authRoute = isAuthRoute(pathname);
  const licenseRoute = isLicenseRoute(pathname);
  const shouldRedirectToAuth = !validSession && !authRoute;

  useEffect(() => {
    if (!validSession) {
      clearAuthSession();
    }

    if (shouldRedirectToAuth) {
      router.replace("/auth");
      return;
    }

    if (!token || licenseRoute) {
      return;
    }

    const authToken = token;
    let cancelled = false;

    async function checkLicense() {
      const response = await getLicenseStatus(authToken);

      if (cancelled) {
        return;
      }

      if (!response.ok) {
        if (response.status === 401) {
          clearAuthSession();
          router.replace("/auth");
        }

        return;
      }

      const status = response.body?.status ?? "pendente";
      setLicenseCheck({ pathname, status, token: authToken });

      if (authRoute && status === "ativa") {
        router.replace(getDashboardRouteForArea(getStoredAccessArea()));
        return;
      }

      if (status !== "ativa") {
        router.replace(status === "expirada" ? "/permissao-expirada" : "/entrar-em-contato");
      }
    }

    void checkLicense();

    return () => {
      cancelled = true;
    };
  }, [authRoute, licenseRoute, pathname, router, shouldRedirectToAuth, token, validSession]);

  if (!isClient || shouldRedirectToAuth) {
    return null;
  }

  if (
    validSession
    && !licenseRoute
    && (licenseCheck?.pathname !== pathname || licenseCheck?.token !== token)
  ) {
    return null;
  }

  return <>{children}</>;
}
