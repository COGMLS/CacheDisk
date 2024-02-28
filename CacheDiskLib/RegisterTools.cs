using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheDiskLib
{
	internal static class RegisterTools
	{
		/// <summary>
		/// Register Tool to find a existent register file by path that will be cached or already is.
		/// </summary>
		/// <param name="path"></param>
		/// <returns>Return the register file full path if found. Otherwise will return null.</returns>
		public static string? FindRegisterByPath (string path)
		{
			string? registerPath = null;

			DirectoryInfo regDataDir = new DirectoryInfo(CacheDiskDefaultValues.DefaultCacheDiskData);

			if (regDataDir.Exists)
			{
				FileInfo[] regs = regDataDir.GetFiles($"*{CacheDiskDefaultValues.DefaultCacheDiskRegExt}", SearchOption.TopDirectoryOnly);

				if (regs.Length > 0)
				{
					bool foundRegister = false;

					for (int i = 0; i < regs.Length; i++)
					{
						try
						{
							FileStream fs = regs[i].OpenRead();

							if (fs.CanRead)
							{
								List<string> regStr = new List<string>();
								List<byte> buffer = new List<byte>();

								// Read the file and try to find the Path= sector:
								for (int j = 0; j < fs.Length; j++)
								{
									byte[] b = new byte[1];
									fs.Read(b, 0, b.Length);

									if (b[0] == '\n')
									{
										string s = new UTF8Encoding(false).GetString(buffer.ToArray());

										// Compare with path variable and close the file if found.
										if (s.StartsWith(CacheDiskDefaultValues.RegisterFileField_Path))
										{
											s = s.Remove(0, CacheDiskDefaultValues.RegisterFileField_Path.Length);

											if (s.ToLower() == path.ToLower())
											{
												registerPath = regs[i].FullName;
												foundRegister = true;
											}

											fs.Close();
											break;
										}
									}
									else
									{
										buffer.Add(b[0]);
									}
								}
							}
						}
						catch (Exception e)
						{
#if DEBUG
							Console.WriteLine(e.Message);
#endif // !DEBUG
							// Do not throw any fail in this moment.
						}

						// Stop looking in other files:
						if (foundRegister)
						{
							break;
						}
					}
				}
			}

			return registerPath;
		}
	}
}
