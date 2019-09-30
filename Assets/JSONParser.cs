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
	}
}
