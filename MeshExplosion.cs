using UnityEngine;

public class MeshExplosion : MonoBehaviour
{
	private MeshExploder.MeshExplosionPreparation preparation;

	private Mesh mesh;

	private Vector3[] vertices;

	private Vector3[] normals;

	private Vector4[] tangents;

	private Vector3[] triangleRotationAxes;

	private float[] triangleSpeeds;

	private float[] triangleRotationSpeeds;

	private Vector3[] triangleCurrentCentroids;

	private bool useGravity;

	private float explosionTime;

	private Transform thisTransform;

	public void Go(MeshExploder.MeshExplosionPreparation prep, float minSpeed, float maxSpeed, float minRotationSpeed, float maxRotationSpeed, bool useGravity)
	{
		preparation = prep;
		int totalFrontTriangles = prep.totalFrontTriangles;
		triangleRotationAxes = new Vector3[totalFrontTriangles];
		triangleSpeeds = new float[totalFrontTriangles];
		triangleRotationSpeeds = new float[totalFrontTriangles];
		float num = maxSpeed - minSpeed;
		float num2 = maxRotationSpeed - minRotationSpeed;
		bool flag = minSpeed == maxSpeed;
		bool flag2 = minRotationSpeed == maxRotationSpeed;
		this.useGravity = useGravity;
		explosionTime = 0f;
		thisTransform = base.transform;
		for (int i = 0; i < totalFrontTriangles; i++)
		{
			ref Vector3 reference = ref triangleRotationAxes[i];
			reference = Random.onUnitSphere;
			triangleSpeeds[i] = ((!flag) ? (minSpeed + Random.value * num) : minSpeed);
			triangleRotationSpeeds[i] = ((!flag2) ? (minRotationSpeed + Random.value * num2) : minRotationSpeed);
		}
		GetComponent<MeshFilter>().mesh = (mesh = (Mesh)Object.Instantiate(prep.startMesh));
		triangleCurrentCentroids = (Vector3[])prep.triangleCentroids.Clone();
		vertices = mesh.vertices;
		normals = mesh.normals;
		tangents = mesh.tangents;
		Update();
	}

	private void Update()
	{
		if (vertices == null)
		{
			string text = GetType().Name;
			Debug.LogError("The " + text + " component should not be used directly. Add the " + typeof(MeshExploder).Name + " component to your object and it will take care of creating the explosion object and adding the " + text + " component.");
			base.enabled = false;
			return;
		}
		float deltaTime = Time.deltaTime;
		explosionTime += deltaTime;
		if (tangents != null && tangents.Length == 0)
		{
			tangents = null;
		}
		Vector3[] triangleNormals = preparation.triangleNormals;
		Vector3 vector = ((!useGravity) ? default(Vector3) : thisTransform.InverseTransformDirection(Physics.gravity));
		int num = vertices.Length / 3 / 2;
		int num2 = 0;
		int num3 = 0;
		while (num3 < num)
		{
			float num4 = triangleSpeeds[num3] * deltaTime;
			float angle = triangleRotationSpeeds[num3] * deltaTime;
			Vector3 vector2 = triangleNormals[num3] * num4;
			if (useGravity)
			{
				vector2 += explosionTime * vector * deltaTime;
			}
			Quaternion quaternion = Quaternion.AngleAxis(angle, triangleRotationAxes[num3]);
			Vector3 vector3 = triangleCurrentCentroids[num3];
			Vector3 vector4 = vector3 + vector2;
			for (int i = 0; i < 3; i++)
			{
				int num5 = num2 + i;
				ref Vector3 reference = ref vertices[num5];
				reference = quaternion * (vertices[num5] - vector3) + vector4;
				if (normals != null)
				{
					ref Vector3 reference2 = ref normals[num5];
					reference2 = quaternion * normals[num5];
				}
				if (tangents != null)
				{
					ref Vector4 reference3 = ref tangents[num5];
					reference3 = quaternion * tangents[num5];
				}
			}
			triangleCurrentCentroids[num3] = vector4;
			num3++;
			num2 += 6;
		}
		SetBackTriangleVertices(vertices, normals, tangents, preparation.totalFrontTriangles);
		mesh.vertices = vertices;
		mesh.normals = normals;
		mesh.tangents = tangents;
		mesh.RecalculateBounds();
	}

	public static void SetBackTriangleVertices(Vector3[] vertices, Vector3[] normals, Vector4[] tangents, int totalFrontTriangles)
	{
		int num = 0;
		for (int i = 0; i < totalFrontTriangles; i++)
		{
			int num2 = num;
			num += 3;
			int num3 = 0;
			while (num3 < 3)
			{
				int num4 = 2 - num3 + num2;
				ref Vector3 reference = ref vertices[num];
				reference = vertices[num4];
				if (normals != null)
				{
					ref Vector3 reference2 = ref normals[num];
					reference2 = -normals[num4];
				}
				if (tangents != null)
				{
					ref Vector4 reference3 = ref tangents[num];
					reference3 = -tangents[num4];
				}
				num3++;
				num++;
			}
		}
	}
}
