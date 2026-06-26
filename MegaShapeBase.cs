using UnityEngine;

public class MegaShapeBase : MonoBehaviour
{
	public virtual void SplineNotify(MegaShape shape, int reason)
	{
	}

	public virtual void LayerNotify(MegaLoftLayerBase layer, int reason)
	{
	}
}
