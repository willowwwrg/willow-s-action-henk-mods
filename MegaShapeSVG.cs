using System;
using System.Text.RegularExpressions;
using UnityEngine;

public class MegaShapeSVG
{
	private const float CIRCLE_VECTOR_LENGTH = 0.5517862f;

	private int splineindex;

	private char[] commaspace = new char[2] { ',', ' ' };

	public void LoadXML(string svgdata, MegaShape shape, bool clear, int start)
	{
		MegaXMLNode node = new MegaXMLReader().read(svgdata);
		if (!clear)
		{
			shape.splines.Clear();
		}
		shape.selcurve = start;
		splineindex = start;
		ParseXML(node, shape);
	}

	public void ParseXML(MegaXMLNode node, MegaShape shape)
	{
		foreach (MegaXMLNode child in node.children)
		{
			switch (child.tagName)
			{
			case "circle":
				ParseCircle(child, shape);
				break;
			case "path":
				ParsePath(child, shape);
				break;
			case "ellipse":
				ParseEllipse(child, shape);
				break;
			case "rect":
				ParseRect(child, shape);
				break;
			case "polygon":
				ParsePolygon(child, shape);
				break;
			}
			ParseXML(child, shape);
		}
	}

	private MegaSpline GetSpline(MegaShape shape)
	{
		MegaSpline megaSpline;
		if (splineindex < shape.splines.Count)
		{
			megaSpline = shape.splines[splineindex];
		}
		else
		{
			megaSpline = new MegaSpline();
			shape.splines.Add(megaSpline);
		}
		splineindex++;
		return megaSpline;
	}

	private Vector3 SwapAxis(Vector3 val, MegaAxis axis)
	{
		float num = 0f;
		switch (axis)
		{
		case MegaAxis.X:
			num = val.x;
			val.x = val.y;
			val.y = num;
			break;
		case MegaAxis.Z:
			num = val.y;
			val.y = val.z;
			val.z = num;
			break;
		}
		return val;
	}

	private void AddKnot(MegaSpline spline, Vector3 p, Vector3 invec, Vector3 outvec, MegaAxis axis)
	{
		spline.AddKnot(SwapAxis(p, axis), SwapAxis(invec, axis), SwapAxis(outvec, axis));
	}

	private void ParseCircle(MegaXMLNode node, MegaShape shape)
	{
		MegaSpline spline = GetSpline(shape);
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		for (int i = 0; i < node.values.Count; i++)
		{
			MegaXMLValue megaXMLValue = node.values[i];
			switch (megaXMLValue.name)
			{
			case "cx":
				num = float.Parse(megaXMLValue.value);
				break;
			case "cy":
				num2 = float.Parse(megaXMLValue.value);
				break;
			case "r":
				num3 = float.Parse(megaXMLValue.value);
				break;
			}
		}
		float num4 = 0.5517862f * num3;
		spline.knots.Clear();
		for (int j = 0; j < 4; j++)
		{
			float f = (float)Math.PI * 2f * (float)j / 4f;
			float num5 = Mathf.Sin(f);
			float num6 = Mathf.Cos(f);
			Vector3 vector = new Vector3(num6 * num3 + num, 0f, num5 * num3 + num2);
			Vector3 vector2 = new Vector3(num5 * num4, 0f, (0f - num6) * num4);
			AddKnot(spline, vector, vector + vector2, vector - vector2, shape.axis);
		}
		spline.closed = true;
	}

