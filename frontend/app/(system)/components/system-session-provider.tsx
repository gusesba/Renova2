"use client";

import {
  useMutation,
  useQuery,
  useQueryClient,
} from "@tanstack/react-query";
import { createContext, useContext, useEffect, useState, type ReactNode } from "react";
import { useRouter } from "next/navigation";

import { SystemLoadingScreen } from "@/components/layout/system-loading-screen";
import { queryKeys } from "@/lib/helpers/query-keys";
import { clearSessionToken, readSessionToken } from "@/lib/helpers/session-storage";
import { changeActiveStore, getMe, logout, type SessionContext } from "@/lib/services/renova-api";

type SystemSessionValue = {
  token: string;
  session: SessionContext;
  changeStore: (storeId: string) => Promise<void>;
  logoutCurrentUser: () => Promise<void>;
};

const SystemSessionContext = createContext<SystemSessionValue | null>(null);

type SystemSessionProviderProps = {
  children: ReactNode;
};

export function SystemSessionProvider({ children }: SystemSessionProviderProps) {
  const router = useRouter();
  const queryClient = useQueryClient();
  const [token, setToken] = useState<string | null>(() => readSessionToken());

  const sessionQuery = useQuery({
    enabled: !!token,
    queryFn: () => getMe(token!),
    queryKey: queryKeys.session(token),
    retry: false,
    staleTime: 1000 * 60 * 5,
  });

  const changeStoreMutation = useMutation({
    mutationFn: (storeId: string) => changeActiveStore(token!, storeId),
    onSuccess: (nextSession) => {
      queryClient.setQueryData(queryKeys.session(token), nextSession);
    },
  });

  const logoutMutation = useMutation({
    mutationFn: async () => {
      if (!token) {
        return;
      }

      try {
        await logout(token);
      } catch {
        return;
      }
    },
    onSettled: () => {
      clearSessionToken();
      setToken(null);
      queryClient.removeQueries({ queryKey: ["session"] });
      router.replace("/login");
    },
  });

  useEffect(() => {
    if (!token) {
      router.replace("/login");
      return;
    }

    if (sessionQuery.isError) {
      clearSessionToken();
      router.replace("/login");
    }
  }, [router, sessionQuery.isError, token]);

  async function handleChangeStore(storeId: string) {
    if (!token) {
      return;
    }

    await changeStoreMutation.mutateAsync(storeId);
  }

  async function handleLogout() {
    await logoutMutation.mutateAsync();
  }

  if (!token || sessionQuery.isLoading || !sessionQuery.data) {
    return <SystemLoadingScreen />;
  }

  return (
    <SystemSessionContext.Provider
      value={{
        token,
        session: sessionQuery.data,
        changeStore: handleChangeStore,
        logoutCurrentUser: handleLogout,
      }}
    >
      {children}
    </SystemSessionContext.Provider>
  );
}

export function useSystemSession() {
  const context = useContext(SystemSessionContext);

  if (!context) {
    throw new Error("useSystemSession deve ser usado dentro de SystemSessionProvider.");
  }

  return context;
}
