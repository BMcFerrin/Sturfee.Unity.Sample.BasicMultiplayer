using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sturfee.Unity.XR.Core.Events;
using Sturfee.Unity.XR.Core.Session;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;

/// <summary>
/// Match manager.
/// 
/// Handles connecting to online match and calling Sturfee camera localization functions
/// 
/// Order of events
/// 1. Tracks the game the player is creating/joining via the menu options
/// 2. Localizes player 
/// 3. Directly after localization competes, connects player to multiplayer match
/// 
/// </summary>

public class MatchManager : MonoBehaviour{

	public static MatchManager Instance;

	public NetworkManager GameNetworkManager;

	[Header("Menu Components")]
	public GameObject StartMenu;
	public GameObject CreateMatchMenu;
	public InputField MatchNameInputField;
	public GameObject MatchButtonList;
	public GameObject JoinMatchButtonPrefab;
	public GameObject CameraInstructionsPanel;
	public GameObject ConnectionsAndGpsStatusPanel;
	public GameObject AlignmentPanel;
	public GameObject InGameMenu;
	public Text RoomName;

	[Header("Platform XR Session")]
	public GameObject DebugXrSession;
	public GameObject AndroidXrSession;
	public GameObject IPhoneXrSession;

	[Header("Prefabs")]
	public GameObject MapPrefab;
	public GameObject PlayerXrCameraPrefab;

	[HideInInspector]
	public GameObject PlayerXrCamera;
	[HideInInspector]
	public GameObject PlayerXrSession;

	private bool _createdGame = false;
	public bool CreatedGame { get { return _createdGame; } }

	private GameObject _mainCamera;
	private bool _performedAlignment = false;
	private int _matchNumToJoin;
	private string[] _defaultMatchNames = {"Atlas", "Emperion", "Vortex", "Bimbo", "Linguini"};

	void Awake()
	{
		Instance = this;
	}

	void Start () {
		_mainCamera = Camera.main.gameObject;
		StartMenu.SetActive (true);
	}

	public void OnCreateMatchClick()
	{
		// create a default match name
		int index = Random.Range (0, _defaultMatchNames.Length);
		MatchNameInputField.text = _defaultMatchNames [index] + Random.Range (1, 9999);
	}

	public void OnCreateClick()
	{
		if (MatchNameInputField.text.Length > 0)
		{
			CreateMatchMenu.SetActive (false);
			_createdGame = true;

			PrepareForAlignment ();
			GameNetworkManager.StartMatchMaker ();
		}
		else
		{
			ScreenMessageController.Instance.SetText ("Please input match name", 3);
		}
	}

	public void OnFindMatchClick()
	{
		GameNetworkManager.StartMatchMaker ();
		GameNetworkManager.matchMaker.ListMatches (0, 20, "", false, 0, 0, OnMatchList);
		ScreenMessageController.Instance.SetText ("Searching for matches...");
	}

