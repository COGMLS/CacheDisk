using System;
using System.IO;
using CacheDiskLib;

namespace CacheDiskConsole
{
	internal class Program
	{
		static void Main (string[] args)
		{
			Console.WriteLine("\nCache Disk Console");
			Console.WriteLine("---------------------------------------------------------");

#if DEBUG
			string path = "";
			string destination = "";
			string backup = "";
			bool useBackup = false;

			if (args.Length > 0)
			{
				path = args[0];
				destination = args[1];
			}

			if (args.Length > 2)
			{
				backup = args[2];
				useBackup = true;
			}

			//Tests.OpenDirectories(path, destination);

			CacheDisk cache;

			if (useBackup)
			{
				cache = new CacheDisk(path, destination, backup);
			}
			else
			{
				cache = new CacheDisk(path, destination);
			}

			cache.SetConsoleOutputOperation(true);

			cache.CacheItem();

			cache.RestoreCache();

			bool remReg = false;
			string? usr = "";

			while ((usr != "Y" || usr != "y") && (usr != "N" || usr != "n"))
			{
				usr = "";
				Console.Write("Remove Cache Reg? (Y/N): ");
				usr = Console.ReadLine();
			}

			if (remReg)
			{
				cache.RemoveCache();
			}
#endif // DEBUG

			Console.WriteLine("End of the test");
		}
	}
}
