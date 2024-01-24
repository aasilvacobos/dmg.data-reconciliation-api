using LanguageExt;
using static LanguageExt.Prelude;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMG.ProviderInvoicing.BL.Utility;
public class DualLayerMemoryCacheWrapper : IDisposable
{
	private readonly MemoryCache _memoryCacheShort;
	private readonly MemoryCache _memoryCacheLong;

	private readonly TimeSpan _cacheTimeoutShort;
	private readonly TimeSpan _cacheTimeoutLong;

	private DualLayerMemoryCacheWrapper(MemoryCache memoryCacheShort, MemoryCache memoryCacheLong, TimeSpan cacheTimeoutShort, TimeSpan cacheTimeoutLong)
	{
		_memoryCacheShort = memoryCacheShort;
		_memoryCacheLong = memoryCacheLong;
		_cacheTimeoutShort = cacheTimeoutShort;
		_cacheTimeoutLong = cacheTimeoutLong;
	}

	public static DualLayerMemoryCacheWrapper CreateWithTimeoutInMilliseconds(MemoryCache memoryCacheShort, MemoryCache memoryCacheLong, uint timeoutInMillisecondsShort, uint timeoutInMillisecondsLong) =>
		new DualLayerMemoryCacheWrapper(memoryCacheShort, memoryCacheLong, TimeSpan.FromMilliseconds(timeoutInMillisecondsShort), TimeSpan.FromMilliseconds(timeoutInMillisecondsLong));

	//public static DualLayerMemoryCacheWrapper CreateWithTimeoutInMilliseconds(MemoryCacheOptions memoryCacheOptions, uint timeoutInMillisecondsShort, uint timeoutInMillisecondsLong) =>
	//        new DualLayerMemoryCacheWrapper(new MemoryCache(memoryCacheOptions), new MemoryCache(memoryCacheOptions), TimeSpan.FromMilliseconds(timeoutInMillisecondsShort), TimeSpan.FromMilliseconds(timeoutInMillisecondsLong));

	public static DualLayerMemoryCacheWrapper CreateWithTimeoutInMilliseconds(uint timeoutInMillisecondsShort, uint timeoutInMillisecondsLong) =>
		new DualLayerMemoryCacheWrapper(new MemoryCache(new MemoryCacheOptions()), new MemoryCache(new MemoryCacheOptions()), TimeSpan.FromMilliseconds(timeoutInMillisecondsShort), TimeSpan.FromMilliseconds(timeoutInMillisecondsLong));

	//public static DualLayerMemoryCacheWrapper CreateWithTimeoutInHours(IMemoryCache memoryCacheShort, IMemoryCache memoryCacheLong, uint timeoutInHoursShort, uint timeoutInHoursLong) =>
	//    new DualLayerMemoryCacheWrapper(memoryCacheShort, memoryCacheLong, TimeSpan.FromHours(timeoutInHoursShort), TimeSpan.FromHours(timeoutInHoursLong));

	//public static DualLayerMemoryCacheWrapper CreateWithTimeoutInHours(MemoryCacheOptions memoryCacheOptions, uint timeoutInHoursShort, uint timeoutInHoursLong) =>
	//        new DualLayerMemoryCacheWrapper(new MemoryCache(memoryCacheOptions), new MemoryCache(memoryCacheOptions), TimeSpan.FromHours(timeoutInHoursShort), TimeSpan.FromHours(timeoutInHoursLong));

	public static DualLayerMemoryCacheWrapper CreateWithTimeoutInHours(uint timeoutInHoursShort, uint timeoutInHoursLong) =>
		new DualLayerMemoryCacheWrapper(new MemoryCache(new MemoryCacheOptions()), new MemoryCache(new MemoryCacheOptions()), TimeSpan.FromHours(timeoutInHoursShort), TimeSpan.FromHours(timeoutInHoursLong));

	public Counts GetCounts() =>
		new Counts(_memoryCacheShort.Count, _memoryCacheLong.Count);


	///// <summary>
	///// This appears to be available in .net 7 https://devblogs.microsoft.com/dotnet/announcing-dotnet-7-preview-4/#added-metrics-for-microsoft-extensions-caching
	///// </summary>
	///// <returns></returns>
	//public Sizes GetSizes()
	//{
	//    //this will be available in .net 7
	//    long shortCacheSize = 1;
	//    long longCacheSize = 2;

	//    return new Sizes(shortCacheSize, longCacheSize);
	//}

	public async Task<Either<TError, TReturnType>> GetOrUpdateAsync<TKey, TError, TReturnType>(TKey key, Func<Task<Either<TError, TReturnType>>> functionToRetrieveValueWhenNotFoundInCacheAsync) where TKey : notnull where TReturnType : notnull
	{
		if (_memoryCacheShort.TryGetValue(key, out TReturnType returnValue))
		{
			return Right(returnValue);
		}

		var either = await functionToRetrieveValueWhenNotFoundInCacheAsync();
		return UpdateMemoryCacheOrPullFromLongCache(key, either);
	}

	public Either<TError, TReturnType> GetOrUpdate<TKey, TError, TReturnType>(TKey key, Func<Either<TError, TReturnType>> functionToRetrieveValueWhenNotFoundInCache) where TKey : notnull where TReturnType : notnull
	{
		if (_memoryCacheShort.TryGetValue(key, out TReturnType returnValue))
		{
			return Right(returnValue);
		}

		var either = functionToRetrieveValueWhenNotFoundInCache();
		return UpdateMemoryCacheOrPullFromLongCache(key, either);
	}

	private Either<TError, TReturnType> UpdateMemoryCacheOrPullFromLongCache<TKey, TError, TReturnType>(TKey key, Either<TError, TReturnType> either)
		where TKey : notnull
		where TReturnType : notnull
	{
		return either.Match(
					Right: value =>
					{
						SetValue<TKey>(key, value);
						return Right(value);
					},
					Left: err =>
					{
						Either<TError, TReturnType> innerReturnValue = err;
						if (_memoryCacheLong.TryGetValue(key, out TReturnType storedValue))
						{
							innerReturnValue = Right(storedValue);
						}

						return innerReturnValue;
					});
	}

	private void SetValue<T>(T key, object value)
	{
		_memoryCacheShort.Set(key, value, _cacheTimeoutShort);
		_memoryCacheLong.Set(key, value, _cacheTimeoutLong);
	}

	public void Dispose()
	{
		_memoryCacheShort.Dispose();
		_memoryCacheLong.Dispose();
	}


	public record Counts(int ShortCacheCount, int LongCacheCount);
	//public record Sizes(long ShortCacheSize, long LongCacheSize);
}