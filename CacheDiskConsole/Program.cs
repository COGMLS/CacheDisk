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

			/**
			 * 0 - Normal test Cache and Restore
			 * 1 - Only Cache
			 * 2 - Only Restore
			 * 3 - Revert cache
			 */
			int testType = 0;

			if (args.Length > 0)
			{
				path = args[0];
				destination = args[1];
				testType = int.Parse(args[2]);
			}

			if (args.Length > 3)
			{
				backup = args[3];
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
			
			if (testType == 0 || testType == 1)
			{
				cache.CacheItem();
			}

			if (testType == 0 ||  testType == 2)
			{
				cache.RestoreCache();
			}

			if (testType == 3)
			{
				cache.RevertCachedItem();
			}

			bool remReg = false;
			string? usr = "";

			while (true)
			{
				usr = "";
				Console.Write("\nRemove Cache Reg? (Y/N): ");
				usr = Console.ReadLine();

				if (usr == "Y" || usr == "y")
				{
					remReg = true;
					break;
				}
				else if (usr == "N" || usr == "n")
				{
					break;
				}
				else
				{
					Console.WriteLine($"The entry {usr} is not valid! Try again.");
				}
			}

			if (remReg)
			{
				cache.RemoveConfigCache();
			}
#endif // DEBUG

			Console.WriteLine("End of the test");
		}
	}
}
