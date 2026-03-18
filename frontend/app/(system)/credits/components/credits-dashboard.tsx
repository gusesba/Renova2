"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  startTransition,
  useEffect,
  useState,
  type FormEvent,
  type SetStateAction,
} from "react";
import { toast } from "sonner";

import { CreditAccountsPanel } from "@/app/(system)/credits/components/credit-accounts-panel";
import { CreditManualPanel } from "@/app/(system)/credits/components/credit-manual-panel";
import { CreditStatementPanel } from "@/app/(system)/credits/components/credit-statement-panel";
import { CreditsOverview } from "@/app/(system)/credits/components/credits-overview";
import {
  createManualCreditForm,
  emptyManualCreditForm,
  type ManualCreditFormState,
} from "@/app/(system)/credits/components/types";
import { useSystemSession } from "@/app/(system)/components/system-session-provider";
import { AccessStateCard } from "@/components/ui/access-state-card";
import {
  accessPermissionCodes,
  hasAnyPermission,
  hasPermission,
} from "@/lib/helpers/access-control";
import { getErrorMessage } from "@/lib/helpers/formatters";
import { queryKeys } from "@/lib/helpers/query-keys";
import {
  creditAccountStatusSchema,
  ensureCreditAccountSchema,
  manualCreditSchema,
} from "@/lib/schemas/credits";
import {
  ensureCreditAccount,
  getCreditAccountByPerson,
  getCreditsWorkspace,
  updateCreditAccountStatus,
  registerManualCredit,
} from "@/lib/services/credits";

