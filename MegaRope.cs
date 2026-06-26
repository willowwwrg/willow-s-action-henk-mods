using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[AddComponentMenu("Procedural/Rope")]
[ExecuteInEditMode]
[RequireComponent(typeof(MeshRenderer))]
public class MegaRope : MonoBehaviour
{
	public bool Rebuild;

	public MegaAxis axis = MegaAxis.Y;

	public Transform top;

	public Transform bottom;

	public LayerMask layer;

	public float fudge = 2f;

	public float boxsize = 0.1f;

	public float RopeLength;

	public bool fixedends = true;

	public float vel = 1f;

	public int drawsteps = 20;

	public bool DisplayDebug = true;

	public float radius = 1f;

	public MegaShape startShape;

	public Vector3 ropeup = Vector3.up;

	public bool DoCollide;

	public bool SelfCollide;

	public MegaRopeChainMesher chainMesher = new MegaRopeChainMesher();

	public MegaRopeStrandedMesher strandedMesher = new MegaRopeStrandedMesher();

	public MegaRopeHoseMesher hoseMesher = new MegaRopeHoseMesher();

	public MegaRopeObjectMesher objectMesher = new MegaRopeObjectMesher();

	public MegaRopeType type;

	public List<MegaRopeMass> masses = new List<MegaRopeMass>();

	public List<MegaRopeSpring> springs = new List<MegaRopeSpring>();

	public List<MegaRopeConstraint> constraints = new List<MegaRopeConstraint>();

	public MegaRopeSolverType solverType;

	public float spring = 1f;

	public float damp = 1f;

	public float timeStep = 0.01f;

	public float friction = 0.99f;

	public float Mass = 0.1f;

	public float Density = 1f;

	public float DampingRatio = 0.25f;

	public Vector3 gravity = new Vector3(0f, -9.81f, 0f);

	public float airdrag = 0.02f;

	public float stiffspring = 1f;

	public float stiffdamp = 0.1f;

