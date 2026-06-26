using System.Collections.Generic;
using UnityEngine;

namespace Rewired.Data;

public class UserDataStore_PlayerPrefs : UserDataStore
{
	[SerializeField]
	private bool isEnabled = true;

	[SerializeField]
	private bool loadDataOnStart = true;

	[SerializeField]
	private string playerPrefsKeyPrefix = "RewiredSaveData";

	public override void Save()
	{
		if (!isEnabled)
		{
			Debug.LogWarning("UserDataStore_PlayerPrefs is disabled and will not save any data.", this);
		}
		else
		{
			SaveAll();
		}
	}

	public override void SaveControllerData(int playerId, ControllerType controllerType, int controllerId)
	{
		if (!isEnabled)
		{
			Debug.LogWarning("UserDataStore_PlayerPrefs is disabled and will not save any data.", this);
		}
		else
		{
			SaveControllerDataNow(playerId, controllerType, controllerId);
		}
	}

	public override void SaveControllerData(ControllerType controllerType, int controllerId)
	{
		if (!isEnabled)
		{
			Debug.LogWarning("UserDataStore_PlayerPrefs is disabled and will not save any data.", this);
		}
		else
		{
			SaveControllerDataNow(controllerType, controllerId);
		}
	}

	public override void SavePlayerData(int playerId)
	{
		if (!isEnabled)
		{
			Debug.LogWarning("UserDataStore_PlayerPrefs is disabled and will not save any data.", this);
		}
		else
		{
			SavePlayerDataNow(playerId);
		}
	}

	public override void SaveInputBehavior(int playerId, int behaviorId)
	{
		if (!isEnabled)
		{
			Debug.LogWarning("UserDataStore_PlayerPrefs is disabled and will not save any data.", this);
		}
		else
		{
			SaveInputBehaviorNow(playerId, behaviorId);
		}
	}

	public override void Load()
	{
		if (!isEnabled)
		{
			Debug.LogWarning("UserDataStore_PlayerPrefs is disabled and will not load any data.", this);
		}
		else
		{
			LoadAll();
		}
	}

	public override void LoadControllerData(int playerId, ControllerType controllerType, int controllerId)
	{
		if (!isEnabled)
		{
			Debug.LogWarning("UserDataStore_PlayerPrefs is disabled and will not load any data.", this);
		}
		else
		{
			LoadControllerDataNow(playerId, controllerType, controllerId);
		}
	}

	public override void LoadControllerData(ControllerType controllerType, int controllerId)
	{
		if (!isEnabled)
		{
			Debug.LogWarning("UserDataStore_PlayerPrefs is disabled and will not load any data.", this);
		}
		else
		{
			LoadControllerDataNow(controllerType, controllerId);
		}
	}

	public override void LoadPlayerData(int playerId)
	{
		if (!isEnabled)
		{
			Debug.LogWarning("UserDataStore_PlayerPrefs is disabled and will not load any data.", this);
		}
		else
		{
			LoadPlayerDataNow(playerId);
		}
	}

	public override void LoadInputBehavior(int playerId, int behaviorId)
	{
		if (!isEnabled)
		{
			Debug.LogWarning("UserDataStore_PlayerPrefs is disabled and will not load any data.", this);
		}
		else
		{
			LoadInputBehaviorNow(playerId, behaviorId);
		}
	}

	protected override void OnInitialize()
	{
		if (loadDataOnStart)
		{
			Load();
		}
	}

	protected override void OnControllerConnected(ControllerStatusChangedEventArgs args)
	{
		if (isEnabled && args.controllerType == ControllerType.Joystick)
		{
			LoadJoystickData(args.controllerId);
		}
	}

	protected override void OnControllerPreDiscconnect(ControllerStatusChangedEventArgs args)
	{
		if (isEnabled && args.controllerType == ControllerType.Joystick)
		{
			SaveJoystickData(args.controllerId);
		}
	}

	protected override void OnControllerDisconnected(ControllerStatusChangedEventArgs args)
	{
		_ = isEnabled;
	}

	private void LoadAll()
	{
		IList<Player> allPlayers = ReInput.players.AllPlayers;
		for (int i = 0; i < allPlayers.Count; i++)
		{
			LoadPlayerDataNow(allPlayers[i]);
		}
		LoadAllJoystickCalibrationData();
	}

	private void LoadPlayerDataNow(int playerId)
	{
		LoadPlayerDataNow(ReInput.players.GetPlayer(playerId));
	}

