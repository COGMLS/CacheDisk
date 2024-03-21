using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

// TODO: Add settings control variable for settings changed

namespace CacheDiskLib
{
	internal class Register
	{
		// Settings file:

		private string SettingsFilePath;
		private FileInfo SettingsFileInfo;
		private FileStream? SettingsFileStream;

		// Controllers:

		private bool SettingsFileOk = false;
		private bool SettingsChanged = false;

		// Error List:

		private List<CacheDiskRegisterErrorCodes> Errors;

		// Internal Register data:

		private string Path;
		private string BackupPath;
		private string CacheDiskPath;
		private CacheID Id;
		private CacheType CacheType = CacheType.UNKNOWN;
		private CacheStatus CacheStatus = CacheStatus.NOT_CACHED;

		// Read the inteire register stream and convert it to text
		private string[] ReadRegStream()
		{
			List<string> RegStr = new List<string>();

			if (this.SettingsFileStream != null)
			{
				List<byte> buffer = new List<byte>();
			
				this.SettingsFileStream.Seek(0, SeekOrigin.Begin);

				// The register file define each configuration by line:

				for (int i = 0; i < this.SettingsFileStream.Length; i++)
				{
					byte[] b = new byte[1];
					this.SettingsFileStream.Read(b, 0, b.Length);

					if (b[0] == '\n')
					{
						string s = new UTF8Encoding(false).GetString(buffer.ToArray());
						RegStr.Add(s);
						buffer.Clear();
					}
					else
					{
						buffer.Add(b[0]);
					}
				}
			}
			else
			{
				this.Errors.Add(CacheDiskRegisterErrorCodes.REGISTER_FILE_NOT_OPENED);
			}

			// Leave the FileStream open for other operations.
			// This function is only responsible to read a opened FileStream and the function caller need be responsible for closing and opening the FileStream.

			return RegStr.ToArray();
		}

		// Write in to the register file stream the value text and insert a new line if does not exist
		private void WriteRegStream(string value)
		{
			// This function write the value on current file pointer position

			if (this.SettingsFileStream != null)
			{
				if (!value.EndsWith('\n'))
				{
					value += '\n';
				}

				byte[] buffer = new UTF8Encoding(false).GetBytes(value);
				this.SettingsFileStream.Write(buffer, 0, buffer.Length);
			}
			else
			{
				this.Errors.Add(CacheDiskRegisterErrorCodes.REGISTER_FILE_NOT_OPENED);
			}
		}

