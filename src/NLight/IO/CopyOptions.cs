// Author(s): Sébastien Lorion

using System;

namespace NLight.IO
{
	[Flags]
	public enum CopyOptions
	{
		None = 0x0,
		DisableBuffering = 0x1,
		AllowHardLinkCreation = 0x2
	}
}