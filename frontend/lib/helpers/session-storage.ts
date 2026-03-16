export const SESSION_TOKEN_KEY = "renova.module01.token";

function canUseDom() {
  return typeof window !== "undefined";
}

export function readSessionToken() {
  if (!canUseDom()) {
    return null;
  }

  return window.localStorage.getItem(SESSION_TOKEN_KEY);
}

export function writeSessionToken(token: string) {
  if (!canUseDom()) {
    return;
  }

  window.localStorage.setItem(SESSION_TOKEN_KEY, token);
}

export function clearSessionToken() {
  if (!canUseDom()) {
    return;
  }

  window.localStorage.removeItem(SESSION_TOKEN_KEY);
}
