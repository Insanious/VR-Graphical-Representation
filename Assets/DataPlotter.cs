using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

public class DataPlotter : MonoBehaviour
{
	enum RenderType { PLANAR, CONE };
	enum RenderMode { SIBLINGS, LEVELS };

	public GameObject folderPrefab;
	public GameObject filePrefab;
	public GameObject linePrefab;
	public int depth;
	public string file;

	private List<List<Linker.Container>> nodes;
	private Queue<Linker.Container> childrenQueue;
	private Linker.Container root;

	private void Start()
	{
		nodes = new List<List<Linker.Container>>();

		root = JSONParser.Read(file);

		InitializeNodes();
		RenderNodes(root, RenderType.CONE, RenderMode.LEVELS, depth);
	}

	public void InitializeNodes()
	{
		Queue<Linker.Container> childrenQueue = new Queue<Linker.Container>();
		Linker.Container parent = null;
		int id = 0;
		float colorTint = 0.2f;

		root.parent = parent;
		root.siblings = new List<Linker.Container>();
		root.depth = 0;
		root.id = id++;

		childrenQueue.Enqueue(root);
		while (childrenQueue.Count != 0)
		{
			parent = childrenQueue.Dequeue();
			foreach (Linker.Container child in parent.children)
			{
				child.siblings = new List<Linker.Container>();
				foreach (var sibling in parent.children) // Add all siblings to all children
					if (child.id != sibling.id)
						child.siblings.Add(sibling);

				child.parent = parent;
				child.depth = Linker.GetDepth(child);
				child.id = id++;

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

	private List<List<Linker.Container>> GetNodes(Linker.Container node, RenderMode mode, int depth)
	{
		switch (mode)
		{
			case RenderMode.SIBLINGS:
				nodes = GetAllSiblings(node, depth);
				break;
			case RenderMode.LEVELS:
				nodes = GetAllLevels(node, depth);
				break;
			default:
				Debug.Log("Wrong mode.");
				break;
		}
		return nodes;
	}

	private List<List<Linker.Container>> GetAllLevels(Linker.Container node, int depth)
	{
		List<List<Linker.Container>> levels = new List<List<Linker.Container>>();
		if (depth == 0) // When depth is 0, the tree is empty
			return levels;
		Queue<Linker.Container> childrenQueue = new Queue<Linker.Container>();
		Linker.Container child = node;
		int level = 0;

		levels.Add(new List<Linker.Container>());
		levels[level++].Add(node);
		levels.Add(new List<Linker.Container>());

		foreach (Linker.Container grandchild in node.children) // Add the parent's children to the queue
			childrenQueue.Enqueue(grandchild);

		while (childrenQueue.Count != 0)
		{
			child = childrenQueue.Dequeue();
			foreach (Linker.Container grandchild in child.children) // Add the parent's children to the queue
				childrenQueue.Enqueue(grandchild);

			if (child.depth != level - 1)
			{
				level++;
				if (level - 1 == depth)
					break;

				levels.Add(new List<Linker.Container>());
			}
			levels[level].Add(child);
		}

		return levels;
	}

	private List<List<Linker.Container>> GetAllSiblings(Linker.Container node, int depth)
	{
		List<List<Linker.Container>> siblings = new List<List<Linker.Container>>();
		Queue<Linker.Container> childrenQueue = new Queue<Linker.Container>();
		Linker.Container parent = node;
		Linker.Container child;
		int level = 0;

		siblings.Add(new List<Linker.Container>());
		siblings[level++].Add(node); // Add node as the single object at the first level
		siblings.Add(new List<Linker.Container>());

		// Cover edge case so we don't need to check against root's parent, since it's parent is null
		foreach (Linker.Container grandchild in node.children) // Add root's children to the queue
			childrenQueue.Enqueue(grandchild);

		while (childrenQueue.Count != 0)
		{
			child = childrenQueue.Dequeue();
			foreach (Linker.Container grandchild in child.children) // Add the parent's children to the queue
				childrenQueue.Enqueue(grandchild);

			if (child.parent.id != parent.id) // If there is a new grandparent, increment level
			{
				level++;
				if (level - 1 == depth)
					break;

				parent = child.parent;
				siblings.Add(new List<Linker.Container>());
			}

			siblings[level].Add(child);
		}

		return siblings;
	}

	private void ConeRendering(Linker.Container node, RenderMode mode, int depth)
	{
		Vector3 size;
		Vector3 position;
		int nrOfLevels = 0;
		int nrOfNodes = 0;
		float radius = .1f;
		float deltaTheta = 0f;
		float theta = 0f;
		float nodeSeparation = 1.25f; // Separate nodes with a gap of one whole node inbetween
		float nodeSize = 0.25f;

		nodes = GetNodes(node, mode, depth);

		nrOfLevels = nodes.Count;

		for (int i = 0; i < nrOfLevels; i++) // Create all folderPrefabs from the 2d list of nodes
		{
			nrOfNodes = nodes[i].Count;

			deltaTheta = (2f * Mathf.PI) / nrOfNodes;
			radius = nrOfNodes / (Mathf.PI / (nodeSize / nodeSeparation));

			for (int j = 0; j < nrOfNodes; j++) // Instantiate all nodes at level = i
			{
				size = new Vector3(.25f, .25f, .25f);
				position = new Vector3(radius * Mathf.Cos(theta), (4*i), radius * Mathf.Sin(theta));
				CreateNode(nodes[i][j], position, size);
				theta += deltaTheta;
			}
		}
	}

	private void PlanarRendering(Linker.Container node, RenderMode mode, int depth)
	{
		Vector3	size;
		Vector3 position;
		int nrOfLevels = 0;
		float gridSize = 0;
		int nrOfNodes = 0;

		nodes = GetNodes(node, mode, depth);

		nrOfLevels = nodes.Count;
		for (int i = 0; i < nrOfLevels; i++) // Create all folderPrefabs from the 2d list of nodes
		{
			nrOfNodes = nodes[i].Count;
			gridSize = (int)Math.Ceiling(Math.Sqrt(nrOfNodes)); // Nearest perfect square is the side for the grid and is calculated as the ceiling of sqrt(nrOfNodes)

			for (int j = 0; j < nrOfNodes; j++) // Instantiate all nodes at level = i
			{
				size = new Vector3(.25f, .25f, .25f);
				position = new Vector3((j / gridSize) - (gridSize / 2), i + 1, (j % gridSize) - (gridSize / 2));
				CreateNode(nodes[i][j], position, size);
			}
		}
	}

	private void RenderNodes(Linker.Container node, RenderType type, RenderMode mode, int depth)
	{
		switch (type)
		{
			case RenderType.PLANAR:
				PlanarRendering(node, mode, depth);
				break;

			case RenderType.CONE:
				ConeRendering(node, mode, depth);
				break;
		}
	}

	private void CreateNode(Linker.Container node, Vector3 position, Vector3 size)
	{
		GameObject prefab;
		if (node.children.Count == 0)
			prefab = filePrefab;
		else
			prefab = folderPrefab;

		var obj = Instantiate(prefab, new Vector3(position.x, position.y, position.z), Quaternion.identity);

		obj.transform.localScale = size;
		obj.GetComponent<Linker>().container = node;
		obj.GetComponent<Linker>().container.self = obj;
		obj.GetComponent<Linker>().container.line = Instantiate(linePrefab);
		obj.GetComponent<Renderer>().material.color = obj.GetComponent<Linker>().container.color;
	}
}
