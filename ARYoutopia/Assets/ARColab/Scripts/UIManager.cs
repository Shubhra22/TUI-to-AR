using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

// Improve
public class UIManager : MonoBehaviourPunCallbacks,IPunObservable
{
    public GameObject mapPanel;
    public AuthorizedButton mapButton;
    public Image unAuthorizedMessage;

    private bool isActiveMapPanel;
    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            mapButton.IsAuthorized = true;
            
        }
        else
        {
            mapButton.IsAuthorized = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        mapPanel.SetActive(isActiveMapPanel);
    }

    public void LoadScene(int level)
    {
        SceneManager.LoadScene(level);
    }

    public void OnClickMapButton()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            isActiveMapPanel = true;
            //ShowUnAuthorizedMessage();
        }
        else
        {
            // Improve
            ShowUnAuthorizedMessage();
        }
    }

    public void OnCloseMapButton()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            isActiveMapPanel = false;
        }
        else
        {
            //Improve
            ShowUnAuthorizedMessage();
        }
        //mapPanel.SetActive(false);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(isActiveMapPanel);
        }
        else
        {
            isActiveMapPanel = (bool)stream.ReceiveNext();
        }
    }

    //Improve
    void ShowUnAuthorizedMessage()
    {
        Sequence sq = DOTween.Sequence();
        sq.Append(unAuthorizedMessage.rectTransform.DOBlendableMoveBy(new Vector3(0,-300,0), 0.5f));
        sq.AppendInterval(2);
       // sq.Append(unAuthorizedMessage.DOFade(1, 1.5f));
        sq.Append(unAuthorizedMessage.rectTransform.DOBlendableMoveBy(new Vector3(0,300,0), 0.5f));
       // sq.Append(unAuthorizedMessage.DOFade(0, 1.5f));
    }
        
}
