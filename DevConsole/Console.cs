using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DevConsole;

[Serializable]
public class Console : MonoBehaviour
{
	private delegate void AddText(string t, string c);

	private const int TEXT_AREA_OFFSET = 7;

	private const int WARNING_THRESHOLD = 15000;

	private const int DANGER_THRESHOLD = 16000;

	private const int AUTOCLEAR_THRESHOLD = 18000;

	[SerializeField]
	private float fadeSpeed = 500f;

	private bool helpEnabled = true;

	private int numHelpCommandsToShow = 5;

	private float helpWindowWidth = 200f;

	[SerializeField]
	private float consoleSize = 0.33f;

	private List<Command> consoleCommands;

	private List<string> candidates = new List<string>();

	private int selectedCandidate;

	private List<string> history = new List<string>();

	private int selectedHistory;

	[SerializeField]
	private KeyCode consoleKey = KeyCode.Backslash;

	[SerializeField]
	private GUISkin skin;

	[SerializeField]
	private bool dontDestroyOnLoad;

	private static Console singleton;

	private bool opening;

	private bool closed = true;

	private bool showHelp = true;

	private bool inHistory;

	private bool showTimeStamp;

	private float numLinesThreshold;

	private float maxConsoleHeight;

	private float currentConsoleHeight;

	private Vector2 consoleScroll = Vector2.zero;

	private Vector2 helpWindowScroll = Vector2.zero;

	private string consoleText = string.Empty;

	private string inputText = string.Empty;

	private string lastText = string.Empty;

	private int numLines;

	private void Awake()
	{
		if (singleton == null)
		{
			singleton = this;
			if (dontDestroyOnLoad)
			{
				UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
			}
			ShowLog("true");
			consoleCommands = new List<Command>
			{
				new Command("HELP", GeneralHelp, "Shows a list of all Commands available"),
				new Command("DC_INFO", ShowInfo, "Shows info about the DevConsole"),
				new Command("DC_CLEAR", Clear, "Clears the console"),
				new Command("DC_CHANGE_KEY", ChangeKey, ChangeKeyHelp),
				new Command("DC_SHOW_DEBUGLOG", ShowLog, "Establishes whether or not to show Unity Debug Log"),
				new Command("DC_SHOW_TIMESTAMP", ShowTimeStamp, "Establishes whether or not to show the time stamp for each command")
			};
		}
		else
		{
			Debug.LogWarning("There can only be one Console per project");
			UnityEngine.Object.Destroy(this);
		}
	}

	private void Start()
	{
	}

