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

import { useSystemSession } from "@/app/(system)/components/system-session-provider";
import { PeopleListPanel } from "@/app/(system)/people/components/people-list-panel";
import { PeopleOverview } from "@/app/(system)/people/components/people-overview";
import { PersonFinancialPanel } from "@/app/(system)/people/components/person-financial-panel";
import { PersonFormPanel } from "@/app/(system)/people/components/person-form-panel";
import {
  emptyPersonForm,
  type PersonFormState,
} from "@/app/(system)/people/components/types";
import { AccessStateCard } from "@/components/ui/access-state-card";
import {
  accessPermissionCodes,
  hasAnyPermission,
  hasPermission,
} from "@/lib/helpers/access-control";
import { getErrorMessage } from "@/lib/helpers/formatters";
import { queryKeys } from "@/lib/helpers/query-keys";
import { getZodErrorMessage } from "@/lib/schemas/access";
import { personFormSchema } from "@/lib/schemas/people";
import {
  createPerson,
  getPersonById,
  listLinkablePeopleUsers,
  listPeople,
  updatePerson,
  type PersonDetail,
} from "@/lib/services/people";

// Converte o detalhe vindo da API para o estado editavel da tela.
function mapPersonDetailToForm(detail: PersonDetail): PersonFormState {
  return {
    id: detail.id,
    tipoPessoa: detail.tipoPessoa as "fisica" | "juridica",
    nome: detail.nome,
    nomeSocial: detail.nomeSocial,
    documento: detail.documento,
    telefone: detail.telefone,
    email: detail.email,
    logradouro: detail.logradouro,
    numero: detail.numero,
    complemento: detail.complemento,
    bairro: detail.bairro,
    cidade: detail.cidade,
    uf: detail.uf,
    cep: detail.cep,
    observacoes: detail.observacoes,
    ativo: detail.ativo,
    perfilRelacionamento: detail.relacaoLoja.ehCliente && detail.relacaoLoja.ehFornecedor
      ? "ambos"
      : detail.relacaoLoja.ehFornecedor
        ? "fornecedor"
        : "cliente",
    aceitaCreditoLoja: detail.relacaoLoja.aceitaCreditoLoja,
    politicaPadraoFimConsignacao: detail.relacaoLoja
      .politicaPadraoFimConsignacao as "devolver" | "doar",
    observacoesInternas: detail.relacaoLoja.observacoesInternas,
    statusRelacao: detail.relacaoLoja.statusRelacao as "ativo" | "inativo",
    usuarioId: detail.usuarioVinculado?.id ?? "",
    contasBancarias: detail.contasBancarias.map((account) => ({
      id: account.id,
      banco: account.banco,
      agencia: account.agencia,
      conta: account.conta,
      tipoConta: account.tipoConta,
      pixTipo: account.pixTipo,
      pixChave: account.pixChave,
      favorecidoNome: account.favorecidoNome,
      favorecidoDocumento: account.favorecidoDocumento,
      principal: account.principal,
    })),
  };
}

// Traduz o estado do formulario para o payload esperado pela API.
function mapFormToPayload(form: PersonFormState) {
  return {
    tipoPessoa: form.tipoPessoa,
    nome: form.nome,
    nomeSocial: form.nomeSocial,
    documento: form.documento,
    telefone: form.telefone,
    email: form.email,
    logradouro: form.logradouro,
    numero: form.numero,
    complemento: form.complemento,
    bairro: form.bairro,
    cidade: form.cidade,
    uf: form.uf,
    cep: form.cep,
    observacoes: form.observacoes,
    ativo: form.ativo,
    usuarioId: form.usuarioId || null,
    relacaoLoja: {
      ehCliente:
        form.perfilRelacionamento === "cliente" ||
        form.perfilRelacionamento === "ambos",
      ehFornecedor:
        form.perfilRelacionamento === "fornecedor" ||
        form.perfilRelacionamento === "ambos",
      aceitaCreditoLoja: form.aceitaCreditoLoja,
      politicaPadraoFimConsignacao: form.politicaPadraoFimConsignacao,
      observacoesInternas: form.observacoesInternas,
      statusRelacao: form.statusRelacao,
    },
    contasBancarias: form.contasBancarias.map((account) => ({
      id: account.id || null,
      banco: account.banco,
      agencia: account.agencia,
      conta: account.conta,
      tipoConta: account.tipoConta,
      pixTipo: account.pixTipo,
      pixChave: account.pixChave,
      favorecidoNome: account.favorecidoNome,
      favorecidoDocumento: account.favorecidoDocumento,
      principal: account.principal,
    })),
  };
}