		// Read the register file and apply to the object variables the correct values
		private void ReadRegister()
		{
			if (this.SettingsFileStream != null)
			{
				if (this.SettingsFileStream.CanRead)
				{
					this.SettingsFileStream.Seek(0, SeekOrigin.Begin);

					string[] RegStr = this.ReadRegStream();

					if (RegStr.Length > 0)
					{
						for (int i = 0; i < this.SettingsFileStream.Length; i++)
						{
							string r = RegStr[i];

							// If the new line exist, remove it
							if (r.EndsWith('\n'))
							{
								r = r.Remove(r.Length - 1);
							}

							if (r.StartsWith(CacheDiskDefaultValues.RegisterFileField_Path))
							{
								this.Path = r.Remove(0, CacheDiskDefaultValues.RegisterFileField_Path.Length);
							}

							if (r.StartsWith(CacheDiskDefaultValues.RegisterFileField_Id))
							{
								string Id = r.Remove(0, CacheDiskDefaultValues.RegisterFileField_Id.Length);
								this.Id = new CacheID(Id);
							}
						
							if (r.StartsWith(CacheDiskDefaultValues.RegisterFileField_CacheDiskPath))
							{
								this.CacheDiskPath = r.Remove(0, CacheDiskDefaultValues.RegisterFileField_CacheDiskPath.Length);
							}

							if (r.StartsWith(CacheDiskDefaultValues.RegisterFileField_BackupPath))
							{
								this.BackupPath = r.Remove(0, CacheDiskDefaultValues.RegisterFileField_BackupPath.Length);
							}

							if (r.StartsWith(CacheDiskDefaultValues.RegisterFileField_Type))
							{
								string typeStr = r.Remove(0, CacheDiskDefaultValues.RegisterFileField_Type.Length);
								bool typeDataOk = false;
								int typeInt = -1;

								if (int.TryParse(typeStr, out typeInt))
								{
									switch (typeInt)
									{
										case (int)CacheType.UNKNOWN:
										{
											this.CacheType = CacheType.UNKNOWN;
											typeDataOk = true;
											break;
										}
										case (int)CacheType.COPY:
										{
											this.CacheType = CacheType.COPY;
											typeDataOk = true;
											break;
										}
										case (int)CacheType.MOVE:
										{
											this.CacheType = CacheType.MOVE;
											typeDataOk = true;
											break;
										}
										default:
										{
											break;
										}
									}
								}

								if (!typeDataOk)
								{
									if (this.BackupPath.Length > 0)
									{
										this.CacheType = CacheType.COPY;
									}
									else
									{
										this.CacheType = CacheType.MOVE;
									}
								}
							}

							if (r.StartsWith(CacheDiskDefaultValues.RegisterFileField_CacheStatus))
							{
								string StatusStr = r.Remove(0, CacheDiskDefaultValues.RegisterFileField_CacheStatus.Length);
								bool StatusDataOk = false;
								int StatusInt = -1;

								if (int.TryParse(StatusStr, out StatusInt))
								{
									switch (StatusInt)
									{
										case (int)CacheStatus.NOT_CACHED:
										{
											this.CacheStatus = CacheStatus.NOT_CACHED;
											StatusDataOk = true;
											break;
										}
										case (int)CacheStatus.CACHED:
										{
											this.CacheStatus = CacheStatus.CACHED;
											StatusDataOk = true;
											break;
										}
										case (int)CacheStatus.FAIL_TO_CACHE:
										{
											this.CacheStatus = CacheStatus.FAIL_TO_CACHE;
											StatusDataOk = true;
											break;
										}
										case (int)CacheStatus.FAIL_TO_RESTORE:
										{
											this.CacheStatus = CacheStatus.FAIL_TO_RESTORE;
											StatusDataOk = true;
											break;
										}
										case (int)CacheStatus.FAIL_TO_REVERT:
										{
											this.CacheStatus = CacheStatus.FAIL_TO_REVERT;
											StatusDataOk = true;
											break;
										}
										default:
										{
											this.CacheStatus = CacheStatus.UNKNOWN;
											break;
										}
									}
								}

								if (!StatusDataOk)
								{
									this.CacheStatus = CacheStatus.UNKNOWN;
								}
							}
						}
					}
					else
					{
						this.Errors.Add(CacheDiskRegisterErrorCodes.REGISTER_FILE_EMPTY);
					}
				}
				else
				{
					this.Errors.Add(CacheDiskRegisterErrorCodes.REGISTER_FILE_CANT_BE_READ);
				}
			}
			else
			{
				this.Errors.Add(CacheDiskRegisterErrorCodes.REGISTER_FILE_NOT_OPENED);
			}
		}

