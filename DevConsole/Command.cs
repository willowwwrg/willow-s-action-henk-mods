using System;

namespace DevConsole;

[Serializable]
public class Command
{
	private enum DelegateTypes
	{
		NoArgs,
		OneStringArg
	}

	public delegate void NoArgs();

	public delegate void OneStringArg(string args);

	public string CommandName { get; set; }

	private DelegateTypes DelegateType { get; set; }

	private string HelpText { get; set; }

	private NoArgs NoArgsDelegate { get; set; }

	private OneStringArg OneStringArgDelegate { get; set; }

	private NoArgs HelpFunction { get; set; }

	public Command(string commandName, NoArgs function)
	{
		CommandName = commandName;
		DelegateType = DelegateTypes.NoArgs;
		NoArgsDelegate = function;
	}

	public Command(string commandName, NoArgs function, string helpText)
	{
		CommandName = commandName;
		DelegateType = DelegateTypes.NoArgs;
		NoArgsDelegate = function;
		HelpText = helpText;
	}

	public Command(string commandName, NoArgs function, NoArgs helpFunction)
	{
		CommandName = commandName;
		DelegateType = DelegateTypes.NoArgs;
		NoArgsDelegate = function;
		HelpFunction = helpFunction;
	}

	public Command(string commandName, OneStringArg function)
	{
		CommandName = commandName;
		DelegateType = DelegateTypes.OneStringArg;
		OneStringArgDelegate = function;
	}

	public Command(string commandName, OneStringArg function, string helpText)
	{
		CommandName = commandName;
		DelegateType = DelegateTypes.OneStringArg;
		OneStringArgDelegate = function;
		HelpText = helpText;
	}

	public Command(string commandName, OneStringArg function, NoArgs helpFunction)
	{
		CommandName = commandName;
		DelegateType = DelegateTypes.OneStringArg;
		OneStringArgDelegate = function;
		HelpFunction = helpFunction;
	}

	internal void Execute()
	{
		NoArgsDelegate();
	}

	internal void Execute(string arg)
	{
		if (DelegateType == DelegateTypes.NoArgs)
		{
			NoArgsDelegate();
		}
		else
		{
			OneStringArgDelegate(arg);
		}
	}

	internal void ShowHelp()
	{
		if (HelpFunction != null)
		{
			HelpFunction();
		}
		else
		{
			Console.LogInfo("Command Info: " + ((HelpText != null) ? HelpText : "There's no help for this command"));
		}
	}
}
