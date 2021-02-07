﻿//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;


	/// <summary>
	/// Key modifiers used for hot keys
	/// </summary>
	[Flags]
	internal enum Hotmods
	{
		Alt = 1,
		Control = 2,
		Shift = 4,
		Windows = 8,

		NoRepeat = 0x4000,

		// combinations...

		AltShift = 5,
		ControlAlt = 3,
		ControlAltShift = 7,
		ControlShift = 6,
		WinControlAltShift = 15
	}
}