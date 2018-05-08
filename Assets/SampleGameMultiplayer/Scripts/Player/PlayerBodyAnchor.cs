using UnityEngine;

// Anchor point that keeps child components upright, regardless of where the Sturfee XR Camera is facing
public class PlayerBodyAnchor : MonoBehaviour
{
	private Vector3 _bodyRot;
		
	private void Update()
    {
		_bodyRot = transform.parent.rotation.eulerAngles;
		_bodyRot.x = 0;
		_bodyRot.z = 0;
		transform.rotation = Quaternion.Euler (_bodyRot);
	}
}
