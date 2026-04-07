import type { ClientListItem, ClientVisibleField } from "@/lib/client";

type ClientsTableProps = {
  clients: ClientListItem[];
  visibleFields: ClientVisibleField[];
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

export function ClientsTable({ clients, visibleFields }: ClientsTableProps) {
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
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