	private void OnGUI()
	{
		if (skin != null && GUI.skin != skin)
		{
			GUI.skin = skin;
		}
		if (consoleKey == KeyCode.None)
		{
			return;
		}
		Event current = Event.current;
		GUI.skin.textArea.richText = true;
		if (current.type == EventType.KeyDown && current.keyCode == consoleKey)
		{
			opening = !opening;
			GUIUtility.keyboardControl = 0;
			StartCoroutine(FadeInOut(opening));
		}
		bool flag = currentConsoleHeight != maxConsoleHeight && currentConsoleHeight != 0f;
		float lineHeight = GUI.skin.textArea.lineHeight;
		float num = lineHeight * (float)numLines;
		float height = ((!(num > currentConsoleHeight)) ? currentConsoleHeight : num);
		if (!closed)
		{
			if (!flag)
			{
				GUI.FocusControl("TextField");
			}
			if (current.type == EventType.KeyDown)
			{
				if (inputText != string.Empty)
				{
					switch (current.keyCode)
					{
					case KeyCode.Return:
						PrintInput(inputText);
						break;
					case KeyCode.Tab:
						if (candidates.Count != 0)
						{
							inputText = candidates[selectedCandidate];
							showHelp = false;
							SetCursorPos(inputText.Length);
							candidates.Clear();
						}
						break;
					case KeyCode.Escape:
						showHelp = false;
						candidates.Clear();
						break;
					case KeyCode.F1:
						showHelp = true;
						break;
					}
				}
				switch (current.keyCode)
				{
				case KeyCode.UpArrow:
					if ((inHistory || inputText == string.Empty) && history.Count != 0)
					{
						selectedHistory = Mathf.Clamp(selectedHistory + (inHistory ? 1 : 0), 0, history.Count - 1);
						inputText = history[selectedHistory];
						showHelp = false;
						inHistory = true;
						lastText = inputText;
					}
					else if (inputText != string.Empty && !inHistory)
					{
						selectedCandidate = Mathf.Clamp(--selectedCandidate, 0, candidates.Count - 1);
						if ((float)selectedCandidate * lineHeight <= helpWindowScroll.y || (float)selectedCandidate * lineHeight > helpWindowScroll.y + lineHeight * (float)(numHelpCommandsToShow - 1))
						{
							helpWindowScroll = new Vector2(0f, (float)selectedCandidate * lineHeight - 1f * lineHeight);
						}
					}
					SetCursorPos(inputText.Length);
					break;
				case KeyCode.DownArrow:
					if ((inHistory || inputText == string.Empty) && history.Count != 0)
					{
						selectedHistory = Mathf.Clamp(selectedHistory - (inHistory ? 1 : 0), 0, history.Count - 1);
						inputText = history[selectedHistory];
						showHelp = false;
						inHistory = true;
						lastText = inputText;
					}
					else if (inputText != string.Empty && !inHistory)
					{
						selectedCandidate = Mathf.Clamp(++selectedCandidate, 0, candidates.Count - 1);
						if ((float)selectedCandidate * lineHeight > helpWindowScroll.y + lineHeight * (float)(numHelpCommandsToShow - 2) || (float)selectedCandidate * lineHeight < helpWindowScroll.y)
						{
							helpWindowScroll = new Vector2(0f, (float)selectedCandidate * lineHeight - (float)(numHelpCommandsToShow - 2) * lineHeight);
						}
					}
					SetCursorPos(inputText.Length);
					break;
				}
			}
			if (lastText != inputText)
			{
				inHistory = false;
				lastText = string.Empty;
			}
			GUI.Box(new Rect(0f, 0f, Screen.width, currentConsoleHeight), new GUIContent());
			GUI.SetNextControlName("TextField");
			inputText = GUI.TextField(new Rect(0f, currentConsoleHeight, Screen.width, 25f), inputText);
			GUI.skin.textArea.normal.background = null;
			GUI.skin.textArea.hover.background = null;
			consoleScroll = GUI.BeginScrollView(new Rect(0f, 0f, Screen.width, currentConsoleHeight), consoleScroll, new Rect(0f, 0f, Screen.width - 20, height));
			GUI.TextArea(new Rect(0f, currentConsoleHeight - ((numLines != 0) ? num : (0f + lineHeight)) + ((!((float)numLines >= numLinesThreshold - 1f)) ? 0f : (lineHeight * ((float)numLines - numLinesThreshold))), Screen.width, 7f + ((numLines != 0) ? num : lineHeight)), consoleText);
			GUI.EndScrollView();
			if (inputText == string.Empty)
			{
				showHelp = true;
			}
		}
		if (!showHelp || !helpEnabled || !(inputText.Trim() != string.Empty))
		{
			return;
		}
		ShowHelp();
		if (candidates.Count == 0)
		{
			return;
		}
		string text = string.Empty;
		foreach (string candidate in candidates)
		{
			text = text + ((!(candidates[selectedCandidate] == candidate)) ? candidate : ("<color=yellow>" + candidate + "</color>")) + "\n";
		}
		GUI.skin.textArea.normal.background = GUI.skin.textField.normal.background;
		GUI.skin.textArea.hover.background = GUI.skin.textField.hover.background;
		if (candidates.Count > numHelpCommandsToShow)
		{
			helpWindowScroll = GUI.BeginScrollView(new Rect(0f, currentConsoleHeight - (float)numHelpCommandsToShow * lineHeight - 7f, helpWindowWidth, 5f + lineHeight * (float)numHelpCommandsToShow), helpWindowScroll, new Rect(0f, 0f, helpWindowWidth - 20f, 7f + (float)candidates.Count * lineHeight));
			GUI.TextArea(new Rect(0f, 0f, helpWindowWidth, 7f + (float)candidates.Count * lineHeight), text);
			GUI.EndScrollView();
		}
		else
		{
			GUI.TextArea(new Rect(0f, currentConsoleHeight - 7f - ((candidates.Count <= numHelpCommandsToShow) ? (lineHeight * (float)candidates.Count) : ((float)numHelpCommandsToShow * lineHeight)), helpWindowWidth, ((candidates.Count <= numHelpCommandsToShow) ? (lineHeight * (float)candidates.Count) : ((float)numHelpCommandsToShow * lineHeight)) + 7f), text);
		}
	}

