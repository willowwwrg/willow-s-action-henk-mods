using UnityEngine;

public class DiscoFloor : MonoBehaviour
{
	public float rotationAngle;

	public TilesRow[] floorTiles;

	public DiscoFloorState state;

	private Color[] pix;

	private void Start()
	{
		SetState(state);
	}

	private void SetState(DiscoFloorState state)
	{
		for (int i = 0; i < floorTiles.Length; i++)
		{
			for (int j = 0; j < floorTiles.Length; j++)
			{
				DiscoTile discoTile = floorTiles[i].rowTiles[j];
				switch (state)
				{
				case DiscoFloorState.Brabant:
					if (i % 2 == 0)
					{
						if (j % 2 == 0)
						{
							discoTile.color = new HSBColor(Color.white);
						}
						else
						{
							discoTile.color = new HSBColor(Color.red);
						}
					}
					else if (j % 2 == 0)
					{
						discoTile.color = new HSBColor(Color.red);
					}
					else
					{
						discoTile.color = new HSBColor(Color.white);
					}
					discoTile.cycleSpeed = 0f;
					discoTile.onChance = 1f;
					break;
				case DiscoFloorState.AllOn:
					discoTile.color = new HSBColor(Random.value, 1f, 1f);
					discoTile.cycleSpeed = 0.25f;
					discoTile.onChance = 1f;
					break;
				case DiscoFloorState.Standard:
					discoTile.color = new HSBColor(Random.value, 1f, 1f);
					discoTile.cycleSpeed = 0.25f;
					discoTile.onChance = 0.8f;
					break;
				}
			}
		}
	}
}
