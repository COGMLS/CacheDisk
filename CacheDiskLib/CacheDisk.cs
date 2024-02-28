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
		private FileSystemInfo? link = null;

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
			{
				if (this.CacheDiskReg.GetRegisterErrors().Count > 0)
				{
					foreach (CacheDiskRegisterErrorCodes e in this.CacheDiskReg.GetRegisterErrors())
					{
						this.ErrorList.Add(new Exception("Register Error Code: " + e.ToString()));
					}
				}
			}
#endif
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

			if (isPathOk)
			{
				this.Path = System.IO.Path.GetFullPath(this.Path);

				if (MakeBackup)
				{
					this.BackupPath = this.Path + ".cached";
				}
			}
			else
			{
				this.ErrorList.Add(new Exception("Path is not fully qualified!"));
			}

			bool isBackupPathOk = System.IO.Path.IsPathFullyQualified(this.BackupPath);

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
			else
			{
				this.ErrorList.Add(new Exception("BackupPath is not fully qualified!"));
			}

			if (isPathOk && isCacheDiskPathOk)
			{
				if (MakeBackup)
				{
					this.CacheDiskReg = new Register(this.Path, this.Id, this.CacheDiskPath);
					this.CacheType = CacheType.MOVE;
				}
				else
				{
					this.CacheDiskReg = new Register(this.Path, this.Id, this.CacheDiskPath, BackupPath);
					this.CacheType = CacheType.COPY;
				}

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

				if (!isBackupPathOk && MakeBackup)
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
					if (this.CacheType == CacheType.MOVE)
					{
						try
						{
							if (this.ShowConsoleOutput)
							{
								Console.WriteLine($"Caching ({this.Path}) into ({this.CacheDiskPath}) with operation type: {this.CacheType.ToString()}");
							}

							CacheDiskDataTools.TransferData(ItemDirInfo.FullName, CacheDirInfo.FullName, this.CacheType, false, this.replicateFileAttributes, this.replicateAccessControl, ref ErrorList, this.ShowConsoleOutput);
							
							this.link = Directory.CreateSymbolicLink(this.Path, this.CacheDiskPath);
							string? target = this.link.LinkTarget;

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

							this.link = Directory.CreateSymbolicLink(this.Path, this.CacheDiskPath);
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
		public void RestoreCache()
		{
			if (this.CacheDiskReg != null && this.CacheType != CacheType.UNKNOWN)
			{
				if (this.ItemCached)
				{
					try
					{
						DirectoryInfo cacheDir = new DirectoryInfo(this.CacheDiskPath);

						// Remove the link:

						if (this.link != null)
						{
							if (this.link.Exists)
							{
								this.link.Delete();
								this.link = null;
							}
						}

						DirectoryInfo itemDir = new DirectoryInfo(this.Path);

						if (cacheDir.Exists)
						{
							bool failToRestore = false;

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
							}
							catch (Exception e)
							{
								failToRestore = true;

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
							if (Directory.Exists(this.Path))
							{
								Directory.Delete(this.Path);
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
						}
						else
						{
							this.ErrorList.Add(new Exception($"Fail to revert the backup ({this.BackupPath} to location: {this.Path}. Leaving the cached directory on {this.CacheDiskPath}"));
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

		public void SetConsoleOutputOperation(bool activeOutput)
		{
			this.ShowConsoleOutput = activeOutput;
		}

		public void SetReplicateFileAttributes(bool replicateFileAttributes)
		{
			this.replicateFileAttributes = replicateFileAttributes;
		}

		public void SetReplicateAccessControl(bool replicateAccessControl)
		{
			this.replicateAccessControl = replicateAccessControl;
		}

		public Exception[] GetErrors()
		{
			return this.ErrorList.ToArray();
		}

		public void CleanErrors()
		{
			this.ErrorList.Clear();
		}
	}
}
