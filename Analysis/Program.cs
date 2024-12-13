class Program
{
    static void Main()
    {
        bool useDefault = PromptForDefault();
        double[] p_S;
        double p_N;
        double P_lo_threshold;

        if (useDefault)
        {
            (p_S, p_N, P_lo_threshold) = GetDefaultParameters();
        }
        else
        {
            p_S = PromptForArray(
                "Enter 6 p_S values separated by spaces (each in the range (0, 1), e.g., 0.29 0.53 0.04 0.04 0.53 0.29):",
                input => double.Parse(input),
                values => values.Length == 6 && values.All(v => v > 0 && v < 1),
                "The p_S array must contain exactly 6 values, each in the range (0, 1), e.g., 0.29 0.53 0.04 0.04 0.53 0.29."
            );

            p_N = PromptForInput(
                "Enter the p_N value (e.g., 0.09):",
                input => double.Parse(input),
                value => value > 0 && value < 1,
                "p_N must be in the range (0, 1)."
            );

            P_lo_threshold = PromptForInput(
                "Enter the threshold probability P_lo_threshold:",
                input => double.Parse(input),
                value => value > 0 && value < 1,
                "P_lo_threshold must be in the range (0, 1)."
            );
        }

        PerformAnalysis(p_S, p_N, P_lo_threshold);
    }

    static void PerformAnalysis(double[] p_S, double p_N, double P_lo_threshold)
    {
        double[] q_S = p_S.Select(p => 1 - p).ToArray();
        double q_N = 1 - p_N;

        var combinations = Enumerable
            .Range(0, 64)
            .Select(i => Convert.ToString(i, 2).PadLeft(6, '0').Select(c => c - '0').ToArray())
            .AsParallel()
            .Select(code =>
                (
                    code,
                    likelihoodRatio: ComputeLikelihoodRatio(code, p_S, q_S, p_N, q_N),
                    falseAlarmProbability: ComputeFalseAlarmProbability(code, p_N, q_N)
                )
            )
            .OrderByDescending(c => c.likelihoodRatio)
            .ToList();

        double cumulativeFalseAlarmProbability = 0.0;
        var logicalExpressions = new List<string>();

        foreach (var (code, likelihoodRatio, falseAlarmProbability) in combinations)
        {
            cumulativeFalseAlarmProbability += falseAlarmProbability;

            bool exceedsThreshold = cumulativeFalseAlarmProbability > P_lo_threshold;

            Console.WriteLine(
                $"Code: {string.Join("", code)} | Likelihood Ratio: {likelihoodRatio:F6} | "
                    + $"False Alarm Probability: {falseAlarmProbability:F6} | "
                    + $"Cumulative Probability: {cumulativeFalseAlarmProbability:F6} | Exceeds Threshold: {exceedsThreshold}"
            );

            if (cumulativeFalseAlarmProbability <= P_lo_threshold)
            {
                logicalExpressions.Add(ConvertToLogicalExpression(code));
            }
        }

        string PDNF = string.Join(" || ", logicalExpressions);
        Console.WriteLine("\nPerfect Disjunctive Normal Form (PDNF):");
        Console.WriteLine(PDNF);

        var minimizedPDNF = MinimizePDNF(logicalExpressions);
        Console.WriteLine("\nMinimized PDNF:");
        Console.WriteLine(string.Join(" || ", minimizedPDNF));
    }

    static (double[] p_S, double p_N, double P_lo_threshold) GetDefaultParameters()
    {
        return (new[] { 0.29, 0.53, 0.04, 0.04, 0.53, 0.29 }, 0.09, 0.0006);
    }

    static bool PromptForDefault()
    {
        while (true)
        {
            Console.WriteLine("Use default data? (y/n):");
            var input = Console.ReadLine()?.Trim().ToLower();
            if (input == "y")
                return true;
            if (input == "n")
                return false;
            Console.WriteLine("Invalid input. Please enter 'y' or 'n'.");
        }
    }

    static T PromptForInput<T>(
        string message,
        Func<string, T> parseFunc,
        Func<T, bool> validateFunc,
        string errorMessage
    )
    {
        while (true)
        {
            try
            {
                Console.WriteLine(message);
                var input = parseFunc(
                    Console.ReadLine()
                        ?? throw new InvalidOperationException("Input cannot be null")
                );
                if (!validateFunc(input))
                    throw new Exception(errorMessage);
                return input;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Input error: {ex.Message}. Please try again.");
            }
        }
    }

    static double[] PromptForArray(
        string message,
        Func<string, double> parseFunc,
        Func<double[], bool> validateFunc,
        string errorMessage
    )
    {
        while (true)
        {
            try
            {
                Console.WriteLine(message);
                var values =
                    (Console.ReadLine()?.Split(' ').Select(parseFunc).ToArray())
                    ?? Array.Empty<double>();
                if (!validateFunc(values))
                    throw new Exception(errorMessage);
                return values;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Input error: {ex.Message}. Please try again.");
            }
        }
    }

    static double ComputeLikelihoodRatio(
        int[] code,
        double[] p_S,
        double[] q_S,
        double p_N,
        double q_N
    )
    {
        return code.Zip(p_S, (bit, p) => bit == 1 ? p / p_N : (1 - p) / q_N)
            .Aggregate(1.0, (acc, value) => acc * value);
    }

    static double ComputeFalseAlarmProbability(int[] code, double p_N, double q_N)
    {
        return code.Aggregate(1.0, (acc, bit) => acc * (bit == 1 ? p_N : q_N));
    }

    static string ConvertToLogicalExpression(int[] code)
    {
        return "("
            + string.Join(" & ", code.Select((bit, i) => bit == 1 ? $"x{i + 1}" : $"!x{i + 1}"))
            + ")";
    }

    static List<string> MinimizePDNF(List<string> expressions)
    {
        expressions = expressions.Distinct().ToList();

        bool merged;
        do
        {
            merged = false;
            for (int i = 0; i < expressions.Count; i++)
            {
                for (int j = i + 1; j < expressions.Count; j++)
                {
                    if (
                        TryMergeTerms(expressions[i], expressions[j], out string? mergedTerm)
                        && mergedTerm != null
                    )
                    {
                        expressions[i] = mergedTerm;
                        expressions.RemoveAt(j);
                        merged = true;
                        break;
                    }
                }
                if (merged)
                    break;
            }
        } while (merged);

        return expressions;
    }

    static bool TryMergeTerms(string term1, string term2, out string? mergedTerm)
    {
        var terms1 = term1.Replace("(", "").Replace(")", "").Split(" & ");
        var terms2 = term2.Replace("(", "").Replace(")", "").Split(" & ");

        if (terms1.Length != terms2.Length)
        {
            mergedTerm = null;
            return false;
        }

        int diffCount = 0;
        var result = new List<string>();

        for (int i = 0; i < terms1.Length; i++)
        {
            if (terms1[i] == terms2[i])
            {
                result.Add(terms1[i]);
            }
            else if (++diffCount > 1)
            {
                mergedTerm = null;
                return false;
            }
        }

        mergedTerm = "(" + string.Join(" & ", result) + ")";
        return true;
    }
}