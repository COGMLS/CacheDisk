using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace CacheDiskLib
{
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
		public static string GenerateUniqueCacheId(ref CacheIdErrorCodes status)
		{
			try
			{
				DirectoryInfo Registers = new DirectoryInfo(CacheDiskDefaultValues.DefaultCacheDiskData);

				List<string> Ids = new List<string>();

				foreach (FileInfo Register in Registers.EnumerateFiles($"*{CacheDiskDefaultValues.DefaultCacheDiskRegExt}", SearchOption.TopDirectoryOnly))
				{
					Ids.Add(Register.Name);
				}

				string id = CacheIdTools.GenerateCacheId();

				if (Ids.Count > 0)
				{
					int i = 0;
					while (i != Ids.Count - 1)
					{
						// If an ID is already exist generate another and test with all IDs again
						if (Ids[i] == id)
						{
							id = CacheIdTools.GenerateCacheId();
							i = 0;
						}

						i++;
					}
				}

				status = CacheIdErrorCodes.CREATED;

				return id;
			}
			catch (Exception)
			{
				status = CacheIdErrorCodes.FAIL_TO_GENERATE_CACHE_ID;
				throw;
			}
		}

		/// <summary>
		/// Try to find a Cache ID register.
		/// </summary>
		/// <param name="ID">ID that need be found</param>
		/// <returns>Cache ID Error Code Status</returns>
		public static CacheIdErrorCodes GetIdStatus(string ID)
		{
			if (!Regex.IsMatch(ID, "[0-9][0-9][0-9][0-9]-[0-9][0-9][0-9][0-9]-[0-9][0-9][0-9][0-9]-[0-9][0-9][0-9][0-9]-[0-9][0-9][0-9][0-9]"))
			{
				return CacheIdErrorCodes.FAIL_ID_MISS_MATCH;
			}

			try
			{
				DirectoryInfo Registers = new DirectoryInfo(CacheDiskDefaultValues.DefaultCacheDiskData);

				foreach (FileInfo Register in Registers.EnumerateFiles($"*{CacheDiskDefaultValues.DefaultCacheDiskRegExt}", SearchOption.TopDirectoryOnly))
				{
					if (Register.Name.Remove(Register.Name.Length - CacheDiskDefaultValues.DefaultCacheDiskRegExt.Length) == ID)
					{
						return CacheIdErrorCodes.ALREADY_EXIST;
					}
				}

				return CacheIdErrorCodes.CACHE_ID_DOES_NOT_EXIST;
			}
			catch (Exception)
			{
				return CacheIdErrorCodes.FAIL_TO_CHECK_CACHE_ID;
			}
		}
	}

	public static class CacheDataTools
	{
		public static void CheckAppDataDirectory()
		{
			DirectoryInfo CacheDiskDir = new DirectoryInfo(CacheDiskDefaultValues.DefaultCacheDiskAppDataPath);

			if (!CacheDiskDir.Exists)
			{
				CacheDiskDir.Create();
			}

			DirectoryInfo CacheDiskData = new DirectoryInfo(CacheDiskDefaultValues.DefaultCacheDiskData);
			DirectoryInfo CacheDiskLogs = new DirectoryInfo(CacheDiskDefaultValues.DefaultCacheDiskLogs);
			
			if (!CacheDiskLogs.Exists)
			{
				CacheDiskLogs.Create();
			}

			if (!CacheDiskData.Exists)
			{
				CacheDiskData.Create();
			}
		}
	}
}
