using System.Collections.ObjectModel;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;
using OpenCVForUnity;
using System.Linq;
using UnityEngine.UI;
using Random = System.Random;
using Rect = OpenCVForUnity.Rect;

public class Binarization : MonoBehaviour
{
    //Texture2D imgTexture;
    public double resizeX;
    public double resizeY;

    public bool originalSize;

    Mat inputImg;
    Mat rgbaImg;
    Mat greyImg;
    Mat output;
    List<Character> characters = new List<Character>();
    List<char> names = new List<char>();

    Dictionary<int, Color> pos_colorDictionary = new Dictionary<int, Color>();
    Dictionary<int, char> id_nameDictionary = new Dictionary<int, char>();

    private bool changed;

    private float[,,,] inputImgMatrix = new float[1, 50, 50, 1];

    public static Binarization instance;
    string path;
    Mat newImage;

    List<Character> oldCharaters = new List<Character>();
    List<MatOfPoint> colorContour = new List<MatOfPoint>();
#if UNITY_EDITOR
    public string testImageName = "h.jpg";

#endif

    private Random rng;

    private void Awake()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        instance = this;
#if UNITY_EDITOR
        path = "/Users/shubhrasarker/Desktop/test/RealTimeTest/";
#elif UNITY_ANDROID
                path = Application.persistentDataPath + "/Output/";
        #endif

        rng = new Random(123456);
    }

    private struct Character
    {
        public char nameOfChar;
        public double xPos;
    }


    //public RawImage camTex;
    public Texture2D camTex = null;

    private double ratioX;

    // Start is called before the first frame update
    private void Start()
    {
    }

    public float alpha = 1.5f;
    public float beta = 50;

    public void Segment(Texture2D imgTexture)
    {
        
    }
    
    public void Segment(Texture2D imgTexture, float threshold)
    {
        ClearList(); // Each time clear the list before adding the new letters on it.
        ArduinoLetterController.Instance.ReplaceEachLetterWithBlank();

        // Initialize Mats
        Mat srcGray = new Mat();
        Mat src;

#if UNITY_EDITOR
        src = Imgcodecs.imread("/Users/shubhrasarker/Desktop/test/" + testImageName);

#elif UNITY_ANDROID
        src=new Mat(imgTexture.height, imgTexture.width, CvType.CV_8UC4);        
        Utils.texture2DToMat(imgTexture,src);
        
#endif
         Mat dest = new Mat(src.rows(),src.cols(),src.type());
         src.convertTo(dest,-1,alpha,beta);

        //

        // De-noise input img
        Imgproc.cvtColor(dest, srcGray, Imgproc.COLOR_BGR2GRAY);

        Mat tempOtsu = new Mat();
        Imgproc.threshold(srcGray, tempOtsu, 3, 255, Imgproc.THRESH_OTSU);
        Imgcodecs.imwrite(path + "/" + "Otsu" + ".jpg", tempOtsu);

        if (containsHandorWhite(tempOtsu))
        {
            ArduinoLetterController.Instance.ReplaceEachLetterWithBlank();
            FindObjectOfType<UserInputRouter>().ClearAnimations();
            Debug.Log("Hand");
            return;
        }

        if (!containsHandorWhite(tempOtsu))
        {
            CNN(tempOtsu);
        }
    }

    void CNN(Mat inputImg)
    {
        int colorIndex = 0;
        Mat resizedImg = new Mat();
        Size size = new Size(resizeX, resizeY);
        if (camTex == null)
        {
            camTex = new Texture2D((int) resizeX, (int) resizeY, TextureFormat.RGBA32, false);
        }

        Imgproc.resize(inputImg, resizedImg, size);

        Mat cannyOutput = new Mat();
        Imgproc.Canny(resizedImg, cannyOutput, 100, 100 * 2);

        List<MatOfPoint> contours = new List<MatOfPoint>();

        Mat hierarchy = new Mat();
        Imgproc.findContours(cannyOutput, contours, hierarchy, Imgproc.RETR_TREE, Imgproc.CHAIN_APPROX_SIMPLE);
        Scalar color = new Scalar(220, 20, 60);
        Mat drawing = Mat.zeros(cannyOutput.size(), CvType.CV_8UC3);

        for (int i = 0; i < contours.Count; i++)
        {
            if (hierarchy.get(0, i)[3] == -1)
            {
                Rect rect = Imgproc.boundingRect(contours[i]);
                if (rect.height > 20)
                {
                    Mat ROI = resizedImg.submat(rect.y, rect.y + rect.height, rect.x, rect.x + rect.width);
                    Imgproc.resize(ROI, ROI, new Size(40, 40));

                    int top, bottom, left, right;
                    int borderType = Core.BORDER_CONSTANT;
                    top = (int) (0.125 * ROI.rows());
                    bottom = top;
                    left = (int) (0.125 * ROI.cols());
                    right = left;
                    Scalar value = new Scalar(255, 255, 255);
                    Core.copyMakeBorder(ROI, ROI, top, bottom, left, right, borderType, value);


                    for (int m = 0; m < ROI.rows() * ROI.cols(); m++)
                    {
                        double[] temp = ROI.get(49 - m / 50, m % 50);
                        inputImgMatrix[0, 49 - m / 50, m % 50, 0] = (float) temp[0];
                        //Debug.Log("The value of " + m + "th pixel is" + temp[0]);
                    }

                    char answer = Classifier.instance.Evaluate(inputImgMatrix);

                    Character answerChar = new Character() {nameOfChar = answer, xPos = rect.x};

                    characters.Add(answerChar);

                    if (!id_nameDictionary.ContainsKey(i))
                    {
                        id_nameDictionary.Add(i, answer);
                    }

                    id_nameDictionary[i] = answer;
                }


                //Imgproc.drawContours(drawing, contours, i, color, Core.FILLED, Imgproc.LINE_4, hierarchy, 0, new Point()); 
            }
        }
        //Imgproc.blur(drawing,drawing,new Size(3,3));

        //Imgcodecs.imwrite(path + "/" + "Otsu" + ".jpg", drawing);
        List<Character> sortedList = characters.OrderBy(o => o.xPos).ToList();
        names.Clear();
        //for(int i = 0;i<sortedList.Count;i++)
        foreach (var c in sortedList)
        {
            //char c = sortedList[i].nameOfChar;
            ArduinoLetterController.Instance.ReceiveNewUserInputLetter(c.nameOfChar, sortedList.IndexOf(c));
            names.Add(c.nameOfChar);
        }

        for (int i = 0; i < contours.Count; i++)
        {
            if (hierarchy.get(0, i)[0] == -1 && hierarchy.get(0, i)[1] == -1 && hierarchy.get(0, i)[2] == -1)
            {
                color = new Scalar(244, 22, 10);
                Imgproc.drawContours(drawing, contours, i, color, Core.FILLED, Imgproc.LINE_8, hierarchy, 0,
                    new Point());
                Imgcodecs.imwrite(path + "/" + "Drawing" + ".jpg", drawing);
                continue;
            }

            if (hierarchy.get(0, i)[3] == -1)
            {
                colorContour.Add(contours[i]);
                if (pos_colorDictionary.ContainsKey(colorIndex))
                {
                    Color c = pos_colorDictionary[colorIndex];
                    color = new Scalar(c.r * 256, c.g * 256, c.b * 256);
                    Imgproc.drawContours(drawing, contours, i, color, Core.FILLED, Imgproc.LINE_4, hierarchy, 0,
                        new Point());
                    colorIndex++;
                }
            }
        }

        Utils.matToTexture2D(drawing, camTex);
        
    }

    private void Init(Texture2D imgTexture)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        inputImg = new Mat(imgTexture.height, imgTexture.width, CvType.CV_8UC4);
        Utils.textureToMat(imgTexture, inputImg);
