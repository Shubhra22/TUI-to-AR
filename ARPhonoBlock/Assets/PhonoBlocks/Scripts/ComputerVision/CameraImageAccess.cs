using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using OpenCVForUnity;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Vuforia;
using Image = Vuforia.Image;
using Random = System.Random;
using Rect = UnityEngine.Rect;

public class CameraImageAccess : MonoBehaviour
{
 
    #region PRIVATE_MEMBERS

    public RawImage img;
    
    
    private Image.PIXEL_FORMAT mPixelFormat = Image.PIXEL_FORMAT.UNKNOWN_FORMAT;
 
    private bool mAccessCameraImage = true;
    private bool mFormatRegistered = false;

    private Mat inputMat;
    private Texture2D outputTexture;
 
    #endregion // PRIVATE_MEMBERS
 
    #region MONOBEHAVIOUR_METHODS
 
    void Start()
    {
 
        #if UNITY_EDITOR
        mPixelFormat = Image.PIXEL_FORMAT.GRAYSCALE; // Need Grayscale for Editor
        #else
        mPixelFormat = Image.PIXEL_FORMAT.RGB888; // Use RGB888 for mobile
        #endif
 
        // Register Vuforia life-cycle callbacks:
        VuforiaARController.Instance.RegisterVuforiaStartedCallback(OnVuforiaStarted);
        VuforiaARController.Instance.RegisterTrackablesUpdatedCallback(OnTrackablesUpdated);
        VuforiaARController.Instance.RegisterOnPauseCallback(OnPause);
        
    }
 
    #endregion // MONOBEHAVIOUR_METHODS
 
    #region PRIVATE_METHODS
 
    void OnVuforiaStarted()
    {
    #if UNITY_EDITOR
           // img.uvRect = new Rect(0,1f,1f,1f);
                        
    #endif  
        // Try register camera image format
        if (CameraDevice.Instance.SetFrameFormat(mPixelFormat, true))
        {
            Debug.Log("Successfully registered pixel format " + mPixelFormat.ToString());
            mFormatRegistered = true;
        }
        else
        {
            Debug.LogError(
                "\nFailed to register pixel format: " + mPixelFormat.ToString() +
                "\nThe format may be unsupported by your device." +
                "\nConsider using a different pixel format.\n");
 
            mFormatRegistered = false;
        }
 
    }
    
    /// <summary>
    /// Called each time the Vuforia state is updated
    /// </summary>
    void OnTrackablesUpdated()
    {

        if (mFormatRegistered)
        {
            if (mAccessCameraImage)
            {
                Vuforia.Image image = CameraDevice.Instance.GetCameraImage(mPixelFormat);

                if (image != null)
                {
                    // Taken from https://github.com/EnoxSoftware/VuforiaWithOpenCVForUnityExample/blob/master/Assets/VuforiaWithOpenCVForUnityExample/CameraImageToMatExample.cs
                    if (inputMat == null)
                    {
                        if (mPixelFormat == Image.PIXEL_FORMAT.GRAYSCALE) 
                        {
                           inputMat = new Mat (image.Height, image.Width, CvType.CV_8UC1);
                        } 
                        else if (mPixelFormat == Image.PIXEL_FORMAT.RGB888) 
                        {
                            inputMat = new Mat (image.Height, image.Width, CvType.CV_8UC3);
                        }
                    }

                    inputMat.put(0, 0, image.Pixels);

                    if (outputTexture == null)
                    {
                        outputTexture = new Texture2D(inputMat.cols(),inputMat.rows(),TextureFormat.RGBA32,false);
                    }
                    
                    Utils.matToTexture2D(inputMat,outputTexture);
                    //img.texture = outputTexture;

                    if (SceneManager.GetActiveScene().name == "Activity")
                    {
                        Painter.instance.Segment(inputMat);
                    }
                    
                    //img.texture = Painter.instance.outputTexture;

//                    if (Time.frameCount % 30 ==0)
//                    {                        
//                        Profiler.BeginSample("My Sample");
//                        Binarization.instance.Segment(outputTexture,100);                    
//                        Profiler.EndSample();
//                        img.texture = Binarization.instance.camTex;
//                    }




                }
            }
        }


    }
 
    /// <summary>
    /// Called when app is paused / resumed
    /// </summary>
    void OnPause(bool paused)
    {
        if (paused)
        {
            Debug.Log("App was paused");
            UnregisterFormat();
        }
        else
        {
            Debug.Log("App was resumed");
            RegisterFormat();
        }
    }
 
    /// <summary>
    /// Register the camera pixel format
    /// </summary>
    void RegisterFormat()
    {
        if (CameraDevice.Instance.SetFrameFormat(mPixelFormat, true))
        {
            Debug.Log("Successfully registered camera pixel format " + mPixelFormat.ToString());
            mFormatRegistered = true;
        }
        else
        {
            Debug.LogError("Failed to register camera pixel format " + mPixelFormat.ToString());
            mFormatRegistered = false;
        }
    }
 
    /// <summary>
    /// Unregister the camera pixel format (e.g. call this when app is paused)
    /// </summary>
    void UnregisterFormat()
    {
        Debug.Log("Unregistering camera pixel format " + mPixelFormat.ToString());
        CameraDevice.Instance.SetFrameFormat(mPixelFormat, false);
        mFormatRegistered = false;
    }
 
    #endregion //PRIVATE_METHODS
}