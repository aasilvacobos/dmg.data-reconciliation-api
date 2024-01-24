using LanguageExt;
using System;
using static LanguageExt.Prelude;

namespace DMG.ProviderInvoicing.BL.Utility;

public static class GuidExtensions
{
	/// <summary>
	/// Converts Guid to Option/<Guid/>. Empty Guid becomes None.
	/// </summary>
	/// <param name="guid"></param>
	/// <returns></returns>
	public static Option<Guid> ToOptional(this Guid guid) =>
		guid == Guid.Empty ? None : Optional(guid);
}