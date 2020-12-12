namespace AspNetCoreRateLimit
{
    public static class WildcardMatcher
    {
        // Thanks to Zoran Horvat
        //http://www.c-sharpcorner.com/uploadfile/b81385/efficient-string-matching-algorithm-with-use-of-wildcard-characters/
        //https://www.codinghelmet.com/?path=net/sysexpand/text/download
        public static bool IsMatch(this string value, string pattern, char singleWildcard = '?', char multipleWildcard = '*')
        {

            var inputPosStack = new int[(value.Length + 1) * (pattern.Length + 1)];   // Stack containing input positions that should be tested for further matching
            var patternPosStack = new int[inputPosStack.Length];                      // Stack containing pattern positions that should be tested for further matching
            var stackPos = -1;                                                          // Points to last occupied entry in stack; -1 indicates that stack is empty
            var pointTested = new bool[value.Length + 1, pattern.Length + 1];       // Each true value indicates that input position vs. pattern position has been tested

            var inputPos = 0;   // Position in input matched up to the first multiple wildcard in pattern
            var patternPos = 0; // Position in pattern matched up to the first multiple wildcard in pattern

            //if (pattern == null)
            //    pattern = string.Empty;

            // Match beginning of the string until first multiple wildcard in pattern
            while (inputPos < value.Length && patternPos < pattern.Length && pattern[patternPos] != multipleWildcard && (value[inputPos] == pattern[patternPos] || pattern[patternPos] == singleWildcard))
            {
                inputPos++;
                patternPos++;
            }

            // Push this position to stack if it points to end of pattern or to a general wildcard character
            if (patternPos == pattern.Length || pattern[patternPos] == multipleWildcard)
            {
                pointTested[inputPos, patternPos] = true;
                inputPosStack[++stackPos] = inputPos;
                patternPosStack[stackPos] = patternPos;
            }

            var matched = false;

            // Repeat matching until either string is matched against the pattern or no more parts remain on stack to test
            while (stackPos >= 0 && !matched)
            {
                inputPos = inputPosStack[stackPos];         // Pop input and pattern positions from stack
                patternPos = patternPosStack[stackPos--];   // Matching will succeed if rest of the input string matches rest of the pattern

                if (inputPos == value.Length && patternPos == pattern.Length)
                    matched = true;     // Reached end of both pattern and input string, hence matching is successful
                else if (patternPos == pattern.Length - 1)
                    matched = true;     // Current pattern character is multiple wildcard and it will match all the remaining characters in the input string
                else
                {
                    // First character in next pattern block is guaranteed to be multiple wildcard
                    // So skip it and search for all matches in value string until next multiple wildcard character is reached in pattern

                    for (var curInputStart = inputPos; curInputStart < value.Length; curInputStart++)
                    {

                        var curInputPos = curInputStart;
                        var curPatternPos = patternPos + 1;

                        while (curInputPos < value.Length && curPatternPos < pattern.Length && pattern[curPatternPos] != multipleWildcard &&
                               (value[curInputPos] == pattern[curPatternPos] || pattern[curPatternPos] == singleWildcard))
                        {
                            curInputPos++;
                            curPatternPos++;
                        }

                        // If we have reached next multiple wildcard character in pattern without breaking the matching sequence, then we have another candidate for full match
                        // This candidate should be pushed to stack for further processing
                        // At the same time, pair (input position, pattern position) will be marked as tested, so that it will not be pushed to stack later again
                        if (((curPatternPos == pattern.Length && curInputPos == value.Length) || (curPatternPos < pattern.Length && pattern[curPatternPos] == multipleWildcard))
                            && !pointTested[curInputPos, curPatternPos])
                        {
                            pointTested[curInputPos, curPatternPos] = true;
                            inputPosStack[++stackPos] = curInputPos;
                            patternPosStack[stackPos] = curPatternPos;
                        }
                    }
                }
            }

            return matched;
        }
    }
}
