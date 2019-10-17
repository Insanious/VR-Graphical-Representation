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
		public int maxDepth { get; set; }
		public string name { get; set; }
		public float size { get; set; }
		public float weight { get; set; }
		public Color color { get; set; }

		private Linker.Container CopyNode()
		{
			Linker.Container copy = new Linker.Container();
			copy.color = new Color();
			copy.children = new List<Container>();
			copy.siblings = new List<Container>();
			copy.parent = null;
			copy.self = null;

			copy.folderPrefab = this.folderPrefab;
			copy.filePrefab = this.filePrefab;
			copy.linePrefab = this.linePrefab;
			copy.isInstantiated = false;
			copy.isDrawingLine = false;
			copy.id = this.id;
			copy.subtreeDepth = -1;
			copy.name = this.name;
			copy.size = this.size;
			copy.weight = this.weight;
			copy.color = this.color;

			return copy;
		}

		public void CopySubtree(Vector3 offset)
		{
			Queue<Linker.Container> childrenQueue = new Queue<Linker.Container>();
			Queue<Linker.Container> newChildrenQueue = new Queue<Linker.Container>();
			Linker.Container parent;
			Linker.Container newParent;
			Linker.Container newChild;

			Linker.Container newRoot = this.CopyNode();
			if (newRoot.subtreeDepth < -1) // Copied a node != root when full tree was displayed
				newRoot.subtreeDepth = -1;
			newRoot.depth = newRoot.GetDepth();

			childrenQueue.Enqueue(this);
			newChildrenQueue.Enqueue(newRoot);
			while (childrenQueue.Count != 0)
			{
				parent = childrenQueue.Dequeue();
				newParent = newChildrenQueue.Dequeue();
				foreach(Linker.Container child in parent.children)
				{
					newChild = child.CopyNode();
					newChild.parent = newParent;
					newChild.depth = newChild.GetDepth();
					newChildrenQueue.Enqueue(newChild);

					newParent.children.Add(newChild);
					childrenQueue.Enqueue(child);
				}

				foreach(Linker.Container child in newParent.children) // Add siblings
					foreach (Linker.Container sibling in newParent.children)
						if (child.id != sibling.id)
							child.siblings.Add(sibling);
			}
			SetMaxDepth(newRoot, GetMaxDepth(newRoot)); // Calculate max depth and set all nodes max depth to it in the new tree starting from the new root

			Vector3 position = self.transform.position;
			Vector3 size = self.transform.localScale;

			Vector3 newPosition = new Vector3(position.x + offset.x, position.y + offset.y, position.z + offset.z);
			Vector3 newSize = new Vector3(size.x, size.y, size.z);

			InstantiateNode(newRoot, newPosition, newSize);
		}

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

		public void IncrementSubtree(RenderMode mode)
		{
			if (depth + subtreeDepth == maxDepth)
				return;
			InstantiateSubtree(mode, 1);
		}

		public void DecrementSubtree(RenderMode mode)
		{
			if (subtreeDepth == 0)
				return;
			DestantiateSubtree(mode, 1);
		}

		public void DestantiateSubtree(RenderMode mode, int depth)
		{
			List<Linker.Container> levels = GetLastLevel();

			Debug.Log("Count = " + levels.Count);
			foreach (Linker.Container leaf in levels)
				leaf.self.SetActive(false);
		}

		private List<Linker.Container> GetLastLevel()
		{
			Queue<Linker.Container> childrenQueue = new Queue<Linker.Container>();
			List<Linker.Container> levels = new List<Linker.Container>();
			Linker.Container parent;

			childrenQueue.Enqueue(this);
			while (childrenQueue.Count != 0)
			{
				parent = childrenQueue.Dequeue();
				if (parent.subtreeDepth == 0) // Reached the 'leaf' nodes
					levels.Add(parent);
				else
					foreach (Linker.Container child in parent.children)
						childrenQueue.Enqueue(child);

				parent.subtreeDepth--;
			}

			return levels;
		}

		private List<List<Linker.Container>> GetNextLevel()
		{
			Queue<Linker.Container> childrenQueue = new Queue<Linker.Container>();
			Queue<Linker.Container> leafQueue = new Queue<Linker.Container>();
			List<List<Linker.Container>> levels = new List<List<Linker.Container>>();
			Linker.Container parent;
			Linker.Container newNode;

			levels.Add(new List<Linker.Container>());

			childrenQueue.Enqueue(this);
			while (childrenQueue.Count != 0)
			{
				parent = childrenQueue.Dequeue();
				if (parent.subtreeDepth == 0) // Reached the 'leaf' nodes
				{
					foreach (Linker.Container child in parent.children)
					{
						leafQueue.Enqueue(child);
						child.subtreeDepth++;
					}
				}

				else if (parent.subtreeDepth > 0)
					foreach (Linker.Container child in parent.children)
						childrenQueue.Enqueue(child);

				parent.subtreeDepth++;
			}

			while (leafQueue.Count != 0)
				levels[0].Add(leafQueue.Dequeue());

			return levels;
		}

		private List<List<Linker.Container>> GetAllLevels(int depth)
		{
			List<List<Linker.Container>> levels = new List<List<Linker.Container>>();
			if (depth == 0)
				return levels;

			int newSubtreeDepth = -1;

			if (this.subtreeDepth > -1)
			{
				newSubtreeDepth = this.subtreeDepth + depth;
				this.subtreeDepth = newSubtreeDepth;
			}

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
			Vector3 parentPosition = self.transform.position;
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
				if (nodes[i].Count != 0)
					childDepth = nodes[i][0].depth - this.depth;

				nrOfNodes = nodes[i].Count;

				deltaTheta = (2f * Mathf.PI) / nrOfNodes;
				radius = nrOfNodes / (Mathf.PI / (nodeSize / nodeSeparation));

				for (int j = 0; j < nrOfNodes; j++) // Instantiate all nodes at level = i
				{

					size = new Vector3(.25f, .25f, .25f);
					position = new Vector3(parentPosition.x + radius * Mathf.Cos(theta), parentPosition.y + heightMultiplier * childDepth, parentPosition.z + radius * Mathf.Sin(theta));

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
				if (child.self == null) // Null check
					return;

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
			if (node.parent == null)
				return depth;

			return RecursiveDepth(node.parent, ++depth);
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
			return RecursiveDepth(this, 0);
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

			if (parent == null) // root
				output += ". Parent = null";
			else
				output += ". Parent = " + parent.name;
			output +=
			". Id = " + id +
			". Depth = " + depth +
			". Subtree depth = " + subtreeDepth +
			". Max depth = " + maxDepth +
			". Size = " + size +
			". Weight = " + weight +
			". Number of children = " + children.Count +
			". Number of siblings = " + siblings.Count;

			Debug.Log(output);
		}

		private int GetMaxDepth(Linker.Container root)
		{
			Queue<Linker.Container> childrenQueue = new Queue<Linker.Container>();
			Linker.Container parent;
			int maxDepth = 0;

			childrenQueue.Enqueue(root);
			while (childrenQueue.Count != 0) // Get max depth
			{
				parent = childrenQueue.Dequeue();
				if (parent.depth > maxDepth)
					maxDepth = parent.depth;

				foreach (Linker.Container child in parent.children)
					childrenQueue.Enqueue(child);
			}
			return maxDepth;
		}

		private void SetMaxDepth(Linker.Container root, int maxDepth)
		{
			Queue<Linker.Container> childrenQueue = new Queue<Linker.Container>();
			Linker.Container parent;

			childrenQueue.Enqueue(root);
			while (childrenQueue.Count != 0) // set max depth
			{
				parent = childrenQueue.Dequeue();
				parent.maxDepth = maxDepth;

				foreach (Linker.Container child in parent.children)
					childrenQueue.Enqueue(child);
			}
		}
	}
}
