import { useState } from "react";

import type { FormValues } from "@/lib/auth";

type AuthFieldProps = {
  autoComplete?: string;
  error?: string;
  label: string;
  name: keyof FormValues;
  onChange: (field: keyof FormValues, value: string) => void;
  placeholder: string;
  type?: string;
  value: string;
};

export function AuthField({
  autoComplete,
  error,
  label,
  name,
  onChange,
  placeholder,
  type = "text",
  value,
}: AuthFieldProps) {
  const [showPassword, setShowPassword] = useState(false);
  const isPassword = type === "password";
  const inputType = isPassword && showPassword ? "text" : type;

  return (
    <div className="block space-y-2">
      <label htmlFor={name} className="text-sm font-medium text-[#867fce]">{label}</label>
      <div className="relative">
        <input
          id={name}
          name={name}
          type={inputType}
          value={value}
          autoComplete={autoComplete}
          placeholder={placeholder}
          onChange={(event) => onChange(name, event.target.value)}
          className={`h-13 w-full rounded-2xl border bg-white px-4 text-sm text-[#2d2464] outline-none transition placeholder:text-[#b1add8] focus:border-[#6a63f4] focus:ring-4 focus:ring-[#6a63f4]/15 ${
            isPassword ? "pr-12" : ""
          } ${error ? "border-[#e86f8f]" : "border-[#dedaf8]"}`}
        />
        {isPassword ? (
          <button
            type="button"
            onClick={() => setShowPassword((current) => !current)}
            className="absolute right-3 top-1/2 flex size-8 -translate-y-1/2 items-center justify-center rounded-xl text-[#5b53eb] transition hover:bg-[#eef1ff]"
            aria-label={showPassword ? "Ocultar senha" : "Mostrar senha"}
            aria-pressed={showPassword}
          >
            {showPassword ? (
              <svg
                aria-hidden="true"
                className="size-5"
                fill="none"
                stroke="currentColor"
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth="2"
                viewBox="0 0 24 24"
              >
                <path d="m2 2 20 20" />
                <path d="M10.58 10.58a2 2 0 0 0 2.83 2.83" />
                <path d="M16.68 16.68A10.94 10.94 0 0 1 12 18c-5 0-9-6-9-6a17.4 17.4 0 0 1 4.33-4.68" />
                <path d="M9.88 5.18A10.84 10.84 0 0 1 12 5c5 0 9 7 9 7a17.44 17.44 0 0 1-2.84 3.74" />
              </svg>
            ) : (
              <svg
                aria-hidden="true"
                className="size-5"
                fill="none"
                stroke="currentColor"
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth="2"
                viewBox="0 0 24 24"
              >
                <path d="M2 12s4-7 10-7 10 7 10 7-4 6-10 6-10-6-10-6Z" />
                <circle cx="12" cy="12" r="3" />
              </svg>
            )}
          </button>
        ) : null}
      </div>
      {error ? <span className="text-xs text-[#d25378]">{error}</span> : null}
    </div>
  );
}
