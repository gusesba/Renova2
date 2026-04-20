"use client";

import { Fragment, useState } from "react";

import {
  formatCurrencyValue,
  type SolicitacaoListItem,
  type SolicitacaoVisibleField,
} from "@/lib/solicitacao";

type SolicitacoesTableProps = {
  solicitacoes: SolicitacaoListItem[];
  visibleFields: SolicitacaoVisibleField[];
  canDeleteSolicitacao: boolean;
  onDeleteSolicitacao: (solicitacao: SolicitacaoListItem) => void;
};

function Cell({
  children,
  subtle = false,
}: {
  children: React.ReactNode;
  subtle?: boolean;
}) {
  return (
    <td className={`px-4 py-4 text-sm ${subtle ? "text-[var(--muted)]" : "text-[var(--foreground)]"}`}>
      {children}
    </td>
  );
}

export function SolicitacoesTable({
  solicitacoes,
  visibleFields,
  canDeleteSolicitacao,
  onDeleteSolicitacao,
}: SolicitacoesTableProps) {
  const [expandedIds, setExpandedIds] = useState<number[]>([]);

  function toggleExpanded(id: number) {
    setExpandedIds((current) =>
      current.includes(id) ? current.filter((currentId) => currentId !== id) : [...current, id],
    );
  }

  const showProduto = visibleFields.includes("produto");
  const showDescricao = visibleFields.includes("descricao");
  const showMarca = visibleFields.includes("marca");
  const showTamanho = visibleFields.includes("tamanho");
  const showCor = visibleFields.includes("cor");
  const showCliente = visibleFields.includes("cliente");
  const showPrecoMaximo = visibleFields.includes("precoMaximo");
  const showMatches = visibleFields.includes("matches");
  const showId = visibleFields.includes("id");

  return (
    <div className="mt-6 overflow-hidden rounded-[24px] border border-[var(--border)]">
      <div className="overflow-x-auto">
        <table className="min-w-full border-collapse bg-white">
          <thead className="bg-[var(--surface-muted)]">
            <tr className="text-left">
              <th className="px-4 py-4 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                Expandir
              </th>
              {showProduto ? (
                <th className="px-4 py-4 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                  Produto
                </th>
              ) : null}
              {showDescricao ? (
                <th className="px-4 py-4 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                  Descricao
                </th>
              ) : null}
              {showMarca ? (
                <th className="px-4 py-4 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                  Marca
                </th>
              ) : null}
              {showTamanho ? (
                <th className="px-4 py-4 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                  Tamanho
                </th>
              ) : null}
              {showCor ? (
                <th className="px-4 py-4 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                  Cor
                </th>
              ) : null}
              {showCliente ? (
                <th className="px-4 py-4 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                  Cliente
                </th>
              ) : null}
              {showPrecoMaximo ? (
                <th className="px-4 py-4 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                  Preco maximo
                </th>
              ) : null}
              {showMatches ? (
                <th className="px-4 py-4 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                  Matches
                </th>
              ) : null}
              {showId ? (
                <th className="px-4 py-4 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                  Identificador
                </th>
              ) : null}
              <th className="px-4 py-4 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                Acoes
              </th>
            </tr>
          </thead>
          <tbody>
            {solicitacoes.map((solicitacao, index) => {
              const isExpanded = expandedIds.includes(solicitacao.id);
              const colSpan =
                1 +
                [
                  showProduto,
                  showDescricao,
                  showMarca,
                  showTamanho,
                  showCor,
                  showCliente,
                  showPrecoMaximo,
                  showMatches,
                  showId,
                  true,
                ].filter(Boolean).length;

              return (
                <Fragment key={solicitacao.id}>
                  <tr
                    className={
                      index % 2 === 0
                        ? "bg-white"
                        : "bg-[color:color-mix(in_srgb,var(--surface-muted)_55%,white)]"
                    }
                  >
                    <Cell>
                      <button
                        type="button"
                        onClick={() => toggleExpanded(solicitacao.id)}
                        className="inline-flex h-10 w-10 cursor-pointer items-center justify-center rounded-2xl border border-[var(--border)] bg-white text-[var(--foreground)] transition hover:border-[var(--border-strong)]"
                        aria-label={isExpanded ? "Recolher matches" : "Expandir matches"}
                      >
                        {isExpanded ? "−" : "+"}
                      </button>
                    </Cell>
                    {showProduto ? <Cell>{solicitacao.produto}</Cell> : null}
                    {showDescricao ? <Cell>{solicitacao.descricao}</Cell> : null}
                    {showMarca ? <Cell>{solicitacao.marca}</Cell> : null}
                    {showTamanho ? <Cell subtle>{solicitacao.tamanho}</Cell> : null}
                    {showCor ? <Cell subtle>{solicitacao.cor}</Cell> : null}
                    {showCliente ? <Cell>{solicitacao.cliente}</Cell> : null}
                    {showPrecoMaximo ? (
                      <Cell>
                        {solicitacao.precoMaximo == null
                          ? "Qualquer preco"
                          : formatCurrencyValue(solicitacao.precoMaximo)}
                      </Cell>
                    ) : null}
                    {showMatches ? <Cell>{solicitacao.produtosCompativeis.length}</Cell> : null}
                    {showId ? <Cell subtle>#{solicitacao.id}</Cell> : null}
                    <Cell>
                      <div className="flex items-center gap-2">
                        {canDeleteSolicitacao ? (
                          <button
                            type="button"
                            onClick={() => onDeleteSolicitacao(solicitacao)}
                            className="inline-flex h-10 w-10 cursor-pointer items-center justify-center rounded-2xl border border-rose-200 bg-rose-50 text-rose-600 transition hover:border-rose-300 hover:bg-rose-100 hover:text-rose-700"
                            aria-label={`Excluir solicitacao #${solicitacao.id}`}
                            title={`Excluir solicitacao #${solicitacao.id}`}
                          >
                            <svg
                              aria-hidden="true"
                              viewBox="0 0 24 24"
                              className="h-4 w-4"
                              fill="none"
                              stroke="currentColor"
                              strokeWidth="2"
                              strokeLinecap="round"
                              strokeLinejoin="round"
                            >
                              <path d="M3 6h18" />
                              <path d="M8 6V4h8v2" />
                              <path d="M19 6l-1 14H6L5 6" />
                              <path d="M10 11v6" />
                              <path d="M14 11v6" />
                            </svg>
                          </button>
                        ) : (
                          <span className="inline-flex h-10 w-10" aria-hidden="true" />
                        )}
                      </div>
                    </Cell>
                  </tr>
                  {isExpanded ? (
                    <tr>
                      <td colSpan={colSpan} className="border-t border-[var(--border)] bg-[var(--surface-muted)] px-4 py-4">
                        <div className="space-y-3">
                          <div className="flex flex-col gap-1 sm:flex-row sm:items-center sm:justify-between">
                            <p className="text-sm font-semibold text-[var(--foreground)]">
                              Produtos compativeis
                            </p>
                            <p className="text-sm text-[var(--muted)]">
                              {solicitacao.precoMaximo == null
                                ? "Sem limite de preco"
                                : `Preco maximo: ${formatCurrencyValue(solicitacao.precoMaximo)}`}
                            </p>
                          </div>

                          {solicitacao.produtosCompativeis.length > 0 ? (
                            <div className="overflow-x-auto rounded-2xl border border-[var(--border)] bg-white">
                              <table className="min-w-full border-collapse">
                                <thead className="bg-[var(--surface-muted)]">
                                  <tr className="text-left">
                                    <th className="px-4 py-3 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                                      Produto
                                    </th>
                                    <th className="px-4 py-3 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                                      Descricao
                                    </th>
                                    <th className="px-4 py-3 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                                      Fornecedor
                                    </th>
                                    <th className="px-4 py-3 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                                      Preco
                                    </th>
                                  </tr>
                                </thead>
                                <tbody>
                                  {solicitacao.produtosCompativeis.map((produto) => (
                                    <tr key={produto.id} className="border-t border-[var(--border)]">
                                      <td className="px-4 py-3 text-sm text-[var(--foreground)]">
                                        {produto.produto} / {produto.marca} / {produto.tamanho} / {produto.cor}
                                      </td>
                                      <td className="px-4 py-3 text-sm text-[var(--foreground)]">
                                        {produto.descricao}
                                      </td>
                                      <td className="px-4 py-3 text-sm text-[var(--muted)]">
                                        {produto.fornecedor}
                                      </td>
                                      <td className="px-4 py-3 text-sm font-semibold text-[var(--foreground)]">
                                        {formatCurrencyValue(produto.preco)}
                                      </td>
                                    </tr>
                                  ))}
                                </tbody>
                              </table>
                            </div>
                          ) : (
                            <div className="rounded-2xl border border-dashed border-[var(--border)] bg-white px-4 py-5 text-sm text-[var(--muted)]">
                              Nenhum produto compativel encontrado para esta solicitacao.
                            </div>
                          )}
                        </div>
                      </td>
                    </tr>
                  ) : null}
                </Fragment>
              );
            })}
          </tbody>
        </table>
      </div>
    </div>
  );
}
