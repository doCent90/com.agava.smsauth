using System.Globalization;

namespace Agava.Wink
{
    /// <summary>
    ///     Access extention.
    /// </summary>
    public static class WinkAcceessHelper
    {
        /// <summary>
        ///     Return formated number after enter.
        /// </summary>
        public static string GetNumber(string otp_code, string phone_number, int minNumberCount, int maxNumberCount, int codeCount, bool additivePlusChar)
        {
            bool isCorrectCode = uint.TryParse(otp_code, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out uint resultCode);
            bool isCorrectNumber = ulong.TryParse(phone_number, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out ulong resultNumber);

            int countCode = resultCode.ToString().Length;
            int countNumber = resultNumber.ToString().Length;

            if (isCorrectNumber == false || isCorrectCode == false
                || string.IsNullOrEmpty(phone_number)
                || (countNumber < minNumberCount || countNumber > maxNumberCount)
                || (countCode > codeCount || countCode <= 0 || resultCode == 0))
            {
                return string.Empty;
            }

            string plus = additivePlusChar == true ? "+" : "";
            string number = $"{plus}{resultCode}{resultNumber}";
            return number;
        }
    }
}
