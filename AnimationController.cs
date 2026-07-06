using UnityEngine;

public class AnimationController : MonoBehaviour
{
	[SerializeField]
	private FlowForceField[] vortexPair0;

	[SerializeField]
	private Transform skimmerClockwise0;

	[SerializeField]
	private Transform skimmerCounterClockwise0;

	[SerializeField]
	private float strengthVortexPair0;

	[SerializeField]
	private FlowForceField[] vortexPair1;

	[SerializeField]
	private Transform skimmerClockwise1;

	[SerializeField]
	private Transform skimmerCounterClockwise1;

	[SerializeField]
	private float strengthVortexPair1;

	[SerializeField]
	private FlowForceField[] vortexPair2;

	[SerializeField]
	private Transform skimmerClockwise2;

	[SerializeField]
	private Transform skimmerCounterClockwise2;

	[SerializeField]
	private float strengthVortexPair2;

	[SerializeField]
	private FluidAddField[] sideAddFluid;

	[SerializeField]
	private float strengthSideAddFluid;

	private void Update()
	{
		for (int i = 0; i < vortexPair0.Length; i++)
			vortexPair0[i].strength = strengthVortexPair0;
		for (int j = 0; j < vortexPair1.Length; j++)
			vortexPair1[j].strength = strengthVortexPair1;
		for (int k = 0; k < vortexPair2.Length; k++)
			vortexPair2[k].strength = strengthVortexPair2;
		for (int l = 0; l < sideAddFluid.Length; l++)
			sideAddFluid[l].strength = strengthSideAddFluid;
		float rot0 = Mathf.Min(0.04f, strengthVortexPair0 * 20f * Time.deltaTime);
		float rot1 = Mathf.Min(0.04f, strengthVortexPair1 * 20f * Time.deltaTime);
		float rot2 = Mathf.Min(0.04f, strengthVortexPair2 * 20f * Time.deltaTime);
		skimmerClockwise0.Rotate(Vector3.up, rot0);
		skimmerCounterClockwise0.Rotate(Vector3.up, -rot0);
		skimmerClockwise1.Rotate(Vector3.up, rot1);
		skimmerCounterClockwise1.Rotate(Vector3.up, -rot1);
		skimmerClockwise2.Rotate(Vector3.up, rot2);
		skimmerCounterClockwise2.Rotate(Vector3.up, -rot2);
	}
}
