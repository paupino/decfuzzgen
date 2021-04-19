using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DecimalFuzzGenerator
{
    public class CombinationGenerator
    {
        private readonly List<Combination> _combinations;

        private CombinationGenerator(List<Combination> combinations)
        {
            _combinations = combinations ?? throw new ArgumentNullException(nameof(combinations));
        }

        public static CombinationGenerator Parse(string pattern)
        {
            if (pattern == null)
                throw new ArgumentNullException(nameof(pattern));
            if (!Regex.IsMatch(pattern, "^[01\\*]{3}$"))
                throw new ArgumentException("Invalid pattern", nameof(pattern));

            var values = pattern.ToCharArray().Select(ParseChar).ToArray();
            var combinations = new List<Combination>();
            for (int combination = 0; combination < 8; combination++)
            {
                bool currentLo = (combination & 0x1) != 0;
                bool currentMid = (combination & 0x2) != 0;
                bool currentHi = (combination & 0x4) != 0;
                bool? requestedHi = values[0];
                bool? requestedMid = values[1];
                bool? requestedLo = values[2];

                // Skip any that we need to
                if (requestedLo != null)
                {
                    if (requestedLo.Value != currentLo)
                        continue;
                }

                if (requestedMid != null)
                {
                    if (requestedMid.Value != currentMid)
                        continue;
                }

                if (requestedHi != null)
                {
                    if (requestedHi.Value != currentHi)
                        continue;
                }

                // Add the combination
                combinations.Add(new Combination(currentLo, currentMid, currentHi));
            }

            return new CombinationGenerator(combinations);
        }

        private static bool? ParseChar(char c)
        {
            switch (c)
            {
                case '1':
                    return true;
                case '0':
                    return false;
                case '*':
                    return null;
                default:
                    throw new ArgumentException("Unrecognized character", nameof(c));
            }
        }

        public List<Combination> Combinations 
        {
            get
            {
                return _combinations;
            }
        }

        public List<Tuple<decimal, decimal>> Generate(Combination combination, int size)
        {
            // All are 0, so we can only have zero for this combination. Skip this, it's obvious.
            if (!combination.Lo && !combination.Mid && !combination.Hi)
                return null;
            
            var numbers = new HashSet<Tuple<decimal, decimal>>();
            for (int i = 0; i < size;)
            {
                var value1 = combination.Generate();
                var value2 = combination.Generate();
                var tuple = Tuple.Create(value1, value2);
                if (numbers.Contains(tuple))
                    continue;
                numbers.Add(tuple);
                i++;
            }

            return numbers.ToList();
        }
    }
}