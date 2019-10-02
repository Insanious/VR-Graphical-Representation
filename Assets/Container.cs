using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Container : MonoBehaviour
{
	public int id { get; set; }
	public GameObject self;

	public List<Container> children { get; set; }
	public List<Container> siblings { get; set; }
	public Container parent { get; set; }

	public string name { get; set; }
	public float size { get; set; }
	public float weight { get; set; }

	public int depth { get; set; }

	public Container() {}

	public Container(Container other)
	{
		this.name = other.name;
		this.size = other.size;
		this.weight = other.weight;
	}

	public static int GetDepth(Container node)
	{
		return RecursiveDepth(node, 0);
	}
	public static int RecursiveDepth(Container node, int depth)
	{
		depth++;
		if (node.parent.name == "root")
			return depth;
		return RecursiveDepth(node.parent, depth);
	}
}
