using System;
using System.Collections.Generic;
using System.IO;

namespace CacheDiskLib
{
	internal class CacheDisk
	{
		private Register? CacheDiskReg = null;
		private List<Exception> ErrorList;

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
			this.ErrorList = new List<Exception>();

			this.Path = Path;
			this.BackupPath = "";
			this.CacheDiskPath = CachePath;
			this.Id = new CacheID();

			bool isPathOk = System.IO.Path.IsPathFullyQualified(this.Path);
			bool isCacheDiskPathOk = System.IO.Path.IsPathFullyQualified(this.CacheDiskPath);

			if (isPathOk)
			{
				this.Path = System.IO.Path.GetFullPath(this.Path);
			}
			else
			{
				this.ErrorList.Add(new Exception("Path is not fully qualified!"));
			}

			if (isCacheDiskPathOk)
			{
				this.CacheDiskPath = System.IO.Path.GetFullPath(this.CacheDiskPath);

				if (isPathOk)
				{
					DirectoryInfo pathInfo = new DirectoryInfo(this.Path);

					// Treat the cache disk path if only the root directory was send
					if (!this.CacheDiskPath.EndsWith(pathInfo.Name))
					{
						this.CacheDiskPath = System.IO.Path.Combine(this.CacheDiskPath, pathInfo.Name);

						if (Directory.Exists(this.CacheDiskPath))
						{
							// If a path to this location already exist generate a error:
							this.ErrorList.Add(new Exception($"Cache Disk Path already used! ({this.CacheDiskPath})"));
						}
					}
				}
			}
			else
			{
				this.ErrorList.Add(new Exception("CacheDiskPath is not fully qualified!"));
			}

			if (isPathOk && isCacheDiskPathOk)
			{
				this.CacheDiskReg = new Register(this.Path, this.Id, this.CacheDiskPath);

				if (this.CacheDiskReg == null)
				{
					this.ErrorList.Add(new Exception("Fail to create object register"));
					this.CacheType = CacheType.UNKNOWN;
				}
			}
			else
			{
				string PathsNotOk = "";
				short PathExceptions = 0;

				if (!isPathOk)
				{
					PathsNotOk += $" {this.Path}";
					PathExceptions++;
				}

				if (!isCacheDiskPathOk)
				{
					PathsNotOk += $" {this.CacheDiskPath}";
					PathExceptions++;
				}

				if (PathExceptions > 1)
				{
					throw new Exception($"Not all paths are qualified to Cache Disk:{PathsNotOk}");
				}
				else
				{
					throw new Exception($"A path is not qualified to Cache Disk:{PathsNotOk}");
				}
			}

#if DEBUG
			this.ErrorList.Add(new Exception("Cache ID Status: " + this.Id.status.ToString()));

			if (this.CacheDiskReg != null)
			if (this.CacheDiskReg.GetRegisterErrors().Count > 0)
			{
				foreach (CacheDiskRegisterErrorCodes e in this.CacheDiskReg.GetRegisterErrors())
				{
					this.ErrorList.Add(new Exception("Register Error Code: " + e.ToString()));
				}
			}
#endif
		}

