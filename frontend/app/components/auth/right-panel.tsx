import type { AuthMode, FieldErrors, FormValues } from "@/lib/auth";

import { CadastroForm } from "./signup-form";
import { LoginForm } from "./login-form";

type AuthRightPanelProps = {
  errors: FieldErrors;
  isSubmitting: boolean;
  mode: AuthMode;
  values: FormValues;
  onFieldChange: (field: keyof FormValues, value: string) => void;
  onModeChange: (mode: AuthMode) => void;
  onSubmit: () => Promise<void>;
};

export function AuthRightPanel({
  errors,
  isSubmitting,
  mode,
  values,
  onFieldChange,
  onModeChange,
  onSubmit,
}: AuthRightPanelProps) {
  const isLogin = mode === "login";

  return (
    <section className="flex items-center bg-white px-6 py-8 sm:px-10 lg:px-12">
      <div className="mx-auto w-full max-w-md">
        {isLogin ? (
          <LoginForm
            errors={errors}
            isSubmitting={isSubmitting}
            values={values}
            onFieldChange={onFieldChange}
            onModeChange={onModeChange}
            onSubmit={onSubmit}
          />
        ) : (
          <CadastroForm
            errors={errors}
            isSubmitting={isSubmitting}
            values={values}
            onFieldChange={onFieldChange}
            onModeChange={onModeChange}
            onSubmit={onSubmit}
          />
        )}
      </div>
    </section>
  );
}
