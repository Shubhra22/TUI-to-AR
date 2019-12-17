using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AuthorizedButton : MonoBehaviour
{
    bool isAuthorized;
    public Color activeColor;

    public Color inactiveColor;

    public bool IsAuthorized 
    {
        get { return isAuthorized; }
        set
        {
            isAuthorized = value;
            if (isAuthorized)
            {
                GetComponent<Image>().color = activeColor;
            }
            else if(!isAuthorized)
            {
                GetComponent<Image>().color = inactiveColor;
            }
        }
    }

}
