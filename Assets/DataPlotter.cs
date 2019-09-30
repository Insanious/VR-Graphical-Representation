using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

public class DataPlotter : MonoBehaviour
{
	enum RenderType { PLANAR, CONE };

	// Initialize variables
	public GameObject PointPrefab;
	public List<Container> nodes;
	public string file;
	public int maxDepth = 0;

	private void Start()
	{
		nodes = populateTree(JSONParser.Read(file));
		maxDepth = nodes[nodes.Count - 1].depth;
		renderNodes(nodes, RenderType.PLANAR);
	}

	private List<List<Container>> getSiblings(List<Container> nodes)
	{
		Container parent = null;
		List<List<Container>> siblings = new List<List<Container>>();
		int level = 0;

		siblings.Add(new List<Container>());
		foreach (Container node in nodes)
		{
			if (node.parent != parent)
			{
				parent = node.parent;
				level++;
				siblings.Add(new List<Container>());
			}
			siblings[level].Add(node);
		}
		return siblings;
	}

	private void planarRendering(List<Container> nodes)
	{
		if (nodes.Count == 0)
		{
			Debug.Log("No nodes in tree");
			return;
		}

		Container parent = null;
		List<List<Container>> siblings;
		siblings = getSiblings(nodes);
		int nrOfLevels = siblings.Count;
		int gridSize = 0;
		int nrOfSiblings = 0;

		for (int i = 0; i < nrOfLevels; i++) // Create all prefabs from the 2d list of siblings
		{
			nrOfSiblings = siblings[i].Count;
			gridSize = (int)Math.Ceiling(Math.Sqrt(nrOfSiblings)); // Nearest perfect square is the side for the grid and is calculated as the ceiling of sqrt(nrOfSiblings)

			for (int j = 0; j < nrOfSiblings; j++)
				createPrefab(siblings[i][j], new Vector3(j / gridSize, i, j % gridSize), Color.black);
		}
	}

	private void renderNodes(List<Container> nodes, RenderType type)
	{
		switch (type)
		{
			case RenderType.PLANAR:
				planarRendering(nodes);
				break;

			case RenderType.CONE:
				break;
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
				Container newChild = new Container(child);
				newChild.parent = parent;
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
