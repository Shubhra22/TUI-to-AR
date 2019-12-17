using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NavigationTab : MonoBehaviour,IDeselectHandler
{
    public GameObject content;

    private Button navTab;

    public Color onselectedColor;
    public Color ondeselectedColor;

    public bool selected;
    // Start is called before the first frame update
    void Start()
    {

        navTab = GetComponent<Button>();
        navTab.onClick.AddListener(ActiveContent);
        if (selected)
        {
            GetComponent<Image>().color = onselectedColor;
            transform.GetChild(0).GetComponent<Text>().color = Color.white;
            
        }
        else
        {
            GetComponent<Image>().color = ondeselectedColor;
            transform.GetChild(0).GetComponent<Text>().color = Color.black;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ActiveContent()
    {
        content.SetActive(true);
        GetComponent<Image>().color = onselectedColor;
        transform.GetChild(0).GetComponent<Text>().color = Color.white;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        content.SetActive(false);
        GetComponent<Image>().color = ondeselectedColor;
        transform.GetChild(0).GetComponent<Text>().color = Color.black;

    }
}
