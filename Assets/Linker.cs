using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Linker : MonoBehaviour
{
	public Container container;

	public class Container
	{
		public GameObject self;
		public GameObject line;
		public List<Container> children { get; set; }
		public List<Container> siblings { get; set; }
		public Container parent { get; set; }
		public bool drawingLine;
		public int id { get; set; }
		public int depth { get; set; }
		public string name { get; set; }
		public float size { get; set; }
		public float weight { get; set; }
		public Color color { get; set; }
	}

	private static int RecursiveDepth(Linker.Container node, int depth)
	{
		depth++;
		if (node.parent.name == "root")
			return depth;
		return RecursiveDepth(node.parent, depth);
	}

	private static float RecursiveSize(Linker.Container node, float size)
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

	public static int GetDepth(Linker.Container node)
	{
		return RecursiveDepth(node, 0); // Folder
	}

	public static float GetSize(Linker.Container node)
	{
		if (node.size > 0) // File
			return node.size;
		return RecursiveSize(node, 0);
	}

	public void Print()
	{
		string output = System.String.Empty;
		if (container.size == 0) // a folder has no size, only files has
			output += "Type = folder";
		else
			output += "Type = file";

		output += ". Name = " + container.name;

		if (container.id == 0) // root
			output += ". Parent = null";
		else
			output += ". Parent = " + container.parent.name;

		output +=
		". Id = " + container.id +
		". Depth = " + container.depth +
		". Size = " + container.size +
		". Weight = " + container.weight +
		". Number of children = " + container.children.Count +
		". Number of siblings = " + container.siblings.Count;

		Debug.Log(output);
	}

	public void ToggleSubtreeLines()
	{
		if (container.children.Count == 0)
			return;

		bool drawing = false;

		foreach (Linker.Container child in container.children)
		{
			if (child.drawingLine) // If any of the children are drawing lines, disable all drawing
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

		childrenQueue.Enqueue(container);
		while(childrenQueue.Count != 0)
		{
			parent = childrenQueue.Dequeue();

			foreach (Linker.Container child in parent.children)
			{
				childrenQueue.Enqueue(child);

				child.self.GetComponent<Linker>().container.line.GetComponent<LineRenderer>().positionCount = 0;
				child.drawingLine = false;
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

		childrenQueue.Enqueue(container);
		while(childrenQueue.Count != 0)
		{
			parent = childrenQueue.Dequeue();
			parentPos = parent.self.transform.position;
			parentColor = parent.color;

			foreach (Linker.Container child in parent.children)
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

				child.drawingLine = true;
			}
		}
	}
}
