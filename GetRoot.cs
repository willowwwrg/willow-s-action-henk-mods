using UnityEngine;

public class GetRoot : Singleton<GetRoot>
{
	public GameObject Get()
	{
		return base.gameObject;
	}
}
