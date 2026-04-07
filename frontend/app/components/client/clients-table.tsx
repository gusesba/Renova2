import type { ClientListItem, ClientVisibleField } from "@/lib/client";

type ClientsTableProps = {
  clients: ClientListItem[];
  visibleFields: ClientVisibleField[];
  onEditClient: (client: ClientListItem) => void;
  onDeleteClient: (client: ClientListItem) => void;
};

function ClientTableCell({
  children,
  subtle = false,
}: {
  children: React.ReactNode;
  subtle?: boolean;
}) {
  return (
    <td
      className={`px-4 py-4 text-sm ${subtle ? "text-[var(--muted)]" : "text-[var(--foreground)]"}`}
    >
      {children}
    </td>
  );
}

function EditIcon() {
  return (
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
      <path d="M12 20h9" />
      <path d="M16.5 3.5a2.12 2.12 0 1 1 3 3L7 19l-4 1 1-4 12.5-12.5Z" />
    </svg>
  );
}

function DeleteIcon() {
  return (
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
  );
}

export function ClientsTable({
  clients,
  visibleFields,
  onEditClient,
  onDeleteClient,
}: ClientsTableProps) {
  const showName = visibleFields.includes("nome");
  const showContact = visibleFields.includes("contato");
  const showUserId = visibleFields.includes("userId");
  const showId = visibleFields.includes("id");

  return (
    <div className="mt-6 overflow-hidden rounded-[24px] border border-[var(--border)]">
      <div className="overflow-x-auto">
        <table className="min-w-full border-collapse bg-white">
          <thead className="bg-[var(--surface-muted)]">
            <tr className="text-left">
              {showName ? (
                <th className="px-4 py-4 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                  Cliente
                </th>
              ) : null}
              {showContact ? (
                <th className="px-4 py-4 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                  Contato
                </th>
              ) : null}
              {showUserId ? (
                <th className="px-4 py-4 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                  UserId
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
            {clients.map((client, index) => (
              <tr
                key={client.id}
                className={
                  index % 2 === 0
                    ? "bg-white"
                    : "bg-[color:color-mix(in_srgb,var(--surface-muted)_55%,white)]"
                }
              >
                {showName ? (
                  <ClientTableCell>
                    <div className="flex items-center gap-3">
                      <div className="flex h-10 w-10 items-center justify-center rounded-2xl bg-[var(--primary-soft)] text-sm font-semibold text-[var(--primary)]">
                        {client.nome
                          .split(" ")
                          .filter(Boolean)
                          .slice(0, 2)
                          .map((part) => part[0]?.toUpperCase())
                          .join("")}
                      </div>
                      <div>
                        <p className="font-semibold text-[var(--foreground)]">{client.nome}</p>
                      </div>
                    </div>
                  </ClientTableCell>
                ) : null}
                {showContact ? <ClientTableCell>{client.contato}</ClientTableCell> : null}
                {showUserId ? (
                  <ClientTableCell subtle>{client.userId ?? "Nao vinculado"}</ClientTableCell>
                ) : null}
                {showId ? <ClientTableCell subtle>#{client.id}</ClientTableCell> : null}
                <ClientTableCell>
                  <div className="flex items-center gap-2">
                    <button
                      type="button"
                      onClick={() => onEditClient(client)}
                      className="inline-flex h-10 w-10 cursor-pointer items-center justify-center rounded-2xl border border-emerald-200 bg-emerald-50 text-emerald-600 transition hover:border-emerald-300 hover:bg-emerald-100 hover:text-emerald-700"
                      aria-label={`Editar cliente ${client.nome}`}
                      title={`Editar cliente ${client.nome}`}
                    >
                      <EditIcon />
                    </button>
                    <button
                      type="button"
                      onClick={() => onDeleteClient(client)}
                      className="inline-flex h-10 w-10 cursor-pointer items-center justify-center rounded-2xl border border-rose-200 bg-rose-50 text-rose-600 transition hover:border-rose-300 hover:bg-rose-100 hover:text-rose-700"
                      aria-label={`Excluir cliente ${client.nome}`}
                      title={`Excluir cliente ${client.nome}`}
                    >
                      <DeleteIcon />
                    </button>
                  </div>
                </ClientTableCell>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