		/// <summary>
		/// Create a Cache Disk object to manage the cached item with backup option support
		/// </summary>
		/// <param name="Path"></param>
		/// <param name="CachePath"></param>
		/// <param name="BackupPath"></param>
		public CacheDisk (string Path, string CachePath, string BackupPath)
		{
			this.ErrorList = new List<Exception>();

			this.Path = Path;
			this.BackupPath = BackupPath;
			this.CacheDiskPath = CachePath;
			this.Id = new CacheID();

			bool isPathOk = System.IO.Path.IsPathFullyQualified(this.Path);
			bool isCacheDiskPathOk = System.IO.Path.IsPathFullyQualified(this.CacheDiskPath);
			bool isBackupPathOk = System.IO.Path.IsPathFullyQualified(this.BackupPath);

			if (isPathOk)
			{
				this.Path = System.IO.Path.GetFullPath(this.Path);
			}
			else
			{
				this.ErrorList.Add(new Exception("Path is not fully qualified!"));
			}

			if (isCacheDiskPathOk)
			{
				this.CacheDiskPath = System.IO.Path.GetFullPath(this.CacheDiskPath);

				if (isPathOk)
				{
					DirectoryInfo pathInfo = new DirectoryInfo(this.Path);

					// Treat the cache disk path if only the root directory was send
					if (!this.CacheDiskPath.EndsWith(pathInfo.Name))
					{
						this.CacheDiskPath = System.IO.Path.Combine(this.CacheDiskPath, pathInfo.Name);

						if (Directory.Exists(this.CacheDiskPath))
						{
							// If a path to this location already exist generate a error:
							this.ErrorList.Add(new Exception($"Cache Disk Path already used! ({this.CacheDiskPath})"));
						}
					}
				}
			}
			else
			{
				this.ErrorList.Add(new Exception("CacheDiskPath is not fully qualified!"));
			}

			if (isBackupPathOk)
			{
				this.BackupPath = System.IO.Path.GetFullPath(this.BackupPath);

				if (isPathOk)
				{
					DirectoryInfo pathInfo = new DirectoryInfo(this.Path);

					// Treat the backup path if only the root directory was send
					if (!this.BackupPath.EndsWith(pathInfo.Name + ".cached"))
					{
						this.BackupPath = System.IO.Path.Combine(this.BackupPath, pathInfo.Name + ".cached");

						if (Directory.Exists(this.BackupPath))
						{
							// If a path to this location already exist generate a error:
							this.ErrorList.Add(new Exception($"Backup Path already used! ({this.BackupPath})"));
						}
					}
				}
			}
			else
			{
				this.ErrorList.Add(new Exception("BackupPath is not fully qualified!"));
			}

			if (isPathOk && isCacheDiskPathOk && isBackupPathOk)
			{
				this.CacheDiskReg = new Register(this.Path, this.Id, this.CacheDiskPath, BackupPath);
			}
			else
			{
				string PathsNotOk = "";
				short PathExceptions = 0;

				if (!isPathOk)
				{
					PathsNotOk += $" {this.Path}";
					PathExceptions++;
				}

				if (!isCacheDiskPathOk)
				{
					PathsNotOk += $" {this.CacheDiskPath}";
					PathExceptions++;
				}

				if (!isBackupPathOk)
				{
					PathsNotOk += $" {this.BackupPath}";
					PathExceptions++;
				}

				if (PathExceptions > 1)
				{
					throw new Exception($"Not all paths are qualified to Cache Disk:{PathsNotOk}");
				}
				else
				{
					throw new Exception($"A path is not qualified to Cache Disk:{PathsNotOk}");
				}
			}

#if DEBUG
			this.ErrorList.Add(new Exception("Cache ID Status: " + this.Id.status.ToString()));

			if (this.CacheDiskReg.GetRegisterErrors().Count > 0)
			{
				foreach (CacheDiskRegisterErrorCodes e in this.CacheDiskReg.GetRegisterErrors())
				{
					this.ErrorList.Add(new Exception("Register Error Code: " + e.ToString()));
				}
			}
#endif // !DEBUG
		}

		public Exception[] GetErrorsDetected()
		{
			return this.ErrorList.ToArray();
		}

		public void CacheItem()
		{
			if (this.CacheDiskReg != null && this.CacheType != CacheType.UNKNOWN)
			{
				if (this.CacheDiskReg.IsRegisterOk())
				{
					DirectoryInfo ItemDirInfo = new DirectoryInfo(this.Path);
					DirectoryInfo CacheDirInfo = new DirectoryInfo(this.CacheDiskPath);

					if (CacheDirInfo.Exists)
					{
						throw new Exception($"The cache disk path already exist!");
					}

					if (this.CacheDiskReg.GetCacheType() == CacheType.MOVE)
					{
						ItemDirInfo.MoveTo(CacheDirInfo.FullName);
						FileSystemInfo link = Directory.CreateSymbolicLink(this.Path, this.CacheDiskPath);
						string? target = link.LinkTarget;

						if (target != null)
						{
							if (target == this.CacheDiskPath)
							{
								this.ItemCached = true;
							}
							else
							{
								throw new Exception($"Fail to create a link with successful target to {this.CacheDiskPath}");
							}
						}
						else
						{
							throw new Exception($"Fail to create a link with target to {this.CacheDiskPath} on path {this.Path}");
						}
					}
					else
					{
						DirectoryInfo BackupDirInfo = new DirectoryInfo(this.BackupPath);

						if (BackupDirInfo.Exists)
						{
							throw new Exception($"The backup path already exist!");
						}

						try
						{
							// Get the contents to copy into the Cache Disk path
							IEnumerable<FileSystemInfo> itemFs = ItemDirInfo.EnumerateFileSystemInfos("*", SearchOption.AllDirectories);

							CacheDirInfo.Create();

							foreach (FileSystemInfo fs in itemFs)
							{
								string p = System.IO.Path.Combine(this.CacheDiskPath, fs.FullName.Remove(this.Path.Length));
								
								if (fs.Attributes.HasFlag(FileAttributes.Directory))
								{
									Directory.CreateDirectory(p);
								}
								else
								{
									File.Copy(fs.FullName, p);
								}
							}

							ItemDirInfo.MoveTo(this.BackupPath);
							if (!ItemDirInfo.Attributes.HasFlag(FileAttributes.Hidden))
							{
								ItemDirInfo.Attributes &= FileAttributes.Hidden;
							}

							Directory.CreateSymbolicLink(this.Path, this.CacheDiskPath);
						}
						catch (Exception e)
						{
							Console.WriteLine(e.Message);
							throw;
						}
					}
				}
			}
		}
		
		public void UnCacheItem()
		{

		}

		public void RestoreCachedItem()
		{

		}
	}
}
