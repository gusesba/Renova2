import type { HTMLAttributes, ReactNode } from "react";

import { cx } from "@/lib/helpers/classnames";

// Bloco visual generico usado como base dos paineis e cards.
type CardProps = HTMLAttributes<HTMLElement> & {
  children: ReactNode;
};

export function Card({ children, className, ...props }: CardProps) {
  return (
    <section className={cx("ui-card", className)} {...props}>
      {children}
    </section>
  );
}

type CardBodyProps = HTMLAttributes<HTMLDivElement> & {
  children: ReactNode;
};

export function CardBody({ children, className, ...props }: CardBodyProps) {
  return (
    <div className={cx("ui-card-body", className)} {...props}>
      {children}
    </div>
  );
}

type CardHeadingProps = {
  title: string;
  subtitle?: string;
};

export function CardHeading({ title, subtitle }: CardHeadingProps) {
  return (
    <header>
      <h2 className="ui-card-title">{title}</h2>
      {subtitle ? <p className="ui-card-subtitle">{subtitle}</p> : null}
    </header>
  );
}
