// Author(s): Sébastien Lorion

using NLight.Collections.Trees;
using NLight.IO.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace NLight.IO
{
	public static class IOHelper
	{
		private const int DefaultBufferSize = 16 * 1024; // 16KB

		public static async Task Copy(string sourceFilePath, string destinationFilePath, OverwriteMode overwriteMode = OverwriteMode.AlwaysOverwrite, CopyOptions options = CopyOptions.AllowHardLinkCreation, CancellationToken? cancellationToken = null, Action<long, long> progressCallback = null)
		{
			if (string.IsNullOrEmpty(sourceFilePath)) throw new ArgumentNullException(nameof(sourceFilePath));
			if (string.IsNullOrEmpty(destinationFilePath)) throw new ArgumentNullException(nameof(destinationFilePath));

			var ct = cancellationToken ?? CancellationToken.None;
			Directory.CreateDirectory(Path.GetDirectoryName(destinationFilePath));

			if (options.HasFlag(CopyOptions.AllowHardLinkCreation))
			{
				if (sourceFilePath.Length > 3 && destinationFilePath.Length > 3
					&& sourceFilePath[1] == Path.VolumeSeparatorChar && sourceFilePath[2] == Path.PathSeparator
					&& sourceFilePath.Take(3).SequenceEqual(destinationFilePath.Take(3)))
				{
					if (NtfsHelper.CreateHardLink(sourceFilePath, destinationFilePath))
					{
						if (progressCallback != null)
						{
							var length = new FileInfo(sourceFilePath).Length;
							progressCallback(length, length);
						}

						return;
					}
				}
			}

			await Win32CopyEx.Copy(sourceFilePath, destinationFilePath, overwriteMode, options, ct, progressCallback).ConfigureAwait(false);

			ct.ThrowIfCancellationRequested();
		}

		public static async Task Copy(Stream source, Stream destination, int bufferSize = DefaultBufferSize, CancellationToken? cancellationToken = null, Action<long, long> progressCallback = null)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));
			if (destination == null) throw new ArgumentNullException(nameof(destination));
			if (bufferSize <= 0) throw new ArgumentOutOfRangeException(nameof(bufferSize), bufferSize, null);

			var ct = cancellationToken ?? CancellationToken.None;

			if (source.CanSeek)
				bufferSize = Math.Min((int) source.Length, bufferSize);

			var buffer = new byte[bufferSize];

			int currentCount;
			long totalCount = 0;
			while ((currentCount = await source.ReadAsync(buffer, 0, buffer.Length, ct).ConfigureAwait(false)) > 0)
			{
				await destination.WriteAsync(buffer, 0, currentCount, ct).ConfigureAwait(false);
				totalCount += currentCount;

				progressCallback?.Invoke(totalCount, source.Length);
			}
		}

		public static Task Move(string sourceFilePath, string destinationFilePath, OverwriteMode overwriteMode = OverwriteMode.AlwaysOverwrite, CancellationToken? cancellationToken = null, Action<long, long> progressCallback = null)
		{
			return Copy(sourceFilePath, destinationFilePath, overwriteMode, CopyOptions.None, cancellationToken, progressCallback)
				.ContinueWith(t => File.Delete(sourceFilePath), TaskContinuationOptions.OnlyOnRanToCompletion);
		}

		public static void DeleteEmptyDirectories(string root, bool deleteRoot = false)
		{
			if (string.IsNullOrEmpty(root)) throw new ArgumentNullException(nameof(root));

			try
			{
				foreach (var directory in Directory.EnumerateDirectories(root, "*", SearchOption.TopDirectoryOnly))
					DeleteEmptyDirectories(directory, true);

				if (deleteRoot && !Directory.EnumerateFileSystemEntries(root).Any())
					Directory.Delete(root);
			}
			catch (UnauthorizedAccessException) { }
			catch (IOException) { }
		}

		public static bool AreSameFile(string filePathA, string filePathB)
		{
			if (string.IsNullOrEmpty(filePathA)) throw new ArgumentNullException(nameof(filePathA));
			if (string.IsNullOrEmpty(filePathB)) throw new ArgumentNullException(nameof(filePathB));

			return AreSameFile(new FileInfo(filePathA), new FileInfo(filePathB));
		}

		public static bool AreSameFile(FileInfo fileInfoA, FileInfo fileInfoB)
		{
			if (fileInfoA == null) throw new ArgumentNullException(nameof(fileInfoA));
			if (fileInfoB == null) throw new ArgumentNullException(nameof(fileInfoB));

			return
				fileInfoA.Exists
				&& fileInfoB.Exists
				&& fileInfoA.Length == fileInfoB.Length
				&& fileInfoA.LastWriteTimeUtc == fileInfoB.LastWriteTimeUtc
				&& string.Equals(fileInfoA.Name, fileInfoB.Name, StringComparison.CurrentCultureIgnoreCase);
		}

		public static IEnumerable<string> EnumerateDirectories(string path, string searchPattern = "*.*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
		{
			if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));
			if (string.IsNullOrEmpty(searchPattern)) throw new ArgumentNullException(nameof(searchPattern));

			Func<string, IEnumerable<string>> getChildren =
				child => SafeGetFileSystemEnumerable(() => Directory.EnumerateDirectories(child, searchPattern, SearchOption.TopDirectoryOnly));

			if (searchOption == SearchOption.TopDirectoryOnly)
				return getChildren(path);
			else
				return Traversals.LevelOrder(path, getChildren);
		}

		public static IEnumerable<string> EnumerateFiles(string path, string searchPattern = "*.*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
		{
			if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));
			if (string.IsNullOrEmpty(searchPattern)) throw new ArgumentNullException(nameof(searchPattern));

			foreach (var dir in EnumerateDirectories(path, "*.*", searchOption))
				foreach (var file in SafeGetFileSystemEnumerable(() => Directory.EnumerateFiles(dir, searchPattern)))
					yield return file;
		}

		private static IEnumerable<string> SafeGetFileSystemEnumerable(Func<IEnumerable<string>> getEnumerable)
		{
			if (getEnumerable == null) throw new ArgumentNullException(nameof(getEnumerable));

			try { return getEnumerable(); }
			catch (SecurityException) { return Enumerable.Empty<string>(); }
			catch (UnauthorizedAccessException) { return Enumerable.Empty<string>(); }
			catch (IOException) { return Enumerable.Empty<string>(); }
		}
	}
}