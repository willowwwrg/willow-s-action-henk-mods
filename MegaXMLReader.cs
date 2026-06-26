public class MegaXMLReader
{
	private static char TAG_START = '<';

	private static char TAG_END = '>';

	private static char SPACE = ' ';

	private static char QUOTE = '"';

	private static char SLASH = '/';

	private static char EQUALS = '=';

	private static string BEGIN_QUOTE = string.Empty + EQUALS + QUOTE;

	public MegaXMLNode read(string xml)
	{
		int num = 0;
		int num2 = 0;
		MegaXMLNode megaXMLNode = new MegaXMLNode();
		MegaXMLNode megaXMLNode2 = megaXMLNode;
		xml = xml.Replace(" \n", string.Empty);
		xml = xml.Replace("\n", string.Empty);
		while (true)
		{
			num = xml.IndexOf(TAG_START, num2);
			if (num < 0 || num >= xml.Length)
			{
				break;
			}
			num++;
			num2 = xml.IndexOf(TAG_END, num);
			if (num2 < 0 || num2 >= xml.Length)
			{
				break;
			}
			int num3 = num2 - num;
			string text = xml.Substring(num, num3);
			if (text[0] == SLASH)
			{
				megaXMLNode2 = megaXMLNode2.parentNode;
				continue;
			}
			bool flag = true;
			if (text[num3 - 1] == SLASH)
			{
				text = text.Substring(0, num3 - 1);
				flag = false;
			}
			MegaXMLNode megaXMLNode3 = parseTag(text);
			megaXMLNode3.parentNode = megaXMLNode2;
			megaXMLNode2.children.Add(megaXMLNode3);
			if (flag)
			{
				megaXMLNode2 = megaXMLNode3;
			}
		}
		return megaXMLNode;
	}

	public MegaXMLNode parseTag(string xmlTag)
	{
		MegaXMLNode megaXMLNode = new MegaXMLNode();
		int num = xmlTag.IndexOf(SPACE, 0);
		if (num < 0)
		{
			megaXMLNode.tagName = xmlTag;
			return megaXMLNode;
		}
		string tagName = xmlTag.Substring(0, num);
		megaXMLNode.tagName = tagName;
		string xmlTag2 = xmlTag.Substring(num, xmlTag.Length - num);
		return parseAttributes(xmlTag2, megaXMLNode);
	}

	public MegaXMLNode parseAttributes(string xmlTag, MegaXMLNode node)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		while (true)
		{
			num = xmlTag.IndexOf(BEGIN_QUOTE, num3);
			if (num < 0 || num > xmlTag.Length)
			{
				break;
			}
			num2 = xmlTag.LastIndexOf(SPACE, num);
			if (num2 < 0 || num2 > xmlTag.Length)
			{
				break;
			}
			num2++;
			string name = xmlTag.Substring(num2, num - num2);
			num += 2;
			num3 = xmlTag.IndexOf(QUOTE, num);
			if (num3 < 0 || num3 > xmlTag.Length)
			{
				break;
			}
			int length = num3 - num;
			string value = xmlTag.Substring(num, length);
			MegaXMLValue megaXMLValue = new MegaXMLValue();
			megaXMLValue.name = name;
			megaXMLValue.value = value;
			node.values.Add(megaXMLValue);
		}
		return node;
	}
}
