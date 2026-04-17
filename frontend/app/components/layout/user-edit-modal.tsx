"use client";

import { useEffect, useRef, useState } from "react";
import { createPortal } from "react-dom";
import { toast } from "sonner";

import type { Usuario } from "@/lib/auth";
import { updateAuthUser } from "@/lib/auth";
import { getAuthToken } from "@/lib/store";
import { updateUser } from "@/services/user-service";

type UserEditModalProps = {
  currentName: string | null;
  isOpen: boolean;
  onClose: () => void;
  onSaved: (user: Usuario) => void;
  userId: number | null;
};

export function UserEditModal({
  currentName,
  isOpen,
  onClose,
  onSaved,
  userId,
}: UserEditModalProps) {
  const [isMounted, setIsMounted] = useState(false);
  const [shouldRender, setShouldRender] = useState(isOpen);
  const [isVisible, setIsVisible] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [name, setName] = useState(currentName ?? "");
  const inputRef = useRef<HTMLInputElement | null>(null);

  useEffect(() => {
    setIsMounted(true);

    return () => {
      setIsMounted(false);
    };
  }, []);

  useEffect(() => {
    if (isOpen) {
      setName(currentName ?? "");
    }
  }, [currentName, isOpen]);

  useEffect(() => {
    let animationFrame = 0;
    let visibilityFrame = 0;
    let closeTimeout = 0;

    if (isOpen) {
      animationFrame = window.requestAnimationFrame(() => {
        setShouldRender(true);
        visibilityFrame = window.requestAnimationFrame(() => {
          setIsVisible(true);
        });
      });
    } else if (shouldRender) {
      animationFrame = window.requestAnimationFrame(() => {
        setIsVisible(false);
      });

      closeTimeout = window.setTimeout(() => {
        setShouldRender(false);
      }, 220);
    }

    function handleEscape(event: KeyboardEvent) {
      if (event.key === "Escape" && !isSaving) {
        onClose();
      }
    }

    window.addEventListener("keydown", handleEscape);

    return () => {
      window.cancelAnimationFrame(animationFrame);
      window.cancelAnimationFrame(visibilityFrame);
      window.clearTimeout(closeTimeout);
      window.removeEventListener("keydown", handleEscape);
    };
  }, [isOpen, isSaving, onClose, shouldRender]);

  useEffect(() => {
    if (!isOpen || !isVisible) {
      return;
    }

    const timeout = window.setTimeout(() => {
      inputRef.current?.focus();
      inputRef.current?.select();
    }, 120);

    return () => {
      window.clearTimeout(timeout);
    };
  }, [isOpen, isVisible]);

  if (!isMounted || !shouldRender) {
    return null;
  }

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (isSaving || !userId) {
      return;
    }

    const token = getAuthToken();

    if (!token) {
      toast.error("Voce precisa estar autenticado para editar o usuario.");
      return;
    }

    const normalizedName = name.trim();

    if (!normalizedName) {
      toast.error("Informe um nome valido.");
      return;
    }

    setIsSaving(true);

    try {
      const response = await updateUser(userId, { nome: normalizedName }, token);

      if (!response.ok) {
        toast.error(response.message ?? "Nao foi possivel atualizar o nome do usuario.");
        return;
      }

      updateAuthUser(response.body);
      onSaved(response.body);
      toast.success("Nome do usuario atualizado.");
      onClose();
    } catch {
      toast.error("Nao foi possivel conectar ao backend. Verifique se a API esta em execucao.");
    } finally {
      setIsSaving(false);
    }
  }

  return createPortal(
    <div
      className={`fixed inset-0 z-[220] flex items-center justify-center bg-[rgba(15,23,42,0.45)] p-4 transition-opacity duration-200 ease-out ${
        isVisible ? "opacity-100" : "opacity-0"
      }`}
    >
      <div
        className={`w-full max-w-lg rounded-[28px] border border-[var(--border)] bg-white p-6 shadow-[0_30px_90px_rgba(15,23,42,0.22)] transition duration-250 ease-out ${
          isVisible ? "translate-y-0 scale-100 opacity-100" : "translate-y-4 scale-[0.98] opacity-0"
        }`}
      >
        <div className="flex items-start justify-between gap-4">
          <div>
            <p className="text-sm font-semibold uppercase tracking-[0.16em] text-[var(--muted)]">
              Perfil
            </p>
            <h2 className="mt-2 text-2xl font-semibold tracking-tight text-[var(--foreground)]">
              Editar usuario
            </h2>
            <p className="mt-2 text-sm leading-7 text-[var(--muted)]">
              Voce pode alterar apenas o nome exibido no sistema.
            </p>
          </div>

          <button
            type="button"
            onClick={onClose}
            disabled={isSaving}
            className="flex h-11 w-11 cursor-pointer items-center justify-center rounded-2xl border border-[var(--border)] bg-[var(--surface-muted)] text-[var(--muted)] transition hover:border-[var(--border-strong)] hover:text-[var(--foreground)] disabled:cursor-not-allowed disabled:opacity-60"
            aria-label="Fechar edicao de usuario"
          >
            x
          </button>
        </div>

        <form className="mt-6 space-y-6" onSubmit={handleSubmit}>
          <label className="block space-y-2">
            <span className="text-sm font-semibold text-[var(--foreground)]">Nome</span>
            <input
              ref={inputRef}
              type="text"
              value={name}
              disabled={isSaving}
              onChange={(event) => {
                setName(event.target.value);
              }}
              className="h-12 w-full rounded-2xl border border-[var(--border)] bg-white px-4 text-sm text-[var(--foreground)] outline-none transition focus:border-[var(--primary)] focus:shadow-[0_0_0_4px_rgba(106,92,255,0.12)] disabled:cursor-not-allowed disabled:bg-[var(--surface-muted)]"
              placeholder="Digite seu nome"
              maxLength={120}
            />
          </label>

          <div className="flex flex-col gap-3 border-t border-[var(--border)] pt-4 sm:flex-row sm:justify-end">
            <button
              type="button"
              onClick={onClose}
              disabled={isSaving}
              className="flex h-12 cursor-pointer items-center justify-center rounded-2xl border border-[var(--border)] bg-[var(--surface-muted)] px-5 text-sm font-semibold text-[var(--foreground)] transition hover:border-[var(--border-strong)] hover:bg-white disabled:cursor-not-allowed disabled:opacity-60"
            >
              Cancelar
            </button>
            <button
              type="submit"
              disabled={isSaving}
              className="flex h-12 cursor-pointer items-center justify-center rounded-2xl bg-[linear-gradient(90deg,_#ff8a3d,_#ff6b3d)] px-5 text-sm font-semibold text-white shadow-[0_16px_30px_rgba(255,107,61,0.28)] transition hover:brightness-105 disabled:cursor-not-allowed disabled:opacity-60"
            >
              {isSaving ? "Salvando..." : "Salvar nome"}
            </button>
          </div>
        </form>
      </div>
    </div>,
    document.body,
  );
}