	private IEnumerator FadeInOut(bool opening)
	{
		maxConsoleHeight = (float)Screen.height * consoleSize;
		numLinesThreshold = maxConsoleHeight / GUI.skin.textArea.lineHeight;
		closed = false;
		do
		{
			if (opening)
			{
				currentConsoleHeight = Mathf.Min(currentConsoleHeight + fadeSpeed * Time.deltaTime, maxConsoleHeight);
			}
			else
			{
				currentConsoleHeight = Mathf.Max(currentConsoleHeight - fadeSpeed * Time.deltaTime, 0f);
			}
			if (currentConsoleHeight == 0f || currentConsoleHeight == maxConsoleHeight)
			{
				opening = !opening;
			}
			yield return null;
		}
		while (opening == this.opening);
		if (currentConsoleHeight == 0f)
		{
			closed = true;
		}
		if (closed)
		{
			inputText = string.Empty;
		}
	}

	private void ShowHelp()
	{
		string text = string.Empty;
		if (candidates.Count != 0 && selectedCandidate >= 0 && candidates.Count > selectedCandidate)
		{
			text = candidates[selectedCandidate];
		}
		candidates.Clear();
		for (int i = 0; i < consoleCommands.Count; i++)
		{
			if (consoleCommands[i].CommandName.StartsWith(inputText.ToUpper()))
			{
				candidates.Add(consoleCommands[i].CommandName);
			}
		}
		if (text == string.Empty)
		{
			selectedCandidate = 0;
			return;
		}
		for (int j = 0; j < candidates.Count; j++)
		{
			if (candidates[j] == text)
			{
				selectedCandidate = j;
				return;
			}
		}
		selectedCandidate = 0;
	}

	private void SetCursorPos(int pos)
	{
		TextEditor obj = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
		obj.pos = pos;
		obj.selectPos = obj.pos;
	}

	public static string ColorToHex(Color color)
	{
		string text = "0123456789ABCDEF";
		int num = (int)(color.r * 255f);
		int num2 = (int)(color.g * 255f);
		int num3 = (int)(color.b * 255f);
		return text[(int)Mathf.Floor(num / 16)].ToString() + text[(int)Mathf.Round(num % 16)] + text[(int)Mathf.Floor(num2 / 16)] + text[(int)Mathf.Round(num2 % 16)] + text[(int)Mathf.Floor(num3 / 16)] + text[(int)Mathf.Round(num3 % 16)];
	}

	private bool StringToBool(string value, out bool result)
	{
		bool result2 = (result = false);
		int result3 = 0;
		if (bool.TryParse(value, out result2))
		{
			result = result2;
			return true;
		}
		if (int.TryParse(value, out result3))
		{
			if (result3 == 1 || result3 == 0)
			{
				result = result3 == 1;
				return true;
			}
			return false;
		}
		if (value.ToLower().Equals("yes") || value.ToLower().Equals("no"))
		{
			result = value.ToLower().Equals("yes");
			return true;
		}
		return false;
	}

	public static void Log(string text)
	{
		singleton.BasePrint(text);
	}

	public static void LogInfo(string text)
	{
		singleton.BasePrint(text, Color.cyan);
	}

	public static void LogWarning(string text)
	{
		singleton.BasePrint(text, Color.yellow);
	}

	public static void LogError(string text)
	{
		singleton.BasePrint(text, Color.red);
	}

	public static void Log(string text, string color)
	{
		singleton.BasePrint(text, color);
	}

	public static void Log(string text, Color color)
	{
		singleton.BasePrint(text, color);
	}

	private void BasePrint(string text)
	{
		BasePrint(text, ColorToHex(Color.white));
	}

