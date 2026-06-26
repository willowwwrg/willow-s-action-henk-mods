using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class MegaKML
{
	private enum kmlGeometryType
	{
		POINT,
		LINESTRING
	}

	private enum kmlTagType
	{
		POINT,
		LINESTRING,
		COORDINATES
	}

	private List<Hashtable> PointsCollection = new List<Hashtable>();

	private List<Hashtable> LinesCollection = new List<Hashtable>();

	private Hashtable Point;

	private Hashtable Line;

	private Hashtable Coordinates;

	private Hashtable KMLCollection = new Hashtable();

	private kmlGeometryType? currentGeometry;

	private kmlTagType? currentKmlTag;

	private string lastError;

	private List<Vector3> points = new List<Vector3>();

	public string LastError
	{
		get
		{
			return lastError;
		}
		set
		{
			lastError = value;
			throw new Exception(lastError);
		}
	}

	public Hashtable KMLDecode(string fileName)
	{
		points.Clear();
		readKML(fileName);
		if (PointsCollection != null)
		{
			KMLCollection.Add("POINTS", PointsCollection);
		}
		if (LinesCollection != null)
		{
			KMLCollection.Add("LINES", LinesCollection);
		}
		return KMLCollection;
	}

	private void readKML(string fileName)
	{
		using XmlReader xmlReader = XmlReader.Create(fileName);
		while (xmlReader.Read())
		{
			switch (xmlReader.NodeType)
			{
			case XmlNodeType.Element:
				switch (xmlReader.Name.ToUpper())
				{
				case "POINT":
					currentGeometry = kmlGeometryType.POINT;
					Point = new Hashtable();
					break;
				case "LINESTRING":
					currentGeometry = kmlGeometryType.LINESTRING;
					Line = new Hashtable();
					break;
				case "COORDINATES":
					currentKmlTag = kmlTagType.COORDINATES;
					break;
				}
				break;
			case XmlNodeType.EndElement:
			{
				string text = xmlReader.Name.ToUpper();
				if (!(text == "POINT"))
				{
					if (text == "LINESTRING")
					{
						if (Line != null)
						{
							LinesCollection.Add(Line);
						}
						Line = null;
						currentGeometry = null;
						currentKmlTag = null;
					}
				}
				else
				{
					if (Point != null)
					{
						PointsCollection.Add(Point);
					}
					Point = null;
					currentGeometry = null;
					currentKmlTag = null;
				}
				break;
			}
			case XmlNodeType.Text:
			case XmlNodeType.CDATA:
			case XmlNodeType.Comment:
			case XmlNodeType.XmlDeclaration:
			{
				kmlTagType? kmlTagType2 = currentKmlTag;
				if (kmlTagType2.HasValue && kmlTagType2.Value == kmlTagType.COORDINATES)
				{
					parseGeometryVal(xmlReader.Value);
				}
				break;
			}
			}
		}
	}

	protected void parseGeometryVal(string tag_value)
	{
		kmlGeometryType? kmlGeometryType2 = currentGeometry;
		if (kmlGeometryType2.HasValue)
		{
			switch (kmlGeometryType2.Value)
			{
			case kmlGeometryType.POINT:
				parsePoint(tag_value);
				break;
			case kmlGeometryType.LINESTRING:
				parseLine(tag_value);
				break;
			}
		}
	}

	protected void parsePoint(string tag_value)
	{
		Hashtable hashtable = null;
		kmlTagType? kmlTagType2 = currentKmlTag;
		if (kmlTagType2.HasValue && kmlTagType2.Value == kmlTagType.COORDINATES)
		{
			hashtable = new Hashtable();
			string[] array = tag_value.Split(',');
			if (array.Length < 2)
			{
				lastError = "ERROR IN FORMAT OF POINT COORDINATES";
			}
			hashtable.Add("LNG", array[0].Trim());
			hashtable.Add("LAT", array[1].Trim());
			Point.Add("COORDINATES", hashtable);
		}
	}

	protected void parseLine(string tag_value)
	{
		kmlTagType? kmlTagType2 = currentKmlTag;
		if (!kmlTagType2.HasValue || kmlTagType2.Value != kmlTagType.COORDINATES)
		{
			return;
		}
		string[] array = tag_value.Trim().Split(' ');
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = array[i].Split(',');
			if (array2.Length < 2)
			{
				LastError = "ERROR IN FORMAT OF LINESTRING COORDINATES";
			}
			points.Add(new Vector3(float.Parse(array2[0]), float.Parse(array2[2]), float.Parse(array2[1])));
		}
	}

	public Vector3[] GetPoints(float scale)
	{
		Bounds bounds = new Bounds(points[0], Vector3.zero);
		for (int i = 0; i < points.Count; i++)
		{
			bounds.Encapsulate(points[i]);
		}
		for (int j = 0; j < points.Count; j++)
		{
			points[j] = ConvertLatLon(points[j], bounds.center, scale, adjust: false);
		}
		return points.ToArray();
	}

	private Vector3 ConvertLatLon(Vector3 pos, Vector3 centre, float scale, bool adjust)
	{
		double num = 111322.3167 / (double)scale;
		double num2 = pos.x - centre.x;
		double num3 = pos.y - centre.y;
		double num4 = pos.z - centre.z;
		Vector3 result = default(Vector3);
		if (adjust)
		{
			double num5 = 6378137.0;
			result.x = (float)(num4 * (2.0 * (double)Mathf.Tan((float)Math.PI / 360f) * num5 * (double)Mathf.Cos((float)Math.PI / 180f * (float)num2)));
		}
		else
		{
			result.x = (float)(num4 * num);
		}
		result.z = (float)((0.0 - num2) * num);
		result.y = (float)num3;
		return result;
	}
}
