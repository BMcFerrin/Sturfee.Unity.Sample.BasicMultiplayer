using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Sturfee.Unity.XR.Core.Models.Location;
using Sturfee.Unity.XR.Core.Session;

/// <summary>
/// Bullet.
/// 
/// Bullets are spawned on the server, but must be positioned on the client
/// Different clients can have different Unity coordinates, so we use GPS coordinates to sync these differences.
/// These synced values are then converted locally into this client's Unity coordinates for improved accuracy.
/// 
/// </summary>

public class Bullet : NetworkBehaviour
{
	public static float CamGunYOffset;

	public int Speed = 50;

	[HideInInspector]
	[SyncVar]
	public GpsStruct GpsPos;
	[HideInInspector]
	[SyncVar]
	public Vector3 Direction;
	[HideInInspector]
	[SyncVar]
	public bool RaycastShot;

	private bool _hitObject = false;
	private float _curYAdjust;
	private float _adjustPerFrame;

	private void Start()
	{
		Shoot ();
	}

	public void Shoot()
	{
		var gps = new GpsPosition
		{
			Latitude = GpsPos.Latitude,
			Longitude = GpsPos.Longitude,
			Height = GpsPos.Height
		};
		transform.position = XRSessionManager.GetSession().GpsToLocalPosition(gps);

		GetComponent<Collider> ().enabled = true;
		GetComponent<Rigidbody> ().velocity = Direction * Speed;

		Destroy(this.gameObject, 3.0f);

		if (!RaycastShot)
		{
			_curYAdjust = 0;
			_adjustPerFrame = 0.005f;

			StartCoroutine (RaiseBullet ());
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.tag == "Target")
		{
			collision.transform.parent.GetComponent<Target> ().Hit ();
			Destroy (gameObject);
		}
		else
		{
			// stop raising the bullet
			_hitObject = true;
		}
	}

	// Moves the bullet upward slightly over time to make up for the offset between the camera and the position the gun
	private IEnumerator RaiseBullet()
	{
		transform.Translate (transform.up * _adjustPerFrame);
		_curYAdjust += _adjustPerFrame;

		yield return new WaitForFixedUpdate ();

		if(!_hitObject && _curYAdjust < CamGunYOffset)
		{
			StartCoroutine(RaiseBullet());
		}
	}
}
