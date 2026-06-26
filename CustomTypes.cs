using ExitGames.Client.Photon;
using UnityEngine;

internal static class CustomTypes
{
	internal static void Register()
	{
		PhotonPeer.RegisterType(typeof(Vector2), 87, SerializeVector2, DeserializeVector2);
		PhotonPeer.RegisterType(typeof(Vector3), 86, SerializeVector3, DeserializeVector3);
		PhotonPeer.RegisterType(typeof(Quaternion), 81, SerializeQuaternion, DeserializeQuaternion);
		PhotonPeer.RegisterType(typeof(PhotonPlayer), 80, SerializePhotonPlayer, DeserializePhotonPlayer);
	}

	private static byte[] SerializeVector3(object customobject)
	{
		Vector3 obj = (Vector3)customobject;
		int targetOffset = 0;
		byte[] array = new byte[12];
		Protocol.Serialize(obj.x, array, ref targetOffset);
		Protocol.Serialize(obj.y, array, ref targetOffset);
		Protocol.Serialize(obj.z, array, ref targetOffset);
		return array;
	}

	private static object DeserializeVector3(byte[] bytes)
	{
		Vector3 vector = default(Vector3);
		int offset = 0;
		Protocol.Deserialize(out vector.x, bytes, ref offset);
		Protocol.Deserialize(out vector.y, bytes, ref offset);
		Protocol.Deserialize(out vector.z, bytes, ref offset);
		return vector;
	}

	private static byte[] SerializeVector2(object customobject)
	{
		Vector2 obj = (Vector2)customobject;
		byte[] array = new byte[8];
		int targetOffset = 0;
		Protocol.Serialize(obj.x, array, ref targetOffset);
		Protocol.Serialize(obj.y, array, ref targetOffset);
		return array;
	}

	private static object DeserializeVector2(byte[] bytes)
	{
		Vector2 vector = default(Vector2);
		int offset = 0;
		Protocol.Deserialize(out vector.x, bytes, ref offset);
		Protocol.Deserialize(out vector.y, bytes, ref offset);
		return vector;
	}

	private static byte[] SerializeQuaternion(object obj)
	{
		Quaternion obj2 = (Quaternion)obj;
		byte[] array = new byte[16];
		int targetOffset = 0;
		Protocol.Serialize(obj2.w, array, ref targetOffset);
		Protocol.Serialize(obj2.x, array, ref targetOffset);
		Protocol.Serialize(obj2.y, array, ref targetOffset);
		Protocol.Serialize(obj2.z, array, ref targetOffset);
		return array;
	}

	private static object DeserializeQuaternion(byte[] bytes)
	{
		Quaternion quaternion = default(Quaternion);
		int offset = 0;
		Protocol.Deserialize(out quaternion.w, bytes, ref offset);
		Protocol.Deserialize(out quaternion.x, bytes, ref offset);
		Protocol.Deserialize(out quaternion.y, bytes, ref offset);
		Protocol.Deserialize(out quaternion.z, bytes, ref offset);
		return quaternion;
	}

	private static byte[] SerializePhotonPlayer(object customobject)
	{
		int iD = ((PhotonPlayer)customobject).ID;
		byte[] array = new byte[4];
		int targetOffset = 0;
		Protocol.Serialize(iD, array, ref targetOffset);
		return array;
	}

	private static object DeserializePhotonPlayer(byte[] bytes)
	{
		int offset = 0;
		Protocol.Deserialize(out int value, bytes, ref offset);
		if (PhotonNetwork.networkingPeer.mActors.ContainsKey(value))
		{
			return PhotonNetwork.networkingPeer.mActors[value];
		}
		return null;
	}
}
