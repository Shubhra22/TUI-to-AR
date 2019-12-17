using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class NetworkLauncher : MonoBehaviourPunCallbacks
{
        [SerializeField] private GameObject controlPanel;
        [SerializeField] private Text feedbackText;
        [SerializeField] private byte maxPlayer;
        [SerializeField] private string sceneName;


        private bool isConnecting;


        private void Awake()
        {
            PhotonNetwork.AutomaticallySyncScene = true;
        }

        public void Connect()
        {
            isConnecting = true;
            controlPanel.SetActive(false);

            if (PhotonNetwork.IsConnected)
            {
                LogFeedback("Joining Room...");
                PhotonNetwork.JoinRandomRoom();
            }
            else
            {
                LogFeedback("Connecting...");

                PhotonNetwork.GameVersion = "1";
                PhotonNetwork.ConnectUsingSettings();
            }
        }

        void LogFeedback(string feedback)
        {
            if (feedbackText == null)
            {
                return;
            }

            feedbackText.text = System.Environment.NewLine + feedback;
        }

        public override void OnConnectedToMaster()
        {
            if (isConnecting)
            {
                PhotonNetwork.JoinRandomRoom();
            }
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            PhotonNetwork.CreateRoom(null, new RoomOptions {MaxPlayers = maxPlayer});
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            LogFeedback("<Color=Red>OnDisconnected</Color> " + cause);

            isConnecting = false;
            controlPanel.SetActive(true);
        }

        public override void OnJoinedRoom()
        {
            LogFeedback(
                "<Color=Green>OnJoinedRoom</Color> with " + PhotonNetwork.CurrentRoom.PlayerCount + " Player(s)");
            Debug.Log(
                "PUN Basics Tutorial/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.\nFrom here on, your game would be running.");

            // #Critical: We only load if we are the first player, else we rely on  PhotonNetwork.AutomaticallySyncScene to sync our instance scene.
            if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
            {
                Debug.Log("We load the 'Room for 1' ");

                // #Critical
                // Load the Room Level. 
                PhotonNetwork.LoadLevel(sceneName);
            }
        }

        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
        }
}