		// Write the configurations on register file
		private void WriteRegister(bool verifyDataIntegrity)
		{
			if (this.SettingsFileStream != null)
			{
				if (this.SettingsFileStream.CanWrite)
				{
					this.SettingsFileStream.Seek(0, SeekOrigin.Begin);

					this.WriteRegStream($"{CacheDiskDefaultValues.RegisterFileField_Path}{this.Path}\n");
					this.WriteRegStream($"{CacheDiskDefaultValues.RegisterFileField_Id}{this.Id.ID}\n");
					this.WriteRegStream($"{CacheDiskDefaultValues.RegisterFileField_CacheDiskPath}{this.CacheDiskPath}\n");
					this.WriteRegStream($"{CacheDiskDefaultValues.RegisterFileField_BackupPath}{this.BackupPath}\n");
					this.WriteRegStream($"{CacheDiskDefaultValues.RegisterFileField_Type}{(int)this.CacheType}");
					this.WriteRegStream($"{CacheDiskDefaultValues.RegisterFileField_CacheStatus}{(int)this.CacheStatus}");

					// Verify the recorded data:
					if (verifyDataIntegrity)
					{
						if (this.SettingsFileStream.CanRead)
						{
							this.SettingsFileStream.Seek(0, SeekOrigin.Begin);

							string[] RegStr = this.ReadRegStream();

							bool PathFieldExist = false;
							bool IdFieldExist = false;
							bool CacheDiskPathFieldExist = false;
							bool BackupPathFieldExist = false;
							bool CacheTypeFieldExist = false;
							bool CacheStatusFieldExist = false;

							if (RegStr.Length > 0)
							{
								// Check for each field if the data was recorded correctly
								foreach (string r in RegStr)
								{
									if (r.StartsWith(CacheDiskDefaultValues.RegisterFileField_Path))
									{
										PathFieldExist = true;

										string Path = r.Remove(0, CacheDiskDefaultValues.RegisterFileField_Path.Length);

										if (Path != this.Path)
										{
											this.Errors.Add(CacheDiskRegisterErrorCodes.PATH_NOT_WRITE_CORRECT);
										}
									}

									if (r.StartsWith(CacheDiskDefaultValues.RegisterFileField_Id))
									{
										IdFieldExist = true;

										string Id = r.Remove(0, CacheDiskDefaultValues.RegisterFileField_Id.Length);

										if (Id != this.Id.ID)
										{
											this.Errors.Add(CacheDiskRegisterErrorCodes.CACHE_ID_NOT_WRITE_CORRECT);
										}
									}

									if (r.StartsWith(CacheDiskDefaultValues.RegisterFileField_CacheDiskPath))
									{
										CacheDiskPathFieldExist= true;

										string CacheDiskPath = r.Remove(0, CacheDiskDefaultValues.RegisterFileField_CacheDiskPath.Length);

										if (CacheDiskPath != this.CacheDiskPath)
										{
											this.Errors.Add(CacheDiskRegisterErrorCodes.CACHE_DISK_PATH_NOT_WRITE_CORRECT);
										}
									}

									if (r.StartsWith(CacheDiskDefaultValues.RegisterFileField_BackupPath))
									{
										BackupPathFieldExist= true;

										string BackupPath = r.Remove(0, CacheDiskDefaultValues.RegisterFileField_BackupPath.Length);

										if (this.BackupPath.Length > 0 && this.CacheType == CacheType.COPY)
										{
											if (BackupPath != this.BackupPath)
											{
												this.Errors.Add(CacheDiskRegisterErrorCodes.BACKUP_PATH_NOT_WRITE_CORRECT);
											}
										}
									}

									if (r.StartsWith(CacheDiskDefaultValues.RegisterFileField_Type))
									{
										CacheTypeFieldExist= true;

										string CacheTypeStr = r.Remove(0, CacheDiskDefaultValues.RegisterFileField_Type.Length);
										int CacheTypeInt = -1;

										if (int.TryParse(CacheTypeStr, out CacheTypeInt))
										{
											switch (CacheTypeInt)
											{
												case (int)CacheType.UNKNOWN:
												case (int)CacheType.COPY:
												case (int)CacheType.MOVE:
												{
													break;
												}
												default:
												{
													this.Errors.Add(CacheDiskRegisterErrorCodes.CACHE_TYPE_NOT_WRITE_CORRECT);
													break;
												}
											}
										}
									}

									if (r.StartsWith(CacheDiskDefaultValues.RegisterFileField_CacheStatus))
									{
										CacheStatusFieldExist = true;

										string CacheStatusStr = r.Remove(0, CacheDiskDefaultValues.RegisterFileField_CacheStatus.Length);
										int CacheStatusInt = -1;

										if (int.TryParse(CacheStatusStr, out CacheStatusInt))
										{
											switch (CacheStatusInt)
											{
												case (int)CacheType.UNKNOWN:
												case (int)CacheType.COPY:
												case (int)CacheType.MOVE:
												{
													break;
												}
												default:
												{
													this.Errors.Add(CacheDiskRegisterErrorCodes.CACHE_STATUS_NOT_WRITE_CORRECT);
													break;
												}
											}
										}
									}
								}

								// If one or more properties wasn't found, report the error:

								if (!PathFieldExist)
								{
									this.Errors.Add(CacheDiskRegisterErrorCodes.PATH_NOT_FOUND);
								}

								if (!IdFieldExist)
								{
									this.Errors.Add(CacheDiskRegisterErrorCodes.CACHE_ID_NOT_FOUND);
								}

								if (!CacheDiskPathFieldExist)
								{
									this.Errors.Add(CacheDiskRegisterErrorCodes.CACHE_DISK_PATH_NOT_FOUND);
								}

								if (!BackupPathFieldExist && this.CacheType == CacheType.COPY)
								{
									this.Errors.Add(CacheDiskRegisterErrorCodes.BACKUP_PATH_NOT_FOUND);
								}

								if (!CacheTypeFieldExist)
								{
									this.Errors.Add(CacheDiskRegisterErrorCodes.CACHE_TYPE_NOT_FOUND);
								}

								if (!CacheStatusFieldExist)
								{
									this.Errors.Add(CacheDiskRegisterErrorCodes.CACHE_STATUS_NOT_FOUND);
								}
							}
							else
							{
								this.Errors.Add(CacheDiskRegisterErrorCodes.REGISTER_FILE_EMPTY);
							}
						}
						else
						{
							this.Errors.Add(CacheDiskRegisterErrorCodes.REGISTER_FILE_CANT_CHECK_RECORD_DATA);
						}
					}
				}
				else
				{
					this.Errors.Add(CacheDiskRegisterErrorCodes.REGISTER_FILE_CANT_BE_WRITE);
				}
			}
			else
			{
				this.Errors.Add(CacheDiskRegisterErrorCodes.REGISTER_FILE_NOT_OPENED);
			}
		}

		/// <summary>
		/// Manage the register file and stream to open with parameters and close
		/// </summary>
		/// <param name="open"></param>
		/// <param name="mode"></param>
		/// <param name="useFileAccess"></param>
		/// <param name="access"></param>
		/// <returns>True is was successful. False if an exception happen.</returns>
		private bool MngRegister(bool open, FileMode mode = FileMode.Open, bool useFileAccess = false, FileAccess access = FileAccess.ReadWrite)
		{
			if (open)
			{
				try
				{
					if (this.SettingsFileStream != null)
					{
						return false;
					}

					if (this.SettingsFileInfo.Exists)
					{
						if (useFileAccess)
						{
							this.SettingsFileStream = this.SettingsFileInfo.Open(mode, access);
						}
						else
						{
							this.SettingsFileStream = this.SettingsFileInfo.Open(mode);
						}
					}
					else
					{
						try
						{
							this.SettingsFileStream = this.SettingsFileInfo.Create();
						}
						catch (Exception e)
						{
							Console.WriteLine(e);
							return false;
						}
					}

					return true;
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
					return false;
				}
			}
			else
			{
				try
				{
					if (this.SettingsFileStream == null)
					{
						return false;
					}

					//this.SettingsFileStream.Dispose();
					this.SettingsFileStream.Close();
					this.SettingsFileStream = null;

					return true;
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
					return false;
				}
			}
		}
		
