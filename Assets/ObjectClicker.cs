using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectClicker : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
		if (Input.GetMouseButtonDown(0))
		{
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			if (Physics.Raycast(ray, out hit, 100.0f))
			{
				GameObject obj = hit.transform.gameObject;
				//Debug.Log(obj.name);
				if (obj.name == "DataBall(Clone)")
				{
					obj.GetComponent<Container>().print();

					foreach (Container child in obj.GetComponent<Container>().parent.self.GetComponent<Container>().children)
						child.self.GetComponent<Renderer>().material.color = Color.blue;
					/*
					Debug.Log(obj.GetComponent<Container>().siblings.Count);

					Debug.Log("nr of siblings = " + obj.GetComponent<Container>().siblings.Count);
					foreach (Container child in obj.GetComponent<Container>().parent.self.GetComponent<Container>().children)
						child.self.GetComponent<Renderer>().material.color = Color.blue;
					obj.GetComponent<Container>().parent.self.GetComponent<Renderer>().material.color = Color.black;
					*/
				}
			}
		}
    }
}
