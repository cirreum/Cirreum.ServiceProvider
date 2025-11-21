namespace System;

using System.Text.RegularExpressions;

/// <summary>
/// Convenience helpers related to strings.
/// </summary>
public static partial class StringExtensions {

	/// <summary>
	/// Converts the specified input string to kebab-case.
	/// </summary>
	/// <param name="input">The string value to convert.</param>
	/// <returns>The kebab-cased string value.</returns>
	public static string ToKebabCase(this string input) {

		if (string.IsNullOrEmpty(input)) {
			return input;
		}

		var result = KebabRegex().Replace(input, "$1-$2"); // Insert hyphen before uppercase letters

		return result.ToLower(); // Convert to lowercase

	}

	[GeneratedRegex(@"([a-z0-9])([A-Z])")]
	private static partial Regex KebabRegex();

}