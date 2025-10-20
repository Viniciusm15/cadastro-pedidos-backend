namespace Common.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Divide um nome completo em primeiro e último nome com capitalização apropriada
        /// </summary>
        /// <param name="fullName">Nome completo a ser dividido</param>
        /// <returns>Tuple contendo primeiro nome e último nome capitalizados</returns>
        public static (string firstName, string lastName) SplitFullName(this string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return (string.Empty, string.Empty);

            var nameParts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (nameParts.Length == 0)
                return (string.Empty, string.Empty);

            if (nameParts.Length == 1)
                return (Capitalize(nameParts[0]), string.Empty);

            var firstName = Capitalize(nameParts[0]);
            var lastNameParts = new string[nameParts.Length - 1];
            Array.Copy(nameParts, 1, lastNameParts, 0, nameParts.Length - 1);
            var lastName = Capitalize(string.Join(" ", lastNameParts));

            return (firstName, lastName);
        }

        private static string Capitalize(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length > 0)
                {
                    words[i] = char.ToUpper(words[i][0]) +
                              (words[i].Length > 1 ? words[i][1..].ToLower() : "");
                }
            }

            return string.Join(" ", words);
        }
    }
}
