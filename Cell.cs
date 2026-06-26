using System.Collections.Generic;
using UnityEngine;

public class Cell
{
	public List<Edge> north;

	public List<Edge> east;

	public List<Edge> south;

	public List<Edge> west;

	public Cell(Edge inNorth, Edge inEast, Edge inSouth, Edge inWest)
	{
		north = new List<Edge> { inNorth };
		north[0].RegisterPrim(this, Quadrant.NORTH);
		east = new List<Edge> { inEast };
		east[0].RegisterPrim(this, Quadrant.EAST);
		south = new List<Edge> { inSouth };
		south[0].RegisterPrim(this, Quadrant.SOUTH);
		west = new List<Edge> { inWest };
		west[0].RegisterPrim(this, Quadrant.WEST);
	}

	public Cell(Edge[] inNorth, Edge[] inEast, Edge[] inSouth, Edge[] inWest)
	{
		north = new List<Edge>(inNorth);
		for (int i = 0; i < north.Count; i++)
		{
			north[i].RegisterPrim(this, Quadrant.NORTH);
		}
		east = new List<Edge>(inEast);
		for (int j = 0; j < east.Count; j++)
		{
			east[j].RegisterPrim(this, Quadrant.EAST);
		}
		south = new List<Edge>(inSouth);
		for (int k = 0; k < south.Count; k++)
		{
			south[k].RegisterPrim(this, Quadrant.SOUTH);
		}
		west = new List<Edge>(inWest);
		for (int l = 0; l < west.Count; l++)
		{
			west[l].RegisterPrim(this, Quadrant.WEST);
		}
	}

	public bool IntersectLines(PolyLine[] lines)
	{
		Vector2 position = north[0].vertices[0].position;
		Vector2 position2 = east[0].vertices[0].position;
		Vector2 position3 = south[0].vertices[0].position;
		Vector2 position4 = west[0].vertices[0].position;
		Rect rect = new Rect(position.x, position.y, position3.x - position.x, position3.y - position.y);
		for (int i = 0; i < lines.Length; i++)
		{
			for (int j = 0; j < lines[i].edgeCount; j++)
			{
				Vector2 vector = lines[i].Point(j);
				Vector2 vector2 = lines[i].Edge(j);
				if (rect.Contains(vector) || rect.Contains(vector + vector2))
				{
					return true;
				}
				if (Collision2D.IntersectLines(vector2, position - vector, position2 - vector) || Collision2D.IntersectLines(vector2, position2 - vector, position3 - vector) || Collision2D.IntersectLines(vector2, position3 - vector, position4 - vector) || Collision2D.IntersectLines(vector2, position4 - vector, position - vector))
				{
					return true;
				}
			}
		}
		return false;
	}

	public Cell[] Subdivide()
	{
		if (north.Count == 1)
		{
			north[0].Split();
		}
		if (east.Count == 1)
		{
			east[0].Split();
		}
		if (south.Count == 1)
		{
			south[0].Split();
		}
		if (west.Count == 1)
		{
			west[0].Split();
		}
		Vtx2D vtx2D = new Vtx2D((north[0].vertices[0].position + east[0].vertices[0].position + south[1].vertices[1].position + west[1].vertices[1].position) * 0.25f);
		Edge[] array = new Edge[4]
		{
			new Edge(north[1].vertices[0], vtx2D),
			new Edge(vtx2D, south[1].vertices[0]),
			new Edge(vtx2D, east[1].vertices[0]),
			new Edge(west[1].vertices[0], vtx2D)
		};
		Cell[] result = new Cell[3]
		{
			new Cell(north[1], east[0], array[2], array[0]),
			new Cell(array[3], array[1], south[0], west[1]),
			new Cell(array[2], east[1], south[1], array[1])
		};
		north[1].RemovePrim(this);
		north.RemoveAt(1);
		east[1].RemovePrim(this);
		east.RemoveAt(1);
		east[0].RemovePrim(this);
		east.RemoveAt(0);
		south[1].RemovePrim(this);
		south.RemoveAt(1);
		south[0].RemovePrim(this);
		south.RemoveAt(0);
		west[1].RemovePrim(this);
		west.RemoveAt(1);
		east.Add(array[0]);
		array[0].RegisterPrim(this, Quadrant.EAST);
		south.Add(array[3]);
		array[3].RegisterPrim(this, Quadrant.SOUTH);
		return result;
	}
}
