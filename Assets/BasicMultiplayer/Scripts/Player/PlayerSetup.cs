using UnityEngine;
using UnityEngine.Networking;
using Sturfee.Unity.XR.Core.Session;
using Mapbox.Utils;
using Mapbox.Unity.Map;

/// <summary>
/// Network player setup.
/// 
/// Sets up this player object to be different for the local player.
/// 
/// 1. Activates the map
/// 2. Activates the Player Controller
/// 3. Changes its color from other players
/// 4. Adjusts obstructive objects from the camera view
/// 
/// </summary>

public class PlayerSetup : MonoBehaviour
{
    [Header("Components")]
	[SerializeField]
	private GameObject _playerCanvas;
	[SerializeField]
	private GameObject _playerBillboard;
	[SerializeField]
	private GameObject _mapPlayer;
	[SerializeField]
	private GameObject _mapCamera;

	[Header("Body Parts")]
	[SerializeField]
	private GameObject _leftEye;
	[SerializeField]
	private GameObject _rightEye;
	[SerializeField]
	private GameObject _body;
	[SerializeField]
	private GameObject _gunNose;
	[SerializeField]
	private GameObject _mohawk;

    private void Start()
    {
		SetPlayerVisuals ();

		_playerCanvas.SetActive (true);
		_playerBillboard.SetActive (false);
	
		LocalMap.Instance.InitializeMap ();

		_mapCamera.SetActive (true);

		GetComponent<PlayerController> ().enabled = true;

		if (MatchManager.Instance.CreatedGame)
		{
			ScreenMessageController.Instance.SetText ("Created Game", 3);
		}
		else
		{
			ScreenMessageController.Instance.SetText ("Joined Game", 3);
		}
	}
		
    private void SetPlayerVisuals()
	{
		// Change player controlled character to blue and prevent player components from obscuring view

		_leftEye.GetComponent<MeshRenderer> ().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
		_rightEye.GetComponent<MeshRenderer> ().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
		_body.GetComponent<MeshRenderer> ().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;

		_gunNose.GetComponent<Renderer> ().material.color = Color.blue;
		_mohawk.GetComponent<Renderer> ().material.color = Color.blue;
		_mapPlayer.GetComponent<Renderer> ().material.color = Color.blue;
		_mapPlayer.transform.GetChild (0).GetComponent<SpriteRenderer> ().color = Color.blue;

		_body.GetComponent<Collider> ().enabled = false;
	}
}
