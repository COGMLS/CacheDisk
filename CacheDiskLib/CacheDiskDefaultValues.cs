using System;
using System.IO;

namespace CacheDiskLib
{
	public static class CacheDiskDefaultValues
	{
		// Cache Disk Default Paths, files and extensions:

		public static readonly string DefaultCacheDiskAppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CacheDisk");
		public static readonly string DefaultCacheDiskLogs = Path.Combine(DefaultCacheDiskAppDataPath, "Logs");
		public static readonly string DefaultCacheDiskData = Path.Combine(DefaultCacheDiskAppDataPath, "Data");
		public static readonly string DefaultCacheDiskConfig = Path.Combine(DefaultCacheDiskAppDataPath, "Settings.ini");
		public static readonly string DefaultCacheDiskRegExt = ".ini";

		// Cache Disk Register Default Fields:

		public static readonly string RegisterFileField_Path = "Path=";
		public static readonly string RegisterFileField_Id = "Id=";
		public static readonly string RegisterFileField_CacheDiskPath = "CacheDiskPath=";
		public static readonly string RegisterFileField_BackupPath = "BackupPath=";
		public static readonly string RegisterFileField_Type = "Type=";
		public static readonly string RegisterFileField_CacheStatus = "Status=";
	}
}
