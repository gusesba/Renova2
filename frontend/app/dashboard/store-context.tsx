"use client";

import { useQuery } from "@tanstack/react-query";
import { createContext, useContext, useEffect, useMemo, useState, type ReactNode } from "react";

import {
  asAccessProfile,
  extractAccessApiMessage,
  type AccessProfile,
  type PermissionKey,
} from "@/lib/access";
import {
  getAuthToken,
  getAuthUser,
  getStoredSelectedStoreId,
  persistSelectedStoreId,
  type LojaResponse,
  type UsuarioResumo,
} from "@/lib/store";
import { asStoreListResponse, getStores } from "@/services/store-service";
import { getStoreAccessProfile } from "@/services/access-service";

type StoreContextValue = {
  stores: LojaResponse[];
  selectedStore: LojaResponse | null;
  selectedStoreId: number | null;
  setSelectedStoreId: (storeId: number | null) => void;
  isLoadingStores: boolean;
  currentUser: UsuarioResumo | null;
  accessProfile: AccessProfile | null;
  isLoadingAccess: boolean;
  hasPermission: (permission: PermissionKey) => boolean;
  hasAnyPermission: (permissionList: PermissionKey[]) => boolean;
};

const StoreContext = createContext<StoreContextValue | null>(null);

export function StoreProvider({ children }: { children: ReactNode }) {
  const [token] = useState<string | null>(() =>
    typeof window === "undefined" ? null : getAuthToken(),
  );
  const [currentUser] = useState<UsuarioResumo | null>(() =>
    typeof window === "undefined" ? null : getAuthUser(),
  );
  const [selectedStoreIdState, setSelectedStoreIdState] = useState<number | null>(() =>
    typeof window === "undefined" ? null : getStoredSelectedStoreId(),
  );

  const storesQuery = useQuery({
    queryKey: ["stores", token],
    queryFn: async () => {
      if (!token) {
        return [];
      }

      const response = await getStores(token);

      if (!response.ok) {
        throw new Error("Nao foi possivel carregar as lojas.");
      }

      return asStoreListResponse(response.body);
    },
    enabled: Boolean(token),
  });

  const stores = useMemo(() => storesQuery.data ?? [], [storesQuery.data]);

  const selectedStoreId = useMemo(() => {
    if (!stores.length) {
      return null;
    }

    return stores.some((store) => store.id === selectedStoreIdState)
      ? selectedStoreIdState
      : (stores[0]?.id ?? null);
  }, [selectedStoreIdState, stores]);

  const accessQuery = useQuery({
    queryKey: ["store-access-profile", token, selectedStoreId],
    queryFn: async () => {
      if (!token || !selectedStoreId) {
        return null;
      }

      const response = await getStoreAccessProfile(selectedStoreId, token);

      if (!response.ok) {
        throw new Error(
          extractAccessApiMessage(response.body) ?? "Nao foi possivel carregar as permissoes da loja.",
        );
      }

      return asAccessProfile(response.body);
    },
    enabled: Boolean(token && selectedStoreId),
  });

  useEffect(() => {
    persistSelectedStoreId(selectedStoreId);
  }, [selectedStoreId]);

  function setSelectedStoreId(storeId: number | null) {
    setSelectedStoreIdState(storeId);
  }

  const value = useMemo<StoreContextValue>(() => {
    const selectedStore = stores.find((store) => store.id === selectedStoreId) ?? null;

    return {
      stores,
      selectedStore,
      selectedStoreId,
      setSelectedStoreId,
      isLoadingStores: storesQuery.isLoading,
      currentUser,
      accessProfile: accessQuery.data ?? null,
      isLoadingAccess: accessQuery.isLoading,
      hasPermission: (permission) =>
        Boolean((accessQuery.data?.funcionalidades ?? []).includes(permission)),
      hasAnyPermission: (permissionList) =>
        permissionList.some((permission) => (accessQuery.data?.funcionalidades ?? []).includes(permission)),
    };
  }, [accessQuery.data, accessQuery.isLoading, currentUser, selectedStoreId, stores, storesQuery.isLoading]);

  return <StoreContext.Provider value={value}>{children}</StoreContext.Provider>;
}

export function useStoreContext() {
  const context = useContext(StoreContext);

  if (!context) {
    throw new Error("useStoreContext deve ser usado dentro de StoreProvider.");
  }

  return context;
}
