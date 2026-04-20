"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";

import { getDashboardRouteForArea, getStoredAccessArea } from "@/lib/access-area";

export default function DashboardPage() {
  const router = useRouter();

  useEffect(() => {
    router.replace(getDashboardRouteForArea(getStoredAccessArea()));
  }, [router]);

  return null;
}
