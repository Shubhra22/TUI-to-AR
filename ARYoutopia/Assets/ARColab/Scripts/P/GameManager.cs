using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
//using Photon.Pun.Demo.PunBasics;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Youtopia
{
    
    public class GameManager : MonoBehaviourPunCallbacks
    {

        public static GameManager Instance;
        [SerializeField]private GameObject playerPrefab;
        [SerializeField]private GameObject cube;
        public Transform buttonPanel;
       

        public GameObject PlayerPrefab 
        { 
            set { playerPrefab = value; }
        }
        // Start is called before the first frame update
        void Start()
        {
           
            
            Instance = this;

            if (!PhotonNetwork.IsConnected)
            {
                SceneManager.LoadScene("LaunchScene");
                return;
            }

            if (playerPrefab == null)
            {
                Debug.LogError("<Color=Red><b>Missing</b></Color> playerPrefab Reference. Please set it up in GameObject 'Game Manager'", this);
            }
            else
            {
                if (PlayerController.LocalPlayerInstance == null)
                {
                    PhotonNetwork.Instantiate(playerPrefab.name, new Vector3(Random.Range(0,10), 5, 0), Quaternion.identity, 0);
                }
                else
                {
                    Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);
                }
            }
            
            if (PhotonNetwork.IsMasterClient)
            {
                for (int i = 0; i < 2; i++)
                {
                      buttonPanel.GetChild(i).gameObject.SetActive(true);
                   // buttonPanel.GetChild(i).gameObject.GetComponent<Button>().interactable = true;
                }
            }
            else
            {
                for (int i = 2; i < 4; i++)
                {
                    buttonPanel.GetChild(i).gameObject.SetActive(true);
                    //buttonPanel.GetChild(i).gameObject.GetComponent<Button>().interactable = true;
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                PlayerController.LocalPlayerInstance.GetComponent<PlayerController>().objectToSpawn = cube;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                LeaveRoom();
            }
        }

        public override void OnPlayerEnteredRoom(Player other)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                LoadArena();
            }

        }

        public override void OnPlayerLeftRoom( Player other  )
        {
            Debug.Log( "OnPlayerLeftRoom() " + other.NickName ); // seen when other disconnects

            if ( PhotonNetwork.IsMasterClient )
            {
                Debug.LogFormat( "OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient ); // called before OnPlayerLeftRoom

                LoadArena(); 
            }
        }
        
        void LoadArena()
        {
            if ( ! PhotonNetwork.IsMasterClient )
            {
                Debug.LogError( "PhotonNetwork : Trying to Load a level but we are not the master Client" );
            }
            
            PhotonNetwork.LoadLevel("GameRoom");
        }
        
        public override void OnLeftRoom()
        {
            SceneManager.LoadScene("LaunchScene");
        }

        public void LeaveRoom()
        {
            PhotonNetwork.LeaveRoom();
        }

        public void QuitApplication()
        {
            Application.Quit();
        }
    }

}
