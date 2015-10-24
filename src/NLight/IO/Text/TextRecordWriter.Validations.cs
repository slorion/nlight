// Author(s): Sébastien Lorion

using System;

namespace NLight.IO.Text
{
	partial class TextRecordWriter
	{
		/// <summary>
		/// Defines the writer validations.
		/// </summary>
		[Flags]
		protected enum Validations
		{
			/// <summary>
			/// No validation.
			/// </summary>
			None = 0,

			/// <summary>
			/// Validates that the writer is not disposed.
			/// </summary>
			IsNotDisposed = 1
		}
	}
}