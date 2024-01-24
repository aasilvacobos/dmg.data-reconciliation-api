using LanguageExt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace DMG.ProviderInvoicing.BL.Utility;

public static class EnumMapperTester
{
	/// <summary>
	/// Performs a basic test that mappings are unique.
	/// </summary>
	/// <typeparam name="TSourceEnum"></typeparam>
	/// <typeparam name="TTargetEnum"></typeparam>
	/// <param name="mapperMethod"></param>
	/// <returns></returns>
	public static Either<string, Unit> TestEnumMapper<TSourceEnum, TTargetEnum>(Func<TSourceEnum, TTargetEnum> mapperMethod) where TSourceEnum : Enum where TTargetEnum : Enum
	{
		return TestEnumMapper(mapperMethod, new Dictionary<TSourceEnum, TTargetEnum>());
	}

	/// <summary>
	/// Performs a basic test that mappings are unique.
	/// </summary>
	/// <typeparam name="TSourceEnum"></typeparam>
	/// <typeparam name="TTargetEnum"></typeparam>
	/// <param name="mapperMethod"></param>
	/// <param name="specifiedMappingsNotCheckedForUniqueness">Specified mappings are verified they are mapped as expected but are not considered when checking for unique mapping results.</param>
	/// <returns></returns>
	public static Either<string, Unit> TestEnumMapper<TSourceEnum, TTargetEnum>(Func<TSourceEnum, TTargetEnum> mapperMethod, Dictionary<TSourceEnum, TTargetEnum> specifiedMappingsNotCheckedForUniqueness) where TSourceEnum : Enum where TTargetEnum : Enum
	{
		return TestEnumMapper(mapperMethod, specifiedMappingsNotCheckedForUniqueness, new System.Collections.Generic.HashSet<TTargetEnum>(), new System.Collections.Generic.HashSet<string>());
	}

	/// <summary>
	/// Performs a basic test that mappings are unique.
	/// </summary>
	/// <typeparam name="TSourceEnum"></typeparam>
	/// <typeparam name="TTargetEnum"></typeparam>
	/// <param name="mapperMethod"></param>
	/// <param name="invalidTargetEnumValues">Invalid source mappings will error if anything maps to the specified target value</param>
	/// <returns></returns>
	public static Either<string, Unit> TestEnumMapper<TSourceEnum, TTargetEnum>(Func<TSourceEnum, TTargetEnum> mapperMethod, System.Collections.Generic.HashSet<TTargetEnum> invalidTargetEnumValues) where TSourceEnum : Enum where TTargetEnum : Enum
	{
		return TestEnumMapper(mapperMethod, new Dictionary<TSourceEnum, TTargetEnum>(), invalidTargetEnumValues, new System.Collections.Generic.HashSet<string>());
	}

	/// <summary>
	/// Performs a basic test that mappings are unique.
	/// </summary>
	/// <typeparam name="TSourceEnum"></typeparam>
	/// <typeparam name="TTargetEnum"></typeparam>
	/// <param name="mapperMethod"></param>
	/// <param name="ignoreSourceEnumsStartingWith">Enum values that start with string contained in the hash set will not be checked.</param>
	/// <returns></returns>
	public static Either<string, Unit> TestEnumMapper<TSourceEnum, TTargetEnum>(Func<TSourceEnum, TTargetEnum> mapperMethod, System.Collections.Generic.HashSet<string> ignoreSourceEnumsStartingWith) where TSourceEnum : Enum where TTargetEnum : Enum
	{
		return TestEnumMapper(mapperMethod, new Dictionary<TSourceEnum, TTargetEnum>(), new System.Collections.Generic.HashSet<TTargetEnum>(), ignoreSourceEnumsStartingWith);
	}

