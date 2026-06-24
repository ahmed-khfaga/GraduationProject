namespace FitZone.Service.Helpers
{
    public static class PaymentCardValidator
    {
        public static bool TryNormalize(string? cardNumber, out string normalized, out string? error)
        {
            normalized = string.Empty;
            error = null;

            if (string.IsNullOrWhiteSpace(cardNumber))
            {
                error = "Card number is required.";
                return false;
            }

            normalized = new string(cardNumber.Where(char.IsDigit).ToArray());

            if (normalized.Length != 16)
            {
                error = "Card number must be exactly 16 digits.";
                return false;
            }

            if (!PassesLuhnCheck(normalized))
            {
                error = "Invalid card number.";
                return false;
            }

            return true;
        }

        private static bool PassesLuhnCheck(string digits)
        {
            var sum = 0;
            var alternate = false;

            for (var i = digits.Length - 1; i >= 0; i--)
            {
                var n = digits[i] - '0';
                if (alternate)
                {
                    n *= 2;
                    if (n > 9)
                        n -= 9;
                }

                sum += n;
                alternate = !alternate;
            }

            return sum % 10 == 0;
        }
    }
}
