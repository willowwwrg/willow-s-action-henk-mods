using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class PostProcess : MonoBehaviour
{
	[SerializeField]
	private Shader postEffect;

	[SerializeField]
	private DepthTextureMode zBufferRequirements;

	[SerializeField]
	private Material mat;

	private void Awake()
	{
		if (mat != null)
		{
			mat.shader = postEffect;
		}
		else
		{
			mat = new Material(postEffect);
		}
		if (zBufferRequirements > base.camera.depthTextureMode)
		{
			base.camera.depthTextureMode = zBufferRequirements;
		}
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (mat == null)
		{
			Awake();
		}
		Matrix4x4 matrix = default(Matrix4x4);
		matrix.SetRow(0, (base.camera.ViewportToWorldPoint(Vector3.up + Vector3.forward) - base.camera.transform.position).normalized);
		matrix.SetRow(1, (base.camera.ViewportToWorldPoint(Vector3.one) - base.camera.transform.position).normalized);
		matrix.SetRow(2, (base.camera.ViewportToWorldPoint(Vector3.forward) - base.camera.transform.position).normalized);
		matrix.SetRow(3, (base.camera.ViewportToWorldPoint(Vector3.right + Vector3.forward) - base.camera.transform.position).normalized);
		mat.SetMatrix("_FrustumCornersWS", matrix);
		Graphics.Blit(source, destination, mat);
	}
}
