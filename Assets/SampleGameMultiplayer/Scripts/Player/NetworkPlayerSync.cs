using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Sturfee.Unity.XR.Core.Session;
using Sturfee.Unity.XR.Core.Models.Location;

// Used to send GPS values across the network
// The Sturfee 'GpsPosition' class values are converted to this data structure when sending values over the network
[Serializable]
public struct GpsStruct
{
	public double Latitude;
	public double Longitude;
	public double Height;
}

/// <summary>
/// Network player sync.
/// 
/// This script is active on all clients, but performs different functions depending if this script is active on the local player or not
/// 
/// Local Player
/// 1. Attaches the player object to the Sturfee XR Camera and turns on the local player scripts
/// 2. Sends this player object's GPS and rotation values across the network
/// 
/// Non-Local Player
/// 1. Applies the synced network variables of the player controlling this object.
/// 
/// </summary>

public class NetworkPlayerSync : NetworkBehaviour
{
	private GpsStruct PlayerGpsPos;
	private Quaternion PlayerOrientation;

	private void Start()
    {
		if (isLocalPlayer)
		{
			AttachToXrCamera ();
			GetComponent<PlayerSetup> ().enabled = true;

			SendNetworkPlayerValues ();
		}
		else
		{
			ApplyNetworkPlayerValues ();
		}
	}
	
	private void Update()
	{
		if (isLocalPlayer)
		{
			SendNetworkPlayerValues ();
		}
		else
		{
			ApplyNetworkPlayerValues ();
		}
	}

	// Prepares to sync the local player's GPS and orientation values across the network
	// Function only called by local player
	private void SendNetworkPlayerValues()
	{
		// Convert the Player's Unity coordinates to GPS coordinates
		var gpsPos = XRSessionManager.GetSession ().LocalPositionToGps (transform.position);

		// Converts the Sturfee 'GpsPosition' class to a struct with the same values so it can be transferred across the network
		GpsStruct newPlayerGpsPos = new GpsStruct
		{
			Latitude = gpsPos.Latitude,
			Longitude = gpsPos.Longitude,
			Height = gpsPos.Height
		};

		CmdSetPlayerTransform (newPlayerGpsPos, transform.rotation);
	}

	// Applies other player's GPS and orientation values to this player object that were synced across the network
	// Function only called by non-local players
	private void ApplyNetworkPlayerValues()
	{
		// Converts the GPS struct value synced across the network back into Sturfee class 'GpsPosition' 
		// to correctly sync this players position in the world.
		var gps = new GpsPosition
		{
			Latitude = PlayerGpsPos.Latitude,
			Longitude = PlayerGpsPos.Longitude,
			Height = PlayerGpsPos.Height
		};

		// Converts the GPS position to this client player's Unity coordinates
		transform.position = XRSessionManager.GetSession().GpsToLocalPosition(gps);
		transform.rotation = PlayerOrientation;
	}

	// Sends Player values from this client to the server
	[Command]
	private void CmdSetPlayerTransform(GpsStruct gpsPos, Quaternion rotation)
	{
		RpcSetPlayerTransform (gpsPos, rotation);
	}

	// Sends Player values from server to all clients
	[ClientRpc]
	private void RpcSetPlayerTransform(GpsStruct gpsPos, Quaternion rotation)
	{
		PlayerGpsPos = gpsPos;
		PlayerOrientation = rotation;
	}

	private void AttachToXrCamera()
	{
		// Parents the network player to the Sturfee Mixed Reality Camera
		transform.SetParent (GameObject.FindGameObjectWithTag ("XRCamera").transform);
		transform.localPosition = Vector3.zero;
		transform.localEulerAngles = Vector3.zero;
	}
}
