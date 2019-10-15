
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class VRInput : MonoBehaviour
{
	public SteamVR_Input_Sources handType; // 1
	public SteamVR_Action_Boolean teleportAction; // 2
	public SteamVR_Action_Boolean grabAction; // 3

	//public SteamVR_Action_Single squeezeAction = new SteamVR_Action_Single();

	void Start()
	{
		Debug.Log(handType);
		Debug.Log(teleportAction);
		Debug.Log(grabAction);
	}

    // Update is called once per frame
    void Update()
    {
		if (GetTeleportDown())
		{
		    Debug.Log("Teleport " + handType);
		}

		if (GetGrab())
		{
		    Debug.Log("Grab " + handType);
		}

    }

	public bool GetTeleportDown() // 1
	{
	    return teleportAction.GetStateDown(handType);
	}

	public bool GetGrab() // 2
	{
	    return grabAction.GetState(handType);
	}

}
