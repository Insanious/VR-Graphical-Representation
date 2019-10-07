using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

public class DataPlotter : MonoBehaviour
{
	enum RenderType { PLANAR, CONE };
	enum RenderMode { SIBLINGS, LEVELS };

	public GameObject prefab;

	private List<List<Linker.Container>> nodes;
	private Queue<Linker.Container> childrenQueue;
	private List<List<Linker.Container>> siblings;
	private Linker.Container root;
	Linker.Container parent;
	private Linker.Container child;
	public string file;
	private int maxDepth;
	private int level;

	private void Start()
	{
		nodes = new List<List<Linker.Container>>();
		childrenQueue = new Queue<Linker.Container>();
		siblings = new List<List<Linker.Container>>();
		maxDepth = level = 0;

		root = JSONParser.Read(file);
		InitializeNodes();
		RenderNodes(RenderType.CONE, RenderMode.LEVELS);
	}

	public void InitializeNodes()
	{
		parent = null;
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

	private List<List<Linker.Container>> GetNodes(RenderMode mode)
	{
		switch (mode)
		{
			case RenderMode.SIBLINGS:
				nodes = GetAllSiblings();
				break;
			case RenderMode.LEVELS:
				nodes = GetAllLevels();
				break;
			default:
				Debug.Log("Wrong mode.");
				break;
		}
		return nodes;
	}

	private List<List<Linker.Container>> GetAllLevels()
	{
		child = root;
		level = 0;
		List<List<Linker.Container>> levels = new List<List<Linker.Container>>();

		levels.Add(new List<Linker.Container>());

		childrenQueue.Enqueue(root);
		while (childrenQueue.Count != 0)
		{
			child = childrenQueue.Dequeue();
			foreach (Linker.Container grandchild in child.children) // Add the parent's children to the queue
				childrenQueue.Enqueue(grandchild);

			if (child.depth != level)
			{
				levels.Add(new List<Linker.Container>());
				level++;
			}
			levels[level].Add(child);
		}
		return levels;
	}

	private List<List<Linker.Container>> GetAllSiblings()
	{
		parent = child = root;
		level = 0;

		siblings.Add(new List<Linker.Container>());
		siblings[level++].Add(root); // Add root as the single object at the first level
		siblings.Add(new List<Linker.Container>());

		// Cover edge case so we don't need to check against root's parent, since it's parent is null
		foreach (Linker.Container grandchild in root.children) // Add root's children to the queue
			childrenQueue.Enqueue(grandchild);

		while (childrenQueue.Count != 0)
		{
			child = childrenQueue.Dequeue();
			foreach (Linker.Container grandchild in child.children) // Add the parent's children to the queue
				childrenQueue.Enqueue(grandchild);

			if (child.parent.id != parent.id) // If there is a new grandparent, increment level
			{
				parent = child.parent;
				level++;
				siblings.Add(new List<Linker.Container>());
			}

			siblings[level].Add(child);
		}

		return siblings;
	}

	private void ConeRendering(RenderMode mode)
	{
		Vector3 size;
		Vector3 position;
		int nrOfLevels = 0;
		int nrOfNodes = 0;
		float radius = .1f;
		float deltaTheta = 0f;
		float theta = 0f;

		nodes = GetNodes(mode);

		nrOfLevels = nodes.Count;

		for (int i = 0; i < nrOfLevels; i++) // Create all prefabs from the 2d list of nodes
		{
			nrOfNodes = nodes[i].Count;

			deltaTheta = (2f * Mathf.PI) / nrOfNodes;
			radius = nrOfNodes / (Mathf.PI / 0.125f);

			for (int j = 0; j < nrOfNodes; j++) // Instantiate all nodes at level = i
			{
				size = new Vector3(.25f, .25f, .25f);
				position = new Vector3(radius * Mathf.Cos(theta), (2*i), radius * Mathf.Sin(theta));
				CreatePrefab(nodes[i][j], position, size);
				theta += deltaTheta;
			}
		}
	}

	private void PlanarRendering(RenderMode mode)
	{
		int nrOfLevels = 0;
		float gridSize = 0;
		int nrOfNodes = 0;
		Vector3	size;
		Vector3 position;

		nodes = GetNodes(mode);

		nrOfLevels = nodes.Count;
		for (int i = 0; i < nrOfLevels; i++) // Create all prefabs from the 2d list of nodes
		{
			nrOfNodes = nodes[i].Count;
			gridSize = (int)Math.Ceiling(Math.Sqrt(nrOfNodes)); // Nearest perfect square is the side for the grid and is calculated as the ceiling of sqrt(nrOfNodes)

			for (int j = 0; j < nrOfNodes; j++) // Instantiate all nodes at level = i
			{
				size = new Vector3(.25f, .25f, .25f);
				position = new Vector3((j / gridSize) - (gridSize / 2), i + 1, (j % gridSize) - (gridSize / 2));
				CreatePrefab(nodes[i][j], position, size);
			}
		}
	}

	private void RenderNodes(RenderType type, RenderMode mode)
	{
		switch (type)
		{
			case RenderType.PLANAR:
				PlanarRendering(mode);
				break;

			case RenderType.CONE:
				ConeRendering(mode);
				break;
		}
	}

	private void CreatePrefab(Linker.Container node, Vector3 position, Vector3 size)
	{
		var obj = Instantiate(prefab, new Vector3(position.x, position.y, position.z), Quaternion.identity);
		obj.transform.localScale = size;

		obj.GetComponent<Linker>().container = new Linker.Container();
		obj.GetComponent<Linker>().container = node;
		obj.GetComponent<Linker>().container.self = obj;

		obj.GetComponent<Renderer>().material.color = obj.GetComponent<Linker>().container.color;
	}
}
