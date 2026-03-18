"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  useEffect,
  useState,
  type FormEvent,
  type SetStateAction,
} from "react";
import { toast } from "sonner";

import { useSystemSession } from "@/app/(system)/components/system-session-provider";
import { CommercialRulesOverview } from "@/app/(system)/commercial-rules/components/commercial-rules-overview";
import { PaymentMethodsPanel } from "@/app/(system)/commercial-rules/components/payment-methods-panel";
import { StoreRulePanel } from "@/app/(system)/commercial-rules/components/store-rule-panel";
import { SupplierRulesPanel } from "@/app/(system)/commercial-rules/components/supplier-rules-panel";
import {
  createEmptyDiscountBand,
  createSupplierRuleFromStoreRule,
  emptyPaymentMethodForm,
  emptyStoreRuleForm,
  mapPaymentMethodToForm,
  mapStoreRuleToForm,
  mapSupplierRuleToForm,
  type PaymentMethodFormState,
  type StoreRuleFormState,
  type SupplierRuleFormState,
} from "@/app/(system)/commercial-rules/components/types";
import { AccessStateCard } from "@/components/ui/access-state-card";
import { accessPermissionCodes, hasPermission } from "@/lib/helpers/access-control";
import { getErrorMessage } from "@/lib/helpers/formatters";
import { queryKeys } from "@/lib/helpers/query-keys";
import { getZodErrorMessage } from "@/lib/schemas/access";
import {
  paymentMethodFormSchema,
  storeCommercialRuleFormSchema,
  supplierCommercialRuleFormSchema,
} from "@/lib/schemas/commercial-rules";
import {
  createPaymentMethod,
  createSupplierCommercialRule,
  getCommercialRulesWorkspace,
  saveStoreCommercialRule,
  updatePaymentMethod,
  updateSupplierCommercialRule,
} from "@/lib/services/commercial-rules";

// Traduz o formulario da regra da loja para o payload esperado pela API.
function mapStoreFormToPayload(form: StoreRuleFormState) {
  const parsed = storeCommercialRuleFormSchema.safeParse(form);
  if (!parsed.success) {
    throw new Error(getZodErrorMessage(parsed.error));
  }

  return {
    percentualRepasseDinheiro: parsed.data.percentualRepasseDinheiro,
    percentualRepasseCredito: parsed.data.percentualRepasseCredito,
    permitePagamentoMisto: parsed.data.permitePagamentoMisto,
    tempoMaximoExposicaoDias: parsed.data.tempoMaximoExposicaoDias,
    politicaDesconto: parsed.data.politicaDesconto.map((band) => ({
      diasMinimos: band.diasMinimos,
      percentualDesconto: band.percentualDesconto,
    })),
    ativo: parsed.data.ativo,
  };
}

// Traduz o formulario da regra do fornecedor para o payload esperado pela API.
function mapSupplierFormToPayload(form: SupplierRuleFormState) {
  const parsed = supplierCommercialRuleFormSchema.safeParse(form);
  if (!parsed.success) {
    throw new Error(getZodErrorMessage(parsed.error));
  }

  return {
    pessoaLojaId: parsed.data.pessoaLojaId,
    percentualRepasseDinheiro: parsed.data.percentualRepasseDinheiro,
    percentualRepasseCredito: parsed.data.percentualRepasseCredito,
    permitePagamentoMisto: parsed.data.permitePagamentoMisto,
    tempoMaximoExposicaoDias: parsed.data.tempoMaximoExposicaoDias,
    politicaDesconto: parsed.data.politicaDesconto.map((band) => ({
      diasMinimos: band.diasMinimos,
      percentualDesconto: band.percentualDesconto,
    })),
    ativo: parsed.data.ativo,
  };
}

// Traduz o formulario do meio de pagamento para o payload esperado pela API.
function mapPaymentMethodFormToPayload(form: PaymentMethodFormState) {
  const parsed = paymentMethodFormSchema.safeParse(form);
  if (!parsed.success) {
    throw new Error(getZodErrorMessage(parsed.error));
  }

  return {
    nome: parsed.data.nome,
    tipoMeioPagamento: parsed.data.tipoMeioPagamento,
    taxaPercentual: parsed.data.taxaPercentual,
    prazoRecebimentoDias: parsed.data.prazoRecebimentoDias,
    ativo: parsed.data.ativo,
  };
}

