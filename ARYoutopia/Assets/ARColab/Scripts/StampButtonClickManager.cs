using System.Collections;
using System.Collections.Generic;
using GoogleARCore.Examples.CloudAnchors;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Youtopia
{  
    public class StampButtonClickManager : MonoBehaviour
    {

        Button m_button;
        [SerializeField]
        GameObject augmentObj;
        // Start is called before the first frame update
        void Start()
        {
            m_button = GetComponent<Button>();
            m_button.onClick.AddListener(() => OnClickStampButton(augmentObj));
        }


        void OnClickStampButton(GameObject augmentedObj)
        {
            ARController.instance.augmentedObject = augmentedObj;
            //uncomment this line only for multiplayer//PlayerController.LocalPlayerInstance.GetComponent<PlayerController>().objectToSpawn = augmentedObj;
            //CmdSyncState(augmentedObj);
        }

//    [ClientRpc]
//    void RpcSyncState(GameObject augmentedObj)
//    {
//        ARController.instance.augmentedObject = augmentedObj;
//    }
//
//    [Command]
//    void CmdSyncState(GameObject augmentObj)
//    {
//        //NetworkIdentity networkIdentity = ARController.instance.GetComponent<NetworkIdentity>();
//
//        //networkIdentity.AssignClientAuthority(connectionToClient);
//        //RpcSyncState(augmentObj);
//        //GameObject.Find("ARController").GetComponent<ARController>().augmentedObject = augmentObj;
//        ARController.instance.augmentedObject = augmentObj;
//        //networkIdentity.RemoveClientAuthority(connectionToClient);
//
//    }
        // Update is called once per frame
        void Update()
        {
        
        }
    }

}