	private void BasePrint(string text, Color color)
	{
		BasePrint(text, ColorToHex(color));
	}

	private void BasePrint(string text, string color)
	{
		text = "> " + text;
		AddText addText = delegate(string t, string c)
		{
			string text2 = consoleText;
			consoleText = text2 + ((!showTimeStamp) ? string.Empty : ("[" + DateTime.Now.ToShortTimeString() + "]  ")) + "<color=#" + c + ">" + t + "</color>";
		};
		int num = 1;
		for (int num2 = 0; num2 < text.Length; num2++)
		{
			if (text[num2] == '\n')
			{
				num++;
			}
		}
		text += "\n";
		numLines += num;
		if ((float)numLines >= numLinesThreshold - 1f)
		{
			consoleScroll = new Vector2(0f, consoleScroll.y + 2.1474836E+09f);
		}
		addText(text, color);
		if (consoleText.Length >= 18000)
		{
			Clear();
			addText("Buffer cleared automatically\n", ColorToHex(Color.yellow));
		}
		else if (consoleText.Length >= 16000)
		{
			addText("Buffer size too large. You should clear the console\n", ColorToHex(Color.red));
		}
		else if (consoleText.Length >= 15000)
		{
			addText("Buffer size too large. You should clear the console\n", ColorToHex(Color.yellow));
		}
	}

	private void PrintInput(string input)
	{
		inputText = string.Empty;
		history.Insert(0, input);
		selectedHistory = 0;
		BasePrint(input);
		for (int i = 0; i < consoleCommands.Count; i++)
		{
			if (input.ToUpper().StartsWith(consoleCommands[i].CommandName))
			{
				if (input.ToUpper() == consoleCommands[i].CommandName + "?")
				{
					consoleCommands[i].ShowHelp();
				}
				else
				{
					consoleCommands[i].Execute(input.Substring(consoleCommands[i].CommandName.Length + (input.Contains(" ") ? 1 : 0)));
				}
			}
		}
	}

	private void LogCallback(string log, string stackTrace, LogType type)
	{
		Color color;
		switch (type)
		{
		case LogType.Error:
		case LogType.Assert:
		case LogType.Exception:
			color = Color.red;
			break;
		case LogType.Warning:
			color = Color.yellow;
			break;
		default:
			color = Color.cyan;
			break;
		}
		BasePrint(log, color);
		BasePrint(stackTrace, color);
		for (int i = 0; i < stackTrace.Length; i++)
		{
			if (stackTrace[i] == '\n')
			{
				numLines++;
			}
		}
	}

	public static void AddCommand(Command c)
	{
		if (!CommandExists(c.CommandName))
		{
			singleton.consoleCommands.Add(c);
		}
	}

	public static void AddCommand(string commandName, Command.NoArgs function)
	{
		if (!CommandExists(commandName))
		{
			singleton.consoleCommands.Add(new Command(commandName, function));
		}
	}

	public static void AddCommand(string commandName, Command.OneStringArg function)
	{
		if (!CommandExists(commandName))
		{
			singleton.consoleCommands.Add(new Command(commandName, function));
		}
	}

	public static void AddCommand(string commandName, Command.NoArgs function, string helpInfo)
	{
		if (!CommandExists(commandName))
		{
			singleton.consoleCommands.Add(new Command(commandName, function, helpInfo));
		}
	}

	public static void AddCommand(string commandName, Command.OneStringArg function, string helpInfo)
	{
		if (!CommandExists(commandName))
		{
			singleton.consoleCommands.Add(new Command(commandName, function, helpInfo));
		}
	}

	public static void AddCommand(string commandName, Command.NoArgs function, Command.NoArgs helpFunction)
	{
		if (!CommandExists(commandName))
		{
			singleton.consoleCommands.Add(new Command(commandName, function, helpFunction));
		}
	}

	public static void AddCommand(string commandName, Command.OneStringArg function, Command.NoArgs helpFunction)
	{
		if (!CommandExists(commandName))
		{
			singleton.consoleCommands.Add(new Command(commandName, function, helpFunction));
		}
	}

