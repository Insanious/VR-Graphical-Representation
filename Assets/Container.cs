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

	public static int GetDepth(Container node)
	{
		return RecursiveDepth(node, 0);
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

	public static int RecursiveDepth(Container node, int depth)
	{
		depth++;
		if (node.parent.name == "root")
			return depth;
		return RecursiveDepth(node.parent, depth);
	}
}
