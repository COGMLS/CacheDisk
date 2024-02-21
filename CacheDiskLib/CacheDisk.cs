using System;
using System.Collections.Generic;
using System.IO;

namespace CacheDiskLib
{
	public class CacheDisk
	{
		private Register? CacheDiskReg = null;
		private List<Exception> ErrorList;

		public string Path;
		public string BackupPath;
		public string CacheDiskPath;
		public CacheID Id;
		public CacheType CacheType = CacheType.UNKNOWN;
		public bool ItemCached = false;

		private Exception ReturnLastError()
		{
			if (this.ErrorList == null || this.ErrorList.Count == 0)
			{
				return new Exception();
			}
			else
			{
				return this.ErrorList[this.ErrorList.Count - 1];
			}
		}

		/// <summary>
		/// Create a Cache Disk object to manage the cached item
		/// </summary>
		/// <param name="Path"></param>
		/// <param name="CachePath"></param>
		public CacheDisk (string Path, string CachePath)
		{
			CacheDataTools.CheckAppDataDirectory();

			this.ErrorList = new List<Exception>();

			this.Path = Path;
			this.BackupPath = "";
			this.CacheDiskPath = CachePath;
			this.Id = new CacheID();

			// Test all paths:

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
				this.CacheType = CacheType.MOVE;

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
					this.ErrorList.Add(new Exception($"Not all paths are qualified to Cache Disk:{PathsNotOk}"));
					throw this.ReturnLastError();
				}
				else
				{
					this.ErrorList.Add(new Exception($"A path is not qualified to Cache Disk:{PathsNotOk}"));
					throw this.ReturnLastError();
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
			CacheDataTools.CheckAppDataDirectory();

			this.ErrorList = new List<Exception>();

			this.Path = Path;
			this.BackupPath = BackupPath;
			this.CacheDiskPath = CachePath;
			this.Id = new CacheID();

			// Test all paths:

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
				this.CacheType = CacheType.COPY;

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

				if (!isBackupPathOk)
				{
					PathsNotOk += $" {this.BackupPath}";
					PathExceptions++;
				}

				if (PathExceptions > 1)
				{
					this.ErrorList.Add(new Exception($"Not all paths are qualified to Cache Disk:{PathsNotOk}"));
					throw this.ReturnLastError();
				}
				else
				{
					this.ErrorList.Add(new Exception($"A path is not qualified to Cache Disk:{PathsNotOk}"));
					throw this.ReturnLastError();
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

		/// <summary>
		/// Get the error detected
		/// </summary>
		/// <returns>Return an array of exceptions founded</returns>
		public Exception[] GetErrorsDetected()
		{
			return this.ErrorList.ToArray();
		}

		/// <summary>
		/// Make a cache for the item
		/// </summary>
		/// <exception cref="Exception"></exception>
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
						this.ErrorList.Add(new Exception("The cache disk path already exist!"));
						throw this.ReturnLastError();
					}

					// Move cache operation:
					if (this.CacheDiskReg.GetCacheType() == CacheType.MOVE)
					{
						try
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
									this.ErrorList.Add(new Exception($"Fail to create a link with successful target to {this.CacheDiskPath}"));
									throw this.ReturnLastError();
								}
							}
							else
							{
								this.ErrorList.Add(new Exception($"Fail to create a link with target to {this.CacheDiskPath} on path {this.Path}"));
								throw this.ReturnLastError();
							}
						}
						catch (Exception e)
						{
							this.ErrorList.Add(e);
							throw this.ReturnLastError();
						}
					}
					else	// Copy Cache operation:
					{
						DirectoryInfo BackupDirInfo = new DirectoryInfo(this.BackupPath);

						//if (BackupDirInfo.Exists)	// Disabled: If the backup folder exist, use it as a folder to store the backup directory
						//{
						//	this.ErrorList.Add(new Exception("The backup path already exist!");
						//}

						// Create the parent backup folder if doesn't exist
						if (BackupDirInfo.Parent != null)
						{
							if (!BackupDirInfo.Parent.Exists)
							{
								BackupDirInfo.Parent.Create();
							}
						}

						try
						{
							// Get the contents to copy into the Cache Disk path
							IEnumerable<FileSystemInfo> itemFs = ItemDirInfo.EnumerateFileSystemInfos("*", SearchOption.AllDirectories);

							CacheDirInfo.Create();

							// Create all directories entries and copy all files inside the item's path
							foreach (FileSystemInfo fs in itemFs)
							{
								string p = System.IO.Path.Combine(this.CacheDiskPath, fs.FullName.Remove(this.Path.Length));
								//string p = System.IO.Path.Combine(this.CacheDiskPath, fs.FullName.Replace(ItemDirInfo.FullName, ""));
								
								if (fs.Attributes.HasFlag(FileAttributes.Directory))
								{
									_ = Directory.CreateDirectory(p);
								}
								else
								{
									File.Copy(fs.FullName, p);
								}
							}

							ItemDirInfo.MoveTo(this.BackupPath);
							ItemDirInfo.Refresh();

							// Only disable if DEBUG if defined
							if (!ItemDirInfo.Attributes.HasFlag(FileAttributes.Hidden))
							{
#if RELEASE
								ItemDirInfo.Attributes &= FileAttributes.Hidden;
#endif //RELEASE
							}

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
									this.ErrorList.Add(new Exception($"Fail to create a link with successful target to {this.CacheDiskPath}"));
									throw this.ReturnLastError();
								}
							}
							else
							{
								this.ErrorList.Add(new Exception($"Fail to create a link with target to {this.CacheDiskPath} on path {this.Path}"));
								throw this.ReturnLastError();
							}
						}
						catch (Exception e)
						{
							this.ErrorList.Add(e);
							Console.WriteLine(e.Message);
							throw this.ReturnLastError();
						}
					}
				}
			}
			else
			{
				if (this.CacheDiskReg == null && this.CacheType != CacheType.UNKNOWN)
				{
					this.ErrorList.Add(new Exception("Can't cache item without register file"));
				}
				else if (this.CacheDiskReg != null && this.CacheType == CacheType.UNKNOWN)
				{
					this.ErrorList.Add(new Exception("Can't cache item without knowing the cache type"));
				}
				else
				{
					this.ErrorList.Add(new Exception("Can't cache item without cache type and register file"));
				}
			}
		}
		
		/// <summary>
		/// Restore the Cache Item to original path
		/// </summary>
		public void UnCacheItem()
		{
			if (this.CacheDiskReg != null && this.CacheType != CacheType.UNKNOWN)
			{
				if (this.ItemCached)
				{
					// Move operation:
					if (this.CacheType == CacheType.MOVE)
					{
						try
						{
							DirectoryInfo cacheDir = new DirectoryInfo(this.CacheDiskPath);

							// Remove the link:
							if (Directory.Exists(this.Path))
							{
								Directory.Delete(this.Path);
							}

							if (cacheDir.Exists)
							{
								cacheDir.MoveTo(this.Path);
							}
						}
						catch (Exception e)
						{
							this.ErrorList.Add(e);
							throw this.ReturnLastError();
						}
					}
					else	// Copy operation:
					{
						try
						{
							DirectoryInfo cacheDir = new DirectoryInfo(this.CacheDiskPath);
							DirectoryInfo backupDir = new DirectoryInfo(this.BackupPath);

							// Remove the link:
							if (Directory.Exists(this.Path))
							{
								Directory.Delete(this.Path);
							}

							try
							{
								if (cacheDir.Exists)
								{
									cacheDir.MoveTo(this.Path);
								}
							}
							catch (Exception e)
							{
								// If fail in restore the cache item to original path, set as visible the backup.
								if (backupDir.Exists)
								{
									if (backupDir.Attributes.HasFlag(FileAttributes.Hidden))
									{
										backupDir.Attributes |= FileAttributes.Hidden;
									}
								}

								this.ErrorList.Add(e);
								throw this.ReturnLastError();
							}
						}
						catch (Exception e)
						{
							this.ErrorList.Add(e);
							throw this.ReturnLastError();
						}
					}
				}
				else
				{
					this.ErrorList.Add(new Exception($"Item ({this.CacheDiskPath}) is not cached to be restored to original location ({this.Path})"));
				}
			}
			else
			{
				if (this.CacheDiskReg == null && this.CacheType != CacheType.UNKNOWN)
				{
					this.ErrorList.Add(new Exception("Can't remove cache item without register file"));
				}
				else if (this.CacheDiskReg != null && this.CacheType == CacheType.UNKNOWN)
				{
					this.ErrorList.Add(new Exception("Can't remove cache item without knowing the cache type"));
				}
				else
				{
					this.ErrorList.Add(new Exception("Can't remove cache item without cache type and register file"));
				}
			}
		}

		/// <summary>
		/// Discard the Cached Item and restore the backup content.
		/// This function only works when the CacheType is defined as COPY.
		/// </summary>
		public void RevertCachedItem()
		{
			if (this.CacheDiskReg != null && this.CacheType != CacheType.UNKNOWN)
			{
				if (this.CacheType == CacheType.COPY)
				{
					DirectoryInfo cacheDir = new DirectoryInfo(this.CacheDiskPath);
					DirectoryInfo backupDir = new DirectoryInfo(this.BackupPath);

					if (backupDir.Exists)
					{
						if (Directory.Exists(this.Path))
						{
							Directory.Delete(this.Path);
						}

						backupDir.MoveTo(this.Path);

						if (cacheDir.Exists)
						{
							cacheDir.Delete(true);
						}
					}
				}
				else
				{
					this.ErrorList.Add(new Exception("Only in COPY mode is possible revert the cache item"));
				}
			}
			else
			{
				if (this.CacheDiskReg == null && this.CacheType != CacheType.UNKNOWN)
				{
					this.ErrorList.Add(new Exception("Can't remove cache item without register file"));
				}
				else if (this.CacheDiskReg != null && this.CacheType == CacheType.UNKNOWN)
				{
					this.ErrorList.Add(new Exception("Can't remove cache item without knowing the cache type"));
				}
				else
				{
					this.ErrorList.Add(new Exception("Can't remove cache item without cache type and register file"));
				}
			}
		}
	}
}
