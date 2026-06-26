using UnityEngine;

public class RealTime : MonoBehaviour
{
	private static RealTime mInst;

	private float mRealTime;

	private float mRealDelta;

	public static float time
	{
		get
		{
			if (mInst == null)
			{
				Spawn();
			}
			return mInst.mRealTime;
		}
	}

	public static float deltaTime
	{
		get
		{
			if (mInst == null)
			{
				Spawn();
			}
			return mInst.mRealDelta;
		}
	}

	private static void Spawn()
	{
		GameObject obj = new GameObject("_RealTime");
		Object.DontDestroyOnLoad(obj);
		mInst = obj.AddComponent<RealTime>();
		mInst.mRealTime = Time.realtimeSinceStartup;
	}

	private void Update()
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		mRealDelta = Mathf.Clamp01(realtimeSinceStartup - mRealTime);
		mRealTime = realtimeSinceStartup;
	}
}
