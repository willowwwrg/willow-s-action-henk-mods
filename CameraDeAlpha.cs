using UnityEngine;

public class CameraDeAlpha : MonoBehaviour
{
	private Material mat;

	private void OnPostRender()
	{
		if (!mat)
		{
			mat = new Material("Shader \"Hidden/Alpha\" {SubShader {    Pass {        ZTest Always Cull Off ZWrite Off        ColorMask A        Color (1,1,1,1)    }}}");
			mat.shader.hideFlags = HideFlags.HideAndDontSave;
			mat.hideFlags = HideFlags.HideAndDontSave;
		}
		GL.PushMatrix();
		GL.LoadOrtho();
		for (int i = 0; i < mat.passCount; i++)
		{
			mat.SetPass(i);
			GL.Begin(7);
			GL.Vertex3(0f, 0f, 0.1f);
			GL.Vertex3(1f, 0f, 0.1f);
			GL.Vertex3(1f, 1f, 0.1f);
			GL.Vertex3(0f, 1f, 0.1f);
			GL.End();
		}
		GL.PopMatrix();
	}
}
