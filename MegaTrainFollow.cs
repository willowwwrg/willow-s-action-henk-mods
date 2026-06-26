using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MegaTrainFollow : MonoBehaviour
{
	public MegaShape path;

	public int curve;

	public List<MegaCarriage> carriages = new List<MegaCarriage>();

	public float distance;

	public float speed;

	public bool showrays;

	private void Update()
	{
		distance += speed * Time.deltaTime;
		if (!path)
		{
			return;
		}
		float num = distance;
		MegaSpline megaSpline = path.splines[curve];
		for (int i = 0; i < carriages.Count; i++)
		{
			float num2 = num / megaSpline.length;
			MegaCarriage megaCarriage = carriages[i];
			megaCarriage.b1 = path.transform.TransformPoint(path.InterpCurve3D(curve, num2, type: true));
			float num3 = (num - megaCarriage.length) / megaSpline.length;
			megaCarriage.b2 = path.transform.TransformPoint(path.InterpCurve3D(curve, num3, type: true));
			megaCarriage.cp = (megaCarriage.b1 + megaCarriage.b2) * 0.5f;
			if ((bool)megaCarriage.carriage)
			{
				megaCarriage.carriage.transform.position = megaCarriage.cp + megaCarriage.carriageOffset;
				Quaternion quaternion = Quaternion.Euler(megaCarriage.rot);
				Quaternion quaternion2 = Quaternion.LookRotation(megaCarriage.b1 - megaCarriage.b2);
				megaCarriage.carriage.transform.rotation = quaternion2 * quaternion;
			}
			if ((bool)megaCarriage.bogey1 && (bool)megaCarriage.carriage)
			{
				megaCarriage.bogey1.transform.position = megaCarriage.carriage.transform.localToWorldMatrix.MultiplyPoint(megaCarriage.bogey1Offset);
				Quaternion quaternion3 = Quaternion.Euler(megaCarriage.bogey1Rot);
				float num4 = num2 - megaCarriage.bogeyoff / megaSpline.length;
				megaCarriage.bp1 = path.transform.TransformPoint(path.InterpCurve3D(curve, num4, type: true));
				Quaternion quaternion4 = Quaternion.LookRotation(path.transform.TransformPoint(path.InterpCurve3D(curve, num4 + 0.0001f, type: true)) - megaCarriage.bp1);
				megaCarriage.bogey1.transform.rotation = quaternion4 * quaternion3;
			}
			if ((bool)megaCarriage.bogey2 && (bool)megaCarriage.carriage)
			{
				megaCarriage.bogey2.transform.position = megaCarriage.carriage.transform.localToWorldMatrix.MultiplyPoint(megaCarriage.bogey2Offset);
				Quaternion quaternion5 = Quaternion.Euler(megaCarriage.bogey2Rot);
				float num5 = num3 + megaCarriage.bogeyoff / megaSpline.length;
				megaCarriage.bp2 = path.transform.TransformPoint(path.InterpCurve3D(curve, num5, type: true));
				Quaternion quaternion6 = Quaternion.LookRotation(path.transform.TransformPoint(path.InterpCurve3D(curve, num5 + 0.0001f, type: true)) - megaCarriage.bp2);
				megaCarriage.bogey2.transform.rotation = quaternion6 * quaternion5;
			}
			num -= megaCarriage.length;
		}
	}
}
