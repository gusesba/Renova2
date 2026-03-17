// Marca simples reutilizada no login e no shell do sistema.
type RenovaMarkProps = {
  subtitle?: string;
  compact?: boolean;
};

export function RenovaMark({ subtitle, compact = false }: RenovaMarkProps) {
  return (
    <div className="auth-brand">
      <span className="auth-brand-mark">R</span>
      {!compact ? (
        <div>
          <div className="auth-brand-copy">Renova</div>
          {subtitle ? <div className="app-nav-meta">{subtitle}</div> : null}
        </div>
      ) : null}
    </div>
  );
}
