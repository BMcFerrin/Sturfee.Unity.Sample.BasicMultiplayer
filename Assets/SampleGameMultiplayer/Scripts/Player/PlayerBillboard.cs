using UnityEngine;

// In each player's local instance, all other player 'billboards' will face the direction of the local player
public class PlayerBillboard : MonoBehaviour
{
	private Transform _localPlayer;

	private void Start()
    {
		_localPlayer = GameObject.FindGameObjectWithTag ("XRCamera").transform;
	}

	private void Update()
    {
		transform.LookAt(_localPlayer.position);
	}
}
