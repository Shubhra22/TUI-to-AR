using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;
using Photon.Pun.Demo.Cockpit;
using UnityEditor;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class ARController : NetworkBehaviour
{
    public Camera FirstPersonCamera;
    public GameObject augmentedObject;
    
    
    private const float k_ModelRotation = 180.0f;
    List<DetectedPlane> m_AllPlanes = new List<DetectedPlane>();

    public bool clickedOnStamp; // change this logic

    public GameObject volumatricCanvas;


    public static ARController instance;
    
    private GameObject vCanvas;
    private Vector3 camPos;
    
    // Raycast against the location the player touched to search for planes.
    TrackableHit hit;

    public GameObject markerPoint;

    private void Start()
    {
        instance = this;

//        camPos = FirstPersonCamera.transform.position;
//        camPos = new Vector3(camPos.x, camPos.y + 100, camPos.z);
        //if(SceneManager.GetActiveScene().buildIndex == 2)
         //   vCanvas = Instantiate(volumatricCanvas, camPos,Quaternion.identity);
    }
    
    // Update is called once per frame
    void Update()
    {
        Session.GetTrackables<DetectedPlane>(m_AllPlanes);

        if (m_AllPlanes.Count > 0)
        {
            camPos = m_AllPlanes[0].CenterPose.position;
            //m_AllPlanes[0].ExtentZ
            if(SceneManager.GetActiveScene().buildIndex == 2)
                vCanvas.transform.position = new Vector3(camPos.x,camPos.y+1,camPos.z+8);
            //vCanvas.transform.Rotate(0, k_ModelRotation, 0, Space.Self);
        }
        // If the player has not touched the screen, we are done with this update.
        Touch touch;

        CalculateMarker(); // Continously calculates the placement marker Pos

       
        touch = Input.GetTouch(0);
            
        if(Input.touchCount<0 || touch.phase != TouchPhase.Began)
            return;

        if(IsPointerOverUIObject(touch)) return;
        
        if (augmentedObject.transform.tag == "Map"||augmentedObject.GetComponent<IManager>().DependencyManager(hit.Pose.position))
        {
            var andyObject = Instantiate(augmentedObject, hit.Pose.position, hit.Pose.rotation);

            // Compensate for the hitPose rotation facing away from the raycast (i.e. camera).
            andyObject.transform.Rotate(0, k_ModelRotation, 0, Space.Self);

            // Create an anchor to allow ARCore to track the hitpoint as understanding of the physical
            // world evolves.
            var anchor = hit.Trackable.CreateAnchor(hit.Pose);

            // Make Andy model a child of the anchor.
            andyObject.transform.parent = anchor.transform;
        }

        else
        {
            augmentedObject.GetComponent<IManager>().ShowError(hit.Pose.position);
        }
        

    }

    private bool IsPointerOverUIObject(Touch touch)
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(touch.position.x, touch.position.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    private bool DidCollideWithObj(Touch t)
    {
        RaycastHit hit;
        Vector3 pos = Camera.main.ScreenToWorldPoint(t.position);
                
        if (Physics.Raycast(pos, Camera.main.transform.forward, out hit))
        {
            Debug.Log(hit.transform.name);
            return true;
        }

        return false;
    }
    
    void CalculateMarker()
    {
        TrackableHitFlags flags = TrackableHitFlags.PlaneWithinBounds | TrackableHitFlags.PlaneWithinPolygon;

        // The idea is to throw a ray from the center of the screen.
        // So take the center of screen in screen point co ordinate.
        Vector3 origin = FirstPersonCamera.ViewportToScreenPoint(new Vector3(0.5f, 0.5f, 0));

        //if the ray thrown from the center of the screen hit any trackables
        if(Frame.Raycast(origin.x,origin.y,flags,out hit))
        {
            markerPoint.transform.eulerAngles = new Vector3(90,0,0);
            
            //change markerpoint position to the hit
            markerPoint.transform.position = hit.Pose.position;
            
            //Taking the center point object of the marker point. This is not important in this moment.
            //But might come handy when we do some animation like "Apple Measure" app
            GameObject markerPointObj = markerPoint.transform.GetChild(0).gameObject;
            
            if(augmentedObject.transform.tag == "Map") return;

            if (!augmentedObject.GetComponent<IManager>().DependencyManager(hit.Pose.position))
            {
                markerPoint.GetComponent<SpriteRenderer>().color = Color.red;
            }
            else
            {
                markerPoint.GetComponent<SpriteRenderer>().color = Color.white;
            }
            
        }
        
        
    }
    
}