// Coordena a tela do modulo 03 com listagem, detalhe, edicao e resumo financeiro.
export function PeopleDashboard() {
  const { token, session } = useSystemSession();
  const queryClient = useQueryClient();
  const [draftForm, setDraftForm] = useState<PersonFormState | null>(null);
  const [search, setSearch] = useState("");
  const [selectedPersonId, setSelectedPersonId] = useState("");
  const canViewPeople = hasAnyPermission(session, [
    accessPermissionCodes.peopleView,
    accessPermissionCodes.peopleManage,
  ]);
  const canManagePeople = hasPermission(session, accessPermissionCodes.peopleManage);

  const peopleQuery = useQuery({
    enabled: Boolean(session.lojaAtivaId && canViewPeople),
    queryFn: () => listPeople(token),
    queryKey: queryKeys.people(token, session.lojaAtivaId),
  });

  const personDetailQuery = useQuery({
    enabled: Boolean(selectedPersonId && canViewPeople),
    queryFn: () => getPersonById(token, selectedPersonId),
    queryKey: queryKeys.personDetail(token, session.lojaAtivaId, selectedPersonId),
  });

  const userOptionsQuery = useQuery({
    enabled: Boolean(session.lojaAtivaId && canManagePeople),
    queryFn: () => listLinkablePeopleUsers(token),
    queryKey: queryKeys.peopleUsers(token, session.lojaAtivaId),
  });

  const peopleMutation = useMutation({
    mutationFn: async () => {
      const currentForm =
        draftForm ??
        (selectedPersonId && personDetailQuery.data
          ? mapPersonDetailToForm(personDetailQuery.data)
          : emptyPersonForm());

      const parsed = personFormSchema.safeParse(currentForm);
      if (!parsed.success) {
        throw new Error(getZodErrorMessage(parsed.error));
      }

      const payload = mapFormToPayload(parsed.data);
      if (currentForm.id) {
        return updatePerson(token, currentForm.id, payload);
      }

      return createPerson(token, payload);
    },
    onError: (error) => {
      toast.error(getErrorMessage(error));
    },
    onSuccess: async (response) => {
      setSelectedPersonId(response.id);
      setDraftForm(mapPersonDetailToForm(response));
      toast.success(
        draftForm?.id ? "Cadastro atualizado com sucesso." : "Cadastro criado com sucesso.",
      );
      queryClient.setQueryData(
        queryKeys.personDetail(token, session.lojaAtivaId, response.id),
        response,
      );
      await Promise.all([
        queryClient.invalidateQueries({
          queryKey: queryKeys.people(token, session.lojaAtivaId),
        }),
        queryClient.invalidateQueries({
          queryKey: queryKeys.peopleUsers(token, session.lojaAtivaId),
        }),
      ]);
    },
  });

  useEffect(() => {
    if (peopleQuery.isError) {
      toast.error(getErrorMessage(peopleQuery.error));
    }
  }, [peopleQuery.error, peopleQuery.isError]);

  useEffect(() => {
    if (personDetailQuery.isError) {
      toast.error(getErrorMessage(personDetailQuery.error));
    }
  }, [personDetailQuery.error, personDetailQuery.isError]);

  useEffect(() => {
    if (userOptionsQuery.isError) {
      toast.error(getErrorMessage(userOptionsQuery.error));
    }
  }, [userOptionsQuery.error, userOptionsQuery.isError]);

  useEffect(() => {
    const people = peopleQuery.data ?? [];
    if (people.length === 0) {
      return;
    }

    if (!selectedPersonId || !people.some((person) => person.id === selectedPersonId)) {
      startTransition(() => {
        setSelectedPersonId(people[0]?.id ?? "");
      });
    }
  }, [peopleQuery.data, selectedPersonId]);

  if (!canViewPeople) {
    return (
      <AccessStateCard
        message="Solicite a permissao adequada para consultar clientes e fornecedores."
        subtitle="Sua conta nao possui acesso ao modulo de pessoas."
        title="Modulo sem permissao"
      />
    );
  }

  const people = peopleQuery.data ?? [];
  const form =
    draftForm ??
    (selectedPersonId && personDetailQuery.data
      ? mapPersonDetailToForm(personDetailQuery.data)
      : emptyPersonForm());
  const selectedSummary =
    people.find((person) => person.id === selectedPersonId) ?? null;
  const busy =
    peopleQuery.isLoading ||
    personDetailQuery.isLoading ||
    userOptionsQuery.isLoading ||
    peopleMutation.isPending;

  function setForm(value: SetStateAction<PersonFormState>) {
    setDraftForm((current) => {
      const baseValue = current ?? form;
      return typeof value === "function"
        ? (value as (current: PersonFormState) => PersonFormState)(baseValue)
        : value;
    });
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    await peopleMutation.mutateAsync();
  }

  function handleNewPerson() {
    setSelectedPersonId("");
    setDraftForm(emptyPersonForm());
  }

  function handleSelectPerson(personId: string) {
    setSelectedPersonId(personId);
    setDraftForm(null);
  }

  return (
    <div className="dashboard-grid">
      <div className="dashboard-column" style={{ gridColumn: "1 / -1" }}>
        <PeopleOverview people={people} />
      </div>

      <div className="dashboard-column">
        <PeopleListPanel
          canManage={canManagePeople}
          onNewPerson={handleNewPerson}
          onSelectPerson={handleSelectPerson}
          people={people}
          search={search}
          selectedPersonId={selectedPersonId}
          setSearch={setSearch}
        />
      </div>

      <div className="dashboard-column">
        <PersonFormPanel
          busy={busy}
          canManage={canManagePeople}
          form={form}
          onSubmit={handleSubmit}
          setForm={setForm}
          userOptions={userOptionsQuery.data ?? []}
        />
        <PersonFinancialPanel
          detail={personDetailQuery.data}
          selectedSummary={selectedSummary}
        />
      </div>
    </div>
  );
}
