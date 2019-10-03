using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

public class DataPlotter : MonoBehaviour
{
	enum RenderType { PLANAR, CONE };

	public GameObject prefab;
	public List<Container> nodes;
	public string file;
	public int maxDepth = 0;

	private void Start()
	{
		Container root = JSONParser.Read(file);
		InitializeNodes(root);
		RenderNodes(root, RenderType.PLANAR);
	}

	public void InitializeNodes(Container root)
	{
		Queue<Container> childrenQueue = new Queue<Container>();
		Container parent = null;
		int id = 0;
		float colorTint = 0.2f;

		root.parent = parent;
		root.depth = 0;
		root.id = id++;

		childrenQueue.Enqueue(root);
		while (childrenQueue.Count != 0)
		{
			parent = childrenQueue.Dequeue();
			foreach (Container child in parent.children)
			{
				child.siblings = new List<Container>();
				foreach (var sibling in parent.children) // Add all siblings to all children
					if (child.id != sibling.id)
						child.siblings.Add(sibling);

				child.parent = parent;
				child.depth = Container.GetDepth(child);
				child.id = id++;

				Color color = new Color();
				if (child.id > 0 && child.id < 4)
				{
					child.color = new Color();
					switch (child.id)
					{
						case 1:
							child.color = Color.red;
							break;
						case 2:
							child.color = Color.green;
							break;
						case 3:
							child.color = Color.blue;
							break;

					}
				}

				else
					child.color = new Color(parent.color.r + colorTint, parent.color.g + colorTint, parent.color.b + colorTint);

				childrenQueue.Enqueue(child);
			}
		}
	}

	private List<List<Container>> GetSiblings(Container root)
	{
		Container parent = root;
		Container grandParent = root;
		Queue<Container> childrenQueue = new Queue<Container>();
		List<List<Container>> siblings = new List<List<Container>>();
		int level = 0;

		siblings.Add(new List<Container>());
		siblings[level++].Add(root); // Add root as the single object at the first level
		siblings.Add(new List<Container>());

		foreach (Container child in root.children) // Add root's children to the queue
			childrenQueue.Enqueue(child);

		while (childrenQueue.Count != 0)
		{
			parent = childrenQueue.Dequeue();
			foreach (Container child in parent.children) // Add the parent's children to the queue
				childrenQueue.Enqueue(child);

			if (grandParent.id != parent.parent.id) // If there is a new grandparent, increment level
			{
				grandParent = parent.parent;
				level++;
				siblings.Add(new List<Container>());
			}

			siblings[level].Add(parent);
		}

		return siblings;
	}

	private void PlanarRendering(Container root)
	{
		int nrOfLevels = 0;
		float gridSize = 0;
		int nrOfSiblings = 0;
		List<List<Container>> siblings;

		siblings = GetSiblings(root);
		nrOfLevels = siblings.Count;

		for (int i = 0; i < nrOfLevels; i++) // Create all prefabs from the 2d list of siblings
		{
			nrOfSiblings = siblings[i].Count;
			gridSize = (int)Math.Ceiling(Math.Sqrt(nrOfSiblings)); // Nearest perfect square is the side for the grid and is calculated as the ceiling of sqrt(nrOfSiblings)

			for (int j = 0; j < nrOfSiblings; j++) // Instantiate all siblings at level = i
				CreatePrefab(siblings[i][j], new Vector3((j / gridSize) - (gridSize / 2), i + 1, (j % gridSize) - (gridSize / 2)));
		}
	}

	private void RenderNodes(Container root, RenderType type)
	{
		switch (type)
		{
			case RenderType.PLANAR:
				PlanarRendering(root);
				break;

			case RenderType.CONE:
				Debug.Log("not implemented yet bruh");
				break;
		}
	}

	private void CreatePrefab(Container node, Vector3 position)
	{
		node.self = Instantiate(prefab, new Vector3(position.x, position.y, position.z), Quaternion.identity);

		node.self.GetComponent<Container>().children = new List<Container>();
		if (node.children != null)
			foreach (Container child in node.children)
				node.self.GetComponent<Container>().children.Add(child);

		node.self.GetComponent<Container>().siblings = new List<Container>();
		if (node.siblings != null)
			foreach (Container sibling in node.siblings)
				node.self.GetComponent<Container>().siblings.Add(sibling);

		node.self.GetComponent<Container>().parent = node.parent;
		node.self.GetComponent<Container>().id = node.id;
		node.self.GetComponent<Container>().depth = node.depth;
		node.self.GetComponent<Container>().name = node.name;
		node.self.GetComponent<Container>().size = node.size;
		node.self.GetComponent<Container>().weight = node.weight;

		node.self.GetComponent<Container>().color = new Color();
		node.self.GetComponent<Container>().color = node.color;

		node.self.GetComponent<Renderer>().material.color = node.self.GetComponent<Container>().color;
	}
}
