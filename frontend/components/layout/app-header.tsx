import { Button } from "@/components/ui/button";
import { SelectField } from "@/components/ui/field";
import { getInitials } from "@/lib/helpers/formatters";
import type { SessionContext } from "@/lib/services/renova-api";

// Header principal do shell com loja ativa, dados do usuario e acoes de sessao.
type AppHeaderProps = {
  session: SessionContext;
  onChangeStore: (storeId: string) => void;
  onLogout: () => void;
  onToggleSidebar: () => void;
  sidebarCollapsed: boolean;
};

export function AppHeader({
  session,
  onChangeStore,
  onLogout,
  onToggleSidebar,
  sidebarCollapsed,
}: AppHeaderProps) {
  return (
    <header className="app-header">
      <div className="app-header-left">
        <button
          aria-label={sidebarCollapsed ? "Expandir menu" : "Recolher menu"}
          className="menu-toggle-button"
          onClick={onToggleSidebar}
          title={sidebarCollapsed ? "Expandir menu" : "Recolher menu"}
          type="button"
        >
          <span />
          <span />
          <span />
        </button>
        <div className="app-header-intro">
          <h1 className="app-header-title">Access Control</h1>
          <p className="app-header-subtitle">Usuarios, cargos e permissoes</p>
        </div>
      </div>
      <div className="app-header-actions">
        {session.lojas.length > 0 ? (
          <div className="app-header-store">
            <SelectField
              label="Loja ativa"
              onChange={(event) => onChangeStore(event.target.value)}
              value={session.lojaAtivaId ?? ""}
            >
              {session.lojas.map((store) => (
                <option key={store.id} value={store.id}>
                  {store.nome}
                </option>
              ))}
            </SelectField>
          </div>
        ) : (
          <div className="app-header-user">
            <div>
              <div className="app-header-user-name">Sem loja vinculada</div>
              <div className="app-nav-meta">Aguardando liberacao de acesso</div>
            </div>
          </div>
        )}
        <div className="app-header-user">
          <div>
            <div className="app-header-user-name">{session.usuario.nome}</div>
            <div className="app-nav-meta">{session.usuario.email}</div>
          </div>
          <span className="avatar-pill">{getInitials(session.usuario.nome)}</span>
        </div>
        <Button className="app-header-logout" onClick={onLogout} variant="ghost">
          Sair
        </Button>
      </div>
    </header>
  );
}
