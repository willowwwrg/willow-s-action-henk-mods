using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
	private static T instance;

	public static T SP
	{
		get
		{
			if (instance == null)
			{
				CreateInstance();
			}
			return instance;
		}
	}

	private static void CreateInstance()
	{
		instance = (T)Object.FindObjectOfType(typeof(T));
		if (Object.FindObjectsOfType(typeof(T)).Length > 1)
		{
			Debug.LogError("[Singleton] Something went wrong, we have more than 1 singleton of " + typeof(T).ToString());
		}
		else if (instance == null)
		{
			GameObject gameObject = new GameObject();
			instance = gameObject.AddComponent<T>();
			gameObject.name = "(Singleton)" + typeof(T).ToString();
			Object.DontDestroyOnLoad(gameObject);
			GameObject gameObject2 = GameObject.Find("MANAGERS");
			if (gameObject2 != null)
			{
				gameObject.transform.parent = gameObject2.transform;
			}
		}
	}
}
