using mset;
using UnityEngine;

public class Sky : mset.Sky
{
	public new static Sky activeSky
	{
		get
		{
			Debug.LogError("Trying to access Sky.activeSky in the global namespace (deprecated script). Use mset.Sky.activeSky instead.");
			return null;
		}
		set
		{
			Debug.LogError("Trying to access Sky.activeSky in the global namespace (deprecated script). Use mset.Sky.activeSky instead.");
		}
	}

	public void OnValidate()
	{
		Debug.LogWarning("Skyshop sky \"" + base.gameObject.name + "\" is using a deprecated script. Please Run the \"Edit->Skyshop->Upgrade Skies\" macro on this scene.");
	}
}
