using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractionManager : MonoBehaviour
{
    private Touch[] touch;

    private float timer;

    private bool didVibrate = false;

    private RaycastHit hit;

    public GameObject removeButton;
    // Start is called before the first frame update
    void Start()
    {
        //Handheld.Vibrate();
    }

    // Update is called once per frame
    void Update()
    {
        if (LongPress())
        {
            //Handheld.Vibrate();
            Handheld.Vibrate();
            
        }
    }

    bool LongPress()
    {
        touch = Input.touches;
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch t = touch[i];
            if (t.deltaTime > 0.2f)
            {
                Vector3 pos = Camera.main.ScreenToWorldPoint(t.position);
                
                if (Physics.Raycast(pos, Camera.main.transform.forward, out hit))
                {
                    Debug.Log(hit.transform.name);
                    Vector3 btnpos = hit.transform.position -
                                  hit.transform.gameObject.GetComponent<BoxCollider>().bounds.center;
                    GameObject btn = Instantiate(removeButton, btnpos, Quaternion.identity);
                    btn.GetComponentInChildren<Button>().onClick.AddListener((() =>
                    {
                        Debug.Log("Vibs" + t.position + " "+ pos);
                        Destroy(hit.transform.parent.gameObject);
                        Destroy(btn);
                    }));
                    return true;
                }    

            } 
            if (t.phase == TouchPhase.Ended)
            {
                //didVibrate = false;
                return false;
            }
        }
        return false;
    }
    
    
}
