using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MegaRopeChainMesher : MegaRopeMesher
{
	public Vector3 linkOff = Vector3.zero;

	public Vector3 linkScale = Vector3.one;

	public Vector3 linkOff1 = new Vector3(0f, 0.1f, 0f);

	public Vector3 linkPivot = Vector3.zero;

	public List<GameObject> LinkObj1 = new List<GameObject>();

	public bool RandomOrder;

	public float LinkSize = 1f;

	public Vector3 LinkRot = new Vector3(90f, -90f, 0f);

	public Vector3 LinkRot1 = new Vector3(0f, -90f, 0f);

	public int seed;

	private Matrix4x4 tm;

	private Matrix4x4 wtm;

	private Matrix4x4 mat;

	private int linkcount;

	private int remain;

	private Transform[] linkobjs;

	public bool rebuild;

	[ContextMenu("Rebuild")]
	public void Rebuild(MegaRope rope)
	{
		BuildMesh(rope);
	}

	public override void BuildMesh(MegaRope rope)
	{
		BuildObjectLinks(rope);
	}

	private void InitLinkObjects(MegaRope rope)
	{
		rebuild = false;
		float ropeLength = rope.RopeLength;
		float num = (linkOff1.y - linkOff.y) * linkScale.x * LinkSize;
		linkcount = (int)(ropeLength / num);
		for (int i = linkcount; i < rope.gameObject.transform.childCount; i++)
		{
			GameObject gameObject = rope.gameObject.transform.GetChild(i).gameObject;
			if (Application.isEditor && !Application.isPlaying)
			{
				UnityEngine.Object.DestroyImmediate(gameObject);
			}
			else
			{
				UnityEngine.Object.Destroy(gameObject);
			}
		}
		if (LinkObj1 == null || LinkObj1.Count == 0)
		{
			return;
		}
		linkobjs = new Transform[linkcount];
		int num2 = 0;
		if (linkcount > rope.gameObject.transform.childCount)
		{
			for (int j = 0; j < rope.gameObject.transform.childCount; j++)
			{
				GameObject gameObject2 = rope.gameObject.transform.GetChild(j).gameObject;
				linkobjs[j] = gameObject2.transform;
			}
			for (int k = rope.gameObject.transform.childCount; k < linkcount; k++)
			{
				num2 = ((!RandomOrder) ? ((num2 + 1) % LinkObj1.Count) : ((int)(UnityEngine.Random.value * (float)LinkObj1.Count)));
				GameObject gameObject3 = LinkObj1[num2];
				if ((bool)gameObject3)
				{
					GameObject gameObject4 = new GameObject();
					gameObject4.name = "Link";
					MeshRenderer component = gameObject3.GetComponent<MeshRenderer>();
					Mesh sharedMesh = MegaUtils.GetSharedMesh(gameObject3);
					MeshRenderer meshRenderer = gameObject4.AddComponent<MeshRenderer>();
					gameObject4.AddComponent<MeshFilter>().sharedMesh = sharedMesh;
					meshRenderer.sharedMaterial = component.sharedMaterial;
					gameObject4.transform.parent = rope.gameObject.transform;
					linkobjs[k] = gameObject4.transform;
				}
			}
		}
		else
		{
			for (int l = 0; l < linkcount; l++)
			{
				GameObject gameObject5 = rope.gameObject.transform.GetChild(l).gameObject;
				gameObject5.SetActive(value: true);
				linkobjs[l] = gameObject5.transform;
			}
		}
		UnityEngine.Random.seed = seed;
		num2 = 0;
		for (int m = 0; m < linkcount; m++)
		{
			num2 = ((!RandomOrder) ? ((num2 + 1) % LinkObj1.Count) : ((int)(UnityEngine.Random.value * (float)LinkObj1.Count)));
			GameObject gameObject6 = LinkObj1[num2];
			if ((bool)gameObject6)
			{
				GameObject gameObject7 = rope.gameObject.transform.GetChild(m).gameObject;
				MeshRenderer component2 = gameObject6.GetComponent<MeshRenderer>();
				Mesh sharedMesh2 = MegaUtils.GetSharedMesh(gameObject6);
				MeshRenderer component3 = gameObject7.GetComponent<MeshRenderer>();
				gameObject7.GetComponent<MeshFilter>().sharedMesh = sharedMesh2;
				component3.sharedMaterial = component2.sharedMaterial;
			}
		}
	}

	private void BuildObjectLinks(MegaRope rope)
	{
		float ropeLength = rope.RopeLength;
		if (LinkSize < 0.1f)
		{
			LinkSize = 0.1f;
		}
		float num = (linkOff1.y - linkOff.y) * linkScale.x * LinkSize;
		if ((int)(ropeLength / num) != linkcount || rebuild)
		{
			InitLinkObjects(rope);
		}
		if (LinkObj1 == null || LinkObj1.Count == 0)
		{
			return;
		}
		Quaternion quaternion = Quaternion.Euler(LinkRot);
		Quaternion quaternion2 = Quaternion.Euler(LinkRot1);
		Vector3 vector = linkPivot;
		vector.Scale(linkScale * LinkSize);
		float last = 0f;
		Vector3 ps = Vector3.zero;
		for (int i = 0; i < linkcount; i++)
		{
			if ((bool)linkobjs[i])
			{
				float num2 = (float)(i + 1) / (float)linkcount;
				Quaternion linkQuat = GetLinkQuat(num2, last, out ps, rope);
				last = num2;
				if ((i & 1) == 1)
				{
					linkQuat *= quaternion;
				}
				else
				{
					linkQuat *= quaternion2;
				}
				linkobjs[i].position = ps;
				linkobjs[i].rotation = linkQuat;
				linkobjs[i].localScale = linkScale * LinkSize;
			}
		}
	}

	private Quaternion GetLinkQuat(float alpha, float last, out Vector3 ps, MegaRope rope)
	{
		ps = rope.Interp(last);
		return Quaternion.LookRotation(rope.Interp(alpha) - ps, rope.ropeup);
	}
}