	private void ParseEllipse(MegaXMLNode node, MegaShape shape)
	{
		MegaSpline spline = GetSpline(shape);
		float num = 0f;
		float num2 = 0f;
		float value = 0f;
		float value2 = 0f;
		for (int i = 0; i < node.values.Count; i++)
		{
			MegaXMLValue megaXMLValue = node.values[i];
			switch (megaXMLValue.name)
			{
			case "cx":
				num = float.Parse(megaXMLValue.value);
				break;
			case "cy":
				num2 = float.Parse(megaXMLValue.value);
				break;
			case "rx":
				value = float.Parse(megaXMLValue.value);
				break;
			case "ry":
				value2 = float.Parse(megaXMLValue.value);
				break;
			}
		}
		value2 = Mathf.Clamp(value2, 0f, float.MaxValue);
		value = Mathf.Clamp(value, 0f, float.MaxValue);
		float num3;
		float x;
		float y;
		if (value2 < value)
		{
			num3 = value;
			x = 1f;
			y = value2 / value;
		}
		else if (value < value2)
		{
			num3 = value2;
			x = value / value2;
			y = 1f;
		}
		else
		{
			num3 = value2;
			x = (y = 1f);
		}
		float num4 = 0.5517862f * num3;
		Vector3 b = new Vector3(x, y, 1f);
		for (int j = 0; j < 4; j++)
		{
			float f = (float)Math.PI * 2f * (float)j / 4f;
			float num5 = Mathf.Sin(f);
			float num6 = Mathf.Cos(f);
			Vector3 vector = new Vector3(num6 * num3 + num, 0f, num5 * num3 + num2);
			Vector3 vector2 = new Vector3(num5 * num4, 0f, (0f - num6) * num4);
			AddKnot(spline, Vector3.Scale(vector, b), Vector3.Scale(vector + vector2, b), Vector3.Scale(vector - vector2, b), shape.axis);
		}
		spline.closed = true;
	}

	private void ParseRect(MegaXMLNode node, MegaShape shape)
	{
		MegaSpline spline = GetSpline(shape);
		Vector3[] array = new Vector3[4];
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		for (int i = 0; i < node.values.Count; i++)
		{
			MegaXMLValue megaXMLValue = node.values[i];
			switch (megaXMLValue.name)
			{
			case "x":
				num3 = float.Parse(megaXMLValue.value);
				break;
			case "y":
				num4 = float.Parse(megaXMLValue.value);
				break;
			case "width":
				num = float.Parse(megaXMLValue.value);
				break;
			case "height":
				num2 = float.Parse(megaXMLValue.value);
				break;
			case "transform":
				Debug.Log("SVG Transform not implemented yet");
				break;
			}
		}
		ref Vector3 reference = ref array[0];
		reference = new Vector3(num3, 0f, num4);
		ref Vector3 reference2 = ref array[1];
		reference2 = new Vector3(num3, 0f, num4 + num2);
		ref Vector3 reference3 = ref array[2];
		reference3 = new Vector3(num3 + num, 0f, num4 + num2);
		ref Vector3 reference4 = ref array[3];
		reference4 = new Vector3(num3 + num, 0f, num4);
		spline.closed = true;
		spline.knots.Clear();
		AddKnot(spline, array[0], array[0], array[0], shape.axis);
		AddKnot(spline, array[1], array[1], array[1], shape.axis);
		AddKnot(spline, array[2], array[2], array[2], shape.axis);
		AddKnot(spline, array[3], array[3], array[3], shape.axis);
	}

	private void ParsePolygon(MegaXMLNode node, MegaShape shape)
	{
		MegaSpline spline = GetSpline(shape);
		spline.knots.Clear();
		spline.closed = true;
		char[] separator = new char[1] { ' ' };
		for (int i = 0; i < node.values.Count; i++)
		{
			MegaXMLValue megaXMLValue = node.values[i];
			if (megaXMLValue.name == "points")
			{
				string[] array = megaXMLValue.value.Split(separator, StringSplitOptions.RemoveEmptyEntries);
				for (int j = 0; j < array.Length; j++)
				{
					Vector3 vector = ParseV2Split(array[j], 0);
					MegaKnot megaKnot = new MegaKnot();
					megaKnot.p = SwapAxis(new Vector3(vector.x, 0f, vector.y), shape.axis);
					megaKnot.invec = megaKnot.p;
					megaKnot.outvec = megaKnot.p;
					spline.knots.Add(megaKnot);
				}
			}
		}
		if (spline.closed)
		{
			Vector3 vector2 = spline.knots[0].outvec - spline.knots[0].p;
			spline.knots[0].invec = spline.knots[0].p - vector2;
		}
	}

