using System;
using System.IO;
using System.Collections.Generic;
using System.Numerics;

namespace CacheDiskLib
{
	public class CacheID
	{
		public readonly string ID;

		public CacheID()
		{
			this.ID = CacheIdTools.GenerateUniqueCacheId();
		}
	}

	public static class CacheIdTools
	{
		// Generate a numeric sequence used for Cache ID
		private static string GenerateCacheId()
		{
			string id = "";
			int maxIdRand = 5;

			Random rand = new Random();

			for (int i = 0; i < maxIdRand; i++)
			{
				int r = rand.Next(0, 9999);
				string rs = "";

				if (r < 10)
				{
					rs = "000" + r.ToString();
				}
				else if (r > 10 && r < 100)
				{
					rs = "00" + r.ToString();
				}
				else if (r > 100 && r < 1000)
				{
					rs = "0" + r.ToString();
				}
				else
				{
					rs = r.ToString();
				}

				if (i == 0)
				{
					id = rs + '-';
				}
				else if (i < maxIdRand - 1 && i != 0)
				{
					id += rs + '-';
				}
				else
				{
					id += rs;
				}
			}

			return id;
		}

		/// <summary>
		/// Create a Unique Cache ID
		/// </summary>
		/// <returns>Cache ID Raw Data</returns>
		public static string GenerateUniqueCacheId ()
		{
			DirectoryInfo Registers = new DirectoryInfo(CacheDiskDefaultValues.DefaultCacheDiskData);

			List<string> Ids = new List<string>();

			foreach (FileInfo Register in Registers.EnumerateFiles($"*{CacheDiskDefaultValues.DefaultCacheDiskRegExt}", SearchOption.TopDirectoryOnly))
			{
				Ids.Add(Register.Name);
			}

			string id = CacheIdTools.GenerateCacheId();
			int i = 0;

			while (i != Ids.Count - 1)
			{
				// If an ID is already exist generate another and test with all IDs again
				if (Ids[i] == id)
				{
					id = CacheIdTools.GenerateCacheId();
					i = 0;
				}
			}

			return id;
		}
	}
}
