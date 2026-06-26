using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class CubemapRenderer : MonoBehaviour
{
	[SerializeField]
	protected int resolution;

	protected Cubemap cache;

	[SerializeField]
	protected Cubemap targetCube;

	[SerializeField]
	private Material[] targetMaterials;

	[SerializeField]
	private bool dynamic;

	protected Vector3 position;

	protected Vector3 extents;

	private void Update()
	{
		if ((dynamic || !Application.isPlaying) && (position != base.transform.position || extents != base.transform.lossyScale))
		{
			Start();
		}
	}

	protected virtual void MakeCube()
	{
		base.camera.RenderToCubemap(cache);
	}

	protected virtual void ManipulateCubemap()
	{
	}

	private void Start()
	{
		if ((bool)targetCube)
		{
			resolution = targetCube.width;
			cache = targetCube;
		}
		if (cache == null || cache.width != resolution)
		{
			cache = new Cubemap(resolution, TextureFormat.RGB24, mipmap: false);
		}
		MakeCube();
		ManipulateCubemap();
		position = base.transform.position;
		extents = base.transform.lossyScale * 0.5f;
		if (targetMaterials != null)
		{
			for (int i = 0; i < targetMaterials.Length; i++)
			{
				ApplyCubemapToTarget(targetMaterials[i]);
			}
		}
	}

	protected virtual void ApplyCubemapToTarget(Material m)
	{
		m.SetVector("_CubeMin", position - extents);
		m.SetVector("_CubeMax", position + extents);
		m.SetVector("_CubeOrigin", position);
	}

	private void OnDrawGizmos()
	{
		Gizmos.DrawWireCube(base.transform.position, base.transform.lossyScale);
	}
}
