using System.Collections;
using System.Collections.Generic;
using JoystickLab;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Generalise Messange box property So that we can use this to create a message box anytime with just one call.
public class MessageBox : SingleToneManager<MessageBox>
{
    public MessageBoxObject messageBoxObject;

    // Show a messagebox with a message and a button that just closes the messagebox  
    public void Show(string message)
    {
        messageBoxObject.Message.text = message;

        Instantiate(messageBoxObject, Vector3.zero, Quaternion.identity);

        messageBoxObject.gameObject.SetActive(true);
        
        Button okButton = messageBoxObject.OkButton;
        
        okButton.onClick.RemoveAllListeners();
        okButton.onClick.AddListener(Close);
    }
    
    // Show a messagebox with a message and an OK button that can be added to custom event
    public void Show(string message, UnityAction okEvent)
    {
        Instantiate(messageBoxObject, Vector3.zero, Quaternion.identity);
        messageBoxObject.Message.text = message;

        Button okButton = messageBoxObject.OkButton;
        
        okButton.onClick.RemoveAllListeners();
        okButton.onClick.AddListener(okEvent);        
        okButton.onClick.AddListener(Close);
    }
    
   
    // Closes a messagebox
    void Close()
    {
        messageBoxObject.gameObject.SetActive(false);
        //Destroy(messageBoxObject);
    }
}