// Coordena a tela do modulo 10 com saldos, extrato e lancamentos manuais.
export function CreditsDashboard() {
  const { token, session } = useSystemSession();
  const queryClient = useQueryClient();
  const [search, setSearch] = useState("");
  const [selectedPersonId, setSelectedPersonId] = useState("");
  const [manualDraft, setManualDraft] = useState<ManualCreditFormState | null>(null);
  const [statusDraft, setStatusDraft] = useState<{ contaId: string; statusConta: string } | null>(null);
  const canViewCredits = hasAnyPermission(session, [
    accessPermissionCodes.creditView,
    accessPermissionCodes.creditManage,
  ]);
  const canManageCredits = hasPermission(session, accessPermissionCodes.creditManage);

  const workspaceQuery = useQuery({
    enabled: Boolean(session.lojaAtivaId && canViewCredits),
    queryFn: () => getCreditsWorkspace(token),
    queryKey: queryKeys.creditsWorkspace(token, session.lojaAtivaId),
  });

  const detailQuery = useQuery({
    enabled: Boolean(selectedPersonId && canViewCredits),
    queryFn: () => getCreditAccountByPerson(token, selectedPersonId),
    queryKey: queryKeys.creditDetail(token, session.lojaAtivaId, selectedPersonId),
  });

  const ensureAccountMutation = useMutation({
    mutationFn: async () => {
      const parsed = ensureCreditAccountSchema.safeParse({ pessoaId: manualForm.pessoaId });
      if (!parsed.success) {
        throw new Error(parsed.error.issues[0]?.message ?? "Pessoa invalida.");
      }

      return ensureCreditAccount(token, parsed.data);
    },
    onError: (error) => {
      toast.error(getErrorMessage(error));
    },
    onSuccess: async (response) => {
      setSelectedPersonId(response.conta.pessoaId);
      setStatusDraft({
        contaId: response.conta.contaId,
        statusConta: response.conta.statusConta,
      });
      toast.success("Conta de credito garantida com sucesso.");
      await refreshModuleData(response.conta.pessoaId);
    },
  });

  const manualCreditMutation = useMutation({
    mutationFn: async () => {
      const parsed = manualCreditSchema.safeParse({
        justificativa: manualForm.justificativa,
        pessoaId: manualForm.pessoaId,
        valor: manualForm.valor,
      });
      if (!parsed.success) {
        throw new Error(parsed.error.issues[0]?.message ?? "Lancamento invalido.");
      }

      return registerManualCredit(token, parsed.data);
    },
    onError: (error) => {
      toast.error(getErrorMessage(error));
    },
    onSuccess: async (response) => {
      setSelectedPersonId(response.conta.pessoaId);
      setManualDraft(emptyManualCreditForm(response.conta.pessoaId));
      setStatusDraft({
        contaId: response.conta.contaId,
        statusConta: response.conta.statusConta,
      });
      toast.success("Credito manual registrado com sucesso.");
      await refreshModuleData(response.conta.pessoaId);
    },
  });

  const statusMutation = useMutation({
    mutationFn: async () => {
      const detail = detailQuery.data;
      const parsed = creditAccountStatusSchema.safeParse({
        contaId: detail?.conta.contaId ?? "",
        statusConta: statusValue,
      });
      if (!parsed.success) {
        throw new Error(parsed.error.issues[0]?.message ?? "Status invalido.");
      }

      return updateCreditAccountStatus(token, parsed.data.contaId, {
        statusConta: parsed.data.statusConta,
      });
    },
    onError: (error) => {
      toast.error(getErrorMessage(error));
    },
    onSuccess: async (response) => {
      setStatusDraft({
        contaId: response.conta.contaId,
        statusConta: response.conta.statusConta,
      });
      toast.success("Status da conta atualizado com sucesso.");
      await refreshModuleData(response.conta.pessoaId);
    },
  });

  useEffect(() => {
    if (workspaceQuery.isError) {
      toast.error(getErrorMessage(workspaceQuery.error));
    }
  }, [workspaceQuery.error, workspaceQuery.isError]);

  useEffect(() => {
    if (detailQuery.isError) {
      toast.error(getErrorMessage(detailQuery.error));
    }
  }, [detailQuery.error, detailQuery.isError]);

  useEffect(() => {
    const accounts = workspaceQuery.data?.contas ?? [];
    if (accounts.length === 0) {
      startTransition(() => {
        setSelectedPersonId("");
      });
      return;
    }

    if (!selectedPersonId || !accounts.some((account) => account.pessoaId === selectedPersonId)) {
      startTransition(() => {
        setSelectedPersonId(accounts[0]?.pessoaId ?? "");
      });
    }
  }, [selectedPersonId, workspaceQuery.data?.contas]);

  if (!canViewCredits) {
    return (
      <AccessStateCard
        message="Solicite permissao para consultar ou gerenciar credito da loja."
        subtitle="Sua conta nao possui acesso ao modulo de credito."
        title="Modulo sem permissao"
      />
    );
  }

  const workspace = workspaceQuery.data;
  const detail = detailQuery.data;
  const busy =
    workspaceQuery.isLoading ||
    detailQuery.isLoading ||
    ensureAccountMutation.isPending ||
    manualCreditMutation.isPending ||
    statusMutation.isPending;
  const manualForm =
    manualDraft ??
    createManualCreditForm(detail, workspace?.pessoas ?? []);
  const statusValue =
    statusDraft && detail && statusDraft.contaId === detail.conta.contaId
      ? statusDraft.statusConta
      : detail?.conta.statusConta ?? "";

  function setManualForm(value: SetStateAction<ManualCreditFormState>) {
    setManualDraft((current) => {
      const baseValue = current ?? manualForm;
      return typeof value === "function"
        ? (value as (current: ManualCreditFormState) => ManualCreditFormState)(
            baseValue,
          )
        : value;
    });
  }

  async function refreshModuleData(pessoaId = selectedPersonId) {
    await Promise.all([
      queryClient.invalidateQueries({
        queryKey: queryKeys.creditsWorkspace(token, session.lojaAtivaId),
      }),
      pessoaId
        ? queryClient.invalidateQueries({
            queryKey: queryKeys.creditDetail(token, session.lojaAtivaId, pessoaId),
          })
        : Promise.resolve(),
    ]);
  }

  async function handleManualCredit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    await manualCreditMutation.mutateAsync();
  }

  async function handleStatusSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    await statusMutation.mutateAsync();
  }

  return (
    <div className="dashboard-grid">
      <div className="dashboard-column" style={{ gridColumn: "1 / -1" }}>
        <CreditsOverview workspace={workspace} />
      </div>

      <div className="dashboard-column">
        <CreditAccountsPanel
          accounts={workspace?.contas ?? []}
          onSelectPerson={setSelectedPersonId}
          search={search}
          selectedPersonId={selectedPersonId}
          setSearch={setSearch}
        />
      </div>

      <div className="dashboard-column">
        <CreditStatementPanel
          busy={busy}
          canManage={canManageCredits}
          detail={detail}
          onChangeStatus={(value) =>
            setStatusDraft((current) => ({
              contaId: detail?.conta.contaId ?? current?.contaId ?? "",
              statusConta: value,
            }))
          }
          onSubmitStatus={handleStatusSubmit}
          statusOptions={workspace?.statusConta ?? []}
          statusValue={statusValue}
        />
        <CreditManualPanel
          busy={busy}
          canManage={canManageCredits}
          form={manualForm}
          onEnsureAccount={async () => {
            await ensureAccountMutation.mutateAsync();
          }}
          onSubmit={handleManualCredit}
          people={workspace?.pessoas ?? []}
          setForm={setManualForm}
        />
      </div>
    </div>
  );
}
