using UnityEngine;

public class PPFXAutodestruct : MonoBehaviour
{
	private ParticleSystem ps;

	private void Start()
	{
		ps = GetComponent<ParticleSystem>();
		if ((bool)ps && !ps.loop)
		{
			Object.Destroy(base.gameObject, ps.duration + ps.startLifetime);
		}
	}

	public void DestroyPSystem(GameObject _ps)
	{
		ParticleSystem component = _ps.GetComponent<ParticleSystem>();
		Object.Destroy(_ps, component.duration + component.startLifetime);
	}
}
