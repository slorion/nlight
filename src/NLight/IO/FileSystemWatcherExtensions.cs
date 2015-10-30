// Author(s): Sébastien Lorion

using System;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace NLight.IO
{
	public static class FileSystemWatcherExtensions
	{
		public static IObservable<FileSystemEventArgs> ToObservable(this FileSystemWatcher source)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));

			return Observable.FromEvent<FileSystemEventHandler, FileSystemEventArgs>(h => source.Changed += h, h => source.Changed -= h)
				.Merge(Observable.FromEvent<FileSystemEventHandler, FileSystemEventArgs>(h => source.Created += h, h => source.Created -= h))
				.Merge(Observable.FromEvent<FileSystemEventHandler, FileSystemEventArgs>(h => source.Deleted += h, h => source.Deleted -= h))
				.Merge(Observable.FromEvent<RenamedEventHandler, RenamedEventArgs>(h => source.Renamed += h, h => source.Renamed -= h))
				.Merge(
					Observable.FromEvent<ErrorEventHandler, ErrorEventArgs>(h => source.Error += h, h => source.Error -= h)
						.Materialize()
						.Select(n => Notification.CreateOnError<FileSystemEventArgs>(n.Value.GetException()))
						.Dematerialize());
		}
	}
}