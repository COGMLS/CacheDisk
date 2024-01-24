using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CacheDiskLib
{
	internal class Register
	{
		// Settings file:

		private string SettingsFilePath;
		private FileInfo SettingsFileInfo;
		private FileStream SettingsFileStream;

		// Controllers:

		private bool SettingsFileOk = false;

		// Error List:

		private List<CacheDiskRegisterErrorCodes> Errors;

		// Internal Register data:

		private string Path;
		private string BackupPath;
		private string CacheDiskPath;
		private CacheID Id;
		private CacheType CacheType = CacheType.UNKNOWN;
		//private bool ItemCached = false;

		// Read the inteire register stream and convert it to text
		private string[] ReadRegStream()
		{
			List<string> RegStr = new List<string>();
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

			// Leave the FileStream open for other operations.
			// This function is only responsible to read a opened FileStream and the function caller need be responsible for closing and opening the FileStream.

			return RegStr.ToArray();
		}

		// Write in to the register file stream the value text and insert a new line if does not exist
		private void WriteRegStream(string value)
		{
			// This function write the value on current file pointer position

			if (!value.EndsWith('\n'))
			{
				value += '\n';
			}

			byte[] buffer = new UTF8Encoding(false).GetBytes(value);
			this.SettingsFileStream.Write(buffer, 0, buffer.Length);
		}

		// Read the register file and apply to the object variables the correct values
		private void ReadRegister()
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
							this.Path = r.Remove(CacheDiskDefaultValues.RegisterFileField_Path.Length);
						}

						if (r.StartsWith(CacheDiskDefaultValues.RegisterFileField_Id))
						{
							string Id = r.Remove(CacheDiskDefaultValues.RegisterFileField_Id.Length);
							this.Id = new CacheID(Id);
						}
						
						if (r.StartsWith(CacheDiskDefaultValues.RegisterFileField_CacheDiskPath))
						{
							this.CacheDiskPath = r.Remove(CacheDiskDefaultValues.RegisterFileField_CacheDiskPath.Length);
						}

						if (r.StartsWith(CacheDiskDefaultValues.RegisterFileField_BackupPath))
						{
							this.BackupPath = r.Remove(CacheDiskDefaultValues.RegisterFileField_BackupPath.Length);
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

		// Write the configurations on register file
		private void WriteRegister(bool verifyDataIntegrity)
		{
			if (this.SettingsFileStream.CanWrite)
			{
				this.SettingsFileStream.Seek(0, SeekOrigin.Begin);

				this.WriteRegStream($"{CacheDiskDefaultValues.RegisterFileField_Path}{this.Path}\n");
				this.WriteRegStream($"{CacheDiskDefaultValues.RegisterFileField_Id}{this.Id.ID}\n");
				this.WriteRegStream($"{CacheDiskDefaultValues.RegisterFileField_CacheDiskPath}{this.CacheDiskPath}\n");
				this.WriteRegStream($"{CacheDiskDefaultValues.RegisterFileField_BackupPath}{this.BackupPath}\n");

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

						if (RegStr.Length > 0)
						{
							// Check for each field if the data was recorded correctly
							foreach (string r in RegStr)
							{
								if (r.StartsWith(CacheDiskDefaultValues.RegisterFileField_Path))
								{
									PathFieldExist = true;

									string Path = r.Remove(CacheDiskDefaultValues.RegisterFileField_Path.Length);

									if (Path != this.Path)
									{
										this.Errors.Add(CacheDiskRegisterErrorCodes.PATH_NOT_WRITE_CORRECT);
									}
								}

								if (r.StartsWith(CacheDiskDefaultValues.RegisterFileField_Id))
								{
									IdFieldExist = true;

									string Id = r.Remove(CacheDiskDefaultValues.RegisterFileField_Id.Length);

									if (Id != this.Id.ID)
									{
										this.Errors.Add(CacheDiskRegisterErrorCodes.CACHE_ID_NOT_WRITE_CORRECT);
									}
								}

								if (r.StartsWith(CacheDiskDefaultValues.RegisterFileField_CacheDiskPath))
								{
									CacheDiskPathFieldExist= true;

									string CacheDiskPath = r.Remove(CacheDiskDefaultValues.RegisterFileField_CacheDiskPath.Length);

									if (CacheDiskPath != this.CacheDiskPath)
									{
										this.Errors.Add(CacheDiskRegisterErrorCodes.CACHE_DISK_PATH_NOT_WRITE_CORRECT);
									}
								}

								if (r.StartsWith(CacheDiskDefaultValues.RegisterFileField_BackupPath))
								{
									BackupPathFieldExist= true;

									string BackupPath = r.Remove(CacheDiskDefaultValues.RegisterFileField_BackupPath.Length);

									if (this.BackupPath.Length > 0 && this.CacheType == CacheType.COPY)
									{
										if (BackupPath != this.BackupPath)
										{
											this.Errors.Add(CacheDiskRegisterErrorCodes.BACKUP_PATH_NOT_WRITE_CORRECT);
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

		/// <summary>
		/// Create a Register in Cache Disk LocalAppData
		/// </summary>
		/// <param name="Path"></param>
		/// <param name="Id"></param>
		/// <param name="CachePath"></param>
		public Register (string Path, CacheID Id, string CachePath)
		{
			this.Errors = new List<CacheDiskRegisterErrorCodes>();

			this.SettingsFilePath = System.IO.Path.Combine(CacheDiskDefaultValues.DefaultCacheDiskData, $"{Id}{CacheDiskDefaultValues.DefaultCacheDiskRegExt}");

			this.SettingsFileInfo = new FileInfo (this.SettingsFilePath);

			this.Path = Path;
			this.BackupPath = "";
			this.CacheDiskPath = CachePath;
			this.Id = Id;
			this.CacheType = CacheType.MOVE;

			if (!this.SettingsFileInfo.Exists)
			{
				this.SettingsFileStream = this.SettingsFileInfo.Create();

				this.WriteRegister(true);

				this.SettingsFileStream.Close();

				if (this.Errors.Count == 0)
				{
					this.SettingsFileOk = true;
				}
			}
			else
			{
				throw new Exception("The register file already exists!");
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

			this.SettingsFilePath = System.IO.Path.Combine(CacheDiskDefaultValues.DefaultCacheDiskData, $"{Id}{CacheDiskDefaultValues.DefaultCacheDiskRegExt}");

			this.SettingsFileInfo = new FileInfo (this.SettingsFilePath);

			this.Path = Path;
			this.BackupPath = BackupPath;
			this.CacheDiskPath = CachePath;
			this.Id = Id;
			this.CacheType = CacheType.COPY;

			if (!this.SettingsFileInfo.Exists)
			{
				this.SettingsFileStream = this.SettingsFileInfo.Create();
				
				this.WriteRegister(true);

				this.SettingsFileStream.Close();

				if (this.Errors.Count == 0)
				{
					this.SettingsFileOk = true;
				}
			}
			else
			{
				throw new Exception("The register file already exists!");
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
					this.SettingsFileStream = this.SettingsFileInfo.Open(FileMode.Open);

					this.ReadRegister();

					this.SettingsFileStream.Close();

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

		public string GetPath()
		{
			return this.Path;
		}

		public string GetBackupPath()
		{
			return this.BackupPath;
		}

		public string GetCacheDiskPath()
		{
			return this.CacheDiskPath;
		}

		public string GetId()
		{
			return this.Id.ID;
		}

		public CacheType GetCacheType()
		{
			return this.CacheType;
		}

		public string GetCacheTypeStr()
		{
			return this.CacheType.ToString();
		}

		//public bool GetItemCached()
		//{
		//	return this.ItemCached;
		//}

		//public string GetItemCachedStr()
		//{
		//	return this.ItemCached.ToString();
		//}

		public bool IsRegisterOk()
		{
			return this.SettingsFileOk;
		}

		//public bool IsCachedItemOk()
		//{
		//	return this.ItemCached;
		//}

		public List<CacheDiskRegisterErrorCodes> GetRegisterErrors()
		{
			return this.Errors;
		}

		public bool RefreshInfo()
		{
			return false;
		}
	}
}
