using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;

//[Serializable]
public class JSONParser
{
	public static Container Read(string file)
	{
		return JsonConvert.DeserializeObject<Container>(File.ReadAllText("Assets/Resources/" + file));
		//root.parent = null;
		//root.connectParent();
		//Debug.Log(root.children.Count);
		//Debug.Log("root's first child is " + root.children[0].name + " and the child's parent is " + root.children[0].parent.name);
		//return root;
	}
}
