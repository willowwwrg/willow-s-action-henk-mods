using UnityEngine;

public class BakeTexturesAtRuntime : MonoBehaviour
{
	public GameObject target;

	private float elapsedTime;

	private void OnGUI()
	{
		GUILayout.Label("Time to bake textures: " + elapsedTime);
		if (GUILayout.Button("Combine textures & build combined mesh"))
		{
			MB2_MeshBaker component = target.GetComponent<MB2_MeshBaker>();
			MB2_TextureBaker component2 = target.GetComponent<MB2_TextureBaker>();
			component2.textureBakeResults = ScriptableObject.CreateInstance<MB2_TextureBakeResults>();
			component2.resultMaterial = new Material(Shader.Find("Diffuse"));
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			component2.CreateAtlases();
			elapsedTime = Time.realtimeSinceStartup - realtimeSinceStartup;
			component.ClearMesh();
			component.textureBakeResults = component2.textureBakeResults;
			component.AddDeleteGameObjects(component2.GetObjectsToCombine().ToArray(), null, disableRendererInSource: true, fixOutOfBoundUVs: false);
			component.Apply();
		}
	}
}
