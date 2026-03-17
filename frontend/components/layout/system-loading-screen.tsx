// Estado visual usado enquanto a sessao autenticada ainda esta sendo hidratada.
export function SystemLoadingScreen() {
  return (
    <div className="system-loading">
      <div className="system-loading-card">
        <h1 className="app-header-title" style={{ fontSize: "1.7rem", marginBottom: "0.75rem" }}>
          Carregando ambiente
        </h1>
        <p className="app-header-subtitle" style={{ marginBottom: "1.25rem" }}>
          Preparando sua sessao e aplicando o shell principal do sistema.
        </p>
        <div className="system-loading-bar" />
      </div>
    </div>
  );
}
