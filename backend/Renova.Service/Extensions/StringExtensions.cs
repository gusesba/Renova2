namespace Renova.Service.Extensions
{
    public static class StringExtensions
    {
        public static string KeepOnlyDigits(this string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return new string(value.Where(char.IsDigit).ToArray());
        }
    }
}
