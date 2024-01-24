using LanguageExt;
using System;
using static LanguageExt.Prelude;

namespace DMG.ProviderInvoicing.BL.Utility;

public static class StringExtensions
{
	/// <summary>
	/// Converts string to Option/<string/>. A null, empty string or white space will be None.
	/// </summary>
	/// <param name="str"></param>
	/// <returns></returns>
	public static Option<string> ToOptional(this string str) =>
		string.IsNullOrWhiteSpace(str) ? None : Optional(str);

	/// <summary>
	/// If a string is null, empty string, or white space, then set to provided default value.
	/// </summary>
	/// <param name="str"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public static string DefaultIfNullOrWhiteSpace(this string str, string defaultValue) =>
		string.IsNullOrWhiteSpace(str) ? defaultValue : str;
}