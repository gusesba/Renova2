"use client";

import {
  Children,
  isValidElement,
  useEffect,
  useId,
  useMemo,
  useRef,
  useState,
  type ChangeEvent,
  type FocusEvent,
  type InputHTMLAttributes,
  type OptionHTMLAttributes,
  type ReactNode,
  type SelectHTMLAttributes,
  type TextareaHTMLAttributes,
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

type SelectOption = {
  disabled: boolean;
  label: string;
  value: string;
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
  disabled,
  id,
  name,
  onBlur,
  onChange,
  value,
  defaultValue,
}: SelectProps) {
  const generatedId = useId();
  const fieldId = id ?? generatedId;
  const shellRef = useRef<HTMLDivElement>(null);
  const [open, setOpen] = useState(false);
  const options = useMemo(() => extractOptions(children), [children]);
  const selectedValue = String(value ?? defaultValue ?? "");
  const selectedOption =
    options.find((option) => option.value === selectedValue) ?? options[0];

  useEffect(() => {
    if (!open) {
      return;
    }

    function handlePointerDown(event: MouseEvent) {
      if (!shellRef.current?.contains(event.target as Node)) {
        setOpen(false);
      }
    }

    function handleEscape(event: KeyboardEvent) {
      if (event.key === "Escape") {
        setOpen(false);
      }
    }

    window.addEventListener("mousedown", handlePointerDown);
    window.addEventListener("keydown", handleEscape);

    return () => {
      window.removeEventListener("mousedown", handlePointerDown);
      window.removeEventListener("keydown", handleEscape);
    };
  }, [open]);

  function emitChange(nextValue: string) {
    if (!onChange) {
      return;
    }

    const syntheticEvent = {
      currentTarget: { name: name ?? "", value: nextValue },
      target: { name: name ?? "", value: nextValue },
    } as ChangeEvent<HTMLSelectElement>;

    onChange(syntheticEvent);
  }

  function handleBlur(event: FocusEvent<HTMLDivElement>) {
    if (event.currentTarget.contains(event.relatedTarget)) {
      return;
    }

    setOpen(false);
    onBlur?.(event as unknown as FocusEvent<HTMLSelectElement>);
  }

  return (
    <label className="ui-field">
      <span className="ui-field-label">{label}</span>
      <div
        className={cx("ui-select-shell", open && "is-open", disabled && "is-disabled")}
        onBlur={handleBlur}
        ref={shellRef}
      >
        {name ? <input name={name} type="hidden" value={selectedValue} /> : null}
        <button
          aria-controls={`${fieldId}-listbox`}
          aria-expanded={open}
          aria-haspopup="listbox"
          className={cx("ui-select", className)}
          disabled={disabled}
          id={fieldId}
          onClick={() => setOpen((current) => !current)}
          type="button"
        >
          <span
            className={cx(
              "ui-select-value",
              !selectedOption?.value && "is-placeholder",
            )}
          >
            {selectedOption?.label ?? "Selecione"}
          </span>
          <span aria-hidden="true" className="ui-select-icon">
            <svg fill="none" height="16" viewBox="0 0 16 16" width="16">
              <path
                d="M4 6.5L8 10L12 6.5"
                stroke="currentColor"
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth="1.8"
              />
            </svg>
          </span>
        </button>

        {open ? (
          <div className="ui-select-dropdown" id={`${fieldId}-listbox`} role="listbox">
            {options.map((option) => (
              <button
                aria-selected={option.value === selectedValue}
                className={cx(
                  "ui-select-option",
                  option.value === selectedValue && "is-selected",
                )}
                disabled={option.disabled}
                key={`${fieldId}-${option.value}`}
                onClick={() => {
                  emitChange(option.value);
                  setOpen(false);
                }}
                role="option"
                type="button"
              >
                <span>{option.label}</span>
                {option.value === selectedValue ? (
                  <span className="ui-select-option-check" aria-hidden="true">
                    ✓
                  </span>
                ) : null}
              </button>
            ))}
          </div>
        ) : null}
      </div>
    </label>
  );
}

function extractOptions(children: ReactNode) {
  return Children.toArray(children).flatMap((child) => {
    if (!isValidElement(child) || child.type !== "option") {
      return [];
    }

    const props = child.props as OptionHTMLAttributes<HTMLOptionElement>;
    const label = extractOptionLabel(props.children);

    return [
      {
        disabled: Boolean(props.disabled),
        label,
        value: String(props.value ?? ""),
      } satisfies SelectOption,
    ];
  });
}

function extractOptionLabel(children: ReactNode) {
  if (typeof children === "string" || typeof children === "number") {
    return String(children);
  }

  return "Opcao";
}
