using System.Collections.Generic;

public class Edge
{
	public List<CellRegister> cells;

	public Vtx2D[] vertices;

	public Edge(Vtx2D p0, Vtx2D p1)
	{
		cells = new List<CellRegister>();
		vertices = new Vtx2D[2] { p0, p1 };
	}

	public void RegisterPrim(Cell input, Quadrant quadrant)
	{
		for (int i = 0; i < cells.Count; i++)
		{
			if (cells[i].cell == input)
			{
				return;
			}
		}
		cells.Add(new CellRegister(input, quadrant));
	}

	public void RemovePrim(Cell input)
	{
		for (int num = cells.Count - 1; num >= 0; num--)
		{
			if (cells[num].cell == input)
			{
				cells.RemoveAt(num);
			}
		}
	}

	public void Split()
	{
		Vtx2D vtx2D = new Vtx2D((vertices[0].position + vertices[1].position) * 0.5f);
		Edge edge = new Edge(vtx2D, vertices[1]);
		edge.cells = new List<CellRegister>(cells);
		vertices[1] = vtx2D;
		for (int i = 0; i < cells.Count; i++)
		{
			switch (cells[i].quadrant)
			{
			case Quadrant.NORTH:
				cells[i].cell.north.Insert(cells[i].cell.north.IndexOf(this) + 1, edge);
				break;
			case Quadrant.EAST:
				cells[i].cell.east.Insert(cells[i].cell.east.IndexOf(this) + 1, edge);
				break;
			case Quadrant.SOUTH:
				cells[i].cell.south.Insert(cells[i].cell.south.IndexOf(this) + 1, edge);
				break;
			case Quadrant.WEST:
				cells[i].cell.west.Insert(cells[i].cell.west.IndexOf(this) + 1, edge);
				break;
			}
		}
	}
}
