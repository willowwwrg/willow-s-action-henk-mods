using System.Collections;
using UnityEngine;

[RequireComponent(typeof(GA_Gui))]
[ExecuteInEditMode]
[RequireComponent(typeof(GA_SpecialEvents))]
public class GA_SystemTracker : MonoBehaviour
{
	public static GA_SystemTracker GA_SYSTEMTRACKER;

	public bool UseForSubsequentLevels = true;

	public bool IncludeSystemSpecs = true;

	public bool IncludeSceneChange = true;

	public bool SubmitErrors = true;

	public int MaxErrorCount = 10;

	public bool SubmitErrorStackTrace = true;

	public bool SubmitErrorSystemInfo = true;

	public bool SubmitFpsAverage = true;

	public bool SubmitFpsCritical = true;

	public int FpsCriticalThreshold = 30;

	public int FpsCirticalSubmitInterval = 10;

	public bool GuiEnabled;

	public bool GuiAllowScreenshot;

	public bool ErrorFoldOut = true;

	public void Awake()
	{
		if (Application.isPlaying)
		{
			if (GA_SYSTEMTRACKER != null)
			{
				GA.LogWarning("Destroying dublicate GA_SystemTracker - only one is allowed per scene!");
				Object.Destroy(base.gameObject);
			}
			else
			{
				GA_SYSTEMTRACKER = this;
			}
		}
	}

	public void Start()
	{
		if (!Application.isPlaying || GA_SYSTEMTRACKER != this)
		{
			return;
		}
		if (UseForSubsequentLevels)
		{
			Object.DontDestroyOnLoad(base.gameObject);
		}
		GA_Gui component = GetComponent<GA_Gui>();
		component.GuiAllowScreenshot = GuiAllowScreenshot;
		component.GuiEnabled = GuiEnabled;
		GA.API.Debugging.SubmitErrors = SubmitErrors;
		GA.API.Debugging.SubmitErrorStackTrace = SubmitErrorStackTrace;
		GA.API.Debugging.SubmitErrorSystemInfo = SubmitErrorSystemInfo;
		GA.API.Debugging.MaxErrorCount = MaxErrorCount;
		if (GA.API.Debugging.SubmitErrors)
		{
			LogCallbackHandler.RegisterLogCallback(GA.API.Debugging.HandleLog);
		}
		if (!IncludeSystemSpecs)
		{
			return;
		}
		foreach (Hashtable item in GA.API.GenericInfo.GetGenericInfo(string.Empty))
		{
			GA_Queue.AddItem(item, GA_Submit.CategoryType.GA_Error, stack: false);
		}
	}

	private void OnDestroy()
	{
		if (Application.isPlaying && GA_SYSTEMTRACKER == this)
		{
			GA_SYSTEMTRACKER = null;
		}
	}
}