	private void LoadPlayerDataNow(Player player)
	{
		if (player == null)
		{
			return;
		}
		LoadInputBehaviors(player.id);
		LoadControllerMaps(player.id, ControllerType.Keyboard, 0);
		LoadControllerMaps(player.id, ControllerType.Mouse, 0);
		foreach (Joystick joystick in player.controllers.Joysticks)
		{
			LoadControllerMaps(player.id, ControllerType.Joystick, joystick.id);
		}
	}

	private void LoadAllJoystickCalibrationData()
	{
		IList<Joystick> joysticks = ReInput.controllers.Joysticks;
		for (int i = 0; i < joysticks.Count; i++)
		{
			LoadJoystickCalibrationData(joysticks[i]);
		}
	}

	private void LoadJoystickCalibrationData(Joystick joystick)
	{
		joystick?.ImportCalibrationMapFromXmlString(GetJoystickCalibrationMapXml(joystick));
	}

	private void LoadJoystickCalibrationData(int joystickId)
	{
		LoadJoystickCalibrationData(ReInput.controllers.GetJoystick(joystickId));
	}

	private void LoadJoystickData(int joystickId)
	{
		IList<Player> allPlayers = ReInput.players.AllPlayers;
		for (int i = 0; i < allPlayers.Count; i++)
		{
			Player player = allPlayers[i];
			if (player.controllers.ContainsController(ControllerType.Joystick, joystickId))
			{
				LoadControllerMaps(player.id, ControllerType.Joystick, joystickId);
			}
		}
		LoadJoystickCalibrationData(joystickId);
	}

	private void LoadControllerDataNow(int playerId, ControllerType controllerType, int controllerId)
	{
		LoadControllerMaps(playerId, controllerType, controllerId);
		LoadControllerDataNow(controllerType, controllerId);
	}

	private void LoadControllerDataNow(ControllerType controllerType, int controllerId)
	{
		if (controllerType == ControllerType.Joystick)
		{
			LoadJoystickCalibrationData(controllerId);
		}
	}

	private void LoadControllerMaps(int playerId, ControllerType controllerType, int controllerId)
	{
		Player player = ReInput.players.GetPlayer(playerId);
		if (player == null)
		{
			return;
		}
		Controller controller = ReInput.controllers.GetController(controllerType, controllerId);
		if (controller != null)
		{
			List<string> allControllerMapsXml = GetAllControllerMapsXml(player, userAssignableMapsOnly: true, controllerType, controller);
			if (allControllerMapsXml.Count != 0)
			{
				player.controllers.maps.AddMapsFromXml(controllerType, controllerId, allControllerMapsXml);
			}
		}
	}

	private void LoadInputBehaviors(int playerId)
	{
		Player player = ReInput.players.GetPlayer(playerId);
		if (player != null)
		{
			IList<InputBehavior> inputBehaviors = ReInput.mapping.GetInputBehaviors(player.id);
			for (int i = 0; i < inputBehaviors.Count; i++)
			{
				LoadInputBehaviorNow(player, inputBehaviors[i]);
			}
		}
	}

	private void LoadInputBehaviorNow(int playerId, int behaviorId)
	{
		Player player = ReInput.players.GetPlayer(playerId);
		if (player != null)
		{
			InputBehavior inputBehavior = ReInput.mapping.GetInputBehavior(playerId, behaviorId);
			if (inputBehavior != null)
			{
				LoadInputBehaviorNow(player, inputBehavior);
			}
		}
	}

	private void LoadInputBehaviorNow(Player player, InputBehavior inputBehavior)
	{
		if (player != null && inputBehavior != null)
		{
			string inputBehaviorXml = GetInputBehaviorXml(player, inputBehavior.id);
			if (inputBehaviorXml != null && !(inputBehaviorXml == string.Empty))
			{
				inputBehavior.ImportXmlString(inputBehaviorXml);
			}
		}
	}

	private void SaveAll()
	{
		IList<Player> allPlayers = ReInput.players.AllPlayers;
		for (int i = 0; i < allPlayers.Count; i++)
		{
			SavePlayerDataNow(allPlayers[i]);
		}
		SaveAllJoystickCalibrationData();
		PlayerPrefs.Save();
	}

	private void SavePlayerDataNow(int playerId)
	{
		SavePlayerDataNow(ReInput.players.GetPlayer(playerId));
	}

	private void SavePlayerDataNow(Player player)
	{
		if (player != null)
		{
			PlayerSaveData saveData = player.GetSaveData(userAssignableMapsOnly: true);
			SaveInputBehaviors(player, saveData);
			SaveControllerMaps(player, saveData);
		}
	}