	// Callback function when the list of active matches is received
	public void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matches)
	{
		GameNetworkManager.OnMatchList (success, extendedInfo, matches);
		InstantiateJoinMatchButtons();
	}
		
	public void RemoveJoinMatchButtons(bool stopMatchMaker = false)
	{
		if (stopMatchMaker)
		{
			GameNetworkManager.StopMatchMaker ();
		}
		for (int i = MatchButtonList.transform.childCount; i > 0 ; i--)
		{
			Destroy(MatchButtonList.transform.GetChild(i - 1).gameObject);
		}
	}

	public void OnExitMatchClick()
	{
		PlayerXrCamera.GetComponentInChildren<PlayerController> ().PlannedDisconnect = true;
		ExitMatch ();
	}

	public void ExitMatch()
	{
		GameNetworkManager.StopHost ();

		// Reset the map
		Destroy (LocalMap.Instance.gameObject);
		Instantiate (MapPrefab);

		InGameMenu.SetActive (false);
		Reset ();

		ScreenMessageController.Instance.ClearText ();
	}

	private void InstantiateJoinMatchButtons()
	{
		if (GameNetworkManager.matches.Count == 0)
		{
			ScreenMessageController.Instance.SetText ("No active matches available");
		}
		else
		{
			ScreenMessageController.Instance.ClearText ();
			for (int i = 0; i < GameNetworkManager.matches.Count; i++)
			{
				GameObject joinButton = Instantiate (JoinMatchButtonPrefab, MatchButtonList.transform);
				joinButton.GetComponent<RectTransform> ().sizeDelta = 
					new Vector2(MatchButtonList.GetComponent<RectTransform> ().rect.width - 40, joinButton.GetComponent<RectTransform>().rect.height);
				int matchNum = i;
				joinButton.GetComponent<Button> ().onClick.AddListener (delegate {OnJoinMatchClick (matchNum);});
				joinButton.GetComponentInChildren<Text> ().text = GameNetworkManager.matches[matchNum].name;
			}
		}
	}	
		
	private void OnJoinMatchClick(int matchNum)
	{
		_matchNumToJoin = matchNum;
		MatchButtonList.transform.parent.gameObject.SetActive (false);
		PrepareForAlignment ();
	}

	private void PrepareForAlignment()
	{
		SturfeeEventManager.Instance.OnSessionReady += OnSessionReady;
		SturfeeEventManager.Instance.OnLocalizationComplete += OnLocalizationComplete;

		// Spawns the Sturfee XR Camera here that the player will control, and the 'NetworkPlayer' will attach to once the player is connected to a match
		PlayerXrCamera = Instantiate (PlayerXrCameraPrefab);

		// Detects the platform the user is on to instantiate the correct Sturfee XR Session
		#if UNITY_EDITOR || UNITY_STANDALONE
		PlayerXrSession = Instantiate(DebugXrSession);
		#elif UNITY_ANDROID
		PlayerXrSession = Instantiate(AndroidXrSession);
		#elif UNITY_IPHONE
		PlayerXrSession = Instantiate(IPhoneXrSession);
		#else
		PlayerXrSession = Instantiate(DebugXrSession);
		#endif

		_mainCamera.gameObject.SetActive (false);
		CameraInstructionsPanel.SetActive (true);
		ConnectionsAndGpsStatusPanel.SetActive (true);
		StartCoroutine (ConnectionAndGpsErrorTimer ());
		}

	private IEnumerator PerformAlignment()
	{
		ConnectionsAndGpsStatusPanel.SetActive (false);
		AlignmentPanel.SetActive (true);
		Text alignmentPanelText = AlignmentPanel.GetComponentInChildren<Text> ();

		for (int i = 3; i > 0; i--)
		{
			alignmentPanelText.text = "Camera alignment begins in " + i.ToString ();
			yield return new WaitForSeconds (1);
		}

		alignmentPanelText.text = "Aligning Camera...";

		XRSessionManager.GetSession ().PerformLocalization ();

		StartCoroutine (AlignmentStuckErrorTimer ());
	}

	// Sturfee event called when the Sturfee XR Session is ready to be used
	private void OnSessionReady()
	{
		if (!_performedAlignment)
		{
			// Begins alignment as soon as XR Session is ready after it spawns
			_performedAlignment = true;	
			StartCoroutine(PerformAlignment());
		}
	}

	// Sturfee event called when camera alignment completes
	private void OnLocalizationComplete(Sturfee.Unity.XR.Core.Constants.Enums.AlignmentStatus status)
	{
		if (status == Sturfee.Unity.XR.Core.Constants.Enums.AlignmentStatus.Done)
		{
			// Creating or joining a game

			SturfeeEventManager.Instance.OnSessionReady -= OnSessionReady;
			SturfeeEventManager.Instance.OnLocalizationComplete -= OnLocalizationComplete;

			ScreenMessageController.Instance.SetText ("Camera Alignment Complete", 3);

			if (_createdGame)
			{
				GameNetworkManager.name = MatchNameInputField.text;
				GameNetworkManager.matchMaker.CreateMatch (MatchNameInputField.text, GameNetworkManager.matchSize, true, "", "", "", 0, 0, GameNetworkManager.OnMatchCreate);

				RoomName.text = "Room: " + MatchNameInputField.text + "\nHost";
			}
			else // Joining Game
			{
				RemoveJoinMatchButtons ();
				MatchButtonList.transform.parent.gameObject.SetActive (false);
				GameNetworkManager.matchMaker.JoinMatch(GameNetworkManager.matches[_matchNumToJoin].networkId, "", "", "", 0, 0, GameNetworkManager.OnMatchJoined);
				RoomName.text = "Room: " + GameNetworkManager.matches [_matchNumToJoin].name + "\nClient";
			}
			CameraInstructionsPanel.SetActive (false);
			AlignmentPanel.SetActive (false);
			InGameMenu.SetActive (true);

			return;
		}
		else if (status == Sturfee.Unity.XR.Core.Constants.Enums.AlignmentStatus.IndoorsError)
		{
			ScreenMessageController.Instance.SetText ("Indoors Error. Please try again outside.", 3);
			CameraInstructionsPanel.SetActive (false);
			AlignmentPanel.SetActive (false);
			Reset ();
		}
		else if (status == Sturfee.Unity.XR.Core.Constants.Enums.AlignmentStatus.Error)
		{
			ScreenMessageController.Instance.SetText ("Alignment Failed", 3);
			CameraInstructionsPanel.SetActive (false);
			AlignmentPanel.SetActive (false);
			Reset ();
		}
	}

	// Resets common components back to the way they were at the starting menu
	private void Reset()
	{
		SturfeeEventManager.Instance.OnSessionReady -= OnSessionReady;
		SturfeeEventManager.Instance.OnLocalizationComplete -= OnLocalizationComplete;
		Destroy (PlayerXrCamera);
		Destroy (PlayerXrSession);
		RemoveJoinMatchButtons ();
		_mainCamera.SetActive (true);
		_performedAlignment = false;
		_createdGame = false;
		StartMenu.SetActive (true);
	}

	private IEnumerator ConnectionAndGpsErrorTimer()
	{
		float endTimer = Time.time + 8;
		while (ConnectionsAndGpsStatusPanel.activeSelf && Time.time < endTimer)
		{
			yield return null;
		}

		if (ConnectionsAndGpsStatusPanel.activeSelf)
		{
			Reset ();
			ConnectionsAndGpsStatusPanel.SetActive (false);
			AlignmentPanel.SetActive (false);
			ScreenMessageController.Instance.SetText ("Weak connection\nor can't find GPS", 3);
		}
	}

	private IEnumerator AlignmentStuckErrorTimer()
	{
		float endTimer = Time.time + 13;
		while (AlignmentPanel.activeSelf && Time.time < endTimer)
		{
			yield return null;
		}

		if (AlignmentPanel.activeSelf)
		{
			Reset ();
			AlignmentPanel.SetActive (false);
			CameraInstructionsPanel.SetActive (false);
			ScreenMessageController.Instance.SetText ("Camera alignment timed out", 3);
		}
	}
}
