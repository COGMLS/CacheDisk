using System;
using System.IO;

namespace CacheDiskLib
{
	internal class Register
	{
		private string SettingsFilePath;
		private FileInfo SettingsFileInfo;
		private FileStream SettingsFileStream;

		private bool SettingsFileOk = false;

		public string Path;
		public string BackupPath;
		public string CacheDiskPath;
		public CacheID Id;
		public CacheType CacheType = CacheType.UNKNOWN;
		public bool ItemCached = false;

		private void ReadRegister()
		{

		}

		private void WriteRegister()
		{

		}

		/// <summary>
		/// Create a Register in Cache Disk LocalAppData
		/// </summary>
		/// <param name="Path"></param>
		/// <param name="Id"></param>
		/// <param name="CachePath"></param>
		public Register (string Path, string Id, string CachePath)
		{
			this.SettingsFilePath = System.IO.Path.Combine(CacheDiskDefaultValues.DefaultCacheDiskData, $"{Id}{CacheDiskDefaultValues.DefaultCacheDiskRegExt}");

			this.SettingsFileInfo = new FileInfo (SettingsFilePath);

			if (!SettingsFileInfo.Exists)
			{
				this.SettingsFileStream = this.SettingsFileInfo.Create();
				this.SettingsFileOk = true;
			}
			else
			{
				throw new Exception("The file already exists!");
			}

			this.Path = Path;
			this.BackupPath = "";
			this.CacheDiskPath = CachePath;
			this.Id = new CacheID();
			this.CacheType = CacheType.MOVE;
		}

		public Register (string Path, string Id, string CachePath, string BackupPath)
		{
			this.SettingsFilePath = System.IO.Path.Combine(CacheDiskDefaultValues.DefaultCacheDiskData, $"{Id}{CacheDiskDefaultValues.DefaultCacheDiskRegExt}");

			this.SettingsFileInfo = new FileInfo (SettingsFilePath);

			if (!SettingsFileInfo.Exists)
			{
				this.SettingsFileStream = this.SettingsFileInfo.Create();
				this.SettingsFileOk = true;
			}
			else
			{
				throw new Exception("The file already exists!");
			}

			this.Path = Path;
			this.BackupPath = BackupPath;
			this.CacheDiskPath = CachePath;
			this.Id = new CacheID();
			this.CacheType = CacheType.COPY;
		}

		public bool IsRegisterOk()
		{
			return this.SettingsFileOk;
		}

		public bool IsCachedItemOk()
		{
			return this.ItemCached;
		}
	}
}