		/// <summary>
		/// Create a Register in Cache Disk LocalAppData
		/// </summary>
		/// <param name="Path"></param>
		/// <param name="Id"></param>
		/// <param name="CachePath"></param>
		public Register (string Path, CacheID Id, string CachePath)
		{
			this.Errors = new List<CacheDiskRegisterErrorCodes>();

			string? settingsFilePathTmp = RegisterTools.FindRegisterByPath(Path);

			if (settingsFilePathTmp == null)
			{
				this.SettingsFilePath = System.IO.Path.Combine(CacheDiskDefaultValues.DefaultCacheDiskData, $"{Id.ID}{CacheDiskDefaultValues.DefaultCacheDiskRegExt}");
				this.SettingsFileInfo = new FileInfo (this.SettingsFilePath);
			}
			else
			{
				this.SettingsFilePath = settingsFilePathTmp;
				this.SettingsFileInfo = new FileInfo (this.SettingsFilePath);
			}

			this.Path = Path;
			this.BackupPath = "";
			this.CacheDiskPath = CachePath;
			this.Id = Id;
			this.CacheType = CacheType.MOVE;

			try
			{
				if (!this.SettingsFileInfo.Exists)
				{
					//this.SettingsFileStream = this.SettingsFileInfo.Create();
					this.MngRegister(true, FileMode.Create);
					this.WriteRegister(true);
					this.MngRegister(false);
					//this.SettingsFileStream.Close();
				}
				else
				{
					//this.SettingsFileStream = this.SettingsFileInfo.Open(FileMode.Open);
					this.MngRegister(true, FileMode.Open);
					this.ReadRegister();
					this.MngRegister(false);
					//this.SettingsFileStream.Close();
				}
			}
			catch (Exception e)
			{
				
			}

			if (this.Id.status != CacheIdErrorCodes.CREATED && this.Id.status != CacheIdErrorCodes.ALREADY_EXIST)
			{
				this.Errors.Add(CacheDiskRegisterErrorCodes.CACHE_ID_NOT_FOUND);
			}

			if (this.Errors.Count == 0)
			{
				this.SettingsFileOk = true;
			}
		}

		/// <summary>
		/// Create a Register in Cache Disk LocalAppData with Backup support
		/// </summary>
		/// <param name="Path"></param>
		/// <param name="Id"></param>
		/// <param name="CachePath"></param>
		/// <param name="BackupPath"></param>
		/// <exception cref="Exception"></exception>
		public Register (string Path, CacheID Id, string CachePath, string BackupPath)
		{
			this.Errors = new List<CacheDiskRegisterErrorCodes>();

			string? settingsFilePathTmp = RegisterTools.FindRegisterByPath(Path);

			if (settingsFilePathTmp == null)
			{
				this.SettingsFilePath = System.IO.Path.Combine(CacheDiskDefaultValues.DefaultCacheDiskData, $"{Id.ID}{CacheDiskDefaultValues.DefaultCacheDiskRegExt}");
				this.SettingsFileInfo = new FileInfo(this.SettingsFilePath);
			}
			else
			{
				this.SettingsFilePath = settingsFilePathTmp;
				this.SettingsFileInfo = new FileInfo(this.SettingsFilePath);
			}

			this.Path = Path;
			this.BackupPath = BackupPath;
			this.CacheDiskPath = CachePath;
			this.Id = Id;
			this.CacheType = CacheType.COPY;

			try
			{
				if (!this.SettingsFileInfo.Exists)
				{
					//this.SettingsFileStream = this.SettingsFileInfo.Create();
					this.MngRegister(true, FileMode.Create);
					this.WriteRegister(true);
					this.MngRegister(false);
					//this.SettingsFileStream.Close();
				}
				else
				{
					//this.SettingsFileStream = this.SettingsFileInfo.Open(FileMode.Open);
					this.MngRegister(true, FileMode.Open);
					this.ReadRegister();
					this.MngRegister(false);
					//this.SettingsFileStream.Close();
				}
			}
			catch (Exception e)
			{

			}

			if (this.Id.status != CacheIdErrorCodes.CREATED && this.Id.status != CacheIdErrorCodes.ALREADY_EXIST)
			{
				this.Errors.Add(CacheDiskRegisterErrorCodes.CACHE_ID_NOT_FOUND);
			}

			if (this.Errors.Count == 0)
			{
				this.SettingsFileOk = true;
			}
		}

