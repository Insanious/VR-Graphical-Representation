using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectClicker : MonoBehaviour
{
	public GameObject obj;

    // Update is called once per frame
    void Update()
    {
		if (Input.GetMouseButtonDown(0))
		{
			obj = RayCastToGameObject();
			if (obj != null && (obj.name == "DataBall(Clone)" || obj.name == "Cube(Clone)"))
			{
				obj.GetComponent<Linker>().container.Print();
				obj.GetComponent<Linker>().container.ToggleSubtreeLines();

				//obj.GetComponent<Linker>().container.CopySubtree(new Vector3(10f, 0f, 10f));
			}
		}

		else if (Input.GetMouseButtonDown(1))
		{
			obj = RayCastToGameObject();
			if (obj != null && (obj.name == "DataBall(Clone)" || obj.name == "Cube(Clone)"))
			{
				if (obj.GetComponent<Linker>().container.children.Count != 0)
				{
					//int currentDepth = obj.GetComponent<Linker>().container.depth;
					obj.GetComponent<Linker>().container.IncrementSubtree(Linker.RenderMode.LEVELS);
					//obj.GetComponent<Linker>().container.InstantiateSubtree(Linker.RenderMode.LEVELS, 1);
				}
			}
		}

		else if (Input.GetMouseButtonDown(2))
		{
			obj = RayCastToGameObject();
			if (obj != null && (obj.name == "DataBall(Clone)" || obj.name == "Cube(Clone)"))
				obj.GetComponent<Linker>().container.CopySubtree(new Vector3(10f, 0f, 10f));
		}
    }

	GameObject RayCastToGameObject()
	{
		RaycastHit hit;
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

		if (Physics.Raycast(ray, out hit, 100.0f))
			return hit.transform.gameObject;

		return null;
	}
}