	private void SaveAllJoystickCalibrationData()
	{
		IList<Joystick> joysticks = ReInput.controllers.Joysticks;
		for (int i = 0; i < joysticks.Count; i++)
		{
			SaveJoystickCalibrationData(joysticks[i]);
		}
	}

	private void SaveJoystickCalibrationData(int joystickId)
	{
		SaveJoystickCalibrationData(ReInput.controllers.GetJoystick(joystickId));
	}

	private void SaveJoystickCalibrationData(Joystick joystick)
	{
		if (joystick != null)
		{
			JoystickCalibrationMapSaveData calibrationMapSaveData = joystick.GetCalibrationMapSaveData();
			PlayerPrefs.SetString(GetJoystickCalibrationMapPlayerPrefsKey(calibrationMapSaveData), calibrationMapSaveData.map.ToXmlString());
		}
	}

	private void SaveJoystickData(int joystickId)
	{
		IList<Player> allPlayers = ReInput.players.AllPlayers;
		for (int i = 0; i < allPlayers.Count; i++)
		{
			Player player = allPlayers[i];
			if (player.controllers.ContainsController(ControllerType.Joystick, joystickId))
			{
				SaveControllerMaps(player.id, ControllerType.Joystick, joystickId);
			}
		}
		SaveJoystickCalibrationData(joystickId);
	}

	private void SaveControllerDataNow(int playerId, ControllerType controllerType, int controllerId)
	{
		SaveControllerMaps(playerId, controllerType, controllerId);
		SaveControllerDataNow(controllerType, controllerId);
	}

	private void SaveControllerDataNow(ControllerType controllerType, int controllerId)
	{
		if (controllerType == ControllerType.Joystick)
		{
			SaveJoystickCalibrationData(controllerId);
		}
	}

	private void SaveControllerMaps(Player player, PlayerSaveData playerSaveData)
	{
		foreach (ControllerMapSaveData allControllerMapSaveDatum in playerSaveData.AllControllerMapSaveData)
		{
			PlayerPrefs.SetString(GetControllerMapPlayerPrefsKey(player, allControllerMapSaveDatum), allControllerMapSaveDatum.map.ToXmlString());
		}
	}

	private void SaveControllerMaps(int playerId, ControllerType controllerType, int controllerId)
	{
		Player player = ReInput.players.GetPlayer(playerId);
		if (player == null || !player.controllers.ContainsController(controllerType, controllerId))
		{
			return;
		}
		ControllerMapSaveData[] mapSaveData = player.controllers.maps.GetMapSaveData(controllerType, controllerId, userAssignableMapsOnly: true);
		if (mapSaveData != null)
		{
			for (int i = 0; i < mapSaveData.Length; i++)
			{
				PlayerPrefs.SetString(GetControllerMapPlayerPrefsKey(player, mapSaveData[i]), mapSaveData[i].map.ToXmlString());
			}
		}
	}

	private void SaveInputBehaviors(Player player, PlayerSaveData playerSaveData)
	{
		if (player != null)
		{
			InputBehavior[] inputBehaviors = playerSaveData.inputBehaviors;
			for (int i = 0; i < inputBehaviors.Length; i++)
			{
				SaveInputBehaviorNow(player, inputBehaviors[i]);
			}
		}
	}

	private void SaveInputBehaviorNow(int playerId, int behaviorId)
	{
		Player player = ReInput.players.GetPlayer(playerId);
		if (player != null)
		{
			InputBehavior inputBehavior = ReInput.mapping.GetInputBehavior(playerId, behaviorId);
			if (inputBehavior != null)
			{
				SaveInputBehaviorNow(player, inputBehavior);
			}
		}
	}

	private void SaveInputBehaviorNow(Player player, InputBehavior inputBehavior)
	{
		if (player != null && inputBehavior != null)
		{
			PlayerPrefs.SetString(GetInputBehaviorPlayerPrefsKey(player, inputBehavior), inputBehavior.ToXmlString());
		}
	}

	private string GetBasePlayerPrefsKey(Player player)
	{
		return playerPrefsKeyPrefix + "|playerName=" + player.name;
	}

