using System;
using UnityEngine;

public class MegaModBut
{
	public string name;

	public Color color;

	public Type classname;

	public GUIContent content;

	public MegaModBut()
	{
	}

	public MegaModBut(string _but, string tooltip, Type _classname, Color _col)
	{
		name = _but;
		color = _col;
		classname = _classname;
		content = new GUIContent(_but, tooltip);
	}
}