		/// <summary>
		/// Recover a register that already exist in LocalAppData using a CacheID data
		/// </summary>
		/// <param name="id"></param>
		public Register (CacheID id)
		{
			this.Errors = new List<CacheDiskRegisterErrorCodes>();

			if (id.status == CacheIdErrorCodes.ALREADY_EXIST)
			{
				this.SettingsFilePath = System.IO.Path.Combine(CacheDiskDefaultValues.DefaultCacheDiskData, $"{id.ID}{CacheDiskDefaultValues.DefaultCacheDiskRegExt}");

				this.SettingsFileInfo = new FileInfo (this.SettingsFilePath);

				if (this.SettingsFileInfo.Exists)
				{
					//this.SettingsFileStream = this.SettingsFileInfo.Open(FileMode.Open);
					this.MngRegister(true, FileMode.Open);
					this.ReadRegister();
					this.MngRegister(false);
					//this.SettingsFileStream.Close();

					if (this.Errors.Count == 0)
					{
						this.SettingsFileOk = true;
					}
				}
				else
				{
					this.Errors.Add(CacheDiskRegisterErrorCodes.REGISTER_FILE_MISSING);
				}
			}
			else
			{
				this.Errors.Add(CacheDiskRegisterErrorCodes.REGISTER_FILE_NOT_FOUND);
			}
		}

		/// <summary>
		/// Get the path to the item
		/// </summary>
		/// <returns></returns>
		public string GetPath()
		{
			return this.Path;
		}

		/// <summary>
		/// Get the path to the backup
		/// </summary>
		/// <returns>Return the path to the backup path if the CacheType is defined as COPY. Otherwise will return null.</returns>
		public string GetBackupPath()
		{
			return this.BackupPath;
		}

		/// <summary>
		/// Get the cached item path
		/// </summary>
		/// <returns></returns>
		public string GetCacheDiskPath()
		{
			return this.CacheDiskPath;
		}

		/// <summary>
		/// Get the CacheID
		/// </summary>
		/// <returns></returns>
		public CacheID GetId()
		{
			return this.Id;
		}

		/// <summary>
		/// Get the type of Cache Item
		/// </summary>
		/// <returns></returns>
		public CacheType GetCacheType()
		{
			return this.CacheType;
		}

		public string GetCacheTypeStr()
		{
			return this.CacheType.ToString();
		}

		/// <summary>
		/// Get the cached item status
		/// </summary>
		/// <returns></returns>
		public CacheStatus GetCachedStatus()
		{
			return this.CacheStatus;
		}

		public string GetCacheStatusStr()
		{
			return this.CacheStatus.ToString();
		}

		/// <summary>
		/// Check if the register data if ok
		/// </summary>
		/// <returns></returns>
		public bool IsRegisterOk()
		{
			return this.SettingsFileOk;
		}

		public bool IsCachedItemOk()
		{
			return this.CacheStatus == CacheStatus.CACHED;
		}

		/// <summary>
		/// Get the error list from register object
		/// </summary>
		/// <returns></returns>
		public List<CacheDiskRegisterErrorCodes> GetRegisterErrors()
		{
			return this.Errors;
		}

		/// <summary>
		/// Refresh the information in register object
		/// </summary>
		/// <returns></returns>
		public bool RefreshInfo()
		{
			try
			{
				//this.SettingsFileStream = this.SettingsFileInfo.Open(FileMode.Open, FileAccess.Read);
				this.MngRegister(true, FileMode.Open, true, FileAccess.Read);
				this.ReadRegister();
				this.MngRegister(false);
				//this.SettingsFileStream.Close();

				return true;
			}
			catch (Exception e)
			{
				this.Errors.Add(CacheDiskRegisterErrorCodes.REGISTER_FILE_CANT_BE_OPEN);
				return false;
			}
		}

