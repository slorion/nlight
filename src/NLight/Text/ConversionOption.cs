// Author(s): Sébastien Lorion

namespace NLight.Text
{
	/// <summary>
	/// Specifies the available options when converting a value.
	/// </summary>
	public enum ConversionOption
	{
		/// <summary>
		/// Loose conversion, meaning the converter will try to guess the input format.
		/// </summary>
		Loose,

		/// <summary>
		/// Strict conversion, meaning the converter will use only the specified input formats.
		/// </summary>
		Strict
	}
}