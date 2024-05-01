using System;
using System.Collections.Generic;
using System.IO;

namespace CacheDiskLib
{
	public class CacheDisk
	{
		private Register? CacheDiskReg = null;
		private List<Exception> ErrorList;
		private bool replicateAccessControl = false;
		private bool replicateFileAttributes = true;
		private bool UseJunction = false;
		private bool ShowConsoleOutput = false;
		private string? link = null;

		public string Path;
		public string BackupPath;
		public string CacheDiskPath;
		public CacheID Id;
		public CacheType CacheType = CacheType.UNKNOWN;
		public CacheStatus ItemCacheStatus = CacheStatus.NOT_CACHED;

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
		/// Resolve the link status.
		/// </summary>
		/// <returns>1 if the Path is a link (Cached directory). 0 if the path is a directory. -1 for any exception or doesn't exist.</returns>
		private int ResolveLink()
		{
			try
			{
				DirectoryInfo tmpLink = new DirectoryInfo(this.Path);

				if (tmpLink.Exists)
				{
					if (tmpLink.Attributes.HasFlag(FileAttributes.Directory) && tmpLink.Attributes.HasFlag(FileAttributes.ReparsePoint))
					{
						this.link = tmpLink.FullName;
						return 1;
					}

					this.link = null;
					return 0;
				}
				else
				{
					this.link = null;
					return -1;
				}
			}
			catch (Exception e)
			{
				this.link = null;
				this.ErrorList.Add(e);
				return -1;
			}
		}

		/// <summary>
		/// Check if the paths used in for Path, CacheDiskPath and BackupPath are ok
		/// </summary>
		/// <param name="isPathOk">Reference variable to Path status</param>
		/// <param name="isCacheDiskPathOk">Reference variable to CacheDiskPath status</param>
		/// <param name="isBackupPathOk">Reference variable to BackupPath status</param>
		/// <param name="testBackupPath">Make test the backup path status or not</param>
		private void CheckPaths(ref bool isPathOk, ref bool isCacheDiskPathOk, ref bool isBackupPathOk, bool testBackupPath = false)
		{
			// Test the path:
			if (isPathOk)
			{
				this.Path = System.IO.Path.GetFullPath(this.Path);
			}
			else
			{
				this.ErrorList.Add(new Exception("Path is not fully qualified!"));
			}

			// Test cache disk path:
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

			// Test the backup path if is not null (with backup support operation):
			if (testBackupPath)
			{
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
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="isPathOk">Reference variable to Path status</param>
		/// <param name="isCacheDiskPathOk">Reference variable to CacheDiskPath status</param>
		/// <param name="isBackupPathOk">Reference variable to BackupPath status</param>
		/// <param name="testBackupPathErrors">Test the backup path erros or not</param>
		private void CheckPathsErrors(ref bool isPathOk, ref bool isCacheDiskPathOk, ref bool isBackupPathOk, bool testBackupPathErrors = false)
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

			if (testBackupPathErrors)
			{
				if (!isBackupPathOk)
				{
					PathsNotOk += $" {this.BackupPath}";
					PathExceptions++;
				}
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

		private void GetDebugInfo()
		{
			this.ErrorList.Add(new Exception("Cache ID Status: " + this.Id.status.ToString()));

			if (this.CacheDiskReg != null)
			{
				if (this.CacheDiskReg.GetRegisterErrors().Count > 0)
				{
					foreach (CacheDiskRegisterErrorCodes e in this.CacheDiskReg.GetRegisterErrors())
					{
						this.ErrorList.Add(new Exception("Register Error Code: " + e.ToString()));
					}
				}
			}
		}

		/// <summary>
		/// Create a Cache Disk object to manage the cached item
		/// </summary>
		/// <param name="Path">Location of the directory to cached it.</param>
		/// <param name="CachePath">Location to store the cached directory. Recommended to store in a faster disk than original location.</param>
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
			bool isBackupPathOk = false;

			// Test the paths:
			this.CheckPaths(ref isPathOk, ref isCacheDiskPathOk, ref isBackupPathOk, false);

			if (isPathOk && isCacheDiskPathOk)
			{
				this.CacheDiskReg = new Register(this.Path, this.Id, this.CacheDiskPath);
				this.CacheType = this.CacheDiskReg.GetCacheType();
				this.Id = this.CacheDiskReg.GetId();
				this.ItemCacheStatus = this.CacheDiskReg.GetCachedStatus();

				if (this.CacheDiskReg == null)
				{
					this.ErrorList.Add(new Exception("Fail to create object register"));
					this.CacheType = CacheType.UNKNOWN;
				}
				else
				{
					this.ItemCacheStatus = this.CacheDiskReg.GetCachedStatus();

					if (this.ItemCacheStatus == CacheStatus.CACHED)
					{
						int resolveLinkStatus = this.ResolveLink();

						switch (resolveLinkStatus)
						{
							case 0:
							{
								this.ErrorList.Add(new Exception("The path is pointing to the original directory. The cache status is incorrect."));
								break;
							}
							case 1:
							{
								break;
							}
							default:
							{
								// The exception is save automatically in ErrorList in ResolveLink method.
								break;
							}
						}
					}
				}
			}
			else
			{
				this.CheckPathsErrors(ref isPathOk, ref isCacheDiskPathOk, ref isBackupPathOk, false);
			}

#if DEBUG
			this.GetDebugInfo();
#endif // !DEBUG
		}

		/// <summary>
		/// Create a Cache Disk object to manage the cached item with backup option support
		/// </summary>
		/// <param name="Path">Location of the directory to cached it.</param>
		/// <param name="CachePath">Location to store the cached directory. Recommended to store in a faster disk than original location.</param>
		/// <param name="BackupPath">Location to store the backup to recover the original data if fail.</param>
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

			// Test the paths:
			this.CheckPaths(ref isPathOk, ref isCacheDiskPathOk, ref isBackupPathOk, true);

			if (isPathOk && isCacheDiskPathOk && isBackupPathOk)
			{
				this.CacheDiskReg = new Register(this.Path, this.Id, this.CacheDiskPath, BackupPath);
				this.CacheType = this.CacheDiskReg.GetCacheType();
				this.Id = this.CacheDiskReg.GetId();
				this.ItemCacheStatus = this.CacheDiskReg.GetCachedStatus();

				if (this.CacheDiskReg == null)
				{
					this.ErrorList.Add(new Exception("Fail to create object register"));
					this.CacheType = CacheType.UNKNOWN;
				}
				else
				{
					this.ItemCacheStatus = this.CacheDiskReg.GetCachedStatus();

					if (this.ItemCacheStatus == CacheStatus.CACHED)
					{
						int resolveLinkStatus = this.ResolveLink();
						
						switch (resolveLinkStatus)
						{
							case 0:
							{
								this.ErrorList.Add(new Exception("The path is pointing to the original directory. The cache status is incorrect."));
								break;
							}
							case 1:
							{
								break;
							}
							default:
							{
								// The exception is save automatically in ErrorList in ResolveLink method.
								break;
							}
						}
					}
				}
			}
			else
			{
				this.CheckPathsErrors(ref isPathOk, ref isCacheDiskPathOk, ref isBackupPathOk, true);
			}

#if DEBUG
			this.GetDebugInfo();
#endif // !DEBUG
		}

		/// <summary>
		/// Create a Cache Disk object to manage the cached item with backup option support
		/// </summary>
		/// <param name="Path">Location of the directory to cached it.</param>
		/// <param name="CachePath">Location to store the cached directory. Recommended to store in a faster disk than original location.</param>
		/// <param name="MakeBackup">Use the Path information to define the name of backup if defined as TRUE</param>
		public CacheDisk(string Path, string CachePath, bool MakeBackup)
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
			bool isBackupPathOk = false;

			if (isPathOk)
			{
				this.Path = System.IO.Path.GetFullPath(this.Path);

				if (MakeBackup)
				{
					this.BackupPath = this.Path + ".cached";
				}
			}

			if (MakeBackup)
			{
				isBackupPathOk = System.IO.Path.IsPathFullyQualified(this.BackupPath);
			}

			this.CheckPaths(ref isPathOk, ref isCacheDiskPathOk, ref isBackupPathOk, MakeBackup);

			if (isPathOk && isCacheDiskPathOk)
			{
				if (MakeBackup)
				{
					this.CacheDiskReg = new Register(this.Path, this.Id, this.CacheDiskPath);
				}
				else
				{
					this.CacheDiskReg = new Register(this.Path, this.Id, this.CacheDiskPath, BackupPath);
				}

				this.CacheType = this.CacheDiskReg.GetCacheType();
				this.Id = this.CacheDiskReg.GetId();
				this.ItemCacheStatus = this.CacheDiskReg.GetCachedStatus();

				if (this.CacheDiskReg == null)
				{
					this.ErrorList.Add(new Exception("Fail to create object register"));
					this.CacheType = CacheType.UNKNOWN;
				}
				else
				{
					this.ItemCacheStatus = this.CacheDiskReg.GetCachedStatus();

					if (this.ItemCacheStatus == CacheStatus.CACHED)
					{
						int resolveLinkStatus = this.ResolveLink();

						switch (resolveLinkStatus)
						{
							case 0:
							{
								this.ErrorList.Add(new Exception("The path is pointing to the original directory. The cache status is incorrect."));
								break;
							}
							case 1:
							{
								break;
							}
							default:
							{
								// The exception is save automatically in ErrorList in ResolveLink method.
								break;
							}
						}
					}
				}
			}
			else
			{
				this.CheckPaths(ref isPathOk, ref isCacheDiskPathOk, ref isBackupPathOk, MakeBackup);
			}

#if DEBUG
			this.GetDebugInfo();
#endif // !DEBUG
		}

		/// <summary>
		/// Get a Cache Disk object to manage the cached item
		/// </summary>
		/// <param name="Id">ID of object that already was created</param>
		public CacheDisk(CacheID Id)
		{
			CacheDataTools.CheckAppDataDirectory();

			this.ErrorList = new List<Exception>();

			//this.Path = Path;
			//this.BackupPath = "";
			//this.CacheDiskPath = CachePath;
			//this.Id = new CacheID();

			this.CacheDiskReg = new Register(Id);

			this.Path = this.CacheDiskReg.GetPath();
			this.CacheDiskPath = this.CacheDiskReg.GetCacheDiskPath();
			this.BackupPath = "";

			if (this.CacheDiskReg.GetBackupPath() != null)
			{
				this.BackupPath = this.CacheDiskReg.GetBackupPath();
			}

			// Test all paths:

			bool isPathOk = System.IO.Path.IsPathFullyQualified(this.Path);
			bool isCacheDiskPathOk = System.IO.Path.IsPathFullyQualified(this.CacheDiskPath);
			bool isBackupPathOk = false;
			bool MakeBackup = false;

			if (this.BackupPath != "")
			{
				isBackupPathOk = System.IO.Path.IsPathFullyQualified(this.CacheDiskPath);
				MakeBackup = true;
			}

			//if (isPathOk)
			//{
			//	this.Path = System.IO.Path.GetFullPath(this.Path);
			//
			//	if (MakeBackup)
			//	{
			//		this.BackupPath = this.Path + ".cached";
			//	}
			//}

			//if (MakeBackup)
			//{
			//	isBackupPathOk = System.IO.Path.IsPathFullyQualified(this.BackupPath);
			//}

			//this.CheckPaths(ref isPathOk, ref isCacheDiskPathOk, ref isBackupPathOk, MakeBackup);

			if (isPathOk && isCacheDiskPathOk && this.CacheDiskReg.IsRegisterOk())
			{
				//if (MakeBackup)
				//{
				//	this.CacheDiskReg = new Register(this.Path, this.Id, this.CacheDiskPath);
				//}
				//else
				//{
				//	this.CacheDiskReg = new Register(this.Path, this.Id, this.CacheDiskPath, BackupPath);
				//}

				this.CacheType = this.CacheDiskReg.GetCacheType();
				//this.Id = this.CacheDiskReg.GetId();
				this.ItemCacheStatus = this.CacheDiskReg.GetCachedStatus();

				if (this.CacheDiskReg == null)
				{
					this.ErrorList.Add(new Exception("Fail to recover object register"));
					this.CacheType = CacheType.UNKNOWN;
				}
				else
				{
					this.ItemCacheStatus = this.CacheDiskReg.GetCachedStatus();

					if (this.ItemCacheStatus == CacheStatus.CACHED)
					{
						int resolveLinkStatus = this.ResolveLink();

						switch (resolveLinkStatus)
						{
							case 0:
							{
								this.ErrorList.Add(new Exception("The path is pointing to the original directory. The cache status is incorrect."));
								break;
							}
							case 1:
							{
								break;
							}
							default:
							{
								// The exception is save automatically in ErrorList in ResolveLink method.
								break;
							}
						}
					}
				}
			}
			else
			{
				this.CheckPaths(ref isPathOk, ref isCacheDiskPathOk, ref isBackupPathOk, MakeBackup);
			}

#if DEBUG
			this.GetDebugInfo();
#endif // !DEBUG
		}

		/// <summary>
		/// Get a Cache Disk object to manage the cached item
		/// </summary>
		/// <param name="IdStr">Use an string ID of object that already was created</param>
		//public CacheDisk(string IdStr)

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
					if (this.CacheType == CacheType.MOVE)
					{
						try
						{
							if (this.ShowConsoleOutput)
							{
								Console.WriteLine($"Caching ({this.Path}) into ({this.CacheDiskPath}) with operation type: {this.CacheType.ToString()}");
							}

							CacheDiskDataTools.TransferData(ItemDirInfo.FullName, CacheDirInfo.FullName, this.CacheType, false, this.replicateFileAttributes, this.replicateAccessControl, ref ErrorList, this.ShowConsoleOutput);
							
							FileSystemInfo link = Directory.CreateSymbolicLink(this.Path, this.CacheDiskPath);
							string? target = link.LinkTarget;
							this.ResolveLink();

							if (target != null)
							{
								if (target == this.CacheDiskPath)
								{
									this.ItemCacheStatus = CacheStatus.CACHED;
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
							this.ItemCacheStatus = CacheStatus.FAIL_TO_CACHE;
							this.ErrorList.Add(e);
							throw this.ReturnLastError();
						}
					}
					else	// Copy Cache operation:
					{
						DirectoryInfo BackupDirInfo = new DirectoryInfo(this.BackupPath);

						// Create the parent backup folder if doesn't exist
						if (BackupDirInfo.Parent != null)
						{
							if (!BackupDirInfo.Parent.Exists)
							{
								try
								{
									BackupDirInfo.Parent.Create();
								}
								catch (Exception e)
								{
									this.ErrorList.Add(e);
								}
							}
						}

						try
						{
							if (this.ShowConsoleOutput)
							{
								Console.WriteLine($"Caching ({this.Path}) into ({this.CacheDiskPath}) with operation type: {this.CacheType.ToString()}");
							}

							// Get the contents to copy into the Cache Disk path
							CacheDiskDataTools.TransferData(ItemDirInfo.FullName, CacheDirInfo.FullName, this.CacheType, false, this.replicateFileAttributes, this.replicateAccessControl, ref this.ErrorList, this.ShowConsoleOutput);

							if (this.ShowConsoleOutput)
							{
								Console.WriteLine($"Making backup of ({this.Path}) into ({this.BackupPath})");
							}

							// Make the backup before create a link file with the same path:
							if (ItemDirInfo.Root.FullName == BackupDirInfo.Root.FullName)
							{
								ItemDirInfo.MoveTo(BackupDirInfo.FullName);
								BackupDirInfo.Refresh();
#if RELEASE
								BackupDirInfo.Attributes &= FileAttributes.Hidden;
#endif // RELEASE
							}
							else	// If the root is not the same use the TransferData function to move the directory
							{
								CacheDiskDataTools.TransferData(ItemDirInfo.FullName, BackupDirInfo.FullName, CacheType.MOVE, true, this.replicateFileAttributes, this.replicateAccessControl, ref this.ErrorList, this.ShowConsoleOutput);
							}

							FileSystemInfo link = Directory.CreateSymbolicLink(this.Path, this.CacheDiskPath);
							string? target = link.LinkTarget;
							this.ResolveLink();

							if (target != null)
							{
								if (target == this.CacheDiskPath)
								{
									this.ItemCacheStatus = CacheStatus.CACHED;
								}
								else
								{
									this.ItemCacheStatus = CacheStatus.FAIL_TO_CACHE;
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
							this.ItemCacheStatus = CacheStatus.FAIL_TO_CACHE;
							this.ErrorList.Add(e);
							Console.WriteLine(e.Message);
							throw this.ReturnLastError();
						}
					}

					// Update the register object:
					if (!this.CacheDiskReg.RefreshInfo(this.ItemCacheStatus))
					{
						this.ErrorList.Add(new Exception("Fail to update the register object"));
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
		public void RestoreCache()
		{
			if (this.CacheDiskReg != null && this.CacheType != CacheType.UNKNOWN)
			{
				if (this.ItemCacheStatus == CacheStatus.CACHED)
				{
					// Try to remove the link and cache directory:
					try
					{
						DirectoryInfo cacheDir = new DirectoryInfo(this.CacheDiskPath);

						// Remove the link:

						if (this.link != null)
						{
							DirectoryInfo tmpLinkInfo = new DirectoryInfo(this.link);
							if (tmpLinkInfo.Exists)
							{
								tmpLinkInfo.Delete();
								this.link = null;
							}
						}

						DirectoryInfo itemDir = new DirectoryInfo(this.Path);

						if (cacheDir.Exists)
						{
							bool failToRestore = false;

							// Try to remove the cache directory:
							try
							{
								if (this.ShowConsoleOutput)
								{
									Console.WriteLine($"Restoring cached item ({this.CacheDiskPath}) into ({this.Path})");
								}

								if (cacheDir.Root.FullName == this.Path)
								{
									cacheDir.MoveTo(this.Path);
								}
								else	// If the root is not the same, move with TransferData function:
								{
									CacheDiskDataTools.TransferData(cacheDir.FullName, itemDir.FullName, CacheType.MOVE, false, this.replicateFileAttributes, this.replicateAccessControl, ref this.ErrorList, this.ShowConsoleOutput);
								}

								// Remove the backup directory if exist:
								if (this.CacheType == CacheType.COPY)
								{
									DirectoryInfo backupDirInfo = new DirectoryInfo(this.BackupPath);

									if (backupDirInfo.Exists)
									{
										backupDirInfo.Delete(true);
									}
								}

								this.ItemCacheStatus = CacheStatus.NOT_CACHED;
							}
							catch (Exception e)
							{
								failToRestore = true;

								this.ItemCacheStatus = CacheStatus.FAIL_TO_RESTORE;
								this.ErrorList.Add(e);
								this.ErrorList.Add(new Exception($"Fail to restore the cached item ({cacheDir.FullName}) on location ({itemDir.FullName})"));

								// If fail to restore the cache, revert to backup
								if (this.CacheType == CacheType.COPY)
								{
									this.RevertCachedItem();
								}
							}
						}
						else
						{
							this.ErrorList.Add(new Exception($"The cached item ({cacheDir.FullName}) is not available to be restored in ({itemDir.FullName})"));
						}
					}
					catch (Exception e)
					{
						this.ItemCacheStatus = CacheStatus.FAIL_TO_RESTORE;
						this.ErrorList.Add(e);
						
						// If the cached item is missing, revert to the backup
						if (this.CacheType == CacheType.COPY)
						{
							this.RevertCachedItem();
						}
					}
				}
				else
				{
					this.ErrorList.Add(new Exception($"Item ({this.CacheDiskPath}) is not cached to be restored to original location ({this.Path})"));
				}

				// Update the register object:
				if (!this.CacheDiskReg.RefreshInfo(this.ItemCacheStatus))
				{
					this.ErrorList.Add(new Exception("Fail to update the register object"));
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
						bool failToRevert = false;

						try
						{
							// Remove the link available in path:
							if (this.link != null)
							{
								DirectoryInfo tmpLinkInfo = new DirectoryInfo(this.link);
								if (tmpLinkInfo.Exists)
								{
									tmpLinkInfo.Delete();
									this.link = null;
								}
							}

							if (this.ShowConsoleOutput)
							{
								Console.WriteLine($"Reverting cached item ({this.CacheDiskPath}) to ({this.BackupPath})");
							}

							// Move the backup folder to original location:
							if (backupDir.Root.FullName == this.Path)
							{
								backupDir.MoveTo(this.Path);
							}
							else
							{
								CacheDiskDataTools.TransferData(backupDir.FullName, this.Path, CacheType.MOVE, false, this.replicateFileAttributes, this.replicateAccessControl, ref this.ErrorList, this.ShowConsoleOutput);
							}
						}
						catch (Exception e)
						{
							failToRevert = true;
							this.ErrorList.Add(e);
						}

						// Remove the cached directory only if the backup was restored with success:
						if (!failToRevert)
						{
							if (cacheDir.Exists)
							{
								cacheDir.Delete(true);
							}

							this.ItemCacheStatus = CacheStatus.NOT_CACHED;
						}
						else
						{
							this.ItemCacheStatus = CacheStatus.FAIL_TO_REVERT;
							this.ErrorList.Add(new Exception($"Fail to revert the backup ({this.BackupPath} to location: {this.Path}. Leaving the cached directory on {this.CacheDiskPath}"));
						}
					}
				}
				else
				{
					this.ErrorList.Add(new Exception("Only in COPY mode is possible revert the cache item"));
				}

				// Update the register object:
				if (!this.CacheDiskReg.RefreshInfo(this.ItemCacheStatus))
				{
					this.ErrorList.Add(new Exception("Fail to update the register object"));
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
		/// Define if the operations will appear into the console
		/// </summary>
		/// <param name="activeOutput">True to define the console output</param>
		public void SetConsoleOutputOperation(bool activeOutput)
		{
			this.ShowConsoleOutput = activeOutput;
		}

		/// <summary>
		/// Define to replicate the files in the operations
		/// </summary>
		/// <param name="replicateFileAttributes"></param>
		public void SetReplicateFileAttributes(bool replicateFileAttributes)
		{
			this.replicateFileAttributes = replicateFileAttributes;
		}

		/// <summary>
		/// Define to replicate the access control to files and directories in cached item.
		/// </summary>
		/// <param name="replicateAccessControl"></param>
		/// <remarks>The user need have rights to apply the access controls</remarks>
		public void SetReplicateAccessControl(bool replicateAccessControl)
		{
			this.replicateAccessControl = replicateAccessControl;
		}

		/// <summary>
		/// Clean the errors registered
		/// </summary>
		public void CleanErrors()
		{
			this.ErrorList.Clear();
		}

		/// <summary>
		/// Remove register file to Cache Disk item from database
		/// </summary>
		/// <remarks>NOTE: This method remove the register file without take action on cache item. Use only when the item is not cached!</remarks>
		public void RemoveConfigCache()
		{
			if (this.CacheDiskReg != null)
			{
				this.CacheDiskReg.RemoveRegister();
			}
			else
			{
				this.ErrorList.Add(new Exception("Register file doesn't exist"));
			}
		}
	}
}
