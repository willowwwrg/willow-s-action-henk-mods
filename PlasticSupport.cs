using UnityEngine;

[ExecuteInEditMode]
public class PlasticSupport : SplineFollow
{
	[SerializeField]
	private Mesh stand;

	[SerializeField]
	private Mesh ring;

	[SerializeField]
	private Mesh segment;

	[SerializeField]
	private Mesh doubler;

	[SerializeField]
	private Mesh[] caps;

	private float[] degrees = new float[6] { 0f, 33f, 45f, 56f, 72f, 90f };

	protected Mesh _output;

	public int topPart;

	public bool doublePillar;

	public float skipObjects;

	public float manualHeightOffset;

	public float manualRotationtOffset;

	public bool mirrored;

	public bool noBottom;

	public void RegenMesh()
	{
		MeshData meshData = new MeshData();
		Create(meshData);
		MeshFilter component = GetComponent<MeshFilter>();
		if (!(component != null))
		{
			return;
		}
		if (_output == null)
		{
			_output = new Mesh();
			if (component.sharedMesh != null)
			{
				Object.DestroyImmediate(component.sharedMesh);
			}
			meshData.Apply(_output);
			component.sharedMesh = _output;
		}
		else
		{
			_output.Clear();
			meshData.Apply(_output);
		}
	}

	public void UpdateSupport()
	{
		topPart = Mathf.Clamp(topPart, -(caps.Length - 1), caps.Length - 1);
		Update();
		RegenMesh();
	}

	protected void Create(MeshData intermediateData)
	{
		float num = 6f;
		if (PlayerPrefs.GetInt("QualitySettings_DEPTHEFFECTS", 1) == 0)
		{
			num = 10f;
		}
		Vector3 vector = base.transform.position - base.transform.forward * 1f;
		Debug.DrawRay(vector, Vector3.up * 20f, Color.red);
		float num2 = 0f;
		RaycastHit hitInfo;
		for (int i = 0; (float)i < skipObjects; i++)
		{
			if (Physics.Raycast(vector, Vector3.up, out hitInfo, 1000f))
			{
				num2 += hitInfo.distance + 1f;
				vector += Vector3.up * (hitInfo.distance + 1f);
			}
		}
		if (!Physics.Raycast(vector, Vector3.up, out hitInfo, 1000f))
		{
			return;
		}
		Debug.DrawRay(vector, Vector3.up * hitInfo.distance, Color.green);
		Vector3 vector2 = new Vector3(0f, hitInfo.distance + num2, 0f);
		vector2.y = Mathf.Ceil(vector2.y);
		vector2.y += manualHeightOffset;
		bool flag = false;
		int num3 = 0;
		Debug.DrawRay(hitInfo.point, hitInfo.normal * 3f, Color.yellow);
		float num4 = Vector3.Angle(Vector3.down, hitInfo.normal) + manualRotationtOffset;
		float num5 = 1000000f;
		for (int j = 0; j < degrees.Length; j++)
		{
			float num6 = Mathf.Abs(num4 - degrees[j]);
			if (num6 < num5)
			{
				num3 = j;
				num5 = num6;
			}
		}
		if (Vector3.Dot(hitInfo.normal, base.transform.right) < 0f)
		{
			flag = true;
		}
		if (mirrored)
		{
			flag = !flag;
		}
		Vector3 localScale = base.transform.localScale;
		if (flag)
		{
			localScale.x = -1f;
		}
		else
		{
			localScale.x = 1f;
		}
		base.transform.localScale = localScale;
		Matrix4x4 localOffset = Matrix4x4.TRS(vector2, Quaternion.identity, Vector3.one);
		intermediateData.AppendMesh(caps[num3], localOffset);
		Vector3 pos = vector2 + new Vector3(caps[num3].bounds.max.x, caps[num3].bounds.min.y, caps[num3].bounds.center.z);
		pos.x -= segment.bounds.extents.x * 1.22f;
		int num7 = 1;
		float num8 = 2.66f;
		if (doublePillar)
		{
			pos.y -= 0.1f;
			localOffset = Matrix4x4.TRS(new Vector3(pos.x, pos.y, pos.z), Quaternion.identity, Vector3.one);
			intermediateData.AppendMesh2(doubler, localOffset);
			num7 = 2;
			pos.z += num8;
		}
		for (int k = 0; k < num7; k++)
		{
			float num9 = segment.bounds.size.y * num;
			bool flag2 = true;
			float num10 = stand.bounds.extents.y * 2f;
			float num11 = ring.bounds.extents.y * 2f;
			localOffset = Matrix4x4.TRS(pos, Quaternion.identity, new Vector3(1f, pos.y / segment.bounds.size.y, 1f));
			intermediateData.AppendMesh(segment, localOffset);
			for (float num12 = pos.y; num12 >= num10; num12 -= num9)
			{
				if (!flag2 && num12 - num10 > num11)
				{
					localOffset = Matrix4x4.TRS(new Vector3(pos.x, num12, pos.z), Quaternion.identity, Vector3.one);
					intermediateData.AppendMesh(ring, localOffset);
				}
				flag2 = false;
			}
			if (!noBottom)
			{
				localOffset = Matrix4x4.TRS(new Vector3(pos.x, 0f, pos.z), Quaternion.identity, Vector3.one);
				intermediateData.AppendMesh(stand, localOffset);
			}
			pos.z -= num8 * 2f;
		}
	}

	public static bool IsPlastic(GameObject go)
	{
		if (go.name.Contains("curve_") || go.name.Contains("plastic_"))
		{
			return true;
		}
		return false;
	}
}
