using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Youtopia
{
    public class PlayerController : MonoBehaviourPunCallbacks,IPunObservable
    {
        public GameObject objectToSpawn;
        public GameObject anchorPrafab;

        public static GameObject LocalPlayerInstance;
        // Start is called before the first frame update
        private void Awake()
        {
            if (photonView.IsMine)
            {
                LocalPlayerInstance = gameObject;
            }
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            //SceneManager.sceneLoaded += OnSceneLoaded;
            //Debug.Log("Came here");
//            if (photonView.IsMine)
//            {
//                GameObject.Find("CloudAnchors").GetComponent<CloudAnchorManager>().OnClickHost();
//            }

            GameObject.Find("CloudAnchors").GetComponent<CloudAnchorManager>().OnClickHost();

        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
        {
            Debug.Log("Came here");
            //GameObject.Find("CloudAnchors").GetComponent<CloudAnchorManager>().OnClickHost();
        }

        // Update is called once per frame
        void Update()
        {           
//            if (photonView.IsMine)
//            {
//              
//                if (Input.GetMouseButtonDown(0))
//                {
//                    PhotonNetwork.Instantiate(objectToSpawn.name, new Vector3(Random.Range(0,10),Random.Range(0,10),0), Quaternion.identity);
//                }
//            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            //throw new System.NotImplementedException();
            if (stream.IsWriting)
            {
                Debug.Log("Writting");
                //stream.SendNext(objectToSpawn);
            }
            else
            {
                Debug.Log("Reading");
                //objectToSpawn = (GameObject) stream.ReceiveNext();
            }
        }

        public void SpawnAnchor(Vector3 position, Quaternion rotation, Component anchor)
        {
            var anchorObject = PhotonNetwork.Instantiate(anchorPrafab.name,position,rotation);
            anchorObject.GetComponent<CloudAnchorController>().HostLastPlacedAnchor(anchor);
        }

        public void SpawnObject(Vector3 position, Quaternion rotation)
        {
            if (photonView.IsMine)
            {
                PhotonNetwork.Instantiate(objectToSpawn.name, position,rotation);
            }
        }
    }
}