	private void ParsePath(MegaXMLNode node, MegaShape shape)
	{
		Vector3 vector = Vector3.zero;
		char[] separator = new char[2] { ',', ' ' };
		MegaSpline megaSpline = null;
		for (int i = 0; i < node.values.Count; i++)
		{
			MegaXMLValue megaXMLValue = node.values[i];
			if (!(megaXMLValue.name == "d"))
			{
				continue;
			}
			string[] array = Regex.Split(megaXMLValue.value, "(?=[MmLlCcSsZzHhVv])");
			for (int j = 0; j < array.Length; j++)
			{
				if (array[j].Length <= 0)
				{
					continue;
				}
				string text = array[j].Substring(1);
				if (text != null && text.Length > 0)
				{
					text = text.Replace("-", ",-");
					while (text.Length > 0 && (text[0] == ',' || text[0] == ' '))
					{
						text = text.Substring(1);
					}
				}
				switch (array[j][0])
				{
				case 'Z':
				case 'z':
					if (megaSpline != null)
					{
						megaSpline.closed = true;
						int index = megaSpline.knots.Count - 1;
						megaSpline.knots[0].invec = megaSpline.knots[index].invec;
						megaSpline.knots.Remove(megaSpline.knots[index]);
					}
					break;
				case 'M':
				{
					megaSpline = GetSpline(shape);
					megaSpline.knots.Clear();
					vector = ParseV2Split(text, 0);
					MegaKnot megaKnot2 = new MegaKnot();
					megaKnot2.p = SwapAxis(new Vector3(vector.x, 0f, vector.y), shape.axis);
					megaKnot2.invec = megaKnot2.p;
					megaKnot2.outvec = megaKnot2.p;
					megaSpline.knots.Add(megaKnot2);
					break;
				}
				case 'm':
				{
					megaSpline = GetSpline(shape);
					megaSpline.knots.Clear();
					string[] array8 = text.Split(" "[0]);
					for (int num3 = 0; num3 < array8.Length - 1; num3++)
					{
						Vector3 vector8 = ParseV2Split(array8[num3], 0);
						vector.x += vector8.x;
						vector.y += vector8.y;
						MegaKnot megaKnot8 = new MegaKnot();
						megaKnot8.p = SwapAxis(new Vector3(vector.x, 0f, vector.y), shape.axis);
						megaKnot8.invec = megaKnot8.p;
						megaKnot8.outvec = megaKnot8.p;
						megaSpline.knots.Add(megaKnot8);
					}
					break;
				}
				case 'l':
				{
					string[] array9 = text.Split(","[0]);
					for (int num4 = 0; num4 < array9.Length; num4 += 2)
					{
						vector += ParseV2(array9, num4);
					}
					megaSpline.knots[megaSpline.knots.Count - 1].outvec = SwapAxis(new Vector3(vector.x, 0f, vector.y), shape.axis);
					MegaKnot megaKnot9 = new MegaKnot();
					megaKnot9.p = SwapAxis(new Vector3(vector.x, 0f, vector.y), shape.axis);
					megaKnot9.invec = SwapAxis(new Vector3(vector.x, 0f, vector.y), shape.axis);
					megaKnot9.outvec = megaKnot9.p - (megaKnot9.invec - megaKnot9.p);
					megaSpline.knots.Add(megaKnot9);
					break;
				}
				case 'c':
				{
					string[] array4 = text.Split(separator, StringSplitOptions.RemoveEmptyEntries);
					for (int m = 0; m < array4.Length; m += 6)
					{
						Vector2 vector5 = vector + ParseV2(array4, m);
						Vector2 vector6 = vector + ParseV2(array4, m + 2);
						vector += ParseV2(array4, m + 4);
						megaSpline.knots[megaSpline.knots.Count - 1].outvec = SwapAxis(new Vector3(vector5.x, 0f, vector5.y), shape.axis);
						MegaKnot megaKnot4 = new MegaKnot();
						megaKnot4.p = SwapAxis(new Vector3(vector.x, 0f, vector.y), shape.axis);
						megaKnot4.invec = SwapAxis(new Vector3(vector6.x, 0f, vector6.y), shape.axis);
						megaKnot4.outvec = megaKnot4.p - (megaKnot4.invec - megaKnot4.p);
						megaSpline.knots.Add(megaKnot4);
					}
					break;
				}
				case 'L':
				{
					string[] array11 = text.Split(","[0]);
					for (int num6 = 0; num6 < array11.Length; num6 += 2)
					{
						vector = ParseV2(array11, num6);
					}
					megaSpline.knots[megaSpline.knots.Count - 1].outvec = SwapAxis(new Vector3(vector.x, 0f, vector.y), shape.axis);
					MegaKnot megaKnot11 = new MegaKnot();
					megaKnot11.p = SwapAxis(new Vector3(vector.x, 0f, vector.y), shape.axis);
					megaKnot11.invec = SwapAxis(new Vector3(vector.x, 0f, vector.y), shape.axis);
					megaKnot11.outvec = megaKnot11.p - (megaKnot11.invec - megaKnot11.p);
					megaSpline.knots.Add(megaKnot11);
					break;
				}
				case 'v':
				{
					string[] array6 = text.Split(","[0]);
					for (int num = 0; num < array6.Length; num++)
					{
						vector.y += float.Parse(array6[num]);
					}
					megaSpline.knots[megaSpline.knots.Count - 1].outvec = SwapAxis(new Vector3(vector.x, 0f, vector.y), shape.axis);
					MegaKnot megaKnot6 = new MegaKnot();
					megaKnot6.p = SwapAxis(new Vector3(vector.x, 0f, vector.y), shape.axis);
					megaKnot6.invec = SwapAxis(new Vector3(vector.x, 0f, vector.y), shape.axis);
					megaKnot6.outvec = megaKnot6.p - (megaKnot6.invec - megaKnot6.p);
					megaSpline.knots.Add(megaKnot6);
					break;
				}
				case 'V':
				{
					string[] array12 = text.Split(","[0]);
					for (int num7 = 0; num7 < array12.Length; num7++)
					{
						vector.y = float.Parse(array12[num7]);
					}
					megaSpline.knots[megaSpline.knots.Count - 1].outvec = SwapAxis(new Vector3(vector.x, 0f, vector.y), shape.axis);
					MegaKnot megaKnot12 = new MegaKnot();
					megaKnot12.p = SwapAxis(new Vector3(vector.x, 0f, vector.y), shape.axis);
					megaKnot12.invec = SwapAxis(new Vector3(vector.x, 0f, vector.y), shape.axis);
					megaKnot12.outvec = megaKnot12.p - (megaKnot12.invec - megaKnot12.p);
					megaSpline.knots.Add(megaKnot12);
					break;
				}
				case 'h':
				{
					string[] array10 = text.Split(","[0]);
					for (int num5 = 0; num5 < array10.Length; num5++)
					{
						vector.x += float.Parse(array10[num5]);
					}
					megaSpline.knots[megaSpline.knots.Count - 1].outvec = SwapAxis(new Vector3(vector.x, 0f, vector.y), shape.axis);
					MegaKnot megaKnot10 = new MegaKnot();
					megaKnot10.p = SwapAxis(new Vector3(vector.x, 0f, vector.y), shape.axis);
					megaKnot10.invec = SwapAxis(new Vector3(vector.x, 0f, vector.y), shape.axis);
					megaKnot10.outvec = megaKnot10.p - (megaKnot10.invec - megaKnot10.p);
					megaSpline.knots.Add(megaKnot10);
					break;
				}
				case 'H':
				{
					string[] array7 = text.Split(","[0]);
					for (int num2 = 0; num2 < array7.Length; num2++)
					{
						vector.x = float.Parse(array7[num2]);
					}
					megaSpline.knots[megaSpline.knots.Count - 1].outvec = SwapAxis(new Vector3(vector.x, 0f, vector.y), shape.axis);
					MegaKnot megaKnot7 = new MegaKnot();
					megaKnot7.p = SwapAxis(new Vector3(vector.x, 0f, vector.y), shape.axis);
					megaKnot7.invec = SwapAxis(new Vector3(vector.x, 0f, vector.y), shape.axis);
					megaKnot7.outvec = megaKnot7.p - (megaKnot7.invec - megaKnot7.p);
					megaSpline.knots.Add(megaKnot7);
					break;
				}
				case 'S':
				{
					string[] array5 = text.Split(","[0]);
					for (int n = 0; n < array5.Length; n += 4)
					{
						vector = ParseV2(array5, n + 2);
						Vector2 vector7 = ParseV2(array5, n);
						MegaKnot megaKnot5 = new MegaKnot();
						megaKnot5.p = SwapAxis(new Vector3(vector.x, 0f, vector.y), shape.axis);
						megaKnot5.invec = SwapAxis(new Vector3(vector7.x, 0f, vector7.y), shape.axis);
						megaKnot5.outvec = megaKnot5.p - (megaKnot5.invec - megaKnot5.p);
						megaSpline.knots.Add(megaKnot5);
					}
					break;
				}
				case 's':
				{
					string[] array3 = text.Split(separator, StringSplitOptions.RemoveEmptyEntries);
					for (int l = 0; l < array3.Length; l += 4)
					{
						Vector2 vector4 = vector + ParseV2(array3, l);
						vector += ParseV2(array3, l + 2);
						MegaKnot megaKnot3 = new MegaKnot();
						megaKnot3.p = SwapAxis(new Vector3(vector.x, 0f, vector.y), shape.axis);
						megaKnot3.invec = SwapAxis(new Vector3(vector4.x, 0f, vector4.y), shape.axis);
						megaKnot3.outvec = megaKnot3.p - (megaKnot3.invec - megaKnot3.p);
						megaSpline.knots.Add(megaKnot3);
					}
					break;
				}
				case 'C':
				{
					string[] array2 = text.Split(separator, StringSplitOptions.RemoveEmptyEntries);
					for (int k = 0; k < array2.Length; k += 6)
					{
						Vector2 vector2 = ParseV2(array2, k);
						Vector2 vector3 = ParseV2(array2, k + 2);
						vector = ParseV2(array2, k + 4);
						megaSpline.knots[megaSpline.knots.Count - 1].outvec = SwapAxis(new Vector3(vector2.x, 0f, vector2.y), shape.axis);
						MegaKnot megaKnot = new MegaKnot();
						megaKnot.p = SwapAxis(new Vector3(vector.x, 0f, vector.y), shape.axis);
						megaKnot.invec = SwapAxis(new Vector3(vector3.x, 0f, vector3.y), shape.axis);
						megaKnot.outvec = megaKnot.p - (megaKnot.invec - megaKnot.p);
						megaSpline.knots.Add(megaKnot);
					}
					break;
				}
				}
			}
		}
	}

	public void importData(string svgdata, MegaShape shape, float scale, bool clear, int start)
	{
		LoadXML(svgdata, shape, clear, start);
		for (int i = start; i < splineindex; i++)
		{
			if (shape.splines[i].Area() < 0f)
			{
				shape.splines[i].reverse = false;
			}
			else
			{
				shape.splines[i].reverse = true;
			}
		}
		shape.Centre(scale, new Vector3(-1f, 1f, 1f), start);
		shape.CalcLength();
	}

	private Vector2 ParseV2Split(string str, int i)
	{
		return ParseV2(str.Split(commaspace, StringSplitOptions.RemoveEmptyEntries), i);
	}

	private Vector3 ParseV2(string[] str, int i)
	{
		Vector3 result = Vector2.zero;
		result.x = float.Parse(str[i]);
		result.y = float.Parse(str[i + 1]);
		return result;
	}
}
