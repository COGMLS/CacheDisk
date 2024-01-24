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
		/// <param name="CachePath"></param>
		public CacheDisk (string Path, string CachePath)
		{
			this.Path = Path;
			this.BackupPath = "";
			this.CacheDiskPath = CachePath;
			this.Id = new CacheID();

			this.CacheDiskReg = new Register(this.Path, this.Id, this.CacheDiskPath);
		}

		/// <summary>
		/// Create a Cache Disk object to manage the cached item with backup option support
		/// </summary>
		/// <param name="Path"></param>
		/// <param name="CachePath"></param>
		/// <param name="BackupPath"></param>
		public CacheDisk (string Path, string CachePath, string BackupPath)
		{
			this.Path = Path;
			this.BackupPath = BackupPath;
			this.CacheDiskPath = CachePath;
			this.Id = new CacheID();

			this.CacheDiskReg = new Register(this.Path, this.Id, this.CacheDiskPath, BackupPath);
		}
	}
}