		/// <summary>
		/// Refresh the register object information and save it in register file
		/// </summary>
		/// <param name="cacheStatus"></param>
		/// <returns>Return true if successful apply the new information in to object and save them in to the register file.</returns>
		public bool RefreshInfo(CacheStatus cacheStatus)
		{
			CacheStatus _cacheStatus = this.CacheStatus;

			try
			{
				this.CacheStatus = cacheStatus;

				//this.SettingsFileStream = this.SettingsFileInfo.Open(FileMode.Open, FileAccess.Write);
				this.MngRegister(true, FileMode.Open, true, FileAccess.Write);
				this.WriteRegister(true);
				this.MngRegister(false);
				//this.SettingsFileStream.Close();

				return true;
			}
			catch (Exception e)
			{
				this.CacheStatus = _cacheStatus;
				this.Errors.Add(CacheDiskRegisterErrorCodes.REGISTER_FILE_CANT_BE_OPEN);

				return false;
			}
		}

		/// <summary>
		/// Refresh the register object information and save it in register file
		/// </summary>
		/// <param name="path"></param>
		/// <param name="cacheDiskPath"></param>
		/// <param name="backupDiskPath"></param>
		/// <param name="cacheStatus"></param>
		/// <returns>Return true if successful apply the new information in to object and save them in to the register file.</returns>
		public bool RefreshInfo(string path, string cacheDiskPath, string? backupDiskPath, CacheStatus cacheStatus)
		{
			string _path = this.Path;
			string _backupPath = this.BackupPath;
			string _cacheDiskPath = this.CacheDiskPath;
			CacheStatus _cacheStatus = this.CacheStatus;

			try
			{
				this.Path = path;
				this.CacheDiskPath = cacheDiskPath;

				if (backupDiskPath == null)
				{
					this.BackupPath = "";
				}
				else
				{
					this.BackupPath = backupDiskPath;
				}

				this.CacheStatus = cacheStatus;

				//this.SettingsFileStream = this.SettingsFileInfo.Open(FileMode.Open, FileAccess.Write);
				this.MngRegister(true, FileMode.Open, true, FileAccess.Write);
				this.WriteRegister(true);
				this.MngRegister(false);
				//this.SettingsFileStream.Close();

				return true;
			}
			catch (Exception e)
			{
				this.Path = _path;
				this.CacheDiskPath = _cacheDiskPath;
				this.BackupPath = _backupPath;
				this.CacheStatus = _cacheStatus;

				this.Errors.Add(CacheDiskRegisterErrorCodes.REGISTER_FILE_CANT_BE_OPEN);

				return false;
			}
		}

		/// <summary>
		/// Delete the register file
		/// </summary>
		public void RemoveRegister()
		{
			if (this.SettingsFileStream != null)
			{
				try
				{
					if (this.SettingsFileStream.CanRead || this.SettingsFileStream.CanWrite)
					{
						//this.SettingsFileStream.Close();
						this.MngRegister(false);
					}
				}
				catch (Exception e)
				{
					this.Errors.Add(CacheDiskRegisterErrorCodes.REGISTER_FILE_CANT_BE_CLOSE);
				}
			}

			this.SettingsFileInfo.Refresh();

			if (this.SettingsFileInfo.Exists)
			{
				try
				{
					this.SettingsFileInfo.Delete();
				}
				catch (Exception e)
				{
					this.Errors.Add(CacheDiskRegisterErrorCodes.REGISTER_FILE_CANT_BE_DELETED);
				}
			}
			else
			{
				this.Errors.Add(CacheDiskRegisterErrorCodes.REGISTER_FILE_MISSING);
			}
		}
	}
}
