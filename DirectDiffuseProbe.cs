using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DirectDiffuseProbe : MonoBehaviour
{
	public int resolution;

	public Cubemap result;

	public Material testTarget;

	private void Update()
	{
		if (result == null || result.width != resolution)
		{
			result = new Cubemap(resolution, TextureFormat.RGB24, mipmap: false);
		}
		Object[] array = Object.FindObjectsOfType(typeof(Light));
		List<Vector4> list = new List<Vector4>();
		List<Vector4> list2 = new List<Vector4>();
		Object[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			Light light = (Light)array2[i];
			if (light.type == LightType.Spot)
			{
				Debug.LogWarning("Not implemented yet error: spotlights are not supported by both cube map tools and shaders. The culprit is:");
				Debug.LogWarning(light);
				continue;
			}
			Vector4 item;
			if (light.type == LightType.Directional)
			{
				item = -(Vector4)light.transform.forward;
				item.w = 0f;
			}
			else
			{
				if (light.range == 0f)
				{
					continue;
				}
				item = light.transform.position;
				item.w = light.range;
			}
			list.Add(item);
			list2.Add(new Vector4(light.color.r, light.color.g, light.color.b, light.intensity * 2f));
		}
		for (int j = 0; j < 6; j++)
		{
			Color[] array3 = new Color[resolution * resolution];
			for (int k = 0; k < resolution; k++)
			{
				float v = (float)k / (float)resolution;
				for (int l = 0; l < resolution; l++)
				{
					float u = (float)l / (float)resolution;
					Vector3 unitVector = GetUnitVector((CubemapFace)j, u, v);
					unitVector.z = 0f - unitVector.z;
					Vector3 zero = Vector3.zero;
					for (int m = 0; m < list.Count; m++)
					{
						float num;
						if (list[m].w == 0f)
						{
							num = Vector3.Dot(unitVector, list[m]);
						}
						else
						{
							Vector3 vector = (Vector3)list[m] - base.transform.position;
							float num2 = 1f / vector.magnitude;
							num = Vector3.Dot(unitVector, vector * num2) * num2;
						}
						if (num > 0f)
						{
							zero += (Vector3)list2[m] * list2[m].w * num;
						}
					}
					ref Color reference = ref array3[k * resolution + l];
					reference = new Color(zero.x, zero.y, zero.z);
				}
			}
			result.SetPixels(array3, (CubemapFace)j);
		}
		result.Apply();
		testTarget.SetTexture("_Cube", result);
	}

	private Vector3 GetUnitVector(CubemapFace f, float u, float v)
	{
		u -= 0.5f;
		v -= 0.5f;
		return f switch
		{
			CubemapFace.NegativeX => new Vector3(-0.5f, 0f - v, 0f - u).normalized, 
			CubemapFace.NegativeY => new Vector3(u, -0.5f, v).normalized, 
			CubemapFace.NegativeZ => new Vector3(0f - u, 0f - v, 0.5f).normalized, 
			CubemapFace.PositiveX => new Vector3(0.5f, 0f - v, u).normalized, 
			CubemapFace.PositiveY => new Vector3(u, 0.5f, 0f - v).normalized, 
			_ => new Vector3(u, 0f - v, -0.5f).normalized, 
		};
	}
}
