import type { AccessArea } from "@/lib/access-area";
import type { AuthMode, FieldErrors, FormValues } from "@/lib/auth";

import { CadastroForm } from "./signup-form";
import { LoginForm } from "./login-form";

type AuthRightPanelProps = {
  accessArea: AccessArea;
  errors: FieldErrors;
  isSubmitting: boolean;
  mode: AuthMode;
  values: FormValues;
  onAccessAreaChange: (area: AccessArea) => void;
  onFieldChange: (field: keyof FormValues, value: string) => void;
  onModeChange: (mode: AuthMode) => void;
  onSubmit: () => Promise<void>;
};

export function AuthRightPanel({
  accessArea,
  errors,
  isSubmitting,
  mode,
  values,
  onAccessAreaChange,
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
            accessArea={accessArea}
            errors={errors}
            isSubmitting={isSubmitting}
            values={values}
            onAccessAreaChange={onAccessAreaChange}
            onFieldChange={onFieldChange}
            onModeChange={onModeChange}
            onSubmit={onSubmit}
          />
        ) : (
          <CadastroForm
            accessArea={accessArea}
            errors={errors}
            isSubmitting={isSubmitting}
            values={values}
            onAccessAreaChange={onAccessAreaChange}
            onFieldChange={onFieldChange}
            onModeChange={onModeChange}
            onSubmit={onSubmit}
          />
        )}
      </div>
    </section>
  );
}
