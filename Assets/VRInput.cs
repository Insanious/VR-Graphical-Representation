
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;

public class VRInput : MonoBehaviour
{
	private GameObject selectedObj = null;
	public Text output;

	public SteamVR_Input_Sources handType;
	public SteamVR_Action_Boolean toggleSubtreeLines;
	public SteamVR_Action_Boolean selectSubtree;
	public SteamVR_Action_Boolean copySubtree;
	public SteamVR_Action_Boolean printNode;
	public SteamVR_Action_Boolean movePlayer;
	public SteamVR_Action_Boolean moveSubtree;
	public SteamVR_Action_Boolean incrementSubtree;
	public SteamVR_Action_Boolean decrementSubtree;

	//public SteamVR_Action_Single squeezeAction = new SteamVR_Action_Single();

	void Start()
	{
		if (false) // right hand
		{
			movePlayer.AddOnStateDownListener(MovePlayer, handType);
			moveSubtree.AddOnStateDownListener(MoveSubtree, handType);

			selectSubtree.AddOnStateDownListener(SelectSubtree, handType);
			printNode.AddOnStateDownListener(PrintNode, handType);

		}
		else if (false)// left hand
		{
			incrementSubtree.AddOnStateDownListener(IncrementSubtree, handType);
			decrementSubtree.AddOnStateDownListener(DecrementSubtree, handType);

			copySubtree.AddOnStateDownListener(CopySubtree, handType);
			toggleSubtreeLines.AddOnStateDownListener(ToggleSubtreeLines, handType);
		}
	}

	public void MovePlayer(SteamVR_Action_Boolean action, SteamVR_Input_Sources handType)
	{
		if (selectedObj == null)
		{

		}
	}

	public void MoveSubtree(SteamVR_Action_Boolean action, SteamVR_Input_Sources handType)
	{
		if (selectedObj != null)
			selectedObj.GetComponent<Linker>().container.MoveSubtree(new Vector3(1, 1, 1));
	}

	public void SelectSubtree(SteamVR_Action_Boolean action, SteamVR_Input_Sources handType)
	{
		GameObject obj = RayCastToGameObject(new Vector3(0, 0, 0), new Vector3(0, 0, 0));

		if (obj != null && (obj.name == "DataBall(Clone)" || obj.name == "Cube(Clone)"))
			selectedObj = obj;
		else
			selectedObj = null;
	}

	public void PrintNode(SteamVR_Action_Boolean action, SteamVR_Input_Sources handType)
	{
		if (selectedObj != null)
		{

		}
	}

	public void IncrementSubtree(SteamVR_Action_Boolean action, SteamVR_Input_Sources handType)
	{
		if (selectedObj != null)
		{

		}
	}

	public void DecrementSubtree(SteamVR_Action_Boolean action, SteamVR_Input_Sources handType)
	{
		if (selectedObj != null)
		{

		}
	}

	public void CopySubtree(SteamVR_Action_Boolean action, SteamVR_Input_Sources handType)
	{
		if (selectedObj != null)
			selectedObj.GetComponent<Linker>().container.CopySubtree(new Vector3(10f, 0f, 10f));
	}

	public void ToggleSubtreeLines(SteamVR_Action_Boolean action, SteamVR_Input_Sources handType)
	{
		if (selectedObj != null)
		{

		}
	}

	private GameObject RayCastToGameObject(Vector3 origin, Vector3 direction)
	{
		RaycastHit hit;
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

		if (Physics.Raycast(ray, out hit, 100.0f))
			return hit.transform.gameObject;

		return null;
	}

	void Update()
	{
		GameObject obj = RayCastToGameObject(new Vector3(0, 0, 0), new Vector3(0, 0, 0));
		if (obj != null && (obj.name == "DataBall(Clone)" || obj.name == "Cube(Clone)"))
			output.text = obj.GetComponent<Linker>().container.ToString();

	}
}
