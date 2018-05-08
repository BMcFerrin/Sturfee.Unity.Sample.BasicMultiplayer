using UnityEngine;
using Mapbox.Unity.Map;
using Sturfee.Unity.XR.Core.Session;
using Mapbox.Utils;

public class LocalMap : MonoBehaviour
{
	public static LocalMap Instance;

	[HideInInspector]
	public AbstractMap Map;

	private void Start()
	{
		Instance = this;
		Map = GetComponent<AbstractMap> ();
	}

	public void InitializeMap()
	{
		transform.position = XRSessionManager.GetSession ().GetXRCameraPosition ();
		transform.rotation = Quaternion.identity;
		transform.position += Vector3.down * 100;

		var gpsPos = XRSessionManager.GetSession ().GetLocationCorrection ();
		Vector2d gpsLatLongPos = new Vector2d (gpsPos.Latitude, gpsPos.Longitude);	

		Map.Initialize (gpsLatLongPos, 15);
	}
}
