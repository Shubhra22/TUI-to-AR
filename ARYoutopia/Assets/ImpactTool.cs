using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ImpactTool : MonoBehaviour
{
    private Button navTab;
    public GameObject obj;
    public GameObject error;

    public Color onselectedColor;
    public Color ondeselectedColor;

    public Sprite cross;
    public Sprite impact;

    public bool selected;

    // Start is called before the first frame update
    void Start()
    {

        navTab = GetComponent<Button>();
        navTab.onClick.AddListener(ActiveContent);
    }

    void ActiveContent()
    {
        if (selected == false)
        {
            obj.SetActive(!obj.activeSelf);
            selected = true;
            
        }

        else
        {
            error.SetActive(true);
        }

        if (!obj.activeSelf)
        {
            GetComponent<Image>().sprite = cross;
        }

        else
        {
            GetComponent<Image>().sprite = impact;
        }
          
    }

    public void SetSelected(bool val)
    {
        selected = val;
    }
    
    
}
