import type {
  InputHTMLAttributes,
  ReactNode,
  SelectHTMLAttributes,
  TextareaHTMLAttributes,
} from "react";

import { cx } from "@/lib/helpers/classnames";

type BaseFieldProps = {
  label: string;
  className?: string;
};

type TextInputProps = BaseFieldProps & InputHTMLAttributes<HTMLInputElement>;
type TextAreaProps = BaseFieldProps &
  TextareaHTMLAttributes<HTMLTextAreaElement>;
type SelectProps = BaseFieldProps &
  SelectHTMLAttributes<HTMLSelectElement> & {
    children: ReactNode;
  };

export function TextInput({ label, className, ...props }: TextInputProps) {
  return (
    <label className="ui-field">
      <span className="ui-field-label">{label}</span>
      <input className={cx("ui-input", className)} {...props} />
    </label>
  );
}

export function TextArea({ label, className, ...props }: TextAreaProps) {
  return (
    <label className="ui-field">
      <span className="ui-field-label">{label}</span>
      <textarea className={cx("ui-textarea", className)} {...props} />
    </label>
  );
}

export function SelectField({
  label,
  className,
  children,
  ...props
}: SelectProps) {
  return (
    <label className="ui-field">
      <span className="ui-field-label">{label}</span>
      <select className={cx("ui-select", className)} {...props}>
        {children}
      </select>
    </label>
  );
}
