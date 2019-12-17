using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowCharacter : MonoBehaviour
{
    public static ShowCharacter instance;

    [SerializeField]
    Text outputText;

    private void Awake()
    {
        instance = this;
    }

    public void SetOutputText(char inputText)
    {
        outputText.text += inputText + " ";
    }

    public void SetOutputText()
    {
        outputText.text = "";

    }

}
