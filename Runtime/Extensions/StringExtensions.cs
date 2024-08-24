using System.Text;

namespace Runtime.Extensions
{
    public static class StringExtensions
    {
        #region Class Methods

        public static string ToSnakeCase(this string inputString)
        {
            if (string.IsNullOrEmpty(inputString))
                return inputString;

            var stringBuilder = new StringBuilder();
            stringBuilder.Append(char.ToLower(inputString[0]));

            for (int i = 1; i < inputString.Length; i++)
            {
                char character = inputString[i];
                if (char.IsUpper(character) || (char.IsDigit(character) && i > 1 && !char.IsDigit(inputString[i - 1])))
                    stringBuilder.Append('_');
                stringBuilder.Append(char.ToLower(character));
            }

            return stringBuilder.ToString();
        }

        #endregion Class Methods
    }
}