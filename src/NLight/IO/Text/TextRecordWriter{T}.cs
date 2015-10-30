// Author(s): Sébastien Lorion

using NLight.Text;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace NLight.IO.Text
{
	public abstract partial class TextRecordWriter<TColumn>
		: TextRecordWriter, IDisposable
		where TColumn : RecordColumn
	{
		protected TextRecordWriter(TextWriter writer)
		{
#if DEBUG
			_allocStackTrace = new StackTrace();
#endif

			if (writer == null) throw new ArgumentNullException(nameof(writer));

			this.CommentCharacter = DefaultCommentCharacter;
			this.CanDisposeBaseWriter = true;
			this.BaseWriter = writer;
			this.Columns = new RecordColumnCollection<TColumn>();
			this.ValueConverter = new StringValueConverter();
		}

		public TextWriter BaseWriter { get; }
		public bool CanDisposeBaseWriter { get; set; }

		public RecordColumnCollection<TColumn> Columns { get; }
		public int CurrentColumnIndex { get; private set; }

		public StringValueConverter ValueConverter { get; }
		public CultureInfo Culture { get; set; }
		public TrimmingOptions FieldTrimmingOptions { get; set; }

		public char CommentCharacter { get; set; }

		public void WriteField(string value)
		{
			ValidateWriter(Validations.IsNotDisposed);

			if (value == null)
				value = string.Empty;

			WriteFieldCore(this.BaseWriter, value);
			this.CurrentColumnIndex++;
		}

		public void WriteField(object value)
		{
			ValidateWriter(Validations.IsNotDisposed);

			WriteField(this.ValueConverter.ConvertFrom(value, string.Empty, string.Empty, this.Culture));
		}

		protected abstract void WriteFieldCore(TextWriter writer, string value);

		public void WriteRecordStart()
		{
			ValidateWriter(Validations.IsNotDisposed);

			this.CurrentColumnIndex = 0;

			WriteRecordStartCore(this.BaseWriter);
		}

		protected virtual void WriteRecordStartCore(TextWriter writer) { }

		public void WriteRecordEnd()
		{
			ValidateWriter(Validations.IsNotDisposed);

			while (this.CurrentColumnIndex < this.Columns.Count)
				WriteField(string.Empty);

			WriteRecordEndCore(this.BaseWriter);
		}

		protected virtual void WriteRecordEndCore(TextWriter writer)
		{
			Debug.Assert(writer != null);
			writer.WriteLine();
		}

		public void WriteRecord(params string[] fields)
		{
			WriteRecord((IEnumerable<string>) fields);
		}

		public void WriteRecord(params object[] fields)
		{
			WriteRecord((IEnumerable) fields);
		}

		public void WriteRecord(Array record)
		{
			WriteRecord((IEnumerable) record);
		}

		public void WriteRecord(IEnumerable record)
		{
			if (record == null)
				WriteRecord((IEnumerable<string>) null);
			else
				WriteRecord(GetStringEnumerable(record));
		}

		public void WriteRecord(IEnumerable<string> record)
		{
			ValidateWriter(Validations.IsNotDisposed);

			if (record == null)
				return;

			WriteRecordStart();

			foreach (string field in record)
				WriteField(field);

			WriteRecordEnd();
		}

		public void WriteComment(string comment)
		{
			ValidateWriter(Validations.IsNotDisposed);

			this.BaseWriter.Write(this.CommentCharacter);
			this.BaseWriter.WriteLine(comment);
		}

		private IEnumerable<string> GetStringEnumerable(IEnumerable record)
		{
			if (record == null)
				yield break;

			foreach (object field in record)
				yield return this.ValueConverter.ConvertFrom(field, string.Empty);
		}

		protected void ValidateWriter(Validations validations)
		{
			if ((validations & Validations.IsNotDisposed) == Validations.IsNotDisposed && this.IsDisposed)
				throw new ObjectDisposedException(null);
		}

		#region IDisposable Members

		private readonly StackTrace _allocStackTrace;

		public bool IsDisposed { get; private set; }

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!this.IsDisposed)
			{
				try
				{
					if (disposing)
					{
						if (this.BaseWriter != null && this.CanDisposeBaseWriter)
							this.BaseWriter.Dispose();
					}
				}
				catch (Exception ex) when (!disposing)
				{
					Log.Source.TraceEvent(TraceEventType.Error, 0, Resources.LogMessages.Shared_ExceptionDuringFinalization, ex);
				}
				finally
				{
					this.IsDisposed = true;
				}
			}
		}

		~TextRecordWriter()
		{
			Log.Source.TraceEvent(TraceEventType.Warning, 0, Resources.LogMessages.Shared_InstanceNotDisposedCorrectly, _allocStackTrace);
			Dispose(false);
		}

		#endregion
	}
}