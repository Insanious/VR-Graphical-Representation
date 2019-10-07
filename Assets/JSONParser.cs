using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;

//[Serializable]
public class JSONParser
{
	public static Linker.Container Read(string file)
	{
		return JsonConvert.DeserializeObject<Linker.Container>(File.ReadAllText("Assets/Resources/" + file));
	}
}
