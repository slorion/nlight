// Author(s): Sébastien Lorion

using System;

namespace NLight.Core
{
	partial class EnumHelper
	{
		private class EnumInfo
		{
			public bool IsFlags;
			public UInt64 AllFlagsSetMask;

			public string[] Names;
			public string[] LowerCaseNames;
			public UInt64[] Values;
		}
	}
}