using System;
using System.ComponentModel;
using Rewired.Platforms;
using Rewired.Utils;
using Rewired.Utils.Interfaces;
using UnityEngine;

namespace Rewired;

[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class InputManager : InputManager_Base
{
	protected override void DetectPlatform()
	{
		editorPlatform = EditorPlatform.None;
		platform = Platform.Unknown;
		webplayerPlatform = WebplayerPlatform.None;
		isEditor = false;
		if (SystemInfo.deviceName == null)
		{
			_ = string.Empty;
		}
		if (SystemInfo.deviceModel == null)
		{
			_ = string.Empty;
		}
		platform = Platform.Windows;
	}

	protected override void CheckRecompile()
	{
	}

	protected override string GetFocusedEditorWindowTitle()
	{
		return string.Empty;
	}

	protected override IExternalTools GetExternalTools()
	{
		return new ExternalTools();
	}

	private bool CheckDeviceName(string searchString, string deviceName, string deviceModel)
	{
		if (deviceName.IndexOf(searchString, 0, StringComparison.OrdinalIgnoreCase) < 0)
		{
			return deviceModel.IndexOf(searchString, 0, StringComparison.OrdinalIgnoreCase) >= 0;
		}
		return true;
	}
}
