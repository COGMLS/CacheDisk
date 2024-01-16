using System;
using System.IO;

namespace CacheDiskLib
{
	internal class CacheDisk
	{
		private Register? CacheDiskReg = null;

		public string Path;
		public string BackupPath;
		public string CacheDiskPath;
		public CacheID Id;
		public CacheType CacheType = CacheType.UNKNOWN;
		public bool ItemCached = false;

		/// <summary>
		/// Create a Cache Disk object to manage the cached item
		/// </summary>
		/// <param name="Path"></param>
		/// <param name="Id"></param>
		/// <param name="CachePath"></param>
		public CacheDisk (string Path, string Id, string CachePath)
		{
			
		}

		/// <summary>
		/// Create a Cache Disk object to manage the cached item
		/// </summary>
		/// <param name="Path"></param>
		/// <param name="Id"></param>
		/// <param name="CachePath"></param>
		/// <param name="BackupPath"></param>
		public CacheDisk (string Path, string Id, string CachePath, string BackupPath)
		{
			
		}
	}
}
