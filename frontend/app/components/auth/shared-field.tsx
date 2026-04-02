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
  return (
    <label className="block space-y-2">
      <span className="text-sm font-medium text-[#867fce]">{label}</span>
      <input
        name={name}
        type={type}
        value={value}
        autoComplete={autoComplete}
        placeholder={placeholder}
        onChange={(event) => onChange(name, event.target.value)}
        className={`h-13 w-full rounded-2xl border bg-white px-4 text-sm text-[#2d2464] outline-none transition placeholder:text-[#b1add8] focus:border-[#6a63f4] focus:ring-4 focus:ring-[#6a63f4]/15 ${
          error ? "border-[#e86f8f]" : "border-[#dedaf8]"
        }`}
      />
      {error ? <span className="text-xs text-[#d25378]">{error}</span> : null}
    </label>
  );
}
