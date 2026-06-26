using UnityEngine;

public class MegaShapeUtils
{
	public static GUIContent[] GetLayersAsContent(MegaShapeLoft loft)
	{
		GUIContent[] array;
		if (!loft)
		{
			array = new GUIContent[1]
			{
				new GUIContent("None")
			};
		}
		else
		{
			MegaLoftLayerSimple[] components = loft.GetComponents<MegaLoftLayerSimple>();
			array = new GUIContent[components.Length + 1];
			array[0] = new GUIContent("None");
			for (int i = 0; i < components.Length; i++)
			{
				array[i + 1] = new GUIContent(components[i].LayerName);
			}
		}
		return array;
	}

	public static string[] GetLayers(MegaShapeLoft loft)
	{
		string[] array;
		if (!loft)
		{
			array = new string[1] { "None" };
		}
		else
		{
			MegaLoftLayerSimple[] components = loft.GetComponents<MegaLoftLayerSimple>();
			array = new string[components.Length + 1];
			array[0] = "None";
			for (int i = 0; i < components.Length; i++)
			{
				array[i + 1] = components[i].LayerName;
			}
		}
		return array;
	}

	public static int FindLayer(MegaShapeLoft loft, int lay)
	{
		if ((bool)loft && lay < loft.Layers.Length)
		{
			int num = -1;
			for (int i = 0; i <= lay; i++)
			{
				if (loft.Layers[i] is MegaLoftLayerSimple)
				{
					num++;
				}
			}
			return num;
		}
		return -1;
	}

	public static void RotateZ(ref Matrix4x4 mat, float ang)
	{
		mat = Matrix4x4.identity;
		float value = Mathf.Cos(ang);
		float num = Mathf.Sin(ang);
		mat[0, 0] = value;
		mat[0, 1] = num;
		mat[1, 0] = 0f - num;
		mat[1, 1] = value;
	}
}
