export type AccessArea = "lojista" | "cliente";

const accessAreaStorageKey = "renova.accessArea";

function isAccessArea(value: unknown): value is AccessArea {
  return value === "lojista" || value === "cliente";
}

export function getDefaultAccessArea(): AccessArea {
  return "lojista";
}

export function getStoredAccessArea(): AccessArea {
  if (typeof window === "undefined") {
    return getDefaultAccessArea();
  }

  const value = window.localStorage.getItem(accessAreaStorageKey);

  return isAccessArea(value) ? value : getDefaultAccessArea();
}

export function persistAccessArea(area: AccessArea) {
  if (typeof window === "undefined") {
    return;
  }

  window.localStorage.setItem(accessAreaStorageKey, area);
}

export function clearStoredAccessArea() {
  if (typeof window === "undefined") {
    return;
  }

  window.localStorage.removeItem(accessAreaStorageKey);
}

export function getDashboardRouteForArea(area: AccessArea) {
  return area === "cliente" ? "/dashboard/area-cliente" : "/dashboard/loja";
}
