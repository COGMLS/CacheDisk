using System;

namespace CacheDiskLib
{
	internal static class CacheDiskConsoleOpOutput
	{
		internal static void WriteProgress(uint total, uint count, bool showItemsProcessed = false, long totalSize = 0, long actualSize = 0, double speed = 0)
		{
			double actualPercentage = ((double)count / (double)total) * 100;

			int c = Console.GetCursorPosition().Left;
			int r = Console.GetCursorPosition().Top;
			int bufferWidth = Console.BufferWidth;

			bufferWidth /= 2;

			int PercentagePerChar = (int)((uint)bufferWidth * actualPercentage / 100);

			Console.SetCursorPosition(0, r);

			for (int i = 0; i < bufferWidth && i < PercentagePerChar; i++)
			{
				//\u2588
				Console.Write('\u25a0');
			}

			Console.SetCursorPosition(bufferWidth + 1, r);

			Console.Write($" {actualPercentage.ToString("N1")}%");

			if (showItemsProcessed)
			{
				Console.Write($" ({count} / {total})");

				if (totalSize > 0 && actualSize >= 0)
				{
					double totalSizeF = totalSize;
					double actualSizeF = actualSize;

					string unitTotalSize = "B";

					if (totalSize >= 1000 && totalSize < 1_000_000)
					{
						unitTotalSize = "KB";
						totalSizeF = totalSize / 1000;
					}
					else if (totalSize >= 1_000_000 && totalSize < 1_000_000_000)
					{
						unitTotalSize = "MB";
						totalSizeF = totalSize / 1_000_000;
					}
					else if (totalSize >= 1_000_000_000 && totalSize < 1_000_000_000_000)
					{
						unitTotalSize = "GB";
						totalSizeF = totalSize / 1_000_000_000;
					}
					else if (totalSize >= 1_000_000_000_000)
					{
						unitTotalSize = "TB";
						totalSizeF = totalSize / 1_000_000_000_000;
					}

					string unitActualSize = "B";

					if (actualSize >= 1000 && actualSize < 1_000_000)
					{
						unitActualSize = "KB";
						actualSizeF = actualSize / 1000;
					}
					else if (actualSize >= 1_000_000 && actualSize < 1_000_000_000)
					{
						unitActualSize = "MB";
						actualSizeF = actualSize / 1_000_000;
					}
					else if (actualSize >= 1_000_000_000 && actualSize < 1_000_000_000_000)
					{
						unitActualSize = "GB";
						actualSizeF = actualSize / 1_000_000_000;
					}
					else if (actualSize >= 1_000_000_000_000)
					{
						unitActualSize = "TB";
						actualSizeF = actualSize / 1_000_000_000_000;
					}

					Console.Write($" ({actualSizeF.ToString("N1")}{unitActualSize} / {totalSizeF.ToString("N1")}{unitTotalSize})");
				}

				if (speed > 0)
				{
					string unit = "B";

					if (speed >= 1000 && speed < 1_000_000)
					{
						unit = "KB";
						speed /= 1000;
					}
					else if (speed >= 1_000_000 && speed < 1_000_000_000)
					{
						unit = "MB";
						speed /= 1_000_000;
					}
					else if (speed >= 1_000_000_000 && speed < 1_000_000_000_000)
					{
						unit = "GB";
						speed /= 1_000_000_000;
					}
					else
					{
						unit = "TB";
						speed /= 1_000_000_000_000;
					}

					Console.Write($" ({speed.ToString("N1")} {unit}/s)");
				}
			}

			c = Console.GetCursorPosition().Left + 1;

			for (; c < Console.BufferWidth; c++)
			{
				Console.Write(' ');
			}

			if (actualPercentage == 100)
			{
				Console.WriteLine();
			}
		}
	}
}
