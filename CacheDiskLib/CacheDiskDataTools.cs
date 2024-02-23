using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security;
using System.Security.AccessControl;

namespace CacheDiskLib
{
	internal static class CacheDiskDataTools
	{
		public static void TransferData (string path, string destination, CacheType operationType, bool hiddenDestination, bool replicateAttributes, bool replicateAccessControl, ref List<Exception> ErrorList, bool showOnConsole = false)
		{
			DirectoryInfo PathInfo = new DirectoryInfo(path);
			DirectoryInfo DestinationInfo = new DirectoryInfo(destination);

			IEnumerable<DirectoryInfo> pathDirs = PathInfo.EnumerateDirectories("*", SearchOption.AllDirectories);
			IEnumerable<FileInfo> pathFiles = PathInfo.EnumerateFiles("*", SearchOption.AllDirectories);

			uint totalDirs = (uint)pathDirs.Count();
			uint totalFiles = (uint)pathFiles.Count();
			
			uint totalItems = totalDirs + totalFiles;
			uint actualProcessed = 0;
			
			// Set the Access Control for destination directory:
			if (replicateAccessControl)
			{
				if (OperatingSystem.IsWindows())
				{
					try
					{
						DirectorySecurity pathAccessControle = PathInfo.GetAccessControl();
						DestinationInfo.SetAccessControl(pathAccessControle);
					}
					catch (Exception e)
					{
						ErrorList.Add(e);
					}
				}
			}

			if (!DestinationInfo.Exists)
			{
				DestinationInfo.Create();
			}

			if (showOnConsole)
			{
				CacheDiskConsoleOpOutput.WriteProgress(totalItems, actualProcessed, true);
			}

			// Replicate the directories:
			foreach (DirectoryInfo pathDir in pathDirs)
			{
				try
				{
					string destTmp = pathDir.FullName.Replace(path, destination);

					if (!Directory.Exists(destTmp))
					{
						DirectoryInfo destInfoTmp = Directory.CreateDirectory(destTmp);

						if (destInfoTmp.Exists)
						{
							actualProcessed++;

							// On Windows, try to set the Security Access Control:
							if (replicateAccessControl)
							{
								if (OperatingSystem.IsWindows())
								{
									try
									{
										DirectorySecurity pathDirSecurity = pathDir.GetAccessControl(System.Security.AccessControl.AccessControlSections.All);
										destInfoTmp.SetAccessControl(pathDirSecurity);
									}
									catch (Exception e)
									{
										ErrorList.Add(e);
									}
								}
							}

							// Set the Directory Attributes on Windows OS and UnixMode on Unix systems:
							if (replicateAttributes)
							{
								try
								{
									if (OperatingSystem.IsWindows())
									{
										FileAttributes pathDirAttr = pathDir.Attributes;
										destInfoTmp.Attributes = pathDirAttr;
									}
									else
									{
										UnixFileMode pathDirUnixMode = pathDir.UnixFileMode;
										destInfoTmp.UnixFileMode = pathDirUnixMode;
									}
								}
								catch (Exception e)
								{
									ErrorList.Add(e);
								}
							}
						}
						else
						{
							ErrorList.Add(new Exception($"Fail to create directory ({destInfoTmp.FullName})"));
						}
					}
				}
				catch (Exception e)
				{
					ErrorList.Add(e);
				}

				if (showOnConsole)
				{
					CacheDiskConsoleOpOutput.WriteProgress(totalItems, actualProcessed, true);
				}
			}

			// Copy the files:
			foreach (FileInfo pathFile in pathFiles)
			{
				try
				{
					string destFileTmp = pathFile.FullName.Replace(path, destination);

					if (!File.Exists(destFileTmp))
					{
						FileInfo destFileInfoTmp = pathFile.CopyTo(destFileTmp);

						// Verify if the copy exist:
						if (destFileInfoTmp.Exists)
						{
							actualProcessed++;

							// On Windows try to apply the Security Access Control:
							if (replicateAccessControl)
							{
								if (OperatingSystem.IsWindows())
								{
									try
									{
										FileSecurity pathFileSecurity = pathFile.GetAccessControl(AccessControlSections.All);
										destFileInfoTmp.SetAccessControl(pathFileSecurity);
									}
									catch (Exception e)
									{
										ErrorList.Add(e);
									}
								}
							}

							// Set the file attributes on Windows and on Unix systems set the Unix Mode:
							if (replicateAttributes)
							{
								try
								{
									if (OperatingSystem.IsWindows())
									{
										FileAttributes pathFileAttr = pathFile.Attributes;
										destFileInfoTmp.Attributes = pathFileAttr;
									}
									else
									{
										UnixFileMode pathFileUnixMode = pathFile.UnixFileMode;
										destFileInfoTmp.UnixFileMode = pathFileUnixMode;
									}
								}
								catch (Exception e)
								{
									ErrorList.Add(e);
								}
							}
						}
						else
						{
							ErrorList.Add(new Exception ($"Fail to copy ({pathFile.FullName}) to ({destFileTmp})"));
						}
					}
				}
				catch (Exception e)
				{
					ErrorList.Add(e);
				}

				if (showOnConsole)
				{
					CacheDiskConsoleOpOutput.WriteProgress(totalItems, actualProcessed, true);
				}
			}

			// Remove the items from original path if is a MOVE operation:
			if (operationType == CacheType.MOVE)
			{
				try
				{
					PathInfo.Delete(true);
				}
				catch (Exception e)
				{
					ErrorList.Add(e);
				}
			}

			// Hidden the destination directory:
			if (hiddenDestination)
			{
				DestinationInfo.Attributes &= FileAttributes.Hidden;
			}
		}
	}
}
