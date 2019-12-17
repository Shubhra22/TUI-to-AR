using System;
using System.Collections;
using System.Collections.Generic;
using GoogleARCore;
using GoogleARCore.CrossPlatform;
//using GoogleARCore.Examples.CloudAnchors;
using Photon.Pun;
using UnityEngine;

namespace Youtopia
{
    public class CloudAnchorController : MonoBehaviourPunCallbacks, IPunObservable
    {
        private string m_CloudAnchorId = string.Empty;

        private bool isHost = false;

        private bool m_ShouldResolve = false;

        /// <summary>
        /// The Cloud Anchors example controller.
        /// </summary>
        private CloudAnchorManager m_CloudAnchorsExampleController;

        // Start is called before the first frame update
        void Start()
        {
            m_CloudAnchorsExampleController = GameObject.Find("CloudAnchors")
                .GetComponent<CloudAnchorManager>();
        }

        // Update is called once per frame
        void Update()
        {
        }

        public void HostLastPlacedAnchor(Component lastPlacedAnchor)
        {
            isHost = true;
            var anchor = (Anchor) lastPlacedAnchor;

#if !UNITY_IOS || ARCORE_IOS_SUPPORT
            XPSession.CreateCloudAnchor(anchor).ThenAction(result =>
            {
                if (result.Response != CloudServiceResponse.Success)
                {
                    Debug.Log(string.Format("Failed to host Cloud Anchor: {0}", result.Response));

                    //m_CloudAnchorsExampleController.OnAnchorHosted(false, result.Response.ToString());
                    return;
                }

                Debug.Log(string.Format("Cloud Anchor {0} was created and saved.", result.Anchor.CloudId));
                m_CloudAnchorId = result.Anchor.CloudId;

                //m_CloudAnchorsExampleController.OnAnchorHosted(true, result.Response.ToString());
            });
#endif
        }

        private void ResolveAnchorFromId(string cloudAnchorId)
        {
            //m_CloudAnchorsExampleController.OnAnchorInstantiated(false);

            // If device is not tracking, let's wait to try to resolve the anchor.
            if (Session.Status != SessionStatus.Tracking)
            {
                return;
            }

            m_ShouldResolve = false;

            XPSession.ResolveCloudAnchor(cloudAnchorId).ThenAction((System.Action<CloudAnchorResult>) (result =>
            {
                if (result.Response != CloudServiceResponse.Success)
                {
                    Debug.LogError(string.Format("Client could not resolve Cloud Anchor {0}: {1}",
                        cloudAnchorId, result.Response));

                    //m_CloudAnchorsExampleController.OnAnchorResolved(false, result.Response.ToString());
                    m_ShouldResolve = true;
                    return;
                }

                Debug.Log(string.Format("Client successfully resolved Cloud Anchor {0}.",
                    cloudAnchorId));

                //m_CloudAnchorsExampleController.OnAnchorResolved(true, result.Response.ToString());
                OnResolved(result.Anchor.transform);
            }));
        }

        private void OnResolved(Transform anchorTransform)
        {
            var cloudAnchorController = GameObject.Find("CloudAnchorManager")
                .GetComponent<CloudAnchorManager>();
            cloudAnchorController.SetWorldOrigin(anchorTransform);
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(this.m_CloudAnchorId);
            }
            else
            {
                m_CloudAnchorId = (string) stream.ReceiveNext();
            }
        }

        private void _OnChangeId(string newId)
        {
            if (!isHost && newId != string.Empty)
            {
                m_CloudAnchorId = newId;
                m_ShouldResolve = true;
            }
        }
    }
}