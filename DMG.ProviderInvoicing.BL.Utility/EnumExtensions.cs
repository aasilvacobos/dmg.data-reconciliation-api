using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMG.ProviderInvoicing.BL.Utility
{
	public static class EnumExtensions
	{
		public static T GetEnumValue<T>(this string enumString, T defaultValue) where T : struct, Enum
		{
			if (!Enum.TryParse(enumString, out T result))
			{
				return defaultValue;
			}

			return result;
		}
	}
}
