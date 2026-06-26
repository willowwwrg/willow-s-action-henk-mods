using UnityEngine;

[ExecuteInEditMode]
public class DebugNormalsInEditmode : MonoBehaviour
{
	private CombineChildrenAFS cc;

	private void Update()
	{
		if (Application.isPlaying)
		{
			return;
		}
		cc = GetComponent<CombineChildrenAFS>();
		Component[] componentsInChildren = GetComponentsInChildren(typeof(MeshFilter));
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Vector3 position = componentsInChildren[i].renderer.transform.position;
			if (cc.GroundMaxDistance < 0f)
			{
				cc.GroundMaxDistance = 0.01f;
			}
			if (Physics.Raycast(position + Vector3.up * cc.GroundMaxDistance * 0.5f, Vector3.down, out var hitInfo, cc.GroundMaxDistance))
			{
				Debug.DrawLine(position + Vector3.up * cc.GroundMaxDistance * 0.5f, position - cc.GroundMaxDistance * Vector3.up * 0.5f, Color.green, 0.1f, depthTest: false);
				Debug.DrawLine(position, position + 1f * hitInfo.normal, Color.red, 0.1f, depthTest: false);
				if (!cc.UnderlayingTerrain)
				{
					continue;
				}
				Vector3 vector = (hitInfo.point - cc.UnderlayingTerrain.transform.position) / cc.UnderlayingTerrain.terrainData.size.x;
				if (hitInfo.transform.gameObject.name == cc.UnderlayingTerrain.name)
				{
					if (cc.debugNormals)
					{
						Debug.DrawLine(position, position + cc.UnderlayingTerrain.terrainData.GetInterpolatedNormal(vector.x, vector.z), Color.blue, 5f, depthTest: false);
					}
					hitInfo.normal = cc.UnderlayingTerrain.terrainData.GetInterpolatedNormal(vector.x, vector.z);
				}
			}
			else
			{
				hitInfo.normal = new Vector3(0f, 1f, 0f);
				Debug.DrawLine(position, position + 1f * hitInfo.normal, Color.yellow, 0.1f, depthTest: false);
			}
		}
	}
}
