using System;
using System.Collections;
using System.Collections.Generic;
using GoogleARCore;
using GoogleARCore.Examples.CloudAnchors;
using Photon.Pun;
using UnityEngine;

namespace Youtopia
{
    public class CloudAnchorManager : MonoBehaviour
    {
        [SerializeField]private GameObject ARCoreRoot;
        
        public enum States
        {
            Initializing,
            Hosting,
            Resolving,
        }

        public States state = States.Initializing;
        public ARCoreWorldOriginHelper ARCoreWorldOriginHelper;

        private bool m_IsOriginPlaced = false;
        private Component m_LastPlacedAnchor = null;

        private bool anchorPlaced;

        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
            if(state!=States.Hosting)
                return;
            
            Touch touch;
            if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
            {
                return;
            }

            if (Application.platform != RuntimePlatform.IPhonePlayer)
            {
                TrackableHit hit;
                if (ARCoreWorldOriginHelper.Raycast(touch.position.x, touch.position.y,
                    TrackableHitFlags.PlaneWithinPolygon, out hit))
                {
                    m_LastPlacedAnchor = hit.Trackable.CreateAnchor(hit.Pose);
                }
            }

            if (m_LastPlacedAnchor != null)
            {
                if (CanPlaceObject())
                {
                    InstantiateObject();
                }
                else if (!anchorPlaced && state == States.Hosting)
                {
                    SetWorldOrigin(m_LastPlacedAnchor.transform);
                    InstantiateAnchor();
                    anchorPlaced = true;
                }
            }
        }

        bool CanPlaceObject()
        {
            if (anchorPlaced)
            {
                return true;
            }

            return false;
        }

        public void SetWorldOrigin(Transform anchorTransform)
        {
            if (m_IsOriginPlaced)
            {
                Debug.LogWarning("The World Origin can be set only once.");
                return;
            }

            m_IsOriginPlaced = true;

            if (Application.platform != RuntimePlatform.IPhonePlayer)
            {
                ARCoreWorldOriginHelper.SetWorldOrigin(anchorTransform);
            }
        }

        private void InstantiateAnchor()
        {
            // The anchor will be spawned by the host, so no networking Command is needed.
            PlayerController.LocalPlayerInstance.GetComponent<PlayerController>().SpawnAnchor(
                Vector3.zero, Quaternion.identity, m_LastPlacedAnchor);
        }

        private void InstantiateObject()
        {
            PlayerController.LocalPlayerInstance.GetComponent<PlayerController>().SpawnObject(
                m_LastPlacedAnchor.transform.position, m_LastPlacedAnchor.transform.rotation);
        }

        public void OnClickHost()
        {
            if (state == States.Hosting)
            {
                state = States.Initializing;
            }
            state = States.Hosting;
            ARCoreRoot.SetActive(true);
        }
        
        
    }
}