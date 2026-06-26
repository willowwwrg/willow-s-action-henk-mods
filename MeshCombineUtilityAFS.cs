using UnityEngine;

public class MeshCombineUtilityAFS
{
	public struct MeshInstance
	{
		public Mesh mesh;

		public int subMeshIndex;

		public Matrix4x4 transform;

		public Vector3 groundNormal;

		public float scale;

		public Vector3 pivot;
	}

	public static Mesh Combine(MeshInstance[] combines, bool bakeGroundLightingGrass, bool bakeGroundLightingFoliage, float randomBrightness, float randomPulse, float randomBending, float randomFluttering, Color HealthyColor, Color DryColor, float NoiseSpread, bool bakeScale, bool simplyCombine, float NoiseSpreadFoliage, bool createUniqueUV2)
	{
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < combines.Length; i++)
		{
			MeshInstance meshInstance = combines[i];
			if ((bool)meshInstance.mesh)
			{
				num += meshInstance.mesh.vertexCount;
				num2 += meshInstance.mesh.GetTriangles(meshInstance.subMeshIndex).Length;
			}
		}
		Vector3[] array = new Vector3[num];
		Vector3[] array2 = new Vector3[num];
		Vector4[] array3 = new Vector4[num];
		Vector2[] array4 = new Vector2[num];
		Vector2[] array5 = new Vector2[num];
		Color[] array6 = new Color[num];
		int[] array7 = new int[num2];
		int offset = 0;
		for (int j = 0; j < combines.Length; j++)
		{
			MeshInstance meshInstance2 = combines[j];
			if ((bool)meshInstance2.mesh)
			{
				Copy(meshInstance2.mesh.vertexCount, meshInstance2.mesh.vertices, array, ref offset, meshInstance2.transform);
			}
		}
		offset = 0;
		for (int k = 0; k < combines.Length; k++)
		{
			MeshInstance meshInstance3 = combines[k];
			if ((bool)meshInstance3.mesh)
			{
				Matrix4x4 transform = meshInstance3.transform;
				transform = transform.inverse.transpose;
				if (bakeGroundLightingGrass)
				{
					CopyNormalGround(meshInstance3.mesh.vertexCount, meshInstance3.mesh.normals, array2, ref offset, transform, meshInstance3.groundNormal);
				}
				else
				{
					CopyNormal(meshInstance3.mesh.vertexCount, meshInstance3.mesh.normals, array2, ref offset, transform);
				}
			}
		}
		offset = 0;
		for (int l = 0; l < combines.Length; l++)
		{
			MeshInstance meshInstance4 = combines[l];
			if ((bool)meshInstance4.mesh)
			{
				Matrix4x4 transform2 = meshInstance4.transform;
				transform2 = transform2.inverse.transpose;
				CopyTangents(meshInstance4.mesh.vertexCount, meshInstance4.mesh.tangents, array3, ref offset, transform2);
			}
		}
		offset = 0;
		for (int m = 0; m < combines.Length; m++)
		{
			MeshInstance meshInstance5 = combines[m];
			if ((bool)meshInstance5.mesh)
			{
				Copy(meshInstance5.mesh.vertexCount, meshInstance5.mesh.uv, array4, ref offset);
			}
		}
		offset = 0;
		if (bakeGroundLightingFoliage)
		{
			for (int n = 0; n < combines.Length; n++)
			{
				MeshInstance meshInstance6 = combines[n];
				if ((bool)meshInstance6.mesh)
				{
					Copy_uv1(meshInstance6.mesh.vertexCount, meshInstance6.mesh.uv, array5, ref offset, new Vector2(meshInstance6.groundNormal.x, meshInstance6.groundNormal.z));
				}
			}
			offset = 0;
		}
		for (int num3 = 0; num3 < combines.Length; num3++)
		{
			MeshInstance meshInstance7 = combines[num3];
			if ((bool)meshInstance7.mesh)
			{
				if (bakeGroundLightingGrass)
				{
					CopyColors_grass(meshInstance7.mesh.vertexCount, meshInstance7.mesh.colors, array6, ref offset, HealthyColor, DryColor, NoiseSpread, meshInstance7.pivot);
				}
				else
				{
					CopyColors(meshInstance7.mesh.vertexCount, meshInstance7.mesh.colors, array6, ref offset, meshInstance7.scale, bakeScale, meshInstance7.pivot, NoiseSpreadFoliage, randomPulse, randomFluttering, randomBrightness, randomBending);
				}
			}
		}
		int num4 = 0;
		int num5 = 0;
		for (int num6 = 0; num6 < combines.Length; num6++)
		{
			MeshInstance meshInstance8 = combines[num6];
			if ((bool)meshInstance8.mesh)
			{
				int[] triangles = meshInstance8.mesh.GetTriangles(meshInstance8.subMeshIndex);
				for (int num7 = 0; num7 < triangles.Length; num7++)
				{
					array7[num7 + num4] = triangles[num7] + num5;
				}
				num4 += triangles.Length;
				num5 += meshInstance8.mesh.vertexCount;
				triangles = null;
			}
		}
		Mesh mesh = new Mesh();
		mesh.name = "Combined Mesh";
		mesh.vertices = array;
		mesh.normals = array2;
		mesh.colors = array6;
		mesh.uv = array4;
		if (bakeGroundLightingFoliage)
		{
			mesh.uv1 = array5;
		}
		mesh.tangents = array3;
		mesh.triangles = array7;
		mesh.Optimize();
		array = null;
		array2 = null;
		array3 = null;
		array4 = null;
		array5 = null;
		array6 = null;
		array7 = null;
		return mesh;
	}

	private static void Copy(int vertexcount, Vector3[] src, Vector3[] dst, ref int offset, Matrix4x4 transform)
	{
		for (int i = 0; i < src.Length; i++)
		{
			ref Vector3 reference = ref dst[i + offset];
			reference = transform.MultiplyPoint(src[i]);
		}
		offset += vertexcount;
	}

	private static void CopyNormal(int vertexcount, Vector3[] src, Vector3[] dst, ref int offset, Matrix4x4 transform)
	{
		for (int i = 0; i < src.Length; i++)
		{
			ref Vector3 reference = ref dst[i + offset];
			reference = transform.MultiplyVector(src[i]).normalized;
		}
		offset += vertexcount;
	}

	private static void CopyNormalGround(int vertexcount, Vector3[] src, Vector3[] dst, ref int offset, Matrix4x4 transform, Vector3 groundNormal)
	{
		for (int i = 0; i < src.Length; i++)
		{
			dst[i + offset] = groundNormal;
		}
		offset += vertexcount;
	}

	private static void Copy(int vertexcount, Vector2[] src, Vector2[] dst, ref int offset)
	{
		for (int i = 0; i < src.Length; i++)
		{
			ref Vector2 reference = ref dst[i + offset];
			reference = src[i];
		}
		offset += vertexcount;
	}

	private static void Copy_uv1(int vertexcount, Vector2[] src, Vector2[] dst, ref int offset, Vector2 groundNormal)
	{
		for (int i = 0; i < src.Length; i++)
		{
			dst[i + offset] = groundNormal;
		}
		offset += vertexcount;
	}

	private static void CopyColors(int vertexcount, Color[] src, Color[] dst, ref int offset, float scale, bool bakeScale, Vector3 pivot, float NoiseSpread, float randomPulse, float randomFluttering, float randomBrightness, float randomBending)
	{
		for (int i = 0; i < src.Length; i++)
		{
			float num = Mathf.PerlinNoise(pivot.x, pivot.y);
			src[i].r += randomPulse * num;
			src[i].g = src[i].g * (1f + randomFluttering * (num - 0.5f));
			if (bakeScale)
			{
				src[i].b = src[i].b * scale * (1f + randomBending * num);
			}
			else
			{
				src[i].b = src[i].b * (1f + randomBending * num);
			}
			src[i].a = src[i].a - num * randomBrightness;
			ref Color reference = ref dst[i + offset];
			reference = src[i];
		}
		offset += vertexcount;
	}

	private static void CopyColors_groundNormal_old(int vertexcount, Color[] src, Color[] dst, ref int offset, Color RandColor, Vector2 groundNormal)
	{
		for (int i = 0; i < src.Length; i++)
		{
			ref Color reference = ref dst[i + offset];
			reference = src[i] + RandColor;
			dst[i + offset].r = groundNormal.x;
			dst[i + offset].g = groundNormal.y;
		}
		offset += vertexcount;
	}

	private static void CopyColors_grass(int vertexcount, Color[] src, Color[] dst, ref int offset, Color HealthyColor, Color DryColor, float NoiseSpread, Vector3 pivot)
	{
		Color color = Color.Lerp(HealthyColor, DryColor, Mathf.PerlinNoise(pivot.x * NoiseSpread, pivot.y * NoiseSpread));
		for (int i = 0; i < src.Length; i++)
		{
			dst[i + offset].a = src[i].a;
			dst[i + offset].r = Mathf.Lerp(1f, color.r, color.a) * src[i].b;
			dst[i + offset].g = Mathf.Lerp(1f, color.g, color.a) * src[i].b;
			dst[i + offset].b = Mathf.Lerp(1f, color.b, color.a) * src[i].b;
		}
		offset += vertexcount;
	}

	private static void CopyTangents(int vertexcount, Vector4[] src, Vector4[] dst, ref int offset, Matrix4x4 transform)
	{
		for (int i = 0; i < src.Length; i++)
		{
			Vector4 vector = src[i];
			Vector3 v = new Vector3(vector.x, vector.y, vector.z);
			v = transform.MultiplyVector(v).normalized;
			ref Vector4 reference = ref dst[i + offset];
			reference = new Vector4(v.x, v.y, v.z, vector.w);
		}
		offset += vertexcount;
	}
}
