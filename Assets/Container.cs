using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Container : MonoBehaviour
{
	public GameObject self;
	public List<Container> children { get; set; }
	public List<Container> siblings { get; set; }
	public Container parent { get; set; }
	public int id { get; set; }
	public int depth { get; set; }
	public string name { get; set; }
	public float size { get; set; }
	public float weight { get; set; }

	private static int RecursiveDepth(Container node, int depth)
	{
		depth++;
		if (node.parent.name == "root")
			return depth;
		return RecursiveDepth(node.parent, depth);
	}

	private static float RecursiveSize(Container node, float size)
	{
		foreach (Container child in node.children)
		{
			if (child.size != 0) // leaf node, no children
				size += child.size;
			else
				size = RecursiveSize(child, size);
		}
		return size;
	}

	public static int GetDepth(Container node)
	{
		return RecursiveDepth(node, 0); // Folder
	}

	public static float GetSize(Container node)
	{
		if (node.size > 0) // File
			return node.size;
		return RecursiveSize(node, 0);
	}

	public void print()
	{
		string output = System.String.Empty;
		if (size == 0) // a folder has no size, only files has
			output += "Type = folder";
		else
			output += "Type = file";

		output += ", Name = " + name;

		if (id == 0) // root
			output += ", Parent = null";
		else
			output += ", Parent = " + parent.name;

		output +=
		", Id = " + id +
		", Depth = " + depth +
		", Size = " + size +
		", Weight = " + weight +
		", Number of children = " + children.Count +
		", Number of siblings = " + siblings.Count;

		Debug.Log(output);
	}


}
