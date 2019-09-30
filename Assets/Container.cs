using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Container
{
	public List<Container> children { get; set; }
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

	public void setDepth(Container node)
	{
		this.depth++;

		if (node.parent == null)
			return;

		setDepth(node.parent);
	}
}
