using System;
using System.Collections.Generic;
using UnityEngine;

namespace DigitalOpus.MB.Core;

public class MB_Utility
{
	private class MB_Triangle
	{
		private int submeshIdx;

		private int[] vs = new int[3];

		public MB_Triangle(int[] ts, int idx, int sIdx)
		{
			vs[0] = ts[idx];
			vs[1] = ts[idx + 1];
			vs[2] = ts[idx + 2];
			submeshIdx = sIdx;
			Array.Sort(vs);
		}

		public bool isSame(object obj)
		{
			MB_Triangle mB_Triangle = (MB_Triangle)obj;
			if (vs[0] == mB_Triangle.vs[0] && vs[1] == mB_Triangle.vs[1] && vs[2] == mB_Triangle.vs[2] && submeshIdx != mB_Triangle.submeshIdx)
			{
				return true;
			}
			return false;
		}

		public bool sharesVerts(MB_Triangle obj)
		{
			if ((vs[0] == obj.vs[0] || vs[0] == obj.vs[1] || vs[0] == obj.vs[2]) && submeshIdx != obj.submeshIdx)
			{
				return true;
			}
			if ((vs[1] == obj.vs[0] || vs[1] == obj.vs[1] || vs[1] == obj.vs[2]) && submeshIdx != obj.submeshIdx)
			{
				return true;
			}
			if ((vs[2] == obj.vs[0] || vs[2] == obj.vs[1] || vs[2] == obj.vs[2]) && submeshIdx != obj.submeshIdx)
			{
				return true;
			}
			return false;
		}
	}

	public static Texture2D createTextureCopy(Texture2D source)
	{
		Texture2D texture2D = new Texture2D(source.width, source.height, TextureFormat.ARGB32, mipmap: true);
		texture2D.SetPixels(source.GetPixels());
		return texture2D;
	}

	public static Material[] GetGOMaterials(GameObject go)
	{
		if (go == null)
		{
			return null;
		}
		Material[] array = null;
		Mesh mesh = null;
		MeshRenderer component = go.GetComponent<MeshRenderer>();
		if (component != null)
		{
			array = component.sharedMaterials;
			MeshFilter component2 = go.GetComponent<MeshFilter>();
			if (component2 == null)
			{
				throw new Exception(string.Concat("Object ", go, " has a MeshRenderer but no MeshFilter."));
			}
			mesh = component2.sharedMesh;
		}
		SkinnedMeshRenderer component3 = go.GetComponent<SkinnedMeshRenderer>();
		if (component3 != null)
		{
			array = component3.sharedMaterials;
			mesh = component3.sharedMesh;
		}
		if (array == null)
		{
			Debug.LogError("Object " + go.name + " does not have a MeshRenderer or a SkinnedMeshRenderer component");
			return null;
		}
		if (mesh == null)
		{
			Debug.LogError("Object " + go.name + " has a MeshRenderer or SkinnedMeshRenderer but no mesh.");
			return null;
		}
		if (mesh.subMeshCount < array.Length)
		{
			Debug.LogWarning(string.Concat("Object ", go, " has only ", mesh.subMeshCount, " submeshes and has ", array.Length, " materials. Extra materials do nothing."));
			Material[] array2 = new Material[mesh.subMeshCount];
			Array.Copy(array, array2, array2.Length);
			array = array2;
		}
		return array;
	}

	public static Mesh GetMesh(GameObject go)
	{
		if (go == null)
		{
			return null;
		}
		MeshFilter component = go.GetComponent<MeshFilter>();
		if (component != null)
		{
			return component.sharedMesh;
		}
		SkinnedMeshRenderer component2 = go.GetComponent<SkinnedMeshRenderer>();
		if (component2 != null)
		{
			return component2.sharedMesh;
		}
		Debug.LogError("Object " + go.name + " does not have a MeshFilter or a SkinnedMeshRenderer component");
		return null;
	}

	public static Renderer GetRenderer(GameObject go)
	{
		if (go == null)
		{
			return null;
		}
		MeshRenderer component = go.GetComponent<MeshRenderer>();
		if (component != null)
		{
			return component;
		}
		SkinnedMeshRenderer component2 = go.GetComponent<SkinnedMeshRenderer>();
		if (component2 != null)
		{
			return component2;
		}
		return null;
	}

	public static void DisableRendererInSource(GameObject go)
	{
		if (go == null)
		{
			return;
		}
		MeshRenderer component = go.GetComponent<MeshRenderer>();
		if (component != null)
		{
			component.enabled = false;
			return;
		}
		SkinnedMeshRenderer component2 = go.GetComponent<SkinnedMeshRenderer>();
		if (component2 != null)
		{
			component2.enabled = false;
		}
	}

	public static bool hasOutOfBoundsUVs(Mesh m, ref Rect uvBounds, int submeshIndex = -1)
	{
		Vector2[] uv = m.uv;
		if (uv.Length == 0)
		{
			return false;
		}
		if (submeshIndex >= m.subMeshCount)
		{
			return false;
		}
		float num;
		float x;
		float num2;
		float y;
		if (submeshIndex >= 0)
		{
			int[] triangles = m.GetTriangles(submeshIndex);
			if (triangles.Length == 0)
			{
				return false;
			}
			num = (x = uv[triangles[0]].x);
			num2 = (y = uv[triangles[0]].y);
			int[] array = triangles;
			foreach (int num3 in array)
			{
				if (uv[num3].x < num)
				{
					num = uv[num3].x;
				}
				if (uv[num3].x > x)
				{
					x = uv[num3].x;
				}
				if (uv[num3].y < num2)
				{
					num2 = uv[num3].y;
				}
				if (uv[num3].y > y)
				{
					y = uv[num3].y;
				}
			}
		}
		else
		{
			num = (x = uv[0].x);
			num2 = (y = uv[0].y);
			for (int j = 0; j < uv.Length; j++)
			{
				if (uv[j].x < num)
				{
					num = uv[j].x;
				}
				if (uv[j].x > x)
				{
					x = uv[j].x;
				}
				if (uv[j].y < num2)
				{
					num2 = uv[j].y;
				}
				if (uv[j].y > y)
				{
					y = uv[j].y;
				}
			}
		}
		uvBounds.x = num;
		uvBounds.y = num2;
		uvBounds.width = x - num;
		uvBounds.height = y - num2;
		if (x > 1f || num < 0f || y > 1f || num2 < 0f)
		{
			return true;
		}
		float num4 = (uvBounds.y = 0f);
		float x2 = num4;
		uvBounds.x = x2;
		num4 = (uvBounds.height = 1f);
		x2 = num4;
		uvBounds.width = x2;
		return false;
	}

	public static void setSolidColor(Texture2D t, Color c)
	{
		Color[] pixels = t.GetPixels();
		for (int i = 0; i < pixels.Length; i++)
		{
			pixels[i] = c;
		}
		t.SetPixels(pixels);
		t.Apply();
	}

	public static Texture2D resampleTexture(Texture2D source, int newWidth, int newHeight)
	{
		TextureFormat format = source.format;
		if (format == TextureFormat.ARGB32 || format == TextureFormat.RGBA32 || format == TextureFormat.BGRA32 || format == TextureFormat.RGB24 || format == TextureFormat.Alpha8 || format == TextureFormat.DXT1)
		{
			Texture2D texture2D = new Texture2D(newWidth, newHeight, TextureFormat.ARGB32, mipmap: true);
			float num = newWidth;
			float num2 = newHeight;
			for (int i = 0; i < newWidth; i++)
			{
				for (int j = 0; j < newHeight; j++)
				{
					float u = (float)i / num;
					float v = (float)j / num2;
					texture2D.SetPixel(i, j, source.GetPixelBilinear(u, v));
				}
			}
			texture2D.Apply();
			return texture2D;
		}
		Debug.LogError("Can only resize textures in formats ARGB32, RGBA32, BGRA32, RGB24, Alpha8 or DXT");
		return null;
	}

	public static bool validateOBuvsMultiMaterial(Material[] sharedMaterials)
	{
		for (int i = 0; i < sharedMaterials.Length; i++)
		{
			for (int j = i + 1; j < sharedMaterials.Length; j++)
			{
				if (sharedMaterials[i] == sharedMaterials[j])
				{
					return false;
				}
			}
		}
		return true;
	}

	public static int doSubmeshesShareVertsOrTris(Mesh m)
	{
		List<MB_Triangle> list = new List<MB_Triangle>();
		bool flag = false;
		bool flag2 = false;
		for (int i = 0; i < m.subMeshCount; i++)
		{
			int[] triangles = m.GetTriangles(i);
			for (int j = 0; j < triangles.Length; j += 3)
			{
				MB_Triangle mB_Triangle = new MB_Triangle(triangles, j, i);
				for (int k = 0; k < list.Count; k++)
				{
					if (mB_Triangle.isSame(list[k]))
					{
						flag2 = true;
					}
					if (mB_Triangle.sharesVerts(list[k]))
					{
						flag = true;
					}
				}
				list.Add(mB_Triangle);
			}
		}
		if (flag2)
		{
			return 2;
		}
		if (flag)
		{
			return 1;
		}
		return 0;
	}

	public static bool GetBounds(GameObject go, out Bounds b)
	{
		if (go == null)
		{
			Debug.LogError("go paramater was null");
			b = new Bounds(Vector3.zero, Vector3.zero);
			return false;
		}
		Renderer renderer = GetRenderer(go);
		if (renderer == null)
		{
			Debug.LogError("GetBounds must be called on an object with a Renderer");
			b = new Bounds(Vector3.zero, Vector3.zero);
			return false;
		}
		if (renderer is MeshRenderer)
		{
			b = renderer.bounds;
			return true;
		}
		if (renderer is SkinnedMeshRenderer)
		{
			b = renderer.bounds;
			return true;
		}
		Debug.LogError("GetBounds must be called on an object with a MeshRender or a SkinnedMeshRenderer.");
		b = new Bounds(Vector3.zero, Vector3.zero);
		return false;
	}

	public static void Destroy(UnityEngine.Object o)
	{
		if (Application.isPlaying)
		{
			UnityEngine.Object.Destroy(o);
		}
		else
		{
			UnityEngine.Object.DestroyImmediate(o, allowDestroyingAssets: false);
		}
	}
}
