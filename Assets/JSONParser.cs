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
		TextAsset textFile = new TextAsset();
		textFile = Resources.Load<TextAsset>(file);
		return JsonConvert.DeserializeObject<Linker.Container>(textFile.text);
		//return JsonConvert.DeserializeObject<Linker.Container>(File.ReadAllText("Assets/Resources/" + file));
	}
}
