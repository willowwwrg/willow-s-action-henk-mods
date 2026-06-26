using System;
using UnityEngine;

public class MegaShapeSXL
{
	private int splineindex;

	private char[] commaspace = new char[2] { ',', ' ' };

	public void LoadXML(string sxldata, MegaShape shape, bool clear, int start)
	{
		MegaXMLNode node = new MegaXMLReader().read(sxldata);
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
			if (child.tagName == "Shape")
			{
				ParseShape(child, shape);
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

	public void ParseShape(MegaXMLNode node, MegaShape shape)
	{
		for (int i = 0; i < node.values.Count; i++)
		{
			_ = node.values[i].name;
		}
		foreach (MegaXMLNode child in node.children)
		{
			if (child.tagName == "Spline")
			{
				ParseSpline(child, shape);
			}
		}
	}

	public void ParseSpline(MegaXMLNode node, MegaShape shape)
	{
		MegaSpline megaSpline = new MegaSpline();
		for (int i = 0; i < node.values.Count; i++)
		{
			_ = node.values[i].name == "flags";
		}
		foreach (MegaXMLNode child in node.children)
		{
			if (child.tagName == "K")
			{
				ParseKnot(child, shape, megaSpline);
			}
		}
		shape.splines.Add(megaSpline);
	}

	public void ParseKnot(MegaXMLNode node, MegaShape shape, MegaSpline spline)
	{
		Vector3 p = Vector3.zero;
		Vector3 invec = Vector3.zero;
		Vector3 outvec = Vector3.zero;
		for (int i = 0; i < node.values.Count; i++)
		{
			MegaXMLValue megaXMLValue = node.values[i];
			switch (megaXMLValue.name)
			{
			case "p":
				p = ParseV3Split(megaXMLValue.value, 0);
				break;
			case "i":
				invec = ParseV3Split(megaXMLValue.value, 0);
				break;
			case "o":
				outvec = ParseV3Split(megaXMLValue.value, 0);
				break;
			}
		}
		spline.AddKnot(p, invec, outvec);
	}

	private Vector3 ParseV3Split(string str, int i)
	{
		return ParseV3(str.Split(commaspace, StringSplitOptions.RemoveEmptyEntries), i);
	}

	private Vector3 ParseV3(string[] str, int i)
	{
		Vector3 zero = Vector3.zero;
		zero.x = float.Parse(str[i]);
		zero.y = float.Parse(str[i + 1]);
		zero.z = float.Parse(str[i + 2]);
		return zero;
	}

	public void importData(string sxldata, MegaShape shape, float scale, bool clear, int start)
	{
		LoadXML(sxldata, shape, clear, start);
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
		shape.Centre(scale, new Vector3(1f, 1f, 1f), start);
		shape.CalcLength();
	}
}
