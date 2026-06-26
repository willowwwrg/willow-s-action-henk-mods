using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MegaShapeFollow : MonoBehaviour
{
	public float Alpha;

	public float distance;

	public MegaFollowMode mode;

	public float tangentDist = 0.001f;

	public float speed;

	public bool rot;

	public float time;

	public float ctime;

	public float gizmodetail = 100f;

	public bool drawpath = true;

	public Vector3 rotate = Vector3.zero;

	public Vector3 offset = Vector3.zero;

	public bool lateupdate;

	public MegaRepeatMode loopmode;

	public List<MegaPathTarget> Targets = new List<MegaPathTarget>();

	[ContextMenu("Help")]
	public void Help()
	{
		Application.OpenURL("http://www.west-racing.com/mf/?page_id=2891");
	}

	public Vector3 GetPos(float alpha)
	{
		Vector3 zero = Vector3.zero;
		if (Targets != null && Targets.Count > 0)
		{
			float num = 0f;
			for (int i = 0; i < Targets.Count; i++)
			{
				num += Targets[i].Weight;
			}
			if (num <= 0f)
			{
				num = Targets[0].Weight;
			}
			if (num == 0f)
			{
				return zero;
			}
			for (int j = 0; j < Targets.Count; j++)
			{
				MegaShape shape = Targets[j].shape;
				float alpha2 = (alpha + Targets[j].offset) * Targets[j].modifier;
				if (shape != null)
				{
					zero += shape.transform.TransformPoint(shape.InterpCurve3D(Targets[j].curve, alpha2, shape.normalizedInterp)) * (Targets[j].Weight / num);
				}
			}
		}
		return zero;
	}

	public Vector3 GetPosDist(float dist)
	{
		Vector3 zero = Vector3.zero;
		if (Targets != null && Targets.Count > 0)
		{
			float num = 0f;
			for (int i = 0; i < Targets.Count; i++)
			{
				num += Targets[i].Weight;
			}
			if (num <= 0f)
			{
				num = Targets[0].Weight;
			}
			if (num == 0f)
			{
				return zero;
			}
			for (int j = 0; j < Targets.Count; j++)
			{
				MegaShape shape = Targets[j].shape;
				if (shape != null)
				{
					float num2 = dist / Targets[j].shape.splines[Targets[j].curve].length;
					num2 = (num2 + Targets[j].offset) * Targets[j].modifier;
					zero += shape.transform.TransformPoint(shape.InterpCurve3D(Targets[j].curve, num2, shape.normalizedInterp)) * (Targets[j].Weight / num);
				}
			}
		}
		return zero;
	}

	public void Draw()
	{
		float num = 0f;
		int num2 = 0;
		Vector3 vector = GetPos(0f);
		while (num < 1f)
		{
			num += 1f / gizmodetail;
			if ((num2++ & 1) == 1)
			{
				Gizmos.color = Color.blue;
			}
			else
			{
				Gizmos.color = Color.yellow;
			}
			Vector3 pos = GetPos(num);
			Gizmos.DrawLine(vector, pos);
			vector = pos;
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (!lateupdate)
		{
			DoUpdate();
		}
	}

	private void LateUpdate()
	{
		if (lateupdate)
		{
			DoUpdate();
		}
	}

	private void DoUpdate()
	{
		float num = Alpha;
		float num2 = distance;
		if (time > 0f)
		{
			ctime += Time.deltaTime;
			Alpha = ctime / time;
			num = Alpha;
			switch (loopmode)
			{
			case MegaRepeatMode.Clamp:
				num = Mathf.Clamp01(Alpha);
				break;
			case MegaRepeatMode.Loop:
				num = Mathf.Repeat(Alpha, 1f);
				break;
			case MegaRepeatMode.PingPong:
				num = Mathf.PingPong(Alpha, 1f);
				break;
			}
			distance = num * Targets[0].shape.splines[Targets[0].curve].length;
		}
		else if (mode == MegaFollowMode.Distance)
		{
			distance += speed * Time.deltaTime;
			num2 = distance;
			switch (loopmode)
			{
			case MegaRepeatMode.Clamp:
				num2 = Mathf.Clamp(num2, 0f, Targets[0].shape.splines[Targets[0].curve].length);
				break;
			case MegaRepeatMode.Loop:
				num2 = Mathf.Repeat(num2, Targets[0].shape.splines[Targets[0].curve].length);
				break;
			case MegaRepeatMode.PingPong:
				num2 = Mathf.PingPong(num2, Targets[0].shape.splines[Targets[0].curve].length);
				break;
			}
		}
		else
		{
			if (speed != 0f)
			{
				Alpha += speed / 1000f * Time.deltaTime;
			}
			num = Alpha;
			switch (loopmode)
			{
			case MegaRepeatMode.Clamp:
				num = Mathf.Clamp01(Alpha);
				break;
			case MegaRepeatMode.Loop:
				num = Mathf.Repeat(Alpha, 1f);
				break;
			case MegaRepeatMode.PingPong:
				num = Mathf.PingPong(Alpha, 1f);
				break;
			}
		}
		if (Targets == null || Targets.Count <= 0)
		{
			return;
		}
		if (Targets[0].shape != null)
		{
			if (mode == MegaFollowMode.Distance)
			{
				Alpha = distance / Targets[0].shape.splines[Targets[0].curve].length;
				base.transform.position = GetPosDist(num2) + offset;
			}
			else
			{
				distance = num * Targets[0].shape.splines[Targets[0].curve].length;
				base.transform.position = GetPos(num) + offset;
			}
		}
		if (rot)
		{
			Vector3 zero = Vector3.zero;
			zero = ((mode != MegaFollowMode.Distance) ? (GetPosDist(num2 + tangentDist) + offset) : (GetPos(num + tangentDist) + offset));
			Quaternion quaternion = Quaternion.LookRotation(base.transform.position - zero);
			Quaternion quaternion2 = Quaternion.Euler(rotate);
			base.transform.rotation = quaternion * quaternion2;
		}
	}

	public MegaPathTarget AddTarget(MegaShape shape, int curve, float weight, float modifier, float offset)
	{
		MegaPathTarget megaPathTarget = new MegaPathTarget();
		megaPathTarget.shape = shape;
		megaPathTarget.Weight = weight;
		megaPathTarget.curve = curve;
		megaPathTarget.modifier = modifier;
		megaPathTarget.offset = offset;
		Targets.Add(megaPathTarget);
		return megaPathTarget;
	}

	public MegaPathTarget AddTarget(MegaShape shape, int curve, float weight)
	{
		MegaPathTarget megaPathTarget = new MegaPathTarget();
		megaPathTarget.shape = shape;
		megaPathTarget.Weight = weight;
		megaPathTarget.curve = curve;
		megaPathTarget.modifier = 1f;
		megaPathTarget.offset = 0f;
		Targets.Add(megaPathTarget);
		return megaPathTarget;
	}

	public int NumTargets()
	{
		return Targets.Count;
	}

	public MegaPathTarget GetTarget(int index)
	{
		if (index >= 0 && index < Targets.Count)
		{
			return Targets[index];
		}
		return null;
	}

	public void DeleteTarget(int index)
	{
		if (index >= 0 && index < Targets.Count)
		{
			Targets.RemoveAt(index);
		}
	}

	public void DeleteTarget(MegaPathTarget target)
	{
		Targets.Remove(target);
	}
}