// Coordena a tela do modulo 05 com regra da loja, fornecedor e meios de pagamento.
export function CommercialRulesDashboard() {
  const { token, session } = useSystemSession();
  const queryClient = useQueryClient();
  const [storeDraft, setStoreDraft] = useState<StoreRuleFormState | null>(null);
  const [supplierDraft, setSupplierDraft] = useState<SupplierRuleFormState | null>(
    null,
  );
  const [paymentDraft, setPaymentDraft] = useState<PaymentMethodFormState | null>(
    null,
  );
  const [selectedSupplierRuleId, setSelectedSupplierRuleId] = useState("");
  const [selectedPaymentMethodId, setSelectedPaymentMethodId] = useState("");
  const canManageCommercialRules = hasPermission(
    session,
    accessPermissionCodes.rulesManage,
  );

  const workspaceQuery = useQuery({
    enabled: Boolean(session.lojaAtivaId && canManageCommercialRules),
    queryFn: () => getCommercialRulesWorkspace(token),
    queryKey: queryKeys.commercialRulesWorkspace(token, session.lojaAtivaId),
  });

  const storeRuleMutation = useMutation({
    mutationFn: async () =>
      saveStoreCommercialRule(token, mapStoreFormToPayload(storeForm)),
    onError: (error) => {
      toast.error(getErrorMessage(error));
    },
    onSuccess: async (response) => {
      setStoreDraft(mapStoreRuleToForm(response));
      toast.success("Regra comercial da loja salva com sucesso.");
      await refreshWorkspace();
    },
  });

  const supplierRuleMutation = useMutation({
    mutationFn: async () => {
      const payload = mapSupplierFormToPayload(supplierForm);
      return supplierForm.id
        ? updateSupplierCommercialRule(token, supplierForm.id, {
            percentualRepasseDinheiro: payload.percentualRepasseDinheiro,
            percentualRepasseCredito: payload.percentualRepasseCredito,
            permitePagamentoMisto: payload.permitePagamentoMisto,
            tempoMaximoExposicaoDias: payload.tempoMaximoExposicaoDias,
            politicaDesconto: payload.politicaDesconto,
            ativo: payload.ativo,
          })
        : createSupplierCommercialRule(token, payload);
    },
    onError: (error) => {
      toast.error(getErrorMessage(error));
    },
    onSuccess: async (response) => {
      setSelectedSupplierRuleId(response.id);
      setSupplierDraft(mapSupplierRuleToForm(response));
      toast.success(
        supplierForm.id
          ? "Regra do fornecedor atualizada com sucesso."
          : "Regra do fornecedor criada com sucesso.",
      );
      await refreshWorkspace();
    },
  });

  const paymentMethodMutation = useMutation({
    mutationFn: async () => {
      const payload = mapPaymentMethodFormToPayload(paymentForm);
      return paymentForm.id
        ? updatePaymentMethod(token, paymentForm.id, payload)
        : createPaymentMethod(token, payload);
    },
    onError: (error) => {
      toast.error(getErrorMessage(error));
    },
    onSuccess: async (response) => {
      setSelectedPaymentMethodId(response.id);
      setPaymentDraft(mapPaymentMethodToForm(response));
      toast.success(
        paymentForm.id
          ? "Meio de pagamento atualizado com sucesso."
          : "Meio de pagamento criado com sucesso.",
      );
      await refreshWorkspace();
    },
  });

  useEffect(() => {
    if (workspaceQuery.isError) {
      toast.error(getErrorMessage(workspaceQuery.error));
    }
  }, [workspaceQuery.error, workspaceQuery.isError]);

  const workspace = workspaceQuery.data;
  const storeForm = storeDraft ?? (
    workspace?.regraLoja ? mapStoreRuleToForm(workspace.regraLoja) : emptyStoreRuleForm()
  );
  const supplierForm = supplierDraft ?? (() => {
    const selectedRule = workspace?.regrasFornecedor.find(
      (rule) => rule.id === selectedSupplierRuleId,
    );
    if (selectedRule) {
      return mapSupplierRuleToForm(selectedRule);
    }

    return createSupplierRuleFromStoreRule(
      storeForm,
      workspace?.fornecedoresDisponiveis[0]?.pessoaLojaId ?? "",
    );
  })();
  const paymentForm = paymentDraft ?? (() => {
    const selectedMethod = workspace?.meiosPagamento.find(
      (method) => method.id === selectedPaymentMethodId,
    );
    if (selectedMethod) {
      return mapPaymentMethodToForm(selectedMethod);
    }

    return emptyPaymentMethodForm(workspace?.tiposMeioPagamento[0]?.codigo ?? "");
  })();

  const busy =
    workspaceQuery.isLoading ||
    storeRuleMutation.isPending ||
    supplierRuleMutation.isPending ||
    paymentMethodMutation.isPending;

  async function refreshWorkspace() {
    await queryClient.invalidateQueries({
      queryKey: queryKeys.commercialRulesWorkspace(token, session.lojaAtivaId),
    });
  }

  function setStoreForm(value: SetStateAction<StoreRuleFormState>) {
    setStoreDraft((current) => {
      const baseValue = current ?? storeForm;
      return typeof value === "function"
        ? (value as (current: StoreRuleFormState) => StoreRuleFormState)(baseValue)
        : value;
    });
  }

  function setSupplierForm(value: SetStateAction<SupplierRuleFormState>) {
    setSupplierDraft((current) => {
      const baseValue = current ?? supplierForm;
      return typeof value === "function"
        ? (value as (current: SupplierRuleFormState) => SupplierRuleFormState)(
            baseValue,
          )
        : value;
    });
  }

  function setPaymentForm(value: SetStateAction<PaymentMethodFormState>) {
    setPaymentDraft((current) => {
      const baseValue = current ?? paymentForm;
      return typeof value === "function"
        ? (value as (current: PaymentMethodFormState) => PaymentMethodFormState)(
            baseValue,
          )
        : value;
    });
  }

  async function handleStoreRuleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    await storeRuleMutation.mutateAsync();
  }

  async function handleSupplierRuleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    await supplierRuleMutation.mutateAsync();
  }

  async function handlePaymentMethodSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    await paymentMethodMutation.mutateAsync();
  }

  function handleNewSupplierRule() {
    setSelectedSupplierRuleId("");
    setSupplierDraft(
      createSupplierRuleFromStoreRule(
        storeForm,
        workspace?.fornecedoresDisponiveis[0]?.pessoaLojaId ?? "",
      ),
    );
  }

  function handleNewPaymentMethod() {
    setSelectedPaymentMethodId("");
    setPaymentDraft(emptyPaymentMethodForm(workspace?.tiposMeioPagamento[0]?.codigo ?? ""));
  }

  if (!canManageCommercialRules) {
    return (
      <AccessStateCard
        message="Solicite a permissao adequada para configurar regras comerciais e meios de pagamento."
        subtitle="Sua conta nao possui acesso ao modulo comercial."
        title="Modulo sem permissao"
      />
    );
  }

  return (
    <div className="dashboard-grid">
      <div className="dashboard-column" style={{ gridColumn: "1 / -1" }}>
        <CommercialRulesOverview workspace={workspace} />
      </div>

      <div className="dashboard-column">
        <StoreRulePanel
          busy={busy}
          form={storeForm}
          onAddBand={() =>
            setStoreForm((current) => ({
              ...current,
              politicaDesconto: [...current.politicaDesconto, createEmptyDiscountBand()],
            }))
          }
          onSubmit={handleStoreRuleSubmit}
          setForm={setStoreForm}
        />
      </div>

      <div className="dashboard-column">
        <PaymentMethodsPanel
          busy={busy}
          form={paymentForm}
          onNewMethod={handleNewPaymentMethod}
          onSelectMethod={(paymentMethodId) => {
            setSelectedPaymentMethodId(paymentMethodId);
            setPaymentDraft(null);
          }}
          onSubmit={handlePaymentMethodSubmit}
          paymentMethodTypes={workspace?.tiposMeioPagamento ?? []}
          paymentMethods={workspace?.meiosPagamento ?? []}
          selectedMethodId={selectedPaymentMethodId}
          setForm={setPaymentForm}
        />
      </div>

      <div className="dashboard-column" style={{ gridColumn: "1 / -1" }}>
        <SupplierRulesPanel
          busy={busy}
          form={supplierForm}
          onAddBand={() =>
            setSupplierForm((current) => ({
              ...current,
              politicaDesconto: [...current.politicaDesconto, createEmptyDiscountBand()],
            }))
          }
          onNewRule={handleNewSupplierRule}
          onSelectRule={(supplierRuleId) => {
            setSelectedSupplierRuleId(supplierRuleId);
            setSupplierDraft(null);
          }}
          onSubmit={handleSupplierRuleSubmit}
          selectedRuleId={selectedSupplierRuleId}
          setForm={setSupplierForm}
          supplierOptions={workspace?.fornecedoresDisponiveis ?? []}
          supplierRules={workspace?.regrasFornecedor ?? []}
        />
      </div>
    </div>
  );
}