#endif

#if UNITY_EDITOR
        //inputImg = Imgcodecs.imread("/Users/shubhrasarker/Desktop/test/" + testImageName);
        inputImg = new Mat(imgTexture.height, imgTexture.width, CvType.CV_8UC4); //
        Utils.textureToMat(imgTexture, inputImg);
#endif
        rgbaImg = new Mat();
        greyImg = new Mat();
        output = new Mat();

        if (originalSize)
        {
            resizeX = inputImg.width();
            resizeY = inputImg.height();
        }

        Size size = new Size(resizeX, resizeY);
        Imgproc.resize(inputImg, rgbaImg, size);
    }

    private void OtsuFilter(Mat imageToFilter)
    {
        #region Trial&Error

        //Mat tempOut = new Mat();
        //Imgproc.adaptiveThreshold(greyImg, output, 255, Imgproc.ADAPTIVE_THRESH_MEAN_C, Imgproc.THRESH_BINARY, 5, 10);
        //Imgproc.threshold(greyImg, output, 0, 255, Imgproc.THRESH_OTSU);
        //Imgproc.threshold(newImage, output, 0, 255, Imgproc.THRESH_OTSU);

        #endregion

        Imgproc.threshold(imageToFilter, output, 0, 255, Imgproc.THRESH_OTSU);
    }

    private void CannyEdge()
    {
        Mat dest = new Mat();
        Mat cropped = null;
        Imgproc.Canny(output, dest, 300, 500, 5, false);
        List<MatOfPoint> contours = new List<MatOfPoint>();
        Imgproc.findContours(dest, contours, new Mat(), Imgproc.RETR_EXTERNAL, Imgproc.CHAIN_APPROX_TC89_KCOS,
            new Point(0, 0));

        if (characters.Count > 0)
            oldCharaters = characters.ToList();

        ClearList(); // Each time clear the list before adding the new letters on it.
        ArduinoLetterController.Instance.ReplaceEachLetterWithBlank();

        var cList = contours.Find(x => Imgproc.contourArea(x) > 15);
//        print(cList.toList().Count);

//        if (cList.toList().Count > 6)
//            return;

        for (var i = 0; i < contours.Count; i++)
        {
            if (Imgproc.contourArea(contours[i]) > 15)
            {
                OpenCVForUnity.Rect rect = Imgproc.boundingRect(contours[i]);

                if ((rect.height > 20) && (rect.width > 5))
                {
                    //Debug.Log("ContourPosition is X: "+ rect.x + " Y: "+rect.y);
                    string newImages = "Image" + i;

                    cropped = new Mat(output, rect);

                    Size size = new Size(50, 50);
                    //Imgproc.resize(cropped, cropped, size);

                    Resize(cropped, 40);

                    #region Write Each Cropped Image In Computer

                    //Imgcodecs.imwrite(path + "/" + newImages + ".jpg", cropped);

                    #endregion

                    for (int m = 0; m < cropped.rows() * cropped.cols(); m++)
                    {
                        double[] temp = cropped.get(49 - m / 50, m % 50);
                        inputImgMatrix[0, 49 - m / 50, m % 50, 0] = (float) temp[0];
                        //Debug.Log("The value of " + m + "th pixel is" + temp[0]);
                    }

                    char answer = Classifier.instance.Evaluate(inputImgMatrix);

                    Character answerChar = new Character() {nameOfChar = answer, xPos = rect.x};

                    //if (!characters.Contains(answerChar))
                    //{
                    characters.Add(answerChar);
                    //}
                    //Debug.Log(answer.ToString());
                }
            }
        }

        //ShowCharacter.instance.SetOutputText();
        List<Character> sortedList = characters.OrderBy(o => o.xPos).ToList();

        //if (!sortedList.All(oldCharaters.Contains))
        //{
        foreach (var item in sortedList)
        {
            //ShowCharacter.instance.SetOutputText(item.nameOfChar);
            ArduinoLetterController.Instance.ReceiveNewUserInputLetter(item.nameOfChar, sortedList.IndexOf(item));
        }

        // }
        //dest.convertTo(output, CvType.CV_8U);
    }

    private void Resize(Mat oldPic, float desiredSize)
    {
        Size oldSize = oldPic.size();

        float ratio = desiredSize / Mathf.Max((float) oldSize.width, (float) oldSize.height);

        Size newSize = new Size()
        {
            width = oldSize.width * ratio,
            height = oldSize.height * ratio,
        };

        Imgproc.resize(oldPic, oldPic, newSize);

        int deltaW = 50 - (int) newSize.width;
        int deltaH = 50 - (int) newSize.height;

        int up, down, left, right;

        if (deltaH % 2 == 0) up = down = deltaH / 2;
        else
        {
            up = deltaH / 2;
            down = deltaH - up;
        }

        if (deltaW % 2 == 0) left = right = deltaW / 2;
        else
        {
            left = deltaW / 2;
            right = deltaW - left;
        }

        Core.copyMakeBorder(oldPic, oldPic, up, down, left, right, Core.BORDER_ISOLATED, Scalar.all(255));
    }

    private byte saturate(float val)
    {
        int iVal = (int) Mathf.Round(val);
        iVal = iVal > 255 ? 255 : (iVal < 0 ? 0 : iVal);
        return (byte) iVal;
    }

    void Luminance(Mat image, float alpha, float beta)
    {
        newImage = Mat.zeros(image.size(), image.type());
        byte[] imageData = new byte[(int) (image.total() * image.channels())];
        image.get(0, 0, imageData);
        byte[] newImageData = new byte[(int) (newImage.total() * newImage.channels())];
        for (int y = 0; y < image.rows(); y++)
        {
            for (int x = 0; x < image.cols(); x++)
            {
                for (int c = 0; c < image.channels(); c++)
                {
                    float pixelValue = imageData[(y * image.cols() + x) * image.channels() + c];
                    pixelValue = pixelValue < 0 ? pixelValue + 256 : pixelValue;
                    newImageData[(y * image.cols() + x) * image.channels() + c]
                        = saturate(alpha * pixelValue + beta);
                }
            }
        }

        newImage.put(0, 0, newImageData);
    }

    bool containsHandorWhite(Mat image)
    {
        OpenCVForUnity.Rect[] roi = new OpenCVForUnity.Rect[4];

        Mat tempOut = new Mat();

        Imgproc.resize(image, tempOut, new Size(resizeX, resizeX));

        double totalPixel = tempOut.cols() * tempOut.rows();
        double whitePixels = Core.countNonZero(tempOut);//Core.sumElems(tempOut).val[0] / 256;
        double blackPixels = totalPixel - whitePixels;
        print("White Pixels "+ whitePixels+ " blackPixels "+blackPixels + "Total"+ totalPixel);
        if (blackPixels < 100)
        {
            
            return true;
        }
        
        return blackPixels > 3500;
        //
        //
    }

    public void ClearList()
    {
        characters.Clear();
        names.Clear();
    }
}