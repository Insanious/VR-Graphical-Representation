using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Linker : MonoBehaviour
{
	public enum RenderMode { SIBLINGS, LEVELS };


	public Container container;

	public class Container
	{
		public GameObject self;
		public GameObject line;

		public GameObject folderPrefab;
		public GameObject filePrefab;
		public GameObject linePrefab;

		public List<Container> children { get; set; }
		public List<Container> siblings { get; set; }
		public Container parent { get; set; }
		public bool isInstantiated { get; set; }
		public bool isDrawingLine { get; set; }
		public int id { get; set; }
		public int depth { get; set; }
		public int subtreeDepth { get; set; }
		public string name { get; set; }
		public float size { get; set; }
		public float weight { get; set; }
		public Color color { get; set; }

		private List<List<Linker.Container>> GetNodes(RenderMode mode, int depth)
		{
			switch (mode)
			{
				case RenderMode.SIBLINGS:
					return GetAllSiblings(depth);
				case RenderMode.LEVELS:
					if (depth == 1)
						return GetNextLevel();
					return GetAllLevels(depth);
				default:
					Debug.Log("Wrong mode.");
					break;
			}
			return null;
		}

		private List<List<Linker.Container>> GetAllSiblings(int depth)
		{
			List<List<Linker.Container>> siblings = new List<List<Linker.Container>>();
			Queue<Linker.Container> childrenQueue = new Queue<Linker.Container>();
			Linker.Container parent = this; // 'this' is the root
			Linker.Container child;
			int level = 0;
			bool depthReached = false;

			//siblings.Add(new List<Linker.Container>());
			//siblings[level++].Add(parent); // Add node as the single object at the first level
			if (depth == 0)
				return siblings;

			siblings.Add(new List<Linker.Container>());

			// Cover edge case so we don't need to check against root's parent, since it's parent is null
			foreach (Linker.Container grandchild in parent.children) // Add root's children to the queue
				childrenQueue.Enqueue(grandchild);

			while (childrenQueue.Count != 0)
			{
				child = childrenQueue.Dequeue();
				foreach (Linker.Container grandchild in child.children) // Add the parent's children to the queue
					childrenQueue.Enqueue(grandchild);

				if (child.parent.id != parent.id) // If there is a new grandparent, increment level
				{
					if (level == depth)
						depthReached = true;
					else
					{
						level++;
						parent = child.parent;
						siblings.Add(new List<Linker.Container>());
					}
				}
				siblings[level].Add(child);

				if (depthReached)
					break;
			}

			return siblings;
		}

		private List<List<Linker.Container>> GetNextLevel()
		{
			this.subtreeDepth++;
			List<List<Linker.Container>> levels = new List<List<Linker.Container>>();
			levels.Add(new List<Linker.Container>());
			foreach (Linker.Container child in this.children)
			{
				child.subtreeDepth++;
				levels[0].Add(child);
			}
			return levels;
		}

		private List<List<Linker.Container>> GetAllLevels(int depth)
		{
			List<List<Linker.Container>> levels = new List<List<Linker.Container>>();
			if (depth == 0)
				return levels;

			int newSubtreeDepth = this.subtreeDepth + depth;
			this.subtreeDepth = newSubtreeDepth;

			int depthOffset = this.depth + 1;
			Queue<Linker.Container> childrenQueue = new Queue<Linker.Container>();
			Linker.Container child = this; // 'this' is the root
			int level = 0;
			bool depthReached = false;

			//levels.Add(new List<Linker.Container>());
			//levels[level++].Add(child); // Add root node on a single level

			levels.Add(new List<Linker.Container>());
			foreach (Linker.Container grandchild in child.children) // Add the parent's children to the queue
				childrenQueue.Enqueue(grandchild);

			while (childrenQueue.Count != 0)
			{
				child = childrenQueue.Dequeue();
				foreach (Linker.Container grandchild in child.children) // Add the parent's children to the queue
					childrenQueue.Enqueue(grandchild);

				if (child.depth > depthOffset + level)
				{
					if (level + 1 == depth)
						depthReached = true;
					else
					{
						level++;
						levels.Add(new List<Linker.Container>());
					}
				}
				if (depthReached)
					break;

				child.subtreeDepth = newSubtreeDepth;
				levels[level].Add(child);
			}

			return levels;
		}

		public void ToggleSubtree(RenderMode mode)
		{
			if (this.size != 0) // A file has no subtree
				return;

			Linker.Container child = this.children[0];
			if (child.isInstantiated) // The tree has been instantiated
			{
				if (child.self.activeSelf) // GameObject is active
				{
					DisableSubtree();
					ToggleSubtreeLines();
				}
				else
					EnableSubtree();
			}

			else
				InstantiateSubtree(mode, 1);
		}

		private void EnableSubtree()
		{
			int depth = 0;
			Queue<Linker.Container> childrenQueue = new Queue<Linker.Container>();

			foreach (Linker.Container child in this.children)
				childrenQueue.Enqueue(child);

			while (childrenQueue.Count != 0)
			{
				parent = childrenQueue.Dequeue();
				if (parent.subtreeDepth == 0) // Stop if we have reached end of subtree
					return;
				else
					depth = parent.depth;
				parent.self.SetActive(true);

				foreach (Linker.Container child in parent.children)
					childrenQueue.Enqueue(child);
			}
		}

		private void DisableSubtree()
		{
			int depth = 0;
			Queue<Linker.Container> childrenQueue = new Queue<Linker.Container>();

			foreach (Linker.Container child in this.children)
				childrenQueue.Enqueue(child);

			while (childrenQueue.Count != 0)
			{
				parent = childrenQueue.Dequeue();
				if (parent.subtreeDepth == 0) // Stop if we have reached end of subtree
					return;
				else
					depth = parent.depth;
				parent.self.SetActive(false);

				foreach (Linker.Container child in parent.children)
					childrenQueue.Enqueue(child);
			}
		}

		public void InstantiateSubtree(RenderMode mode, int depth) // Circular rendering
		{
			List<List<Linker.Container>> nodes;
			Vector3 size;
			Vector3 position;
			int nrOfLevels = 0;
			int nrOfNodes = 0;
			int childDepth = 0;
			float heightMultiplier = 2;
			float radius = .1f;
			float deltaTheta = 0f;
			float theta = 0f;
			float nodeSeparation = 1.25f; // Separate nodes with a gap of one whole node inbetween
			float nodeSize = 0.25f;

			nodes = GetNodes(mode, depth);

			nrOfLevels = nodes.Count;

			for (int i = 0; i < nrOfLevels; i++) // Create all folderPrefabs from the 2d list of nodes
			{
				childDepth = nodes[i][0].depth;
				nrOfNodes = nodes[i].Count;

				deltaTheta = (2f * Mathf.PI) / nrOfNodes;
				radius = nrOfNodes / (Mathf.PI / (nodeSize / nodeSeparation));

				for (int j = 0; j < nrOfNodes; j++) // Instantiate all nodes at level = i
				{

					size = new Vector3(.25f, .25f, .25f);
					position = new Vector3(radius * Mathf.Cos(theta), (heightMultiplier * childDepth), radius * Mathf.Sin(theta));

					nodes[i][j].folderPrefab = this.folderPrefab;
					nodes[i][j].filePrefab = this.filePrefab;
					nodes[i][j].linePrefab = this.linePrefab;

					InstantiateNode(nodes[i][j], position, size);
					theta += deltaTheta;
				}
			}
		}

		public void InstantiateNode(Linker.Container node, Vector3 position, Vector3 size)
		{
			if (node.size == 0)
				node.self = Instantiate(node.folderPrefab, new Vector3(position.x, position.y, position.z), Quaternion.identity);
			else
				node.self = Instantiate(node.filePrefab, new Vector3(position.x, position.y, position.z), Quaternion.identity);

			node.line = Instantiate(node.linePrefab);
			node.isInstantiated = true;
			node.self.GetComponent<Linker>().container = node;
			node.self.transform.localScale = size; // Change size
			node.self.GetComponent<Renderer>().material.color = node.color; // Change color
		}

		public void ToggleSubtreeLines()
		{
			if (children.Count == 0)
				return;

			bool drawing = false;

			foreach (Linker.Container child in children)
			{
				if (!child.self.activeSelf) // If tree is inactive, disable lines
				{
					DisableSubtreeLines();
					return;
				}

				if (child.isDrawingLine) // If any of the children are drawing lines, disable all drawing
				{
					drawing = true;
					break;
				}
			}

			if (drawing)
				DisableSubtreeLines();
			else
				EnableSubtreeLines();
		}

		private void DisableSubtreeLines()
		{
			Queue<Linker.Container> childrenQueue = new Queue<Linker.Container>();
			Linker.Container parent;

			childrenQueue.Enqueue(this);
			while(childrenQueue.Count != 0)
			{
				parent = childrenQueue.Dequeue();

				foreach (Linker.Container child in parent.children)
				{
					if (child.isInstantiated)
					{
						childrenQueue.Enqueue(child);

						child.line.GetComponent<LineRenderer>().positionCount = 0;
						child.isDrawingLine = false;
					}
				}
			}
		}

		private void EnableSubtreeLines()
		{
			Queue<Linker.Container> childrenQueue = new Queue<Linker.Container>();
			Linker.Container parent;
			Vector3 parentPos;
			Color parentColor;
			Vector3 childPos;
			Color childColor;

			childrenQueue.Enqueue(this);
			while(childrenQueue.Count != 0)
			{
				parent = childrenQueue.Dequeue();
				parentPos = parent.self.transform.position;
				parentColor = parent.color;

				foreach (Linker.Container child in parent.children)
				{
					if (child.isInstantiated)
					{
						childrenQueue.Enqueue(child);

						childPos = child.self.transform.position;
						childColor = child.color;

						LineRenderer lineRenderer = child.self.GetComponent<Linker>().container.line.GetComponent<LineRenderer>();
						lineRenderer.positionCount = 2;
						lineRenderer.SetPosition(0, new Vector3(childPos.x, childPos.y, childPos.z));
						lineRenderer.SetPosition(1, new Vector3(parentPos.x, parentPos.y, parentPos.z));
						lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
						lineRenderer.startColor = childColor;
						lineRenderer.endColor = parentColor;

						child.isDrawingLine = true;
					}
				}
			}
		}

		private void Move(Vector3 increment)
		{
			self.transform.position += increment;
		}

		public void MoveSubtree(Vector3 increment)
		{
			Move(increment);

			if (children.Count == 0) // Basecase
				return;

			foreach (Linker.Container child in children) // Move all nodes recursively
				child.MoveSubtree(increment);
		}

		private int RecursiveDepth(Linker.Container node, int depth)
		{
			depth++;

			if (node.parent.name == "root")
				return depth;

			return RecursiveDepth(node.parent, depth);
		}

		private float RecursiveSize(Linker.Container node, float size)
		{
			foreach (Linker.Container child in node.children)
			{
				if (child.size != 0) // leaf node, no children
					size += child.size;
				else
					size = RecursiveSize(child, size);
			}

			return size;
		}

		public int GetDepth()
		{
			return RecursiveDepth(this, 0); // Folder
		}

		public float GetSize()
		{
			if (size > 0) // File
				return size;

			return RecursiveSize(this, 0);
		}

		public void Print()
		{
			string output = System.String.Empty;
			if (size == 0) // a folder has no size, only files has
				output += "Type = folder";
			else
				output += "Type = file";

			output += ". Name = " + name;

			if (id == 0) // root
				output += ". Parent = null";
			else
				output += ". Parent = " + parent.name;
			output +=
			". Id = " + id +
			". Depth = " + depth +
			". Subtree depth = " + subtreeDepth +
			". Size = " + size +
			". Weight = " + weight +
			". Number of children = " + children.Count +
			". Number of siblings = " + siblings.Count;

			Debug.Log(output);
		}
	}
}
