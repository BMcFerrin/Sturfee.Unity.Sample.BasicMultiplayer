using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Sturfee.Unity.XR.Core.Events;
using Sturfee.Unity.XR.Core.Session;
using UnityEngine.UI;

/// <summary>
/// Network player controller.
///
/// Allows the local player to shoot bullets and place targets using Sturfee functions.
/// 
/// </summary>
/// 
public class PlayerController : NetworkBehaviour
{
	public LayerMask BulletRaycastMask;

	[HideInInspector]
	public bool PlannedDisconnect = false;

	[Header("Spawnables")]
	[SerializeField]
	private GameObject _bulletPrefab;
	[SerializeField]
	private GameObject _targetPrefab;

	[Header("Components")]
	[SerializeField]
	private Transform _bulletSpawnPoint;
	[SerializeField]
	private Button _targetButton;

	private Vector2 _screenCenterPos; 
	private bool _activeHitscan = false;

	private void Start()
    {
		if (isLocalPlayer)
		{
			SturfeeEventManager.Instance.OnDetectSurfacePointComplete += OnDetectSurfacePointComplete;
			SturfeeEventManager.Instance.OnDetectSurfacePointFailed += OnDetectSurfacePointFailed;

			_screenCenterPos = new Vector2 (Screen.width / 2, Screen.height / 2);
			Bullet.CamGunYOffset = transform.position.y - _bulletSpawnPoint.position.y;
		}
	}

	private void OnDestroy()
	{
		SturfeeEventManager.Instance.OnDetectSurfacePointComplete -= OnDetectSurfacePointComplete;
		SturfeeEventManager.Instance.OnDetectSurfacePointFailed -= OnDetectSurfacePointFailed;

		// Return to starting menu if connection was lost or host left the game causing the match to be destroyed
		if (!PlannedDisconnect && isLocalPlayer)
		{
			MatchManager.Instance.ExitMatch ();
			ScreenMessageController.Instance.SetText ("Lost Connection", 3);
		}
	}

	public void OnShootClick()
	{
		CalculateBulletProjection ();
	}

	public void OnPlaceTargetClick()
	{
		if (!_activeHitscan)
		{
			_activeHitscan = true;

			// Sends forward raycast from the center of the screen
			// Sturfee server will detect if the raycast hit a building or terrain and return a GPS position of that point
			XRSessionManager.GetSession ().DetectSurfaceAtPoint (_screenCenterPos);

			_targetButton.interactable = false;
			StartCoroutine (DetectSurfacePointTimer ());

			ScreenMessageController.Instance.SetText ("Placing Target...");
		}
	}

	// Sturfee event called when 'DetectSurfaceAtPoint' completes
	public void OnDetectSurfacePointComplete(Sturfee.Unity.XR.Core.Models.Location.GpsPosition gpsPos, UnityEngine.Vector3 normal)
	{
		ScreenMessageController.Instance.SetText ("Target Placed", 2.5f);

		_activeHitscan = false;
		_targetButton.interactable = true;

		GpsStruct targetGpsPos = new GpsStruct
		{
			Latitude = gpsPos.Latitude,
			Longitude = gpsPos.Longitude,
			Height = gpsPos.Height
		};

		CmdSpawnTarget (targetGpsPos, Quaternion.LookRotation (normal));
	}

	// Sturfee event called when 'DetectSurfaceAtPoint' fails
	public void OnDetectSurfacePointFailed()
	{
			ScreenMessageController.Instance.SetText ("Target Placement Failed", 2.5f);
			_activeHitscan = false;
			_targetButton.interactable = true;
	}

	private void CalculateBulletProjection()
	{
		var camRay = GetComponentInParent<Camera>().ScreenPointToRay(_screenCenterPos);
		Vector3 direction;

		bool raycastShot;

		RaycastHit hit;
		if (Physics.Raycast (camRay, out hit, 1000.0f, BulletRaycastMask))
		{
			direction = (hit.point - _bulletSpawnPoint.position).normalized;
			raycastShot = true;
		}
		else
		{
			direction = transform.forward;
			raycastShot = false;
		}

		// Convert the bullet's spawn point to a GPS position that can be synced across the network
		var bulletSpawnGpsPos = XRSessionManager.GetSession ().LocalPositionToGps (_bulletSpawnPoint.position);
		GpsStruct bulletGpsPos = new GpsStruct {
			Latitude = bulletSpawnGpsPos.Latitude,
			Longitude = bulletSpawnGpsPos.Longitude,
			Height = bulletSpawnGpsPos.Height
		};

		CmdShootBullet (bulletGpsPos, direction, raycastShot);
	}
		
	private IEnumerator DetectSurfacePointTimer()
	{
		float endTimer = Time.time + 7;
		while (_activeHitscan && Time.time < endTimer)
		{
			yield return null;
		}

		if (_activeHitscan)
		{
			// Either the server is taking too long to compute the hitscan location, or there was an error during the call

			_targetButton.interactable = true;
			_activeHitscan = false;
			ScreenMessageController.Instance.SetText ("Surface detection timed out", 3);
		}
	}
		
	[Command]
	private void CmdSpawnTarget(GpsStruct gpsPos, Quaternion rotation)
	{
		// Spawns target in temporary safe location before each client moves the target to their 
		// own corresponding world coordinates in the 'Target' script based on the synced GPS position
		GameObject target = Instantiate(_targetPrefab, Vector3.up * 1000, rotation); 

		target.GetComponent<Target> ().GpsPos = gpsPos;
		target.GetComponent<Target> ().Orientation = rotation;

		NetworkServer.Spawn(target);	// Spawns the target on clients
	}

	[Command]
	private void CmdShootBullet(GpsStruct bulletGpsPos, Vector3 direction, bool raycastShot)
	{
		// Spawns the bullet, its location and direction will be adjusted on the clients in the 'Bullet' script afterwards
		GameObject bullet = Instantiate (_bulletPrefab);

		Bullet bulletScript = bullet.GetComponent<Bullet> ();
		bulletScript.GpsPos = bulletGpsPos;
		bulletScript.Direction = direction;
		bulletScript.RaycastShot = raycastShot;

		NetworkServer.Spawn(bullet);	// Spawns the bullet on clients
	}
}
