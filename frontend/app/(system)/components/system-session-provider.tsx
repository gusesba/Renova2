"use client";

import {
  createContext,
  startTransition,
  useContext,
  useEffect,
  useEffectEvent,
  useState,
  type ReactNode,
} from "react";
import { useRouter } from "next/navigation";

import { SystemLoadingScreen } from "@/components/layout/system-loading-screen";
import { changeActiveStore, getMe, logout, type SessionContext } from "@/lib/services/renova-api";
import { clearSessionToken, readSessionToken } from "@/lib/helpers/session-storage";

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
  const [token, setToken] = useState<string | null>(null);
  const [session, setSession] = useState<SessionContext | null>(null);

  const hydrateSession = useEffectEvent(async (storedToken: string) => {
    try {
      const nextSession = await getMe(storedToken);
      startTransition(() => {
        setToken(storedToken);
        setSession(nextSession);
      });
    } catch {
      clearSessionToken();
      startTransition(() => {
        setToken(null);
        setSession(null);
      });
      router.replace("/login");
    }
  });

  useEffect(() => {
    const storedToken = readSessionToken();

    if (!storedToken) {
      router.replace("/login");
      return;
    }

    void hydrateSession(storedToken);
  }, [router]);

  async function handleChangeStore(storeId: string) {
    if (!token) {
      return;
    }

    const nextSession = await changeActiveStore(token, storeId);
    startTransition(() => {
      setSession(nextSession);
    });
  }

  async function handleLogout() {
    if (token) {
      try {
        await logout(token);
      } catch {
        // nao bloqueia a limpeza local quando a API de logout falhar
      }
    }

    clearSessionToken();
    startTransition(() => {
      setToken(null);
      setSession(null);
    });
    router.replace("/login");
  }

  if (!token || !session) {
    return <SystemLoadingScreen />;
  }

  return (
    <SystemSessionContext.Provider
      value={{
        token,
        session,
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
