using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Mesh/Mesh Exploder")]
public class MeshExploder : MonoBehaviour
{
	public enum ExplosionType
	{
		Visual,
		Physics
	}

	public struct MeshExplosionPreparation
	{
		public Mesh startMesh;

		public Vector3[] triangleNormals;

		public Vector3[] triangleCentroids;

		public int totalFrontTriangles;

		public Mesh[] physicsMeshes;

		public Quaternion[] rotations;

		public int[] frontMeshTrianglesPerSubMesh;
	}

	public ExplosionType type;

	public float minSpeed = 1f;

	public float maxSpeed = 5f;

	public float minRotationSpeed = 90f;

	public float maxRotationSpeed = 360f;

	public float fadeWaitTime = 0.5f;

	public float fadeTime = 2f;

	public bool useGravity;

	public float colliderThickness = 0.125f;

	public bool useNormals;

	public bool useMeshBoundsCenter;

	public bool allowShadows;

	public bool shadersAlreadyHandleTransparency;

	private MeshExplosionPreparation preparation;

	private static Dictionary<Mesh, MeshExplosionPreparation> cache = new Dictionary<Mesh, MeshExplosionPreparation>();

	private string ComponentName => GetType().Name;

	private void Start()
	{
		MeshFilter component = GetComponent<MeshFilter>();
		if (component == null)
		{
			Debug.LogError(ComponentName + " must be on a GameObject with a MeshFilter component.");
			return;
		}
		Mesh sharedMesh = component.sharedMesh;
		if (sharedMesh == null)
		{
			Debug.LogError("The MeshFilter does not reference a mesh.");
		}
		else
		{
			Prepare(sharedMesh);
		}
	}

	private void PrepareWithoutCaching(Mesh oldMesh)
	{
		Prepare(oldMesh, cachePreparation: false);
	}

	private void Prepare(Mesh oldMesh, bool cachePreparation = true)
	{
		bool flag = type == ExplosionType.Physics;
		if (cache.TryGetValue(oldMesh, out var value) && ((flag && value.physicsMeshes != null) || (!flag && value.startMesh != null)))
		{
			preparation = value;
			return;
		}
		Vector3[] vertices = oldMesh.vertices;
		Vector3[] normals = oldMesh.normals;
		Vector4[] tangents = oldMesh.tangents;
		Vector2[] uv = oldMesh.uv;
		Vector2[] uv2 = oldMesh.uv2;
		Color[] colors = oldMesh.colors;
		int subMeshCount = oldMesh.subMeshCount;
		int[][] array = new int[subMeshCount][];
		int[] array2 = ((!flag) ? null : (value.frontMeshTrianglesPerSubMesh = new int[subMeshCount]));
		int num = 0;
		for (int i = 0; i < subMeshCount; i++)
		{
			int num2 = (array[i] = oldMesh.GetTriangles(i)).Length / 3;
			if (flag)
			{
				array2[i] = num2;
			}
			num += num2;
		}
		value.totalFrontTriangles = num;
		int num3 = num * 2;
		int num4 = ((!flag) ? (num3 * 3) : 6);
		if (num4 > 65534)
		{
			Debug.LogError("The mesh has too many triangles to explode. It must have " + 10922 + " or fewer triangles.");
			return;
		}
		Vector3[] array3 = new Vector3[num4];
		Vector3[] array4 = ((normals != null && normals.Length != 0) ? new Vector3[num4] : null);
		Vector4[] array5 = ((tangents != null && tangents.Length != 0) ? new Vector4[num4] : null);
		Vector2[] array6 = ((uv != null && uv.Length != 0) ? new Vector2[num4] : null);
		Vector2[] array7 = ((uv2 != null && uv2.Length != 0) ? new Vector2[num4] : null);
		Color[] array8 = ((colors != null && colors.Length != 0) ? new Color[num4] : null);
		Vector3[] array9 = (value.triangleCentroids = new Vector3[num]);
		Mesh[] array10 = (value.physicsMeshes = ((!flag) ? null : new Mesh[num]));
		Quaternion[] array11 = (value.rotations = ((!flag) ? null : new Quaternion[num]));
		int[] triangles = ((!flag) ? null : new int[6] { 0, 1, 2, 3, 4, 5 });
		int num5 = 0;
		int num6 = 0;
		Quaternion quaternion = Quaternion.identity;
		for (int j = 0; j < subMeshCount; j++)
		{
			int[] array12 = array[j];
			int num7 = array12.Length;
			int num8 = 0;
			while (num8 < num7)
			{
				int num9 = num8;
				Vector3 vector = Vector3.zero;
				for (int k = 0; k < 2; k++)
				{
					num8 = num9;
					bool flag2 = k == 1;
					while (num8 < num7)
					{
						int num10 = array12[(!flag2) ? num8 : (num9 + (2 - (num8 - num9)))];
						if (flag && num5 % 6 == 0)
						{
							Vector3 vector2 = vertices[num10];
							Vector3 vector3 = vertices[array12[num8 + 1]];
							Vector3 vector4 = vertices[array12[num8 + 2]];
							Vector3 vector5 = Vector3.Cross(vector3 - vector2, vector4 - vector2);
							ref Quaternion reference = ref array11[num6];
							reference = Quaternion.FromToRotation(Vector3.up, vector5);
							quaternion = Quaternion.FromToRotation(vector5, Vector3.up);
							ref Vector3 reference2 = ref array9[num6];
							vector = (reference2 = (vector2 + vector3 + vector4) / 3f);
						}
						if (!flag2)
						{
							ref Vector3 reference3 = ref array3[num5];
							reference3 = quaternion * (vertices[num10] - vector);
							if (array4 != null)
							{
								ref Vector3 reference4 = ref array4[num5];
								reference4 = quaternion * normals[num10];
							}
							if (array5 != null)
							{
								ref Vector4 reference5 = ref array5[num5];
								reference5 = quaternion * tangents[num10];
							}
						}
						if (array6 != null)
						{
							ref Vector2 reference6 = ref array6[num5];
							reference6 = uv[num10];
						}
						if (array7 != null)
						{
							ref Vector2 reference7 = ref array7[num5];
							reference7 = uv2[num10];
						}
						if (array8 != null)
						{
							ref Color reference8 = ref array8[num5];
							reference8 = colors[num10];
						}
						num8++;
						num5++;
						if (num5 % 6 == 0)
						{
							if (flag)
							{
								MeshExplosion.SetBackTriangleVertices(array3, array4, array5, 1);
								Mesh mesh = new Mesh();
								mesh.vertices = array3;
								if (array4 != null)
								{
									mesh.normals = array4;
								}
								if (array5 != null)
								{
									mesh.tangents = array5;
								}
								if (array6 != null)
								{
									mesh.uv = array6;
								}
								if (array7 != null)
								{
									mesh.uv2 = array7;
								}
								if (array8 != null)
								{
									mesh.colors = array8;
								}
								mesh.triangles = triangles;
								array10[num6] = mesh;
								num5 = 0;
							}
							break;
						}
						if (num5 % 3 == 0 && !flag2)
						{
							break;
						}
					}
				}
				num6++;
			}
		}
		Vector3 vector6 = Vector3.zero;
		if (!flag)
		{
			Mesh mesh2 = (value.startMesh = new Mesh());
			mesh2.vertices = array3;
			if (array4 != null)
			{
				mesh2.normals = array4;
			}
			if (array5 != null)
			{
				mesh2.tangents = array5;
			}
			if (array6 != null)
			{
				mesh2.uv = array6;
			}
			if (array7 != null)
			{
				mesh2.uv2 = array7;
			}
			if (array8 != null)
			{
				mesh2.colors = array8;
			}
			mesh2.subMeshCount = subMeshCount;
			num5 = 0;
			for (int l = 0; l < subMeshCount; l++)
			{
				int num11 = array[l].Length * 2;
				int[] array13 = new int[num11];
				int num12 = 0;
				while (num12 < num11)
				{
					array13[num12] = num5;
					num12++;
					num5++;
				}
				mesh2.SetTriangles(array13, l);
			}
			if (useMeshBoundsCenter)
			{
				vector6 = mesh2.bounds.center;
			}
		}
		Vector3[] array14 = (value.triangleNormals = new Vector3[num]);
		int num13 = 0;
		int num14 = 0;
		while (num14 < num)
		{
			Vector3 vector7 = (flag ? array9[num14] : (array9[num14] = (array3[num13] + array3[num13 + 1] + array3[num13 + 2]) / 3f));
			Vector3 vector8;
			if (useNormals && array4 != null)
			{
				if (flag)
				{
					array4 = array10[num14].normals;
					num13 = 0;
				}
				vector8 = ((array4[num13] + array4[num13 + 1] + array4[num13 + 2]) / 3f).normalized;
			}
			else
			{
				vector8 = vector7;
				if (useMeshBoundsCenter)
				{
					vector8 -= vector6;
				}
				vector8.Normalize();
			}
			array14[num14] = vector8;
			num14++;
			num13 += 6;
		}
		preparation = value;
		if (cachePreparation)
		{
			cache[oldMesh] = value;
		}
		if (fadeTime == 0f || shadersAlreadyHandleTransparency)
		{
			return;
		}
		Material[] sharedMaterials = base.renderer.sharedMaterials;
		for (int m = 0; m < sharedMaterials.Length; m++)
		{
			Shader shader = sharedMaterials[m].shader;
			Shader replacementFor = Fade.GetReplacementFor(shader);
			if (replacementFor == null || !replacementFor.name.StartsWith("Transparent/"))
			{
				Debug.LogWarning("Couldn't find an explicitly transparent version of shader '" + shader.name + "' so fading may not work. If the shader does support transparency then this warning can be avoided by enabling the 'Shaders Already Handle Transparency' option.");
			}
		}
	}

	public GameObject Explode()
	{
		if (preparation.startMesh == null && preparation.physicsMeshes == null)
		{
			return null;
		}
		string text = base.gameObject.name + " (Mesh Explosion)";
		if (type == ExplosionType.Physics)
		{
			Mesh[] physicsMeshes = preparation.physicsMeshes;
			Quaternion[] rotations = preparation.rotations;
			float num = maxSpeed - minSpeed;
			float num2 = maxRotationSpeed - minRotationSpeed;
			bool flag = minSpeed == maxSpeed;
			bool flag2 = minRotationSpeed == maxRotationSpeed;
			Vector3[] triangleCentroids = preparation.triangleCentroids;
			Vector3[] triangleNormals = preparation.triangleNormals;
			Transform obj = base.transform;
			Quaternion rotation = obj.rotation;
			Vector3 position = obj.position;
			int num3 = physicsMeshes.Length;
			int num4 = 0;
			Material[] materials = GetComponent<Renderer>().materials;
			int[] frontMeshTrianglesPerSubMesh = preparation.frontMeshTrianglesPerSubMesh;
			int num5 = frontMeshTrianglesPerSubMesh[0];
			Material material = null;
			float waitTime = fadeWaitTime + fadeTime;
			int num6 = 0;
			int num7 = 0;
			while (num6 < num3)
			{
				if (num7 == num5)
				{
					num7 = 0;
					num4++;
					num5 = frontMeshTrianglesPerSubMesh[num4];
					material = null;
				}
				GameObject gameObject = SetUpExplosionPiece(text, material == null);
				if (material == null)
				{
					material = materials[num4];
					if (fadeTime != 0f)
					{
						gameObject.GetComponent<Fade>().materials = new Material[1] { material };
					}
				}
				else if (fadeTime != 0f)
				{
					gameObject.AddComponent<DestroyAfterTime>().waitTime = waitTime;
				}
				gameObject.GetComponent<MeshRenderer>().sharedMaterials = new Material[1] { material };
				Vector3 vector = triangleCentroids[num6];
				Quaternion quaternion = rotations[num6];
				vector = rotation * vector + position;
				quaternion = rotation * quaternion;
				Transform obj2 = gameObject.transform;
				obj2.localPosition = vector;
				obj2.localRotation = quaternion;
				Mesh mesh = preparation.physicsMeshes[num6];
				gameObject.GetComponent<MeshFilter>().mesh = mesh;
				Rigidbody obj3 = gameObject.AddComponent<Rigidbody>();
				obj3.angularVelocity = Quaternion.AngleAxis((!flag2) ? (minRotationSpeed + Random.value * num2) : minRotationSpeed, Random.onUnitSphere).eulerAngles;
				obj3.velocity = ((!flag) ? (minSpeed + Random.value * num) : minSpeed) * triangleNormals[num6];
				BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
				Vector3 size = mesh.bounds.size;
				size.y = colliderThickness;
				boxCollider.size = size;
				obj3.SetDensity(1f);
				num6++;
				num7++;
			}
			return null;
		}
		GameObject obj4 = SetUpExplosionPiece(text);
		obj4.AddComponent<MeshExplosion>().Go(preparation, minSpeed, maxSpeed, minRotationSpeed, maxRotationSpeed, useGravity);
		return obj4;
	}

	private GameObject SetUpExplosionPiece(string name, bool addFade = true)
	{
		GameObject gameObject = new GameObject(name);
		Transform transform = base.transform;
		Transform obj = gameObject.transform;
		obj.localPosition = transform.position;
		obj.localRotation = transform.rotation;
		gameObject.AddComponent<MeshFilter>();
		MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
		Renderer renderer = base.renderer;
		meshRenderer.castShadows = renderer.castShadows;
		meshRenderer.receiveShadows = renderer.receiveShadows;
		meshRenderer.sharedMaterials = renderer.sharedMaterials;
		meshRenderer.useLightProbes = renderer.useLightProbes;
		if (fadeTime != 0f)
		{
			if (addFade)
			{
				Fade fade = gameObject.AddComponent<Fade>();
				fade.waitTime = fadeWaitTime;
				fade.fadeTime = fadeTime;
				fade.replaceShaders = !shadersAlreadyHandleTransparency;
				gameObject.AddComponent<DestroyOnFadeCompletion>();
			}
			if (!allowShadows)
			{
				meshRenderer.castShadows = false;
				meshRenderer.receiveShadows = false;
			}
		}
		return gameObject;
	}
}
