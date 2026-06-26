using UnityEngine;

namespace WellFired;

[USequencerEvent("Spawn/Spawn Prefab")]
[USequencerFriendlyName("Spawn Prefab")]
[USequencerEventHideDuration]
public class USSpawnPrefabEvent : USEventBase
{
	public GameObject spawnPrefab;

	public Transform spawnTransform;

	public override void FireEvent()
	{
		if (!spawnPrefab)
		{
			Debug.Log("Attempting to spawn a prefab, but you haven't given a prefab to the event from USSpawnPrefabEvent::FireEvent");
		}
		else if ((bool)spawnTransform)
		{
			Object.Instantiate(spawnPrefab, spawnTransform.position, spawnTransform.rotation);
		}
		else
		{
			Object.Instantiate(spawnPrefab, Vector3.zero, Quaternion.identity);
		}
	}

	public override void ProcessEvent(float deltaTime)
	{
	}
}
