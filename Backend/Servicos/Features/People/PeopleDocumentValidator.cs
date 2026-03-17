namespace Renova.Services.Features.People;

// Concentra a normalizacao e a validacao basica de CPF/CNPJ do cadastro mestre de pessoa.
public static class PeopleDocumentValidator
{
    /// <summary>
    /// Normaliza o documento e valida o tipo de pessoa informado.
    /// </summary>
    public static string NormalizeAndValidate(string tipoPessoa, string documento)
    {
        var normalizedTipoPessoa = NormalizeTipoPessoa(tipoPessoa);
        var normalizedDocumento = NormalizeDocument(documento);

        if (normalizedTipoPessoa == PeopleStatusValues.TipoPessoa.Fisica)
        {
            if (!IsValidCpf(normalizedDocumento))
            {
                throw new InvalidOperationException("CPF invalido.");
            }

            return normalizedDocumento;
        }

        if (!IsValidCnpj(normalizedDocumento))
        {
            throw new InvalidOperationException("CNPJ invalido.");
        }

        return normalizedDocumento;
    }

    /// <summary>
    /// Normaliza o tipo da pessoa para os valores aceitos pelo sistema.
    /// </summary>
    public static string NormalizeTipoPessoa(string tipoPessoa)
    {
        var normalized = tipoPessoa.Trim().ToLowerInvariant();
        if (!PeopleStatusValues.TipoPessoa.Todos.Contains(normalized))
        {
            throw new InvalidOperationException("Tipo de pessoa invalido.");
        }

        return normalized;
    }

    /// <summary>
    /// Remove formatacao do documento para comparar e persistir sem mascara.
    /// </summary>
    public static string NormalizeDocument(string documento)
    {
        if (string.IsNullOrWhiteSpace(documento))
        {
            throw new InvalidOperationException("Informe o documento da pessoa.");
        }

        var normalized = new string(documento.Where(char.IsDigit).ToArray());
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new InvalidOperationException("Informe o documento da pessoa.");
        }

        return normalized;
    }

    private static bool IsValidCpf(string cpf)
    {
        if (cpf.Length != 11 || cpf.Distinct().Count() == 1)
        {
            return false;
        }

        var numbers = cpf.Select(character => character - '0').ToArray();
        var firstDigit = CalculateDigit(numbers, 9, 10);
        var secondDigit = CalculateDigit(numbers, 10, 11);

        return numbers[9] == firstDigit && numbers[10] == secondDigit;
    }

    private static bool IsValidCnpj(string cnpj)
    {
        if (cnpj.Length != 14 || cnpj.Distinct().Count() == 1)
        {
            return false;
        }

        var numbers = cnpj.Select(character => character - '0').ToArray();
        var firstWeights = new[] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        var secondWeights = new[] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        var firstDigit = CalculateDigit(numbers, firstWeights);
        var secondDigit = CalculateDigit(numbers, secondWeights);

        return numbers[12] == firstDigit && numbers[13] == secondDigit;
    }

    private static int CalculateDigit(IReadOnlyList<int> numbers, int length, int startWeight)
    {
        var sum = 0;
        for (var index = 0; index < length; index++)
        {
            sum += numbers[index] * (startWeight - index);
        }

        var remainder = sum % 11;
        return remainder < 2 ? 0 : 11 - remainder;
    }

    private static int CalculateDigit(IReadOnlyList<int> numbers, IReadOnlyList<int> weights)
    {
        var sum = 0;
        for (var index = 0; index < weights.Count; index++)
        {
            sum += numbers[index] * weights[index];
        }

        var remainder = sum % 11;
        return remainder < 2 ? 0 : 11 - remainder;
    }
}
