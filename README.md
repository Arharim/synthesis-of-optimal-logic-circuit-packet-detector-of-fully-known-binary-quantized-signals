# synthesis-of-optimal-logic-circuit-packet-detector-of-fully-known-binary-quantized-signals

This project is a C# console application for performing logical analysis on a set of probabilities. The program evaluates combinations of binary states to compute likelihood ratios, false alarm probabilities, and generates logical expressions in both perfect disjunctive normal form (PDNF) and its minimized version.

## Features

- Accepts default or custom input paraInimeters.
- Computes:
  - Likelihood Ratios
  - False Alarm Probabilities
- Evaluates and sorts combinations based on likelihood ratios.
- Generates logical expressions for valid combinations:
  - Perfect Disjunctive Normal Form (PDNF)
  - Minimized PDNF
- Provides detailed step-by-step output for transparency and debugging.

## Input Requirements

### Default Parameters

- **`p_S`**: Probability values for six binary states. Default: `[0.29, 0.53, 0.04, 0.04, 0.53, 0.29]`
- **`p_N`**: Noise probability. Default: `0.09`
- **`P_lo_threshold`**: Threshold probability. Default: `0.0006`

### Custom Parameters

- `p_S`: Six values in the range (0, 1), separated by spaces.
- `p_N`: A single value in the range (0, 1).
- `P_lo_threshold`: A single value in the range (0, 1).

## How to Run

1. Compile the project using a C# compiler or open it in an IDE like Visual Studio.
2. Run the program.
3. Choose to use default parameters or provide custom inputs.
4. Follow the on-screen prompts to enter data if custom input is chosen.
5. View the analysis results in the console output.

## Approach to the Problem

The general approach to solving the problem of static synthesis of optimal logical circuits for detecting fully known signals of burst structure is as follows:

1. All possible combinations of zeros and ones in the positions of the burst are enumerated. The number of such combinations will be 2^N, where N is the length of the signal burst.
2. For each combination, the conditional probability of its occurrence in the signal region and in the noise region is computed. The likelihood ratio  is calculated.
3. Based on the chosen optimality criterion, a decision is made on whether the signal is detected or not when receiving the given combination.
4. During the enumeration process, the set of all combinations  is divided into two subsets:
   - : associated with the decision that the useful signal is present.
   - : associated with the decision that the useful signal is absent.
5. For the subset , the perfect disjunctive normal form (PDNF) of the logical function is recorded. The form is then minimized.
6. A combinational circuit implementing the minimized form is constructed. This resulting circuit represents the optimal detector.

## Output

- **Likelihood Ratios**: Sorted values for each binary combination.
- **False Alarm Probabilities**: Computed cumulative probabilities.
- **Logical Expressions**:
  - PDNF for combinations that meet the threshold criteria.
  - Minimized PDNF for concise representation.

## Example

### Input:

```
Use default data? (y/n): n
Enter 6 p_S values separated by spaces (e.g., 0.29 0.53 0.04 0.04 0.53 0.29): 0.3 0.5 0.1 0.1 0.5 0.3
Enter the p_N value (e.g., 0.09): 0.1
Enter the threshold probability P_lo_threshold: 0.001
```

### Output:

```
Code: 000001 | Likelihood Ratio: 1.123456 | False Alarm Probability: 0.000100 | Cumulative Probability: 0.000100 | Exceeds Threshold: False
...
Perfect Disjunctive Normal Form (PDNF):
(x1 & !x2 & x3) || (!x1 & x2 & x3)

Minimized PDNF:
(x1 & x3) || (x2 & x3)
```

## Files

- **`Program.cs`**: Main application logic.
- **`README.md`**: Documentation.

## Extensibility

This application can be extended to:

- Support more complex logical combinations.
- Integrate graphical user interfaces.
- Export results to external formats like JSON or CSV.

## License

This project is released under the MIT License. See `LICENSE` for details.

---

Enjoy exploring logical analysis with this tool! If you encounter issues or have suggestions, feel free to contribute or raise an issue on the GitHub repository.