	private static bool CommandExists(string commandName)
	{
		foreach (Command consoleCommand in singleton.consoleCommands)
		{
			if (consoleCommand.CommandName.ToUpper() == commandName.ToUpper())
			{
				LogError("The command " + commandName + " already exists");
				return true;
			}
		}
		return false;
	}

	public static void RemoveCommand(string commandName)
	{
		foreach (Command consoleCommand in singleton.consoleCommands)
		{
			if (consoleCommand.CommandName == commandName)
			{
				singleton.consoleCommands.Remove(consoleCommand);
				Log("Command " + commandName + " removed successfully", Color.green);
				return;
			}
		}
		LogWarning("The command " + commandName + " could not be found");
	}

	private void GeneralHelp()
	{
		string text = string.Empty;
		for (int i = 0; i < consoleCommands.Count; i++)
		{
			text += consoleCommands[i].CommandName;
			text += "\n";
		}
		LogInfo("List of commands available:\n" + text);
	}

	private void Clear()
	{
		singleton.consoleText = string.Empty;
		singleton.numLines = 0;
	}

	private void ChangeKey(string key)
	{
		if (!int.TryParse(key, out var result))
		{
			try
			{
				singleton.consoleKey = (KeyCode)(int)Enum.Parse(typeof(KeyCode), key, ignoreCase: true);
				Log("Change successful", Color.green);
				return;
			}
			catch
			{
				LogError("The entered value is not a valid KeyCode value");
				return;
			}
		}
		string[] names = Enum.GetNames(typeof(KeyCode));
		if (result >= 0 || result < names.Length)
		{
			try
			{
				singleton.consoleKey = (KeyCode)(int)Enum.Parse(typeof(KeyCode), names[result], ignoreCase: true);
				Log("Change successful", Color.green);
				return;
			}
			catch
			{
				LogError("The entered value is not a valid KeyCode value");
				return;
			}
		}
		LogError("The entered value is not a valid KeyCode value");
	}

	private void ChangeKeyHelp()
	{
		string[] names = Enum.GetNames(typeof(KeyCode));
		string text = "\nSPECIAL KEYS 1: ";
		int num = 0;
		for (int i = 0; i < names.Length; i++)
		{
			string text2 = string.Empty;
			switch (i)
			{
			case 22:
				text2 = "\n\nNUMERIC KEYS: ";
				num = 0;
				break;
			case 32:
				text2 = "\n\nSPECIAL KEYS 2: ";
				num = 0;
				break;
			case 45:
				text2 = "\n\nALPHA KEYS: ";
				num = 0;
				break;
			case 71:
				text2 = "\n\nKEYPAD KEYS: ";
				num = 0;
				break;
			case 89:
				text2 = "\n\nSPECIAL KEYS 3: ";
				num = 0;
				break;
			case 98:
				text2 = "\n\nF KEYS: ";
				num = 0;
				break;
			case 113:
				text2 = "\n\nSPECIAL KEYS 4: ";
				num = 0;
				break;
			case 134:
				text2 = "\n\nMOUSE: ";
				num = 0;
				break;
			case 141:
				text2 = "\n\nJOYSTICK KEYS: ";
				num = 0;
				break;
			}
			string text3 = text2;
			text2 = text3 + names[i] + "[" + i + "]" + ((i == names.Length - 1) ? string.Empty : ",");
			num += text2.Length;
			text += text2;
			if (num >= 65)
			{
				text += "\n";
				num = 0;
			}
		}
		LogInfo("Command Info: " + text);
	}

	private void ShowLog(string value)
	{
		if (StringToBool(value, out var result))
		{
			if (result)
			{
				LogCallbackHandler.RegisterLogCallback(LogCallback);
			}
			else
			{
				LogCallbackHandler.RemoveLogCallback(LogCallback);
			}
		}
		else
		{
			LogError("The entered value is not a valid boolean value");
		}
	}

	private void ShowTimeStamp(string value)
	{
		if (StringToBool(value, out var result))
		{
			showTimeStamp = result;
			Log("Change successful", Color.green);
		}
		else
		{
			LogError("The entered value is not a valid boolean value");
		}
	}

	private void ShowInfo()
	{
		LogInfo("DevConsole by CobsTech \nVersion 1.0\nContact/Support: antoniocogo@gmail.com\n+More updates soon");
	}
}
