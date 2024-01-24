using Microsoft.Extensions.Caching.Memory;

namespace DMG.ProviderInvoicing.BL.Utility
{
	/// <summary>
	/// Functionality for caching data into memory for quicker access.
	/// </summary>
	public class MemoryCacheWrapper
		: IDisposable
	{
		// The backing store for the cache data.
		private readonly IMemoryCache _memoryCache;

		// The timeout for the cache.
		private readonly TimeSpan _cacheTimeout;

		/// <summary>
		/// Initializes a new instance of the <see cref="MemoryCacheWrapper" /> class.
		/// </summary>
		/// <param name="memoryCache">The memory cache backing store.</param>
		/// <param name="cacheTimeout">The cache timeout.</param>
		private MemoryCacheWrapper(IMemoryCache memoryCache, TimeSpan cacheTimeout)
		{
			_memoryCache = memoryCache;
			_cacheTimeout = cacheTimeout;
		}

		/// <summary>
		/// Function to create a memory cache wrapper with a timeout in milliseconds.
		/// </summary>
		/// <param name="memoryCache">The memory cache backing store.</param>
		/// <param name="timeoutInMilliseconds">The cache timeout, in milliseconds.</param>
		/// <returns>The memory wrapper object.</returns>
		public static MemoryCacheWrapper CreateWithTimeoutInMilliseconds(IMemoryCache memoryCache, int timeoutInMilliseconds) =>
			new(memoryCache, TimeSpan.FromMilliseconds(timeoutInMilliseconds));

		/// <summary>
		/// Function to create a memory cache wrapper with a timeout in milliseconds.
		/// </summary>
		/// <param name="memoryCacheOptions">The options to apply to the internal memory cache backing store.</param>
		/// <param name="timeoutInMilliseconds">The cache timeout, in milliseconds.</param>
		/// <returns>The memory wrapper object.</returns>
		public static MemoryCacheWrapper CreateWithTimeoutInMilliseconds(MemoryCacheOptions memoryCacheOptions, uint timeoutInMilliseconds) =>
			new(new MemoryCache(memoryCacheOptions), TimeSpan.FromMilliseconds(timeoutInMilliseconds));

		/// <summary>
		/// Function to create a memory cache wrapper with a timeout in milliseconds.
		/// </summary>
		/// <param name="timeoutInMillisecondss">The cache timeout, in milliseconds.</param>
		/// <returns>The memory wrapper object.</returns>
		public static MemoryCacheWrapper CreateWithTimeoutInMilliseconds(uint timeoutInMilliseconds) =>
			new(new MemoryCache(new MemoryCacheOptions()), TimeSpan.FromMilliseconds(timeoutInMilliseconds));

		/// <summary>
		/// Function to create a memory cache wrapper with a timeout in hours.
		/// </summary>
		/// <param name="memoryCache">The memory cache backing store.</param>
		/// <param name = "timeoutInHours" > The cache timeout, in hours.</param>
		/// <returns>The memory wrapper object.</returns>
		public static MemoryCacheWrapper CreateWithTimeoutInHours(IMemoryCache memoryCache, uint timeoutInHours) =>
			new(memoryCache, TimeSpan.FromHours(timeoutInHours));

		/// <summary>
		/// Function to create a memory cache wrapper with a timeout in hours.
		/// </summary>
		/// <param name="memoryCacheOptions">The options to apply to the internal memory cache backing store.</param>
		/// <param name="timeoutInHours">The cache timeout, in hours.</param>
		/// <returns>The memory wrapper object.</returns>
		public static MemoryCacheWrapper CreateWithTimeoutInHours(MemoryCacheOptions memoryCacheOptions, uint timeoutInHours) =>
			new(new MemoryCache(memoryCacheOptions), TimeSpan.FromHours(timeoutInHours));

		/// <summary>
		/// Function to create a memory cache wrapper with a timeout in hours.
		/// </summary>
		/// <param name="timeoutInHours">The cache timeout, in hours.</param>
		/// <returns>The memory wrapper object.</returns>
		public static MemoryCacheWrapper CreateWithTimeoutInHours(uint timeoutInHours) =>
			new(new MemoryCache(new MemoryCacheOptions()), TimeSpan.FromHours(timeoutInHours));

		/// <summary>
		/// Function to retrieve an item from the memory cache.
		/// </summary>
		/// <typeparam name="T">The type of item to retrieve.</typeparam>
		/// <param name="key">The cache key used to look up the item.</param>
		/// <param name="functionToRetrieveValueWhenNotFoundInCache">The function to retrieve value when not found in cache.</param>
		/// <returns>The item requested from the cache.</returns>
		public T GetItem<T>(Guid key, Func<T> functionToRetrieveValueWhenNotFoundInCache) =>
			_memoryCache.GetOrCreate(key, t =>
			{
				t.AbsoluteExpirationRelativeToNow = _cacheTimeout;
				return functionToRetrieveValueWhenNotFoundInCache();
			});

		/// <summary>
		/// Function to retrieve an item from the memory cache.
		/// </summary>
		/// <typeparam name="T">The type of item to retrieve.</typeparam>
		/// <param name="key">The cache key used to look up the item.</param>
		/// <param name="returnValue">The item requested from the cache.</param>
		/// <returns><b>true</b> if the item was found in the cache, <b>false</b> if not.</returns>
		public bool TryGetValue<T>(Guid key, out T returnValue) => _memoryCache.TryGetValue(key, out returnValue);

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose() => _memoryCache.Dispose();
	}
}