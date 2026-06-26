using UnityEngine;

[AddComponentMenu("MegaShapes/Track")]
[ExecuteInEditMode]
public class MegaTracks : MonoBehaviour
{
	public MegaShape shape;

	public int curve;

	public float start;

	public Vector3 rotate = Vector3.zero;

	public bool displayspline = true;

	public Vector3 linkOff = Vector3.zero;

	public Vector3 linkScale = Vector3.one;

	public Vector3 linkOff1 = new Vector3(0f, 1f, 0f);

	public Vector3 linkPivot = Vector3.zero;

	public Vector3 linkRot = Vector3.zero;

	public GameObject LinkObj;

	public bool RandomOrder;

	public float LinkSize = 1f;

	public bool dolateupdate;

	public bool animate;

	public float speed;

	public Vector3 trackup = Vector3.up;

	public bool InvisibleUpdate;

	public int seed;

	public bool rebuild = true;

	private bool visible = true;

	public bool randRot;

	private float lastpos = -1f;

	private Matrix4x4 tm;

	private Matrix4x4 wtm;

	private int linkcount;

	private int remain;

	private Transform[] linkobjs;

	[ContextMenu("Help")]
	public void Help()
	{
		Application.OpenURL("http://www.west-racing.com/mf/?page_id=3538");
	}

	private void Awake()
	{
		lastpos = -1f;
		rebuild = true;
		Rebuild();
	}

	private void Reset()
	{
		Rebuild();
	}

	public void Rebuild()
	{
		BuildTrack();
	}

	private void Update()
	{
		if (animate)
		{
			start += speed * Time.deltaTime;
		}
		if ((visible || InvisibleUpdate) && !dolateupdate)
		{
			BuildTrack();
		}
	}

	private void LateUpdate()
	{
		if ((visible || InvisibleUpdate) && dolateupdate)
		{
			BuildTrack();
		}
	}

	private void BuildTrack()
	{
		if (shape != null && LinkObj != null && (rebuild || lastpos != start))
		{
			rebuild = false;
			lastpos = start;
			BuildObjectLinks(shape);
		}
	}

	private void OnBecameVisible()
	{
		visible = true;
	}

	private void OnBecameInvisible()
	{
		visible = false;
	}

	private void InitLinkObjects(MegaShape path)
	{
		if (LinkObj == null)
		{
			return;
		}
		float length = path.splines[curve].length;
		float num = (linkOff1.y - linkOff.y) * linkScale.x * LinkSize;
		linkcount = (int)(length / num);
		for (int i = linkcount; i < base.gameObject.transform.childCount; i++)
		{
			GameObject obj = base.gameObject.transform.GetChild(i).gameObject;
			if (Application.isEditor)
			{
				Object.DestroyImmediate(obj);
			}
			else
			{
				Object.Destroy(obj);
			}
		}
		linkobjs = new Transform[linkcount];
		if (linkcount > base.gameObject.transform.childCount)
		{
			for (int j = 0; j < base.gameObject.transform.childCount; j++)
			{
				GameObject gameObject = base.gameObject.transform.GetChild(j).gameObject;
				gameObject.SetActive(value: true);
				linkobjs[j] = gameObject.transform;
			}
			for (int k = base.gameObject.transform.childCount; k < linkcount; k++)
			{
				GameObject gameObject2 = new GameObject();
				gameObject2.name = "Link";
				GameObject linkObj = LinkObj;
				if ((bool)linkObj)
				{
					MeshRenderer component = linkObj.GetComponent<MeshRenderer>();
					Mesh sharedMesh = MegaUtils.GetSharedMesh(linkObj);
					MeshRenderer meshRenderer = gameObject2.AddComponent<MeshRenderer>();
					gameObject2.AddComponent<MeshFilter>().sharedMesh = sharedMesh;
					meshRenderer.sharedMaterial = component.sharedMaterial;
					gameObject2.transform.parent = base.gameObject.transform;
					linkobjs[k] = gameObject2.transform;
				}
			}
		}
		else
		{
			for (int l = 0; l < linkcount; l++)
			{
				GameObject gameObject3 = base.gameObject.transform.GetChild(l).gameObject;
				gameObject3.SetActive(value: true);
				linkobjs[l] = gameObject3.transform;
			}
		}
		Random.seed = 0;
		for (int m = 0; m < linkcount; m++)
		{
			GameObject linkObj2 = LinkObj;
			GameObject gameObject4 = base.gameObject.transform.GetChild(m).gameObject;
			MeshRenderer component2 = linkObj2.GetComponent<MeshRenderer>();
			Mesh sharedMesh2 = MegaUtils.GetSharedMesh(linkObj2);
			MeshRenderer component3 = gameObject4.GetComponent<MeshRenderer>();
			gameObject4.GetComponent<MeshFilter>().sharedMesh = sharedMesh2;
			component3.sharedMaterials = component2.sharedMaterials;
		}
	}

	private void BuildObjectLinks(MegaShape path)
	{
		float length = path.splines[curve].length;
		if (LinkSize < 0.1f)
		{
			LinkSize = 0.1f;
		}
		float num = (linkOff1.y - linkOff.y) * linkScale.x * LinkSize;
		if ((int)(length / num) != linkcount)
		{
			InitLinkObjects(path);
		}
		Quaternion identity = Quaternion.identity;
		identity = Quaternion.Euler(rotate);
		float num2 = start * 0.01f;
		Vector3 pos = linkPivot * linkScale.x * LinkSize;
		float last = num2;
		Vector3 ps = Vector3.zero;
		Matrix4x4 matrix4x = Matrix4x4.TRS(pos, identity, Vector3.one);
		Vector3 euler = Vector3.zero;
		Quaternion identity2 = Quaternion.identity;
		Random.seed = seed;
		for (int i = 0; i < linkcount; i++)
		{
			float num3 = (float)(i + 1) / (float)linkcount + num2;
			Quaternion linkQuat = GetLinkQuat(num3, last, out ps, path);
			last = num3;
			Quaternion quaternion = Quaternion.Euler(euler);
			identity2 = linkQuat * identity * quaternion;
			if ((bool)linkobjs[i])
			{
				Matrix4x4 matrix4x2 = Matrix4x4.TRS(ps, linkQuat, Vector3.one) * matrix4x;
				linkobjs[i].localPosition = matrix4x2.GetColumn(3);
				linkobjs[i].localRotation = identity2;
				linkobjs[i].localScale = linkScale * LinkSize;
			}
			if (randRot)
			{
				euler = (int)(Random.Range(0f, 1f) * (float)(int)(360f / MegaUtils.LargestValue1(linkRot))) * linkRot;
			}
			else
			{
				euler += linkRot;
			}
		}
	}

	private Quaternion GetLinkQuat(float alpha, float last, out Vector3 ps, MegaShape path)
	{
		int k = 0;
		ps = path.splines[curve].InterpCurve3D(last, shape.normalizedInterp, ref k);
		return Quaternion.LookRotation(path.splines[curve].InterpCurve3D(alpha, shape.normalizedInterp, ref k) - ps, trackup);
	}
}
