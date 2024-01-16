using System;
using System.IO;

namespace CacheDiskLib
{
	internal static class CacheDiskDefaultValues
	{
		public static readonly string DefaultCacheDiskAppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CacheDisk");
		public static readonly string DefaultCacheDiskLogs = Path.Combine(DefaultCacheDiskAppDataPath, "Logs");
		public static readonly string DefaultCacheDiskData = Path.Combine(DefaultCacheDiskAppDataPath, "Data");
		public static readonly string DefaultCacheDiskConfig = Path.Combine(DefaultCacheDiskAppDataPath, "Settings.ini");
		public static readonly string DefaultCacheDiskRegExt = ".ini";
	}
}
