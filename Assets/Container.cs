using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Container
{
	public List<Container> children;
	public Container parent;

	public string name;
	public float size;
	public float weight;

	public void connectParent()
	{
		foreach (Container child in children)
		{
			child.parent = this;
			child.connectParent();
		}
	}
}
