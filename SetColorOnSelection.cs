using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Examples/Set Color on Selection")]
[RequireComponent(typeof(UIWidget))]
public class SetColorOnSelection : MonoBehaviour
{
	private UIWidget mWidget;

	public void SetSpriteBySelection()
	{
		if (UIPopupList.current == null)
		{
			return;
		}
		if (mWidget == null)
		{
			mWidget = GetComponent<UIWidget>();
		}
		string value = UIPopupList.current.value;
		if (value == null)
		{
			return;
		}
		switch (value.Length)
		{
		case 5:
			switch (value[0])
			{
			case 'W':
				if (value == "White")
				{
					mWidget.color = Color.white;
				}
				break;
			case 'G':
				if (value == "Green")
				{
					mWidget.color = Color.green;
				}
				break;
			}
			break;
		case 4:
			switch (value[0])
			{
			case 'B':
				if (value == "Blue")
				{
					mWidget.color = Color.blue;
				}
				break;
			case 'C':
				if (value == "Cyan")
				{
					mWidget.color = Color.cyan;
				}
				break;
			}
			break;
		case 3:
			if (value == "Red")
			{
				mWidget.color = Color.red;
			}
			break;
		case 6:
			if (value == "Yellow")
			{
				mWidget.color = Color.yellow;
			}
			break;
		case 7:
			if (value == "Magenta")
			{
				mWidget.color = Color.magenta;
			}
			break;
		}
	}
}