	private string GetControllerMapPlayerPrefsKey(Player player, ControllerMapSaveData saveData)
	{
		string basePlayerPrefsKey = GetBasePlayerPrefsKey(player);
		basePlayerPrefsKey += "|dataType=ControllerMap";
		basePlayerPrefsKey = basePlayerPrefsKey + "|controllerMapType=" + saveData.mapTypeString;
		string text = basePlayerPrefsKey;
		basePlayerPrefsKey = text + "|categoryId=" + saveData.map.categoryId + "|layoutId=" + saveData.map.layoutId;
		basePlayerPrefsKey = basePlayerPrefsKey + "|hardwareIdentifier=" + saveData.controllerHardwareIdentifier;
		if (saveData.mapType == typeof(JoystickMap))
		{
			basePlayerPrefsKey = basePlayerPrefsKey + "|hardwareGuid=" + ((JoystickMapSaveData)saveData).joystickHardwareTypeGuid.ToString();
		}
		return basePlayerPrefsKey;
	}

	private string GetControllerMapXml(Player player, ControllerType controllerType, int categoryId, int layoutId, Controller controller)
	{
		string basePlayerPrefsKey = GetBasePlayerPrefsKey(player);
		basePlayerPrefsKey += "|dataType=ControllerMap";
		basePlayerPrefsKey = basePlayerPrefsKey + "|controllerMapType=" + controller.mapTypeString;
		string text = basePlayerPrefsKey;
		basePlayerPrefsKey = text + "|categoryId=" + categoryId + "|layoutId=" + layoutId;
		basePlayerPrefsKey = basePlayerPrefsKey + "|hardwareIdentifier=" + controller.hardwareIdentifier;
		if (controllerType == ControllerType.Joystick)
		{
			Joystick joystick = (Joystick)controller;
			basePlayerPrefsKey = basePlayerPrefsKey + "|hardwareGuid=" + joystick.hardwareTypeGuid.ToString();
		}
		if (!PlayerPrefs.HasKey(basePlayerPrefsKey))
		{
			return string.Empty;
		}
		return PlayerPrefs.GetString(basePlayerPrefsKey);
	}

	private List<string> GetAllControllerMapsXml(Player player, bool userAssignableMapsOnly, ControllerType controllerType, Controller controller)
	{
		List<string> list = new List<string>();
		IList<InputMapCategory> mapCategories = ReInput.mapping.MapCategories;
		for (int i = 0; i < mapCategories.Count; i++)
		{
			InputMapCategory inputMapCategory = mapCategories[i];
			if (userAssignableMapsOnly && !inputMapCategory.userAssignable)
			{
				continue;
			}
			IList<InputLayout> list2 = ReInput.mapping.MapLayouts(controllerType);
			for (int j = 0; j < list2.Count; j++)
			{
				InputLayout inputLayout = list2[j];
				string controllerMapXml = GetControllerMapXml(player, controllerType, inputMapCategory.id, inputLayout.id, controller);
				if (!(controllerMapXml == string.Empty))
				{
					list.Add(controllerMapXml);
				}
			}
		}
		return list;
	}

	private string GetJoystickCalibrationMapPlayerPrefsKey(JoystickCalibrationMapSaveData saveData)
	{
		return string.Concat(string.Concat(string.Concat(playerPrefsKeyPrefix + "|dataType=CalibrationMap", "|controllerType=", saveData.controllerType.ToString()), "|hardwareIdentifier=", saveData.hardwareIdentifier), "|hardwareGuid=", saveData.joystickHardwareTypeGuid.ToString());
	}

	private string GetJoystickCalibrationMapXml(Joystick joystick)
	{
		string text = playerPrefsKeyPrefix;
		text += "|dataType=CalibrationMap";
		text = text + "|controllerType=" + joystick.type;
		text = text + "|hardwareIdentifier=" + joystick.hardwareIdentifier;
		text = text + "|hardwareGuid=" + joystick.hardwareTypeGuid.ToString();
		if (!PlayerPrefs.HasKey(text))
		{
			return string.Empty;
		}
		return PlayerPrefs.GetString(text);
	}

	private string GetInputBehaviorPlayerPrefsKey(Player player, InputBehavior saveData)
	{
		return string.Concat(GetBasePlayerPrefsKey(player) + "|dataType=InputBehavior", "|id=", saveData.id.ToString());
	}

	private string GetInputBehaviorXml(Player player, int id)
	{
		string basePlayerPrefsKey = GetBasePlayerPrefsKey(player);
		basePlayerPrefsKey += "|dataType=InputBehavior";
		basePlayerPrefsKey = basePlayerPrefsKey + "|id=" + id;
		if (!PlayerPrefs.HasKey(basePlayerPrefsKey))
		{
			return string.Empty;
		}
		return PlayerPrefs.GetString(basePlayerPrefsKey);
	}
}