	public AnimationCurve stiffnessCrv = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f));

	public float floorfriction = 0.9f;

	public float bounce = 1f;

	public int points = 10;

	public int iters = 4;

	private Vector3 bsize = new Vector3(2f, 2f, 2f);

	private Matrix4x4 wtm;

	private Matrix4x4 mat = Matrix4x4.identity;

	private PointConstraint1 endcon;

	private Vector3[] masspos;

	private MegaRopeSolver solver = new MegaRopeSolverDefault();

	private MegaRopeSolver verletsolver = new MegaRopeSolverVertlet();

	public Mesh mesh;

	public bool stiffsprings;

	public float rbodyforce = 10f;

	private void InitFromShape(MegaShape shape)
	{
		float length = shape.splines[0].length;
		float num = length * 2f * (float)Math.PI * radius;
		float num2 = Density * num;
		int num3 = (int)(length * 0.75f / radius) + 1;
		float num4 = num2 / (float)num3;
		if (DampingRatio > 1f)
		{
			DampingRatio = 1f;
		}
		damp = DampingRatio * 0.45f * (2f * Mathf.Sqrt(num4 * spring));
		RopeLength = 0f;
		if (masses == null)
		{
			masses = new List<MegaRopeMass>();
		}
		base.transform.position = Vector3.zero;
		masses.Clear();
		float num5 = 0f;
		Vector3 a = Vector3.zero;
		for (int i = 0; i <= num3; i++)
		{
			float alpha = (float)i / (float)num3;
			Vector3 vector = shape.transform.TransformPoint(shape.InterpCurve3D(0, alpha, type: true));
			if (i != 0)
			{
				num5 += Vector3.Distance(a, vector);
				a = vector;
			}
			MegaRopeMass item = new MegaRopeMass(num4, vector);
			masses.Add(item);
		}
		if (springs == null)
		{
			springs = new List<MegaRopeSpring>();
		}
		springs.Clear();
		if (constraints == null)
		{
			constraints = new List<MegaRopeConstraint>();
		}
		constraints.Clear();
		for (int j = 0; j < masses.Count - 1; j++)
		{
			MegaRopeSpring megaRopeSpring = new MegaRopeSpring(j, j + 1, spring, damp, this);
			springs.Add(megaRopeSpring);
			RopeLength += megaRopeSpring.restlen;
			LengthConstraint item2 = new LengthConstraint(j, j + 1, megaRopeSpring.restlen);
			constraints.Add(item2);
		}
		if (stiffsprings)
		{
			int num6 = 2;
			for (int k = 0; k < masses.Count - num6; k++)
			{
				float time = (float)k / (float)masses.Count;
				MegaRopeSpring megaRopeSpring2 = new MegaRopeSpring(k, k + num6, stiffspring * stiffnessCrv.Evaluate(time), stiffdamp * stiffnessCrv.Evaluate(time), this);
				springs.Add(megaRopeSpring2);
				LengthConstraint item3 = new LengthConstraint(k, k + num6, megaRopeSpring2.restlen);
				constraints.Add(item3);
			}
		}
		if ((bool)top)
		{
			top.position = masses[0].pos;
			float magnitude = (masses[0].pos - masses[1].pos).magnitude;
			NewPointConstraint item4 = new NewPointConstraint(0, 1, magnitude, top.transform);
			constraints.Add(item4);
		}
		if ((bool)bottom)
		{
			bottom.position = masses[masses.Count - 1].pos;
			float magnitude2 = (masses[masses.Count - 1].pos - masses[masses.Count - 2].pos).magnitude;
			NewPointConstraint item5 = new NewPointConstraint(masses.Count - 1, masses.Count - 2, magnitude2, bottom.transform);
			constraints.Add(item5);
		}
		if ((bool)top)
		{
			new PointConstraint1
			{
				p1 = 1,
				off = new Vector3(0f, springs[0].restlen, 0f),
				obj = top.transform
			};
		}
		masspos = new Vector3[masses.Count + 2];
		for (int l = 0; l < masses.Count; l++)
		{
			ref Vector3 reference = ref masspos[l + 1];
			reference = masses[l].pos;
		}
		ref Vector3 reference2 = ref masspos[0];
		reference2 = masspos[1];
		ref Vector3 reference3 = ref masspos[masspos.Length - 1];
		reference3 = masspos[masspos.Length - 2];
	}

	public void Init()
	{
		if (startShape != null)
		{
			InitFromShape(startShape);
		}
		else if (!(top == null) && !(bottom == null))
		{
			Vector3 position = top.position;
			Vector3 position2 = bottom.position;
			RopeLength = (position - position2).magnitude;
			if (masses == null)
			{
				masses = new List<MegaRopeMass>();
			}
			base.transform.position = Vector3.zero;
			masses.Clear();
			float m = Mass / (float)(points + 1);
			for (int i = 0; i <= points; i++)
			{
				float t = (float)i / (float)points;
				MegaRopeMass item = new MegaRopeMass(m, Vector3.Lerp(position, position2, t));
				masses.Add(item);
			}
			if (springs == null)
			{
				springs = new List<MegaRopeSpring>();
			}
			springs.Clear();
			if (constraints == null)
			{
				constraints = new List<MegaRopeConstraint>();
			}
			constraints.Clear();
			for (int j = 0; j < masses.Count - 1; j++)
			{
				MegaRopeSpring megaRopeSpring = new MegaRopeSpring(j, j + 1, spring, damp, this);
				springs.Add(megaRopeSpring);
				LengthConstraint item2 = new LengthConstraint(j, j + 1, megaRopeSpring.restlen);
				constraints.Add(item2);
			}
			int num = 2;
			for (int k = 0; k < masses.Count - num; k++)
			{
				float time = (float)k / (float)masses.Count;
				MegaRopeSpring megaRopeSpring2 = new MegaRopeSpring(k, k + num, stiffspring * stiffnessCrv.Evaluate(time), stiffdamp * stiffnessCrv.Evaluate(time), this);
				springs.Add(megaRopeSpring2);
				LengthConstraint item3 = new LengthConstraint(k, k + num, megaRopeSpring2.restlen);
				constraints.Add(item3);
			}
			PointConstraint item4 = new PointConstraint(0, top.transform);
			constraints.Add(item4);
			item4 = new PointConstraint(masses.Count - 1, bottom.transform);
			constraints.Add(item4);
			PointConstraint1 pointConstraint = new PointConstraint1();
			pointConstraint.p1 = 1;
			pointConstraint.off = new Vector3(0f, springs[0].restlen, 0f);
			pointConstraint.obj = top.transform;
			constraints.Add(pointConstraint);
			endcon = pointConstraint;
			masspos = new Vector3[masses.Count + 2];
			for (int l = 0; l < masses.Count; l++)
			{
				ref Vector3 reference = ref masspos[l + 1];
				reference = masses[l].pos;
			}
			ref Vector3 reference2 = ref masspos[0];
			reference2 = masspos[1];
			ref Vector3 reference3 = ref masspos[masspos.Length - 1];
			reference3 = masspos[masspos.Length - 2];
		}
	}

	private void RopeUpdate(float t)
	{
		float num = Time.deltaTime * fudge;
		if (num > 0.05f)
		{
			num = 0.05f;
		}
		MegaRopeSolverType megaRopeSolverType = solverType;
		if (megaRopeSolverType != MegaRopeSolverType.Euler)
		{
			if (megaRopeSolverType == MegaRopeSolverType.Verlet)
			{
				while (num > 0f)
				{
					num -= timeStep;
					verletsolver.doIntegration1(this, timeStep);
				}
			}
		}
		else
		{
			while (num > 0f)
			{
				num -= timeStep;
				solver.doIntegration1(this, timeStep);
			}
		}
		Collide();
	}

	private void Start()
	{
		Init();
	}

	[ContextMenu("Rebuild Rope")]
	public void RebuildRope()
	{
		Init();
	}

	private void LateUpdate()
	{
		if (Rebuild)
		{
			Rebuild = false;
			Init();
		}
		if (masses == null || masses.Count == 0 || masspos == null || masspos.Length == 0)
		{
			return;
		}
		if (endcon != null)
		{
			endcon.active = fixedends;
		}
		RopeUpdate(timeStep);
		for (int i = 0; i < masses.Count; i++)
		{
			ref Vector3 reference = ref masspos[i + 1];
			reference = masses[i].pos;
		}
		ref Vector3 reference2 = ref masspos[0];
		reference2 = masses[0].pos - (masses[1].pos - masses[0].pos);
		ref Vector3 reference3 = ref masspos[masspos.Length - 1];
		reference3 = masses[masses.Count - 1].pos + (masses[masses.Count - 1].pos - masses[masses.Count - 2].pos);
		if (mesh == null)
		{
			Debug.Log("No mesh");
			MeshFilter meshFilter = base.gameObject.GetComponent<MeshFilter>();
			if (meshFilter == null)
			{
				meshFilter = base.gameObject.AddComponent<MeshFilter>();
			}
			meshFilter.sharedMesh = new Mesh();
			MeshRenderer meshRenderer = base.gameObject.GetComponent<MeshRenderer>();
			if (meshRenderer == null)
			{
				meshRenderer = base.gameObject.AddComponent<MeshRenderer>();
			}
			Material[] sharedMaterials = new Material[1];
			meshRenderer.sharedMaterials = sharedMaterials;
			mesh = meshFilter.sharedMesh;
			mesh.name = "Rope Mesh";
		}
		switch (type)
		{
		case MegaRopeType.Rope:
			strandedMesher.BuildMesh(this);
			break;
		case MegaRopeType.Chain:
			chainMesher.BuildMesh(this);
			break;
		case MegaRopeType.Hose:
			hoseMesher.BuildMesh(this);
			break;
		case MegaRopeType.Object:
			objectMesher.BuildMesh(this);
			break;
		}
		if ((bool)top)
		{
			_ = (bool)top.rigidbody;
		}
		if (!bottom)
		{
			return;
		}
		Rigidbody rigidbody = bottom.rigidbody;
		if ((bool)rigidbody)
		{
			float num = (springs[springs.Count - 1].len - springs[springs.Count - 1].restlen) * rbodyforce;
			if (num > 0f)
			{
				Vector3 normalized = (masses[springs[springs.Count - 1].p1].pos - masses[springs[springs.Count - 1].p2].pos).normalized;
				rigidbody.AddForce(normalized * num);
			}
		}
	}

	public void OnDrawGizmos()
	{
		Display();
	}

	private void Display()
	{
		if (masses == null || masses.Count == 0 || masspos == null || masspos.Length == 0 || !DisplayDebug)
		{
			return;
		}
		DrawSpline(drawsteps, vel * 0f);
		Color green = Color.green;
		green.a = 0.5f;
		Gizmos.color = green;
		for (int i = 0; i < masses.Count; i++)
		{
			Gizmos.DrawSphere(masses[i].pos, radius);
		}
		Vector3[] vertices = mesh.vertices;
		for (int j = 0; j < vertices.Length; j++)
		{
		}
		if (type != MegaRopeType.Rope)
		{
			return;
		}
		Vector3 to = Vector3.zero;
		for (int k = 18; k < vertices.Length; k += 9)
		{
			Gizmos.color = Color.red;
			if (k > 18)
			{
				Gizmos.DrawLine(vertices[k], to);
			}
			Gizmos.DrawCube(vertices[k], bsize * boxsize * 0.1f);
			to = vertices[k];
		}
	}

	public Vector3 Interp1(float t)
	{
		int num = masses.Count - 3;
		int num2 = Mathf.Min(Mathf.FloorToInt(t * (float)num), num - 1);
		float num3 = t * (float)num - (float)num2;
		Vector3 pos = masses[num2].pos;
		Vector3 pos2 = masses[num2 + 1].pos;
		Vector3 pos3 = masses[num2 + 2].pos;
		Vector3 pos4 = masses[num2 + 3].pos;
		return 0.5f * ((-pos + 3f * pos2 - 3f * pos3 + pos4) * (num3 * num3 * num3) + (2f * pos - 5f * pos2 + 4f * pos3 - pos4) * (num3 * num3) + (-pos + pos3) * num3 + 2f * pos2);
	}

	public Vector3 Velocity1(float t)
	{
		int num = masses.Count - 3;
		int num2 = Mathf.Min(Mathf.FloorToInt(t * (float)num), num - 1);
		float num3 = t * (float)num - (float)num2;
		Vector3 pos = masses[num2].pos;
		Vector3 pos2 = masses[num2 + 1].pos;
		Vector3 pos3 = masses[num2 + 2].pos;
		Vector3 pos4 = masses[num2 + 3].pos;
		return 1.5f * (-pos + 3f * pos2 - 3f * pos3 + pos4) * (num3 * num3) + (2f * pos - 5f * pos2 + 4f * pos3 - pos4) * num3 + 0.5f * pos3 - 0.5f * pos;
	}

	public Vector3 Interp(float t)
	{
		int num = masspos.Length - 3;
		int num2 = Mathf.Min(Mathf.FloorToInt(t * (float)num), num - 1);
		float num3 = t * (float)num - (float)num2;
		Vector3 vector = masspos[num2];
		Vector3 vector2 = masspos[num2 + 1];
		Vector3 vector3 = masspos[num2 + 2];
		Vector3 vector4 = masspos[num2 + 3];
		return 0.5f * ((-vector + 3f * vector2 - 3f * vector3 + vector4) * (num3 * num3 * num3) + (2f * vector - 5f * vector2 + 4f * vector3 - vector4) * (num3 * num3) + (-vector + vector3) * num3 + 2f * vector2);
	}

	public Vector3 Velocity(float t)
	{
		int num = masspos.Length - 3;
		int num2 = Mathf.Min(Mathf.FloorToInt(t * (float)num), num - 1);
		float num3 = t * (float)num - (float)num2;
		Vector3 vector = masspos[num2];
		Vector3 vector2 = masspos[num2 + 1];
		Vector3 vector3 = masspos[num2 + 2];
		Vector3 vector4 = masspos[num2 + 3];
		return 1.5f * (-vector + 3f * vector2 - 3f * vector3 + vector4) * (num3 * num3) + (2f * vector - 5f * vector2 + 4f * vector3 - vector4) * num3 + 0.5f * vector3 - 0.5f * vector;
	}

	private void DrawSpline(int steps, float t)
	{
		if (masses == null || masses.Count == 0 || masspos == null || masspos.Length == 0)
		{
			return;
		}
		Vector3 to = Interp(0f);
		for (int i = 1; i <= steps; i++)
		{
			if ((i & 1) == 1)
			{
				Gizmos.color = Color.white;
			}
			else
			{
				Gizmos.color = Color.black;
			}
			float t2 = (float)i / (float)steps;
			Vector3 vector = Interp(t2);
			Gizmos.DrawLine(vector, to);
			to = vector;
		}
		Gizmos.color = Color.blue;
		Vector3 vector2 = Interp(t);
		Gizmos.DrawLine(vector2, vector2 + Velocity(t));
	}

	public Matrix4x4 GetDeformMat(float percent)
	{
		Vector3 vector = Interp(percent);
		Quaternion q = Quaternion.LookRotation(vector + Velocity(percent) - vector, ropeup);
		wtm.SetTRS(vector, q, Vector3.one);
		wtm = mat * wtm;
		return wtm;
	}

	public Matrix4x4 CalcFrame(Vector3 T, ref Vector3 N, ref Vector3 B)
	{
		Matrix4x4 identity = Matrix4x4.identity;
		N = Vector3.Cross(B, T).normalized;
		B = Vector3.Cross(T, N).normalized;
		identity.SetColumn(2, T);
		identity.SetColumn(0, N);
		identity.SetColumn(1, B);
		return identity;
	}

	public Matrix4x4 GetDeformMat(float percent, Vector3 up)
	{
		Vector3 vector = Interp(percent);
		Quaternion q = Quaternion.LookRotation(vector + Velocity(percent) - vector, up);
		wtm.SetTRS(vector, q, Vector3.one);
		wtm = mat * wtm;
		return wtm;
	}

	private Matrix4x4 GetLinkMat(float alpha, float last)
	{
		Vector3 vector = Interp(last);
		Quaternion q = Quaternion.LookRotation(Interp(alpha) - vector, ropeup);
		wtm.SetTRS(vector, q, Vector3.one);
		wtm = mat * wtm;
		return wtm;
	}

	private Quaternion GetLinkQuat(float alpha, float last, out Vector3 ps)
	{
		ps = Interp(last);
		return Quaternion.LookRotation(Interp(alpha) - ps, ropeup);
	}

	private void Collide()
	{
		if (!DoCollide)
		{
			return;
		}
		int num = masses.Count - 1;
		for (int i = 0; i < num; i++)
		{
			for (int j = i + 2; j < num; j++)
			{
			}
		}
	}
}
