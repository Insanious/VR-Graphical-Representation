using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

public class DataPlotter : MonoBehaviour
{

	// Initialize variables
	public GameObject PointPrefab;
	public string file;
	public Container root;

	public Queue referenceQueue = new Queue();
	public Queue childrenQueue = new Queue();
	public Queue tempQueue = new Queue();
	public List<Container> nodes = new List<Container>();
	public List<Container> siblings = new List<Container>();

	public int nrOfChildren = 0;
	public int nrOfSiblings = 0;
	public float level = 0;
	public int gridSize = 0;

	public Container parent;

	void Start()
	{
		Container oldRoot = JSONParser.Read(file);

		oldRoot.parent = null;

		//referenceQueue.Enqueue(oldRoot);
		root = new Container();
		root.parent = null;
		root.name = oldRoot.name;

		//Debug.Log(oldRoot.GetType());
		//Debug.Log(root.GetType());

		referenceQueue.Enqueue(oldRoot);
		childrenQueue.Enqueue(root);



		int counter = 0;

		while (referenceQueue.Count != 0)
		{
			Container temp1 = (Container)childrenQueue.Peek();
			Container temp2 = (Container)referenceQueue.Peek();

			//Debug.Log("ref queue count = " + referenceQueue.Count + " children queue count = " + childrenQueue.Count + "ref peek = " + temp1.name + " children peek = " + temp2.name);
			// Populate the reference queue
			Container node = (Container)referenceQueue.Dequeue();
			foreach(Container child in node.children)
			{
				tempQueue.Enqueue(child);
				referenceQueue.Enqueue(child);
			}

			parent = (Container)childrenQueue.Dequeue();
			foreach(Container child in tempQueue)
			{
				// Create the fucking containers dude
				Container newChild = new Container();
				counter++;
				newChild.parent = parent;
				newChild.name = child.name;
				newChild.size = child.size;
				newChild.weight = child.weight;

				parent.children = new List<Container>(); // Initialize list
				parent.children.Add(newChild); // Add the new child to the list
				childrenQueue.Enqueue(newChild); // Enqueue the new child top be able to assign it to parent later on
			}

			while (tempQueue.Count != 0)
				tempQueue.Dequeue();
			//while (referenceQueue.Count != 0)
				//referenceQueue.Dequeue();

			nodes.Add(parent);
		}

		int nrOfNodes = nodes.Count;

		parent = null;

		foreach (Container node in nodes)
		{
			if (node.parent == parent)
				siblings.Add(node);
			else
			{
				Color color = new Color();
				if (level % 10 == 0)
					color = Color.blue;
				else if (level % 10 == 1)
					color = Color.red;
				else if (level % 10 == 2)
					color = Color.cyan;
				else if (level % 10 == 3)
					color = Color.green;
				else if (level % 10 == 4)
					color = Color.magenta;
				else if (level % 10 == 5)
					color = Color.yellow;
				//color = new Color(0.5f, 0.5f, 0.5f, 1f);
				parent = node.parent;




				gridSize = (int)Math.Ceiling(Math.Sqrt(siblings.Count)); // Nearest perfect square is the ceiling of the square root

				nrOfSiblings = siblings.Count;
				if (nrOfSiblings > 0)
					level++;
				Debug.Log(nrOfSiblings);

				for (int i = 0; i < gridSize; i++)
				{
					for (int j = 0; j < gridSize; j++)
					{
						if (i*j + j > nrOfSiblings)
							i = j = gridSize;
						else
							createPrefab(siblings[i], new Vector3(i, level, j), color);
					}
				}
				// Empty the list
				siblings.Clear();
			}
		}
/*
		nrOfChildren = root.children.Count;
		parent = root;

		referenceQueue.Enqueue(root);
		childrenQueue.Enqueue(root);

		//Debug.Log("child " + tempChild.name + " with parent " + tempChild.parent.name + ", " tempChild.children[0].name + ", " tempChild.children[0].parent.name);
		// Enqueue all children
		while (referenceQueue.Count != 0)
		{
			//Debug.Log("temp queue count = " + referenceQueue.Count + " total queue count = " + childrenQueue.Count + "\n");
			Container child = (Container)referenceQueue.Peek();


			foreach (Container grandChild in child.children) // Enqueue all grandchildren
			{
				referenceQueue.Enqueue(grandChild);
				childrenQueue.Enqueue(grandChild);
			}
		}
		//Debug.Log(childrenQueue.Count + "\n");

		Queue temp = new Queue(childrenQueue);
		for (int i = 0; i < temp.Count; i++)
		{
			Container theChild = (Container)temp.Dequeue();
			//Debug.Log("child #" + i + ", " + theChild.name + " with parent " + theChild.parent.name);
		}

		int counter = 0;
		while(childrenQueue.Count != 0)
		{
			//Debug.Log(counter++);
			// Structure: P1, P1, ... , P1, P2, P2, ... , P2, P3, P3, ... , Pn, Pn, ... , Pn

			// Put all siblings in list
			Container container = (Container)childrenQueue.Dequeue();
			//Debug.Log("parent=" + parent.name + ", new parent=" + container.parent.name + "\n");
			//Debug.Log("child " + container.name + " with parent " + container.parent.name + "\n");
			if (container.name == "logical_coupling.clj")
				;//Debug.Log("hereeee boiiiiiiiiiiii\n");
			if (container.parent.name == parent.name)
			{
				siblings.Add(container);
				if (container.name == "logical_coupling.clj")
					;//Debug.Log("hereeee\n");
			}

			// New parent, create prefab of all siblings and increment level
			else
			{
				//Debug.Log(siblings.Count + "\n");


				parent = container;
				//Debug.Log("nr of siblings = " + siblings.Count + "\n");
				level++;
				// Create prefabs from all siblings
				// Prefab pos will be at grid(x, y)
				// Grid size will be the nearest perfect square of the number of children the parent has

				// Calculate grid size
				gridSize = (int)Math.Ceiling(Math.Sqrt(siblings.Count)); // Nearest perfect square is the ceiling of the square root

				// Gather all grid of the grid
				// The case where there is an only child, Vector3 grid will be a 2d-Vector3 with size (1, 1) and it's only entry will be (0, 0, level) at index [0, 0]
					//for (int i = 0; i < gridSize; i++)
						//for (int j = 0; j < gridSize; j++)
							//grid[i, j].Add(new Vector3(i, j, level));

				// Create prefabs from children
				nrOfSiblings = siblings.Count;

				//Debug.Log("count=" + nrOfSiblings);
				//Debug.Log("nr of siblings = " + nrOfSiblings + ", gridsize^2 = " + gridSize*gridSize);

				for (int i = 0; i < gridSize; i++)
				{
					for (int j = 0; j < gridSize; j++)
					{
						if (i*j + j > nrOfSiblings)
							i = j = gridSize;
						else
							createPrefab(siblings[i], new Vector3(i, level, j));
					}
				}
				// Empty the list
				siblings.Clear();
			}

			//childrenQueue.Dequeue();
		}
		/*
		var obj = Instantiate(PointPrefab, new Vector3(iris.sepalLength*4, iris.sepalWidth*4, iris.petalLength+iris.petalWidth), Quaternion.identity);
		Color color = new Color(iris.petalLength/2, iris.petalWidth/2, iris.petalLength+iris.petalWidth/2, 1);
		obj.GetComponent<Renderer>().material.color = color;
		obj.transform.localScale = obj.transform.localScale * ((iris.sepalLength * iris.sepalWidth + iris.petalLength * iris.petalWidth) / 10);
		*/
	}

	void createPrefab(Container node, Vector3 position, Color color)
	{
		//Debug.Log("creating node " + node.name + " with parent " + node.parent.name);
		var obj = Instantiate(PointPrefab, new Vector3(position.x, position.y, position.z), Quaternion.identity);
		obj.GetComponent<Renderer>().material.color = color;

		//Debug.Log(node.name + " is at pos=(" +  position.x + ", "+ position.y + ", " + position.z + ")\n");
	}
	/*
	void recursiveCreatePrefab(Container node, Vector3 offset)
	{
		Debug.Log(node);
		if (node.name == null)
			return;

		var obj = Instantiate(PointPrefab, new Vector3(offset.x, offset.y, offset.z), Quaternion.identity);
		Debug.Log(node.name + " is at pos=(" +  offset.x + ", "+ offset.y + ", " + offset.z + ")\n");
		//Color color = new Color(iris.petalLength/2, iris.petalWidth/2, iris.petalLength+iris.petalWidth/2, 1);
		//obj.GetComponent<Renderer>().material.color = color;
		//obj.transform.localScale = obj.transform.localScale * ((iris.sepalLength * iris.sepalWidth + iris.petalLength * iris.petalWidth) / 10);

		foreach(Container child in node.children)
		{
			if (offset.x != 0)
				offset.x++;
			else if (offset.y != 0)
				offset.y++;
			else if (offset.z != 0)
				offset.z++;

			recursiveCreatePrefab(child, offset);
		}
	}
	*/
}
