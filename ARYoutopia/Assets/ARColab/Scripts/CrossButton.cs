using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class CrossButton : MonoBehaviour
{
    private Button button;

    private GameObject augmentedObj;
    // Start is called before the first frame update
    void Start()
    {
        button = GetComponent<Button>();
    }

    void OnClickRemove(GameObject obj)
    {
        Destroy(obj);
    }

    public void OnClickRemove()
    {
        button.onClick.AddListener(()=>OnClickRemove(augmentedObj));

    }
}
