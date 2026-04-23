"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";

import { clearAuthSession, getAuthToken, isTokenValid } from "@/lib/auth";

export default function NotFound() {
  const router = useRouter();

  useEffect(() => {
    const token = getAuthToken();
    const validSession = isTokenValid(token);

    if (!validSession) {
      clearAuthSession();
      router.replace("/auth");
      return;
    }

    router.replace("/dashboard/loja");
  }, [router]);

  return null;
}
