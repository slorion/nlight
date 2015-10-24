// Author(s): Sébastien Lorion

using System;

namespace NLight.IO.Text
{
	partial class TextRecordReader
	{
		/// <summary>
		/// Defines the reader validations.
		/// </summary>
		[Flags]
		protected enum Validations
		{
			/// <summary>
			/// No validation.
			/// </summary>
			None = 0,

			/// <summary>
			/// Validates that the reader is not disposed.
			/// </summary>
			IsNotDisposed = 1,

			/// <summary>
			/// Validates that the reader has a current record (first validates that the reader is not disposed).
			/// </summary>
			HasCurrentRecord = 2
		}
	}
}