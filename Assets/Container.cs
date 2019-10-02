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
		if (id == 0) // root
			Debug.Log(
			"Type = folder" +
			", Name = " + name +
			", Parent = null" +
			", Id = " + id +
			", Depth = " + depth +
			", Size = " + size +
			", Weight = " + weight +
			", Number of children = " + children.Count +
			", Number of siblings = " + siblings.Count
			);
		else if (size == 0) // Folder
			Debug.Log(
			"Type = folder" +
			", Name = " + name +
			", Parent = " + parent.name +
			", Id = " + id +
			", Depth = " + depth +
			", Size = " + size +
			", Weight = " + weight +
			", Number of children = " + children.Count +
			", Number of siblings = " + siblings.Count
			);
		else // File
			Debug.Log(
			"Type = file" +
			", Name = " + name +
			", Parent = " + parent.name +
			", Id = " + id +
			", Depth = " + depth +
			", Size = " + size +
			", Weight = " + weight +
			", Number of children = " + children.Count +
			", Number of siblings = " + siblings.Count
			);
	}

	public static int RecursiveDepth(Container node, int depth)
	{
		depth++;
		if (node.parent.name == "root")
			return depth;
		return RecursiveDepth(node.parent, depth);
	}
}
