using System;
using CacheDiskLib;

namespace CacheDiskConsole
{
	internal class Program
	{
		static void Main (string[] args)
		{
			Console.WriteLine("\nCache Disk Console");
			Console.WriteLine("---------------------------------------------------------");

			CacheDisk cache = new CacheDisk("F:\\DirA", "F:\\CacheTemp");

			cache.CacheItem();

			cache.UnCacheItem();

			Console.WriteLine("End of the test");
		}
	}
}
