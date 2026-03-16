import type { ButtonHTMLAttributes, ReactNode } from "react";

import { cx } from "@/lib/helpers/classnames";

type ButtonVariant = "primary" | "secondary" | "ghost" | "soft";

type ButtonProps = ButtonHTMLAttributes<HTMLButtonElement> & {
  children: ReactNode;
  variant?: ButtonVariant;
  fullWidth?: boolean;
};

export function Button({
  children,
  className,
  variant = "primary",
  fullWidth = false,
  type = "button",
  ...props
}: ButtonProps) {
  return (
    <button
      className={cx(
        "ui-button",
        variant === "primary" && "ui-button-primary",
        variant === "secondary" && "ui-button-secondary",
        variant === "ghost" && "ui-button-ghost",
        variant === "soft" && "ui-button-soft",
        fullWidth && "ui-button-block",
        className
      )}
      type={type}
      {...props}
    >
      {children}
    </button>
  );
}
