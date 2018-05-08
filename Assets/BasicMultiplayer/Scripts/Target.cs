using UnityEngine;
using UnityEngine.Networking;
using Sturfee.Unity.XR.Core.Models.Location;
using Sturfee.Unity.XR.Core.Session;

/// <summary>
/// Target.
/// 
/// Targets are spawned on the server, but must be positioned on the client
/// Different clients can have different Unity coordinates, so we use GPS coordinates to sync these differences.
/// These synced values are then converted locally into this client's Unity coordinates for improved accuracy.
/// 
/// </summary>

public class Target : NetworkBehaviour
{
	public GameObject MapTarget;

	[HideInInspector]
	[SyncVar]
	public GpsStruct GpsPos;
	[HideInInspector]
	[SyncVar]
	public Quaternion Orientation;

	private void Start()
	{
		AdjustTargetToPlayerCoordinates ();
	}
		
	public void Hit()
	{
		if (!isServer)
		{
			return;
		}
		RpcDestroyTarget();
	}

	[ClientRpc]
	private void RpcDestroyTarget()
	{
		Destroy (this.gameObject);
	}
		
	// Takes the synced GPS position and converts them to this client player's Unity coordinates
	private void AdjustTargetToPlayerCoordinates()
	{
		var gps = new GpsPosition
		{
			Latitude = GpsPos.Latitude,
			Longitude = GpsPos.Longitude,
			Height = GpsPos.Height
		};
		transform.position = XRSessionManager.GetSession().GpsToLocalPosition(gps);
		transform.rotation = Orientation;

		// Rotate the map representation of the target independently to have correct orientation on map view
		MapTarget.transform.rotation = Quaternion.identity;
	}
}
