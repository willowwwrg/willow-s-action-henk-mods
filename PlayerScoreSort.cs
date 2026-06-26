using System.Collections.Generic;

public class PlayerScoreSort : Comparer<PhotonPlayer>
{
	public override int Compare(PhotonPlayer x, PhotonPlayer y)
	{
		float num = -1000000f;
		float value = -1000000f;
		if (x.customProperties["score"] != null)
		{
			num = 0f - (float)x.customProperties["score"];
		}
		if (y.customProperties["score"] != null)
		{
			value = 0f - (float)y.customProperties["score"];
		}
		return -num.CompareTo(value);
	}
}