	/// <summary>
	/// Performs a basic test that mappings are unique.
	/// </summary>
	/// <typeparam name="TSourceEnum"></typeparam>
	/// <typeparam name="TTargetEnum"></typeparam>
	/// <param name="mapperMethod"></param>
	/// <param name="specifiedMappingsNotCheckedForUniqueness">Specified mappings are verified they are mapped as expected but are not considered when checking for unique mapping results.</param>
	/// <param name="invalidTargetEnumValues">Invalid source mappings will error if anything maps to the specified target value</param>
	/// <param name="ignoreSourceEnumsStartingWith">Enum values that start with string contained in the hash set will not be checked.</param>
	/// <returns></returns>
	public static Either<string, Unit> TestEnumMapper<TSourceEnum, TTargetEnum>(Func<TSourceEnum, TTargetEnum> mapperMethod, Dictionary<TSourceEnum, TTargetEnum> specifiedMappingsNotCheckedForUniqueness, System.Collections.Generic.HashSet<TTargetEnum> invalidTargetEnumValues, System.Collections.Generic.HashSet<string> ignoreSourceEnumsStartingWith) where TSourceEnum : Enum where TTargetEnum : Enum
	{
		Lst<Mappings<TSourceEnum, TTargetEnum>> results = ProcessAllMappings(mapperMethod);
		return ValidateEnumMappings(results, specifiedMappingsNotCheckedForUniqueness, invalidTargetEnumValues, ignoreSourceEnumsStartingWith);
	}

	private static Either<string, Unit> ValidateEnumMappings<TSourceEnum, TTargetEnum>(Lst<Mappings<TSourceEnum, TTargetEnum>> results, Dictionary<TSourceEnum, TTargetEnum> specifiedMappingsNotCheckedForUniqueness, System.Collections.Generic.HashSet<TTargetEnum> invalidTargetEnumValues, System.Collections.Generic.HashSet<string> ignoreSourceEnumsStartingWith)
		where TSourceEnum : Enum
		where TTargetEnum : Enum
	{
		//We reversed the order here because we want to lookup the target results to see if we have any duplicates
		//If we do have a prior target result we need to know the source that mapped to that result.
		Dictionary<TTargetEnum, TSourceEnum> mappingResults = new Dictionary<TTargetEnum, TSourceEnum>();

		foreach (var item in results)
		{
			if (ignoreSourceEnumsStartingWith.Any(valueToIgnore => item.Source.ToString().StartsWith(valueToIgnore)))
			{
				continue;
			}

			if (specifiedMappingsNotCheckedForUniqueness.TryGetValue(item.Source, out var specifiedTarget))
			{
				//we have a specified mapping, we need to verify the target matches what we expect
				//and if so we will ignore it 

				if (item.Target.Equals(specifiedTarget))
				{
					continue;
				}
				else
				{
					return Left($"Source {item.Source} was expected to be mapped to {specifiedTarget} but was instead mapped to {item.Target}.");
				}
			}

			if (invalidTargetEnumValues.Contains(item.Target))
			{
				return Left($"Source {item.Source} was mapped to {item.Target} which is not a valid mapping result.");
			}

			if (mappingResults.TryGetValue(item.Target, out var source))
			{
				//duplicate mapping
				return Left($"Duplicate Mapping: Source {source} and {item.Source} both mapped to {item.Target}.");
			}
			else
			{
				mappingResults.Add(item.Target, item.Source);
			}
		}
		return Right(Unit.Default);
	}

	private static Lst<Mappings<TSourceEnum, TTargetEnum>> ProcessAllMappings<TSourceEnum, TTargetEnum>(Func<TSourceEnum, TTargetEnum> mapperMethod)
		where TSourceEnum : Enum
		where TTargetEnum : Enum
	{
		Lst<Mappings<TSourceEnum, TTargetEnum>> results = new Lst<Mappings<TSourceEnum, TTargetEnum>>();

		foreach (TSourceEnum item in Enum.GetValues(typeof(TSourceEnum)))
		{
			var mappedResult = mapperMethod.Invoke(item);

			results = results.Add(new Mappings<TSourceEnum, TTargetEnum>(item, mappedResult));
		}

		return results;
	}

	private class Mappings<TSourceEnum, TTargetEnum> where TSourceEnum : Enum where TTargetEnum : Enum
	{
		public Mappings(TSourceEnum source, TTargetEnum target)
		{
			Source = source;
			Target = target;
		}

		public TSourceEnum Source { get; }
		public TTargetEnum Target { get; }
	}
}
