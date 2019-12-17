using UnityEngine;
using System.IO;
using UnityEngine.UI;
public class SyncCapture : MonoBehaviour
{
    string inputPath; //= Application.persistentDataPath + "/Input/";
    string outputPath; //= Application.persistentDataPath + "/Output/";

    public GameObject catObj;
    bool click = true;

    public RawImage camTex;
    
    private void Awake()
    {
        inputPath = Application.persistentDataPath + "/Input/";
        outputPath = Application.persistentDataPath + "/Output/";

        if (!Directory.Exists(inputPath))
        {
            Directory.CreateDirectory(inputPath);
        }

        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        var tempRT = RenderTexture.GetTemporary(source.width, source.height);
        Graphics.Blit(source, tempRT);

        var tempTex = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
        tempTex.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0, false);
        tempTex.Apply();

        
        
        if (Time.frameCount % 30 == 0 && click == true)
        {
            Binarization.instance.Segment(tempTex,100);
            camTex.texture = Binarization.instance.camTex;
            //File.WriteAllBytes(inputPath + "test.png", ImageConversion.EncodeToPNG(tempTex));
            //click = false;

        }

        Destroy(tempTex);
        RenderTexture.ReleaseTemporary(tempRT);

        Graphics.Blit(source, destination);
    }

    public void OnClickOk(Text output)
    {
        click = false;
        if(output.text == "pet")
        {
            catObj.SetActive(true);
        }

    }

    public void OnClickReset()
    {
        catObj.SetActive(false);
        click = true;
    }
}

