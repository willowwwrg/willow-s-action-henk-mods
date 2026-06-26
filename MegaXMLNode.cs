using System.Collections.Generic;

public class MegaXMLNode
{
	public string tagName;

	public MegaXMLNode parentNode;

	public List<MegaXMLNode> children;

	public List<MegaXMLValue> values;

	public MegaXMLNode()
	{
		tagName = "NONE";
		parentNode = null;
		children = new List<MegaXMLNode>();
		values = new List<MegaXMLValue>();
	}
}
