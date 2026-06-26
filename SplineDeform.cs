using UnityEngine;

[ExecuteInEditMode]
public class SplineDeform : SplineObject
{
	public Mesh originalMesh;

	public Mesh originalCollider;

	private Vector3[] baseVertices;

	private Vector3[] baseNormals;

	private Vector4[] baseTangents;

	private Vector3[] baseVertices_collider;

	public bool clickToRegenMesh;

	public float xScale = 1f;

	public float yScale = 1f;

	public float rotation;

	private float prevXscale;

	private float prevYScale;

	private float prevRotation;

	private void OnDestroy()
	{
		if ((bool)GetComponent<MeshFilter>())
		{
			Object.DestroyImmediate(GetComponent<MeshFilter>().sharedMesh);
		}
		if ((bool)GetComponent<MeshCollider>())
		{
			Object.DestroyImmediate(GetComponent<MeshCollider>().sharedMesh);
		}
	}

	public override void Update()
	{
		if ((Application.isPlaying || spline == null) && !createdAtRuntime)
		{
			return;
		}
		ApplyHandleOffset();
		SnapCoordinates();
		RecalcHandlePosition();
		if (!clickToRegenMesh || firstFrame)
		{
			if (originalMesh != null && (bool)GetComponent<MeshFilter>())
			{
				if ((bool)GetComponent<MeshFilter>().sharedMesh && !firstFrame)
				{
					GetComponent<MeshFilter>().sharedMesh = null;
				}
				Mesh mesh = DuplicateMesh(originalMesh);
				mesh.name = "dform" + Random.Range(0, 100000000);
				GetComponent<MeshFilter>().sharedMesh = mesh;
				baseVertices = originalMesh.vertices;
				baseNormals = originalMesh.normals;
				baseTangents = originalMesh.tangents;
			}
			if (originalCollider != null && (bool)GetComponent<MeshCollider>())
			{
				if ((bool)GetComponent<MeshCollider>().sharedMesh && !firstFrame)
				{
					GetComponent<MeshCollider>().sharedMesh = null;
				}
				Mesh mesh2 = DuplicateMesh(originalCollider);
				mesh2.name = "dform_col" + Random.Range(0, 100000000);
				GetComponent<MeshCollider>().sharedMesh = mesh2;
				baseVertices_collider = originalCollider.vertices;
			}
			clickToRegenMesh = true;
		}
		if (splineOffset != prevSplineOffset || xScale != prevXscale || yScale != prevYScale || rotation != prevRotation || firstFrame)
		{
			Mesh sharedMesh = GetComponent<MeshFilter>().sharedMesh;
			DeformMesh(sharedMesh, originalMesh, collisionMesh: false);
			if ((bool)GetComponent<MeshCollider>())
			{
				sharedMesh = GetComponent<MeshCollider>().sharedMesh;
				DeformMesh(sharedMesh, originalCollider, collisionMesh: true);
				GetComponent<MeshCollider>().sharedMesh = null;
				GetComponent<MeshCollider>().sharedMesh = sharedMesh;
			}
		}
		ClearFirstFrame();
		ClearTransform();
		prevSplineOffset = splineOffset;
		prevXscale = xScale;
		prevYScale = yScale;
		prevRotation = rotation;
	}

	public static Mesh DuplicateMesh(Mesh original)
	{
		return new Mesh
		{
			vertices = original.vertices,
			triangles = original.triangles,
			uv = original.uv,
			normals = original.normals,
			colors = original.colors,
			tangents = original.tangents
		};
	}

	private void DeformMesh(Mesh deformedMesh, Mesh original, bool collisionMesh)
	{
		Vector3[] array = new Vector3[deformedMesh.vertices.Length];
		Vector3[] array2 = new Vector3[deformedMesh.normals.Length];
		Vector4[] array3 = new Vector4[deformedMesh.tangents.Length];
		for (int i = 0; i < array.Length; i++)
		{
			Vector3 zero = Vector3.zero;
			zero = ((!collisionMesh) ? new Vector3(baseVertices[i].x * xScale, baseVertices[i].y * yScale, baseVertices[i].z) : new Vector3(baseVertices_collider[i].x * xScale, baseVertices_collider[i].y * yScale, baseVertices_collider[i].z));
			zero = Quaternion.Euler(0f, 0f, rotation) * zero;
			ref Vector3 reference = ref array[i];
			reference = spline.DeformVertex(zero, splineOffset);
		}
		if (!collisionMesh)
		{
			for (int j = 0; j < array2.Length; j++)
			{
				Vector3 vector = new Vector3(baseNormals[j].x * xScale, baseNormals[j].y * yScale, baseNormals[j].z);
				vector = Quaternion.Euler(0f, 0f, rotation) * vector;
				ref Vector3 reference2 = ref array2[j];
				reference2 = spline.DeformDirection(vector, splineOffset.x).normalized;
			}
			for (int k = 0; k < array3.Length; k++)
			{
				Vector3 vector2 = new Vector3(baseTangents[k].x * xScale, baseTangents[k].y * yScale, baseTangents[k].z);
				vector2 = Quaternion.Euler(0f, 0f, rotation) * vector2;
				vector2 = spline.DeformDirection(vector2, splineOffset.x).normalized;
				ref Vector4 reference3 = ref array3[k];
				reference3 = new Vector4(vector2.x, vector2.y, vector2.z, baseTangents[k].w);
			}
		}
		deformedMesh.vertices = array;
		deformedMesh.normals = array2;
		deformedMesh.tangents = array3;
		CheckFlipTriangles(deformedMesh, original);
		if (collisionMesh)
		{
			deformedMesh.RecalculateNormals();
		}
		deformedMesh.RecalculateBounds();
	}

	private void CheckFlipTriangles(Mesh deformedMesh, Mesh original)
	{
		if (Mathf.Sign(xScale * yScale) < 0f)
		{
			int[] triangles = original.triangles;
			for (int i = 0; i < triangles.Length; i += 3)
			{
				int num = triangles[i];
				triangles[i] = triangles[i + 1];
				triangles[i + 1] = num;
			}
			deformedMesh.triangles = triangles;
		}
		else
		{
			deformedMesh.triangles = original.triangles;
		}
	}

	private void OnDrawGizmos()
	{
	}
}
