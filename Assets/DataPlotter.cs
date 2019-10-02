using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

public class DataPlotter : MonoBehaviour
{
	enum RenderType { PLANAR, CONE };

	// Initialize variables
	public GameObject pointPrefab;
	public List<Container> nodes;
	public string file;
	public int maxDepth = 0;

	private void Start()
	{
		//nodes = populateTree(JSONParser.Read(file));
		Container root = JSONParser.Read(file);
		InitializeNodes(root);
		renderNodes(root, RenderType.PLANAR);
	}

	public void InitializeNodes(Container root)
	{
		Queue<Container> childrenQueue = new Queue<Container>();
		Container parent = null;
		Container grandParent = null;
		int id = 0;

		root.parent = parent;
		root.depth = 0;
		root.id = id++;

		childrenQueue.Enqueue(root);
		while (childrenQueue.Count != 0)
		{
			parent = childrenQueue.Dequeue();
			foreach (Container child in parent.children)
			{
				List<Container> siblings = new List<Container>();
				foreach (var sibling in parent.children)
					if (child != sibling)
						siblings.Add(sibling);

				child.siblings = siblings;
				child.parent = parent;
				child.depth = Container.GetDepth(child);
				child.id = id++;

				childrenQueue.Enqueue(child);
			}
		}
	}

	private List<List<Container>> getSiblings(Container root)
	{
		Container parent = root;
		Container grandParent = root;
		Queue<Container> childrenQueue = new Queue<Container>();
		List<List<Container>> siblings = new List<List<Container>>();
		int level = 0;

		siblings.Add(new List<Container>());
		childrenQueue.Enqueue(root);
		while (childrenQueue.Count != 0)
		{

			parent = childrenQueue.Dequeue();
			foreach (Container child in parent.children)
				childrenQueue.Enqueue(child);

			if (parent.id != root.id && grandParent.id != parent.parent.id)
			{
				grandParent = parent.parent;
				level++;
				siblings.Add(new List<Container>());
			}
			siblings[level].Add(parent);
		}
		return siblings;
	}

	private void planarRendering(Container root)
	{
		List<List<Container>> siblings;
		siblings = getSiblings(root);
		int nrOfLevels = siblings.Count;
		int gridSize = 0;
		int nrOfSiblings = 0;

		for (int i = 0; i < nrOfLevels; i++) // Create all prefabs from the 2d list of siblings
		{
			nrOfSiblings = siblings[i].Count;
			gridSize = (int)Math.Ceiling(Math.Sqrt(nrOfSiblings)); // Nearest perfect square is the side for the grid and is calculated as the ceiling of sqrt(nrOfSiblings)
			for (int j = 0; j < nrOfSiblings; j++)
				createPrefab(siblings[i][j], new Vector3((j / gridSize) - ((float)gridSize / 2), i + 1, (j % gridSize) - ((float)gridSize / 2)), Color.black);
		}
	}

	private void renderNodes(Container root, RenderType type)
	{
		switch (type)
		{
			case RenderType.PLANAR:
				planarRendering(root);
				break;

			case RenderType.CONE:
			Debug.Log("not implemented yet bruh");
				break;
		}
	}


	private void createPrefab(Container node, Vector3 position, Color color)
	{
		var obj = Instantiate(pointPrefab, new Vector3(position.x, position.y, position.z), Quaternion.identity);
		obj.GetComponent<Renderer>().material.color = color;
		//Container container = obj.AddComponent<Container>() as Container;
	}
}
