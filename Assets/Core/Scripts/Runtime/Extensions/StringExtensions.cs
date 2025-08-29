using System;
using System.Text;

namespace Nexora
{
    public static class StringExtensions
    {
        public static string RemovePrefix(this string str, char delimeter = '_')
        {
            int prefixEnd = str.IndexOf(delimeter);
            
            return prefixEnd != StringConstants.NotFound ? str.Substring(prefixEnd + 1) : str;
        }

        public static ReadOnlySpan<char> RemovePrefix(this ReadOnlySpan<char> span, char delimeter = '_')
        {
            int prefixEnd = span.IndexOf(delimeter);

            return prefixEnd != StringConstants.NotFound ? span.Slice(prefixEnd + 1) : span;
        }

        public static string AddSpacesToCamelCase(this string str)
        {
            if(string.IsNullOrEmpty(str))
            {
                return str;
            }

            var result = new StringBuilder(str.Length * 2);
            result.Append(str[0]);

            for (int i = 1; i < str.Length; i++)
            {
                if (Char.IsUpper(str[i]))
                {
                    result.Append(' ');
                }

                result.Append(str[i]);
            }

            return result.ToString();
        }

        /// <summary>
        /// Computes the Damerau-Levenshtein distance between two strings (minimum edits to transform str1 into str2)
        /// </summary>
        public static int ComputeEditDistance(this string str1, string str2)
        {
            // Early exits for null/empty cases
            if (string.IsNullOrEmpty(str1))
            {
                return string.IsNullOrEmpty(str2) ? 0 : str2.Length;
            }

            if (string.IsNullOrEmpty(str2))
            {
                return str1.Length;
            }

            // To memoize values
            int[,] distanceMatrix = new int[str1.Length + 1, str2.Length + 1];

            // Initialize base cases (all insertions/deletions)
            for (int i = 0; i <= str1.Length; i++)
            {
                distanceMatrix[i, 0] = i;
            }

            for (int j = 0; j <= str2.Length; j++)
            {
                distanceMatrix[0, j] = j;
            }

            // Fill the matrix
            for (int i = 1; i <= str1.Length; i++)
            {
                for (int j = 1; j <= str2.Length; j++)
                {
                    // Cost of substitution (0 if characters match, 1 otherwise)
                    int substitutionCost = str1[i - 1] == str2[j - 1] ? 0 : 1;

                    // Standard operations: deletion, insertion, substitution
                    int deletion = distanceMatrix[i - 1, j] + 1;
                    int insertion = distanceMatrix[i, j - 1] + 1;
                    int substitution = distanceMatrix[i - 1, j - 1] + substitutionCost;

                    // Minimum cost of the three operations
                    distanceMatrix[i, j] = Math.Min(Math.Min(deletion, insertion), substitution);

                    // Damerau extension: transposition (adjacent character swap)
                    if (i > 1 && j > 1 && str1[i - 1] == str2[j - 2] && str1[i - 2] == str2[j - 1])
                    {
                        int transpositionCost = distanceMatrix[i - 2, j - 2] + substitutionCost;
                        distanceMatrix[i, j] = Math.Min(distanceMatrix[i, j], transpositionCost);
                    }
                }
            }

            return distanceMatrix[str1.Length, str2.Length];
        }
    }
}
