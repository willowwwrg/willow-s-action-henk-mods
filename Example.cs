using DevConsole;
using UnityEngine;

public class Example : MonoBehaviour
{
	private void Start()
	{
		Console.AddCommand("TIME_TIMESCALE", TimeScale);
		Console.AddCommand("TIME_SHOWTIME", ShowTime);
		Console.AddCommand("PHYSICS_GRAVITY_X", XGravity);
		Console.AddCommand("PHYSICS_GRAVITY_Y", YGravity);
		Console.AddCommand("PHYSICS_GRAVITY_Z", ZGravity);
		Console.AddCommand("EXAMPLE_HELP", ExampleCommand, ExampleCommandHelp);
	}

	private static void ExampleCommand()
	{
		Console.Log("Type EXAMPLE_HELP? to use this command");
	}

	private static void ExampleCommandHelp()
	{
		string text = "The help for this command is shown through a custom function";
		string text2 = string.Empty;
		for (int i = 0; i < text.Length; i++)
		{
			Color color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
			string text3 = text2;
			text2 = text3 + "<color=#" + Console.ColorToHex(color) + ">" + text[i] + "</color>";
		}
		Console.Log(text2);
	}

	private static void TimeScale(string sValue)
	{
		if (float.TryParse(sValue, out var result))
		{
			Time.timeScale = result;
			Console.Log("Change successful", Color.green);
		}
		else
		{
			Console.LogError("The entered value is not a valid float value");
		}
	}

	private static void ShowTime()
	{
		Console.Log(Time.time.ToString());
	}

	private static void XGravity(string sValue)
	{
		if (float.TryParse(sValue, out var result))
		{
			Physics.gravity = new Vector3(result, Physics.gravity.y, Physics.gravity.z);
			Console.Log("Change successful", Color.green);
		}
		else
		{
			Console.LogError("The entered value is not a valid float value");
		}
	}

	private static void YGravity(string sValue)
	{
		if (float.TryParse(sValue, out var result))
		{
			Physics.gravity = new Vector3(Physics.gravity.x, result, Physics.gravity.z);
			Console.Log("Change successful", Color.green);
		}
		else
		{
			Console.LogError("The entered value is not a valid float value");
		}
	}

	private static void ZGravity(string sValue)
	{
		if (float.TryParse(sValue, out var result))
		{
			Physics.gravity = new Vector3(Physics.gravity.x, Physics.gravity.y, result);
			Console.Log("Change successful", Color.green);
		}
		else
		{
			Console.LogError("The entered value is not a valid float value");
		}
	}
}
