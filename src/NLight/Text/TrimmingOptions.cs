// Author(s): Sébastien Lorion

using System;

namespace NLight.Text
{
	/// <summary>
	/// Specifies the options to be used when trimming a string.
	/// </summary>
	[Flags]
	public enum TrimmingOptions
	{
		/// <summary>
		/// No trimming.
		/// </summary>
		None = 0,

		/// <summary>
		/// Trimming is done at the start of the string only.
		/// </summary>
		Start = 1,

		/// <summary>
		/// Trimming is done at the end of the string only.
		/// </summary>
		End = 2,

		/// <summary>
		/// Trimming is done at both the start and the end of the string.
		/// </summary>
		Both = Start | End
	}
}