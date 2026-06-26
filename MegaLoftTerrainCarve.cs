using System;
using UnityEngine;

public class MegaLoftTerrainCarve : MonoBehaviour
{
	public Terrain terrain;

	public GameObject tobj;

	public bool doterraindeform;

	public float falloff = 1f;

	public bool conform;

	public float startray = 100f;

	public float raydist = 1000f;

	public float offset = 1f;

	public MegaShapeLoft surfaceLoft;

	public int surfaceLayer;

	public float start;

	public float end;

	public float cstart;

	public float cend;

	public float dist = 1f;

	public float scale = 1f;

	public float leftscale;

	public float rightscale;

	public AnimationCurve sectioncrv = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, -0.2f), new Keyframe(1f, 0f));

	public int numpasses;

	public bool restorebefore = true;

	public bool leftenabled = true;

	public bool rightenabled = true;

	public float leftfalloff = 1f;

	public float rightfalloff = 1f;

	public AnimationCurve leftfallcrv = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f));

	public AnimationCurve rightfallcrv = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f));

	private TerrainData tdata;

	public float[] savedheights;

	private MeshCollider mcol;

	private Mesh mesh;

	private GameObject colobj;

	public Vector3[] verts;

	public Vector2[] uvs;

	public int steps;

	public Vector3[] vertsl;

	public Vector2[] uvsl;

	public Vector3[] vertsr;

	public Vector2[] uvsr;

	public MegaSpline leftfall = new MegaSpline();

	public MegaSpline rightfall = new MegaSpline();

	public float rightalphaoff;

	public float leftalphaoff;

	[ContextMenu("Save Current Heights")]
	public void SaveHeights()
	{
		if (terrain == null)
		{
			terrain = tobj.GetComponent<Terrain>();
		}
		if ((bool)terrain)
		{
			tdata = terrain.terrainData;
			float[,] heights = tdata.GetHeights(0, 0, tdata.heightmapWidth, tdata.heightmapHeight);
			savedheights = new float[tdata.heightmapWidth * tdata.heightmapHeight];
			Convert(heights, savedheights, tdata.heightmapWidth, tdata.heightmapHeight);
		}
	}

	[ContextMenu("Reset Heights")]
	public void ResetHeights()
	{
		if (terrain == null)
		{
			terrain = tobj.GetComponent<Terrain>();
		}
		if ((bool)terrain)
		{
			tdata = terrain.terrainData;
			if (savedheights != null)
			{
				float[,] array = new float[tdata.heightmapWidth, tdata.heightmapHeight];
				Convert(savedheights, array, tdata.heightmapWidth, tdata.heightmapHeight);
				tdata.SetHeights(0, 0, array);
			}
		}
	}

	public void ClearMem()
	{
		savedheights = null;
	}

	public static float[,] Convert(float[] data, float[,] dest, int width, int height)
	{
		int num = 0;
		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < width; j++)
			{
				dest[i, j] = data[num++];
			}
		}
		return dest;
	}

	public static float[] Convert(float[,] data, float[] dest, int width, int height)
	{
		int num = 0;
		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < width; j++)
			{
				dest[num++] = data[i, j];
			}
		}
		return dest;
	}

	private void Start()
	{
		BuildVerts();
	}

	private void OnDrawGizmosSelected()
	{
		if (!mesh || !surfaceLoft)
		{
			return;
		}
		if (verts == null || verts.Length == 0)
		{
			BuildVerts();
		}
		if (verts != null && verts.Length > 2)
		{
			Gizmos.matrix = surfaceLoft.transform.localToWorldMatrix;
			Gizmos.DrawLine(verts[0], verts[1]);
			int num = 0;
			for (int i = 0; i < verts.Length - 2; i += 2)
			{
				num++;
				if ((num & 1) != 0)
				{
					Gizmos.color = Color.white;
				}
				else
				{
					Gizmos.color = Color.black;
				}
				Gizmos.DrawLine(verts[i], verts[i + 2]);
				Gizmos.DrawLine(verts[i + 1], verts[i + 3]);
			}
			Gizmos.DrawLine(verts[verts.Length - 2], verts[verts.Length - 1]);
		}
		if (leftenabled && vertsl != null && vertsl.Length > 2)
		{
			Gizmos.matrix = surfaceLoft.transform.localToWorldMatrix;
			Gizmos.color = Color.red;
			for (int j = 0; j < vertsl.Length - 2; j += 2)
			{
				Gizmos.DrawLine(vertsl[j + 1], vertsl[j + 3]);
			}
			Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.25f);
			for (int k = 0; k < vertsl.Length - 2; k += 2)
			{
				Gizmos.DrawLine(vertsl[k], vertsl[k + 1]);
			}
		}
		if (rightenabled && vertsr != null && vertsr.Length > 2)
		{
			Gizmos.matrix = surfaceLoft.transform.localToWorldMatrix;
			Gizmos.color = Color.green;
			for (int l = 0; l < vertsr.Length - 2; l += 2)
			{
				Gizmos.DrawLine(vertsr[l + 1], vertsr[l + 3]);
			}
			Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.15f);
			for (int m = 0; m < vertsr.Length - 2; m += 2)
			{
				Gizmos.DrawLine(vertsr[m], vertsr[m + 1]);
			}
		}
	}

	public void ConformTerrain()
	{
		if (!tobj)
		{
			return;
		}
		if (restorebefore)
		{
			ResetHeights();
		}
		BuildVerts();
		BuildCollider();
		GameObject obj = null;
		GameObject obj2 = null;
		MeshCollider meshCollider = null;
		MeshCollider meshCollider2 = null;
		if (leftenabled)
		{
			meshCollider = BuildCollider(vertsl, uvsl, steps, 1, cw: false, ref obj);
		}
		if (rightenabled)
		{
			meshCollider2 = BuildCollider(vertsr, uvsr, steps, 1, cw: true, ref obj2);
		}
		doterraindeform = false;
		if (terrain == null)
		{
			terrain = tobj.GetComponent<Terrain>();
		}
		Collider collider = mcol;
		if ((bool)collider && (bool)terrain)
		{
			tdata = terrain.terrainData;
			float[,] heights = tdata.GetHeights(0, 0, tdata.heightmapWidth, tdata.heightmapHeight);
			float[,] array = new float[tdata.heightmapWidth, tdata.heightmapHeight];
			bool[,] array2 = new bool[tdata.heightmapWidth, tdata.heightmapHeight];
			Matrix4x4 localToWorldMatrix = terrain.transform.localToWorldMatrix;
			Matrix4x4 worldToLocalMatrix = terrain.transform.worldToLocalMatrix;
			Vector3 zero = Vector3.zero;
			Ray ray = default(Ray);
			for (int i = 0; i < tdata.heightmapHeight; i++)
			{
				zero.z = (float)i / (float)tdata.heightmapHeight * tdata.size.z;
				for (int j = 0; j < tdata.heightmapWidth; j++)
				{
					zero.y = heights[i, j];
					array[i, j] = zero.y;
					zero.y *= tdata.size.y;
					zero.x = (float)j / (float)tdata.heightmapWidth * tdata.size.x;
					Vector3 origin = localToWorldMatrix.MultiplyPoint3x4(zero);
					origin.y += startray;
					ray.origin = origin;
					ray.direction = Vector3.down;
					if (collider.Raycast(ray, out var hitInfo, raydist))
					{
						Vector3 vector = worldToLocalMatrix.MultiplyPoint3x4(hitInfo.point);
						float num = sectioncrv.Evaluate(hitInfo.textureCoord.x);
						float num2 = (vector.y - offset + num) / tdata.size.y;
						array[i, j] = num2;
						array2[i, j] = true;
					}
					else if (leftenabled && meshCollider.Raycast(ray, out hitInfo, raydist))
					{
						int num3 = hitInfo.triangleIndex / 2;
						Vector3 vector2 = worldToLocalMatrix.MultiplyPoint3x4(vertsl[num3 * 2]);
						Vector3 vector3 = worldToLocalMatrix.MultiplyPoint3x4(vertsl[(num3 + 1) * 2]);
						float num4 = (Mathf.Lerp(vector2.y, vector3.y, hitInfo.barycentricCoordinate.x) - offset) / tdata.size.y;
						array[i, j] = Mathf.Lerp(num4, array[i, j], leftfallcrv.Evaluate(hitInfo.textureCoord.x));
						array2[i, j] = true;
					}
					else if (rightenabled && meshCollider2.Raycast(ray, out hitInfo, raydist))
					{
						int num5 = hitInfo.triangleIndex / 2;
						Vector3 vector4 = worldToLocalMatrix.MultiplyPoint3x4(vertsr[num5 * 2]);
						Vector3 vector5 = worldToLocalMatrix.MultiplyPoint3x4(vertsr[(num5 + 1) * 2]);
						float num6 = (Mathf.Lerp(vector4.y, vector5.y, hitInfo.barycentricCoordinate.x) - offset) / tdata.size.y;
						array[i, j] = Mathf.Lerp(num6, array[i, j], 1f - rightfallcrv.Evaluate(1f - hitInfo.textureCoord.x));
						array2[i, j] = true;
					}
				}
			}
			if (numpasses > 0)
			{
				float[,] heights2 = SmoothTerrain(array, numpasses, tdata.heightmapWidth, tdata.heightmapHeight, array2);
				tdata.SetHeights(0, 0, heights2);
			}
			else
			{
				tdata.SetHeights(0, 0, array);
			}
		}
		if (Application.isEditor && !Application.isPlaying)
		{
			if ((bool)colobj)
			{
				UnityEngine.Object.DestroyImmediate(colobj);
			}
			if ((bool)obj)
			{
				UnityEngine.Object.DestroyImmediate(obj);
			}
			if ((bool)obj2)
			{
				UnityEngine.Object.DestroyImmediate(obj2);
			}
		}
		else
		{
			if ((bool)colobj)
			{
				UnityEngine.Object.Destroy(colobj);
			}
			if ((bool)obj)
			{
				UnityEngine.Object.Destroy(obj);
			}
			if ((bool)obj2)
			{
				UnityEngine.Object.Destroy(obj2);
			}
		}
	}

	private Vector3 GetNormalizedPositionRelativeToTerrain(Vector3 pos, Terrain terrain)
	{
		Vector3 vector = pos - terrain.gameObject.transform.position;
		return new Vector3
		{
			x = vector.x / tdata.size.x,
			y = vector.y / tdata.size.y,
			z = vector.z / tdata.size.z
		};
	}

	private Vector3 GetRelativeTerrainPositionFromPos(Vector3 pos, Terrain terrain, int mapWidth, int mapHeight)
	{
		Vector3 normalizedPositionRelativeToTerrain = GetNormalizedPositionRelativeToTerrain(pos, terrain);
		return new Vector3(normalizedPositionRelativeToTerrain.x * (float)mapWidth, 0f, normalizedPositionRelativeToTerrain.z * (float)mapHeight);
	}

	public void BuildVerts()
	{
		if (surfaceLoft == null)
		{
			return;
		}
		if (mesh == null)
		{
			mesh = new Mesh();
		}
		MegaLoftLayerSimple megaLoftLayerSimple = (MegaLoftLayerSimple)surfaceLoft.Layers[surfaceLayer];
		steps = (int)(megaLoftLayerSimple.layerPath.splines[megaLoftLayerSimple.curve].length / dist);
		verts = new Vector3[(steps + 1) * 2];
		uvs = new Vector2[(steps + 1) * 2];
		int num = 0;
		Vector2 zero = Vector2.zero;
		Matrix4x4 localToWorldMatrix = surfaceLoft.transform.localToWorldMatrix;
		for (int i = 0; i <= steps; i++)
		{
			float num2 = (float)i / (float)steps;
			Vector3 pos = megaLoftLayerSimple.GetPos(surfaceLoft, cstart, num2);
			Vector3 pos2 = megaLoftLayerSimple.GetPos(surfaceLoft, cend, num2);
			Vector3 normalized = (pos2 - pos).normalized;
			pos -= normalized * leftscale;
			pos2 += normalized * rightscale;
			zero.y = num2;
			zero.x = 0f;
			uvs[num] = zero;
			ref Vector3 reference = ref verts[num];
			reference = localToWorldMatrix.MultiplyPoint3x4(pos);
			zero.x = 1f;
			uvs[num + 1] = zero;
			ref Vector3 reference2 = ref verts[num + 1];
			reference2 = localToWorldMatrix.MultiplyPoint3x4(pos2);
			num += 2;
		}
		if (leftenabled && leftfalloff > 0f)
		{
			num = 0;
			vertsl = new Vector3[(steps + 1) * 2];
			uvsl = new Vector2[(steps + 1) * 2];
			OutlineSpline(megaLoftLayerSimple.layerPath.splines[megaLoftLayerSimple.curve], leftfall, 0f - leftfalloff, centered: true);
			leftfall.constantSpeed = true;
			leftfall.CalcLength(10);
			int k = 0;
			for (int j = 0; j <= steps; j++)
			{
				float num3 = (float)j / (float)steps;
				Vector3 pos3 = megaLoftLayerSimple.GetPos(surfaceLoft, cstart, num3);
				Vector3 normalized2 = (megaLoftLayerSimple.GetPos(surfaceLoft, cend, num3) - pos3).normalized;
				pos3 -= normalized2 * leftscale;
				pos3 = localToWorldMatrix.MultiplyPoint3x4(pos3);
				zero.y = num3;
				zero.x = 0f;
				vertsl[num] = pos3;
				uvsl[num++] = zero;
				zero.x = 1f;
				ref Vector3 reference3 = ref vertsl[num];
				reference3 = localToWorldMatrix.MultiplyPoint3x4(leftfall.InterpCurve3D(num3 + leftalphaoff * 0.01f, type: true, ref k));
				uvsl[num++] = zero;
			}
		}
		if (rightenabled && rightfalloff > 0f)
		{
			num = 0;
			vertsr = new Vector3[(steps + 1) * 2];
			uvsr = new Vector2[(steps + 1) * 2];
			OutlineSpline(megaLoftLayerSimple.layerPath.splines[megaLoftLayerSimple.curve], rightfall, rightfalloff, centered: true);
			rightfall.constantSpeed = false;
			rightfall.CalcLength(10);
			int k2 = 0;
			for (int l = 0; l <= steps; l++)
			{
				float num4 = (float)l / (float)steps;
				Vector3 pos4 = megaLoftLayerSimple.GetPos(surfaceLoft, cend, num4);
				Vector3 normalized3 = (megaLoftLayerSimple.GetPos(surfaceLoft, cstart, num4) - pos4).normalized;
				pos4 -= normalized3 * rightscale;
				pos4 = localToWorldMatrix.MultiplyPoint3x4(pos4);
				zero.y = num4;
				zero.x = 1f;
				vertsr[num] = pos4;
				uvsr[num++] = zero;
				zero.x = 0f;
				ref Vector3 reference4 = ref vertsr[num];
				reference4 = localToWorldMatrix.MultiplyPoint3x4(rightfall.InterpCurve3D(num4 + rightalphaoff * 0.01f, type: true, ref k2));
				uvsr[num++] = zero;
			}
		}
	}

	public void OutlineSpline(MegaSpline inSpline, MegaSpline outSpline, float size, bool centered)
	{
		float num = ((!centered) ? 0f : (size / 2f));
		int count = inSpline.knots.Count;
		outSpline.knots.Clear();
		if (inSpline.closed)
		{
			for (int i = 0; i < count; i++)
			{
				int knot = (i + count - 1) % count;
				float num2 = MegaShape.CurveLength(inSpline, knot, 0.5f, 1f, 0f);
				float num3 = MegaShape.CurveLength(inSpline, i, 0f, 0.5f, 0f);
				Vector3 p = inSpline.knots[i].p;
				Vector3 vector = Vector3.Normalize(inSpline.InterpBezier3D(knot, 0.99f) - p);
				Vector3 vector2 = Vector3.Normalize(inSpline.InterpBezier3D(i, 0.01f) - p);
				Vector3 vector3 = Vector3.Normalize(vector2 - vector);
				vector3.y = 0f;
				float num4 = Vector3.Dot(vector, vector2);
				float f = ((!(num4 >= -0.9999939f)) ? ((float)Math.PI / 2f) : ((0f - Mathf.Acos(num4)) / 2f));
				float num5 = num / Mathf.Tan(f);
				float num6 = ((!(num < 0f)) ? 1f : (-1f));
				float num7 = Mathf.Sqrt(num5 * num5 + num * num) * num6;
				Vector3 vector4 = new Vector3(vector3.z * num7, 0f, (0f - vector3.x) * num7);
				float num8 = MegaShape.CurveLength(inSpline, knot, 0.5f, 1f, num);
				float num9 = MegaShape.CurveLength(inSpline, i, 0f, 0.5f, num);
				Vector3 vector5 = p + vector4;
				float num10 = num8 / num2;
				float num11 = num9 / num3;
				outSpline.AddKnot(vector5, vector5 + (inSpline.knots[i].invec - p) * num10, vector5 + (inSpline.knots[i].outvec - p) * num11);
			}
			outSpline.closed = true;
			return;
		}
		for (int j = 0; j < count; j++)
		{
			Vector3 p2 = inSpline.knots[j].p;
			float num12 = ((j != 0) ? MegaShape.CurveLength(inSpline, j - 1, 0.5f, 1f, 0f) : 1f);
			float num13 = ((j != count - 1) ? MegaShape.CurveLength(inSpline, j, 0f, 0.5f, 0f) : 1f);
			float num14 = 0f;
			Vector3 vector6;
			if (j == 0)
			{
				vector6 = Vector3.Normalize(inSpline.InterpBezier3D(j, 0.01f) - p2);
				num14 = num;
			}
			else if (j == count - 1)
			{
				vector6 = Vector3.Normalize(p2 - inSpline.InterpBezier3D(j - 1, 0.99f));
				num14 = num;
			}
			else
			{
				Vector3 vector7 = Vector3.Normalize(inSpline.InterpBezier3D(j - 1, 0.99f) - p2);
				Vector3 vector8 = Vector3.Normalize(inSpline.InterpBezier3D(j, 0.01f) - p2);
				vector6 = Vector3.Normalize(vector8 - vector7);
				float num15 = Vector3.Dot(vector7, vector8);
				if (num15 >= -0.9999939f)
				{
					float f2 = (0f - Mathf.Acos(num15)) / 2f;
					float num16 = num / Mathf.Tan(f2);
					float num17 = ((!(num < 0f)) ? 1f : (-1f));
					num14 = Mathf.Sqrt(num16 * num16 + num * num) * num17;
				}
				else
				{
					num14 = num;
				}
			}
			vector6.y = 0f;
			Vector3 vector9 = new Vector3(vector6.z * num14, 0f, (0f - vector6.x) * num14);
			float num18 = ((j != 0) ? MegaShape.CurveLength(inSpline, j - 1, 0.5f, 1f, num) : 1f);
			float num19 = ((j != count - 1) ? MegaShape.CurveLength(inSpline, j, 0f, 0.5f, num) : 1f);
			float num20 = num18 / num12;
			float num21 = num19 / num13;
			Vector3 vector10 = p2 + vector9;
			outSpline.AddKnot(vector10, vector10 + (inSpline.knots[j].invec - p2) * num20, vector10 + (inSpline.knots[j].outvec - p2) * num21);
		}
		outSpline.closed = false;
	}

	private void BuildCollider()
	{
		MegaLoftLayerSimple megaLoftLayerSimple = (MegaLoftLayerSimple)surfaceLoft.Layers[surfaceLayer];
		colobj = new GameObject();
		mcol = colobj.AddComponent<MeshCollider>();
		mesh = new Mesh();
		int num = (int)(megaLoftLayerSimple.layerPath.splines[megaLoftLayerSimple.curve].length / dist);
		int[] array = new int[num * 6];
		int num2 = 0;
		num2 = 0;
		for (int i = 0; i < num; i++)
		{
			array[num2] = i * 2;
			array[num2 + 1] = i * 2 + 3;
			array[num2 + 2] = i * 2 + 1;
			array[num2 + 3] = i * 2;
			array[num2 + 4] = i * 2 + 2;
			array[num2 + 5] = i * 2 + 3;
			num2 += 6;
		}
		mesh.Clear();
		mesh.subMeshCount = 1;
		mesh.vertices = verts;
		mesh.uv = uvs;
		mesh.triangles = array;
		mesh.RecalculateNormals();
		if (mcol != null)
		{
			mcol.sharedMesh = null;
			mcol.sharedMesh = mesh;
		}
	}

	private MeshCollider BuildCollider(Vector3[] verts, Vector2[] uvs, int steps, int csteps, bool cw, ref GameObject obj)
	{
		obj = new GameObject();
		MeshCollider meshCollider = obj.AddComponent<MeshCollider>();
		Mesh mesh = new Mesh();
		int[] array = new int[steps * (csteps * 2 * 3)];
		int num = 0;
		num = 0;
		int num2 = csteps + 1;
		if (cw)
		{
			for (int i = 0; i < steps; i++)
			{
				for (int j = 0; j < csteps; j++)
				{
					array[num] = (i + 1) * num2 + j;
					array[num + 1] = (i + 1) * num2 + (j + 1) % num2;
					array[num + 2] = i * num2 + j;
					array[num + 3] = (i + 1) * num2 + (j + 1) % num2;
					array[num + 4] = i * num2 + (j + 1) % num2;
					array[num + 5] = i * num2 + j;
					num += 6;
				}
			}
		}
		else
		{
			for (int k = 0; k < steps; k++)
			{
				for (int l = 0; l < csteps; l++)
				{
					array[num] = (k + 1) * num2 + l;
					array[num + 2] = (k + 1) * num2 + (l + 1) % num2;
					array[num + 1] = k * num2 + l;
					array[num + 3] = (k + 1) * num2 + (l + 1) % num2;
					array[num + 5] = k * num2 + (l + 1) % num2;
					array[num + 4] = k * num2 + l;
					num += 6;
				}
			}
		}
		mesh.Clear();
		mesh.subMeshCount = 1;
		mesh.vertices = verts;
		mesh.uv = uvs;
		mesh.triangles = array;
		mesh.RecalculateNormals();
		if (meshCollider != null)
		{
			meshCollider.sharedMesh = null;
			meshCollider.sharedMesh = mesh;
		}
		return meshCollider;
	}

	private float[,] SmoothTerrain(float[,] data, int Passes, int w, int h, bool[,] hit)
	{
		float[,] array = new float[h, w];
		float num = 0f;
		while (Passes > 0)
		{
			Passes--;
			num = 0f;
			for (int i = 0; i < w; i++)
			{
				for (int j = 0; j < h; j++)
				{
					if (hit[j, i])
					{
						int num2 = 0;
						float num3 = 0f;
						if (i - 1 > 0)
						{
							num3 += data[j, i - 1];
							num2++;
							if (j - 1 > 0)
							{
								num3 += data[j - 1, i - 1];
								num2++;
							}
							if (j + 1 < h)
							{
								num3 += data[j + 1, i - 1];
								num2++;
							}
						}
						if (i + 1 < w)
						{
							num3 += data[j, i + 1];
							num2++;
							if (j - 1 > 0)
							{
								num3 += data[j - 1, i + 1];
								num2++;
							}
							if (j + 1 < h)
							{
								num3 += data[j + 1, i + 1];
								num2++;
							}
						}
						if (j - 1 > 0)
						{
							num3 += data[j - 1, i];
							num2++;
						}
						if (j + 1 < h)
						{
							num3 += data[j + 1, i];
							num2++;
						}
						array[j, i] = (data[j, i] + num3 / (float)num2) * 0.5f;
						if (array[j, i] > num)
						{
							num = array[j, i];
						}
					}
					else
					{
						array[j, i] = data[j, i];
					}
				}
			}
			for (int k = 0; k < w; k++)
			{
				for (int l = 0; l < h; l++)
				{
					data[l, k] = array[l, k];
				}
			}
		}
		return array;
	}
}
