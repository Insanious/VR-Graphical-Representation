using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

public class DataPlotter : MonoBehaviour
{

	// Initialize variables
	public GameObject PointPrefab;
	public string file;
	public List<Container> nodes;
	public List<Container> siblings = new List<Container>();

	public int nrOfChildren = 0;
	public int nrOfSiblings = 0;
	public float level = 0;
	public int gridSize = 0;
	public int maxDepth = 0;

	public Container parent;

	void Start()
	{
		nodes = populateTree(JSONParser.Read(file));
		renderNodes(nodes);
	}

	private void renderNodes(List<Container> nodes)
	{
		Container parent = null;
		List<Container> siblings = new List<Container>();
		int gridSize = 0;
		int nrOfSiblings = 0;
		int counter = 0;
		int level = 0;

		foreach (Container node in nodes)
		{
			if (node.parent != parent || node == nodes[nodes.Count - 1])
			{
				parent = node.parent;
				level++;

				nrOfSiblings = siblings.Count;
				gridSize = (int)Math.Ceiling(Math.Sqrt(nrOfSiblings)); // Nearest perfect square is the ceiling of the square root

				for (int i = 0; i < nrOfSiblings; i++)
					createPrefab(siblings[i], new Vector3(i / gridSize, level, i % gridSize), Color.black);

				siblings.Clear(); // Empty the list
			}
			siblings.Add(node);
		}
	}

	private List<Container> populateTree(Container rootReference)
	{
		List<Container> nodes = new List<Container>();
		Queue referenceQueue = new Queue();
		Queue childrenQueue = new Queue();
		Queue tempQueue = new Queue();
		Container root = new Container();
		Container parent;

		root.parent = null;
		root.name = rootReference.name;
		root.setDepth(root);

		childrenQueue.Enqueue(root);
		referenceQueue.Enqueue(rootReference);

		while (referenceQueue.Count != 0)
		{
			Container node = (Container)referenceQueue.Dequeue();
			foreach(Container child in node.children) // Populate the reference queue
			{
				tempQueue.Enqueue(child);
				referenceQueue.Enqueue(child);
			}

			parent = (Container)childrenQueue.Dequeue(); // Pop the parent and then populate its' children list with children
			parent.children = new List<Container>();

			foreach(Container child in tempQueue) //Create new children from reference
			{
				Container newChild = new Container();
				newChild.parent = parent;
				newChild.name = child.name;
				newChild.size = child.size;
				newChild.weight = child.weight;
				newChild.setDepth(newChild);

				parent.children.Add(newChild); // Add the new child to the list
				childrenQueue.Enqueue(newChild); // Enqueue the new child top be able to assign it to parent later on
			}
			tempQueue.Clear();

			nodes.Add(parent); // Add parent to nodes now that its' children are stored in queue
		}

		return nodes;
	}

	private void createPrefab(Container node, Vector3 position, Color color)
	{
		var obj = Instantiate(PointPrefab, new Vector3(position.x, position.y, position.z), Quaternion.identity);
		obj.GetComponent<Renderer>().material.color = color;
	}
}
