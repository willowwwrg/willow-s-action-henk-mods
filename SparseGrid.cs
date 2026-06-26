using System.Collections.Generic;
using UnityEngine;

public class SparseGrid
{
	public List<Cell> grid;

	private PolyLine[] attractors;

	public SparseGrid(PolyLine[] inAttractors, Rect baseplane)
	{
		attractors = inAttractors;
		Vtx2D p = new Vtx2D(new Vector2(baseplane.xMin, baseplane.yMin));
		Vtx2D vtx2D = new Vtx2D(new Vector2(baseplane.xMax, baseplane.yMin));
		Vtx2D p2 = new Vtx2D(new Vector2(baseplane.xMax, baseplane.yMax));
		Vtx2D vtx2D2 = new Vtx2D(new Vector2(baseplane.xMin, baseplane.yMax));
		grid = new List<Cell>
		{
			new Cell(new Edge[1]
			{
				new Edge(p, vtx2D)
			}, new Edge[1]
			{
				new Edge(vtx2D, p2)
			}, new Edge[1]
			{
				new Edge(vtx2D2, p2)
			}, new Edge[1]
			{
				new Edge(p, vtx2D2)
			})
		};
	}

	public void SubDivide(int depth)
	{
		for (int i = 0; i < depth; i++)
		{
			int count = grid.Count;
			for (int j = 0; j < count; j++)
			{
				if (grid[j].IntersectLines(attractors))
				{
					grid.AddRange(grid[j].Subdivide());
				}
			}
		}
	}
}
