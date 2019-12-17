using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCVForUnity;
using UnityEngine.Playables;
using UnityEngine.UI;
using Rect = OpenCVForUnity.Rect;
using Text = UnityEngine.UI.Text;

public class Region {
    private Rect bounding;
    private Point centroid;

    public Region(Rect bounding, Point centroid) {
        this.bounding = bounding;
        this.centroid = centroid;
    }

    public Rect getBounding() {
        return bounding;
    }

    public Point getCentroid() {
        return centroid;
    }
}


public class Painter : MonoBehaviour
{

    public static Painter instance;

    Color[] letterColour = new Color[6];
    
    [HideInInspector]
    public Texture2D outputTexture;

    public RawImage img;
    
    public string testImageName = "g.jpg";

    private int compressionfactor =3;
    
    
    private Region[] regions = null;
    private Mat labeled = null;
    
    private float[,,,] inputImgMatrix = new float[1, 50, 50, 1];

    
    private struct Character
    {
        public char nameOfChar;
        public double xPos;

        public Character(char nameOfChar, double xPos)
        {
            this.nameOfChar = nameOfChar;
            this.xPos = xPos;
            
        }
    }
    List<Character> characters = new List<Character>();
    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
        for (int i =0;i<letterColour.Length;i++)
        {
            letterColour[i] = Color.black;
        }
    }
    
    public float alpha = 1.5f;
    public float beta = 50;

    public void Segment(Mat inputStream)
    {
        if (outputTexture == null)
        {
            outputTexture = new Texture2D(inputStream.cols()/compressionfactor,inputStream.rows()/compressionfactor,TextureFormat.RGBA32,false);
        }


        
        Mat src = inputStream.clone();//Imgcodecs.imread("/Users/shubhrasarker/Desktop/test/" + testImageName);//
        src.convertTo(src,-1,alpha,beta);


        Imgproc.resize(src,src,new Size(inputStream.cols()/compressionfactor,inputStream.rows()/compressionfactor));
        
        Imgproc.cvtColor(src,src,Imgproc.COLOR_BGR2GRAY);
   
        Imgproc.GaussianBlur(src,src,new Size(7,7),0);               
      
        Mat imThresh = new Mat();
        Imgproc.threshold(src, imThresh, 127, 255, Imgproc.THRESH_BINARY_INV);

        Mat morphedImage = Morphology(imThresh);


        Mat rotatedImg = RotateImage(morphedImage, 1);//morphedImage.clone();//
        
        Mat invertedImg = new Mat();
        Core.bitwise_not(rotatedImg,invertedImg);


        createRegions(rotatedImg);
        
        
        Mat outputImg;

#region TestInPC
        
#if UNITY_INCLUDE_TESTS
        //outputImg = new Mat(new Size(src.cols(),src.rows()),CvType.CV_8UC3);//RotateImage(src, 1);
        //outputImg = RotateImage(outputImg, 1);
#endif 

#endregion
     
        outputImg = inputStream.clone();//new Mat(inputStream.rows(),inputStream.cols(),CvType.CV_8UC3);//
        Imgproc.resize(outputImg,outputImg,new Size(inputStream.cols()/compressionfactor,inputStream.rows()/compressionfactor));
        outputImg = RotateImage(outputImg, 1);

        
        for (int i = 0; i < regions.Length; i++)
        {
            
            char answer = CNN(getRegionImage(invertedImg, regions[i]),morphedImage);
            
            if (answer != ' ')
            {
                ArduinoLetterController.Instance.ChangeTheLetterOfASingleCell(i,' ',false);
                ArduinoLetterController.Instance.ReceiveNewUserInputLetter(answer,i);
            }
            
            //Imgcodecs.imwrite("/Users/shubhrasarker/Desktop/test/RealTimeTest/con"+i+".jpg",getRegionImage(invertedImg, regions[i]));
        
            //outputImg.setTo(new Scalar(60,200,255),getRegionMask(regions[i]));
            outputImg.setTo(colorToScalar(letterColour[i]), getRegionMask(regions[i]));
            
        }

        if (regions.Length < 6)
        {
            for (int i = regions.Length; i < 6; i++)
            {
                ArduinoLetterController.Instance.ChangeTheLetterOfASingleCell(i, ' ', false);
            }
        }

        //Imgcodecs.imwrite("/Users/shubhrasarker/Desktop/test/RealTimeTest/con"+"s"+".jpg",outputImg);
        outputImg = RotateImage(outputImg, 0);
        Imgproc.GaussianBlur(outputImg,outputImg,new Size(3,3),0);
        ShowOutput(outputImg);
        outputImg.release();
    }

    public void SetColor(int index,Color c)
    {
        letterColour[index] = c;
        //outputImg.setTo(colorToScalar(letterColour[index]), getRegionMask(regions[index]));
        //Debug.Log("The color in index"+index+ "is"+letterColour[index]);
    }
    
    Scalar colorToScalar(Color c)
    {
        return new Scalar(c.r*256,c.g*256,c.b*256);
    }

    char CNN(Mat inputImage, Mat binaryImg)
    {
        Mat rotateImg = RotateImage(inputImage, 0);

        
        Mat ROI = new Mat();
        
        Imgproc.resize(rotateImg, ROI, new Size(40, 40));
        
        
        int top, bottom, left, right;
        int borderType = Core.BORDER_CONSTANT;
        top = (int) (0.125 * ROI.rows());
        bottom = top;
        left = (int) (0.125 * ROI.cols());
        right = left;
        Scalar value = new Scalar(255, 255, 255);
        Core.copyMakeBorder(ROI, ROI, top, bottom, left, right, borderType, value);
        Imgcodecs.imwrite("/Users/shubhrasarker/Desktop/test/RealTimeTest/con"+"s"+".jpg",ROI);

        for (int m = 0; m < ROI.rows() * ROI.cols(); m++)
        {
            double[] temp = ROI.get(49 - m / 50, m % 50);
            inputImgMatrix[0, 49 - m / 50, m % 50, 0] = (float) temp[0];
            //Debug.Log("The value of " + m + "th pixel is" + temp[0]);
        }

        char answer = ' ';
        if (Time.frameCount % 20 == 0)
        {
            answer = Classifier.instance.Evaluate(inputImgMatrix);           
        }

        return answer;
    }

    void DisplayConnectedComponents(Mat connectedLabels,  Mat sourceImage,int ret)
    {
        
        Mat imLabels = connectedLabels.clone();
        print(ret);

        Mat mask = new Mat();
        Mat output = sourceImage.clone();//new Mat();//new Mat(sourceImage.rows(),sourceImage.cols(),CvType.CV_8UC3);
        Imgproc.resize(output,output,new Size(sourceImage.cols()/compressionfactor,sourceImage.rows()/compressionfactor));
        
        Mat rotatedOutput = RotateImage(output,1);

        for (int i = 1; i < ret; i++)
        {
            Core.inRange(imLabels,new Scalar(i),new Scalar(i), mask );
            rotatedOutput.setTo(new Scalar(50,100,200), mask);
        }

        output = RotateImage(rotatedOutput, 0);
        Imgproc.GaussianBlur(output,output,new Size(3,3),0.7f);
        ShowOutput(output);
    }

    void ShowOutput(Mat input)
    {
        //Imgcodecs.imwrite("/Users/shubhrasarker/Desktop/test/RealTimeTest/con1.jpg",input);
        Utils.matToTexture2D(input,outputTexture);
        img.texture = outputTexture;
    }

    Mat Morphology(Mat input)
    {
        int iterations = 3;
        int erosionSize = 2;

        Mat element =
            Imgproc.getStructuringElement(Imgproc.MORPH_ELLIPSE, new Size(2 * erosionSize + 1, 2 * erosionSize + 1));
        Mat imageMorphOpened = new Mat();
        Imgproc.morphologyEx(input, imageMorphOpened, Imgproc.MORPH_OPEN, element, new Point(-1,-1),iterations);

        return imageMorphOpened;
    }

    Mat RotateImage(Mat input, int angle)
    {
        Mat rotatedImg = input.clone();
        Core.transpose(rotatedImg,rotatedImg);
        Core.flip(rotatedImg,rotatedImg,angle);
        return rotatedImg;
    }
    
    private void createRegions(Mat image) {
        labeled = new Mat(image.size(), image.type());

        // Extract components
        Mat rectComponents = Mat.zeros(new Size(0, 0), 0);
        Mat centComponents = Mat.zeros(new Size(0, 0), 0);
        int ret = Imgproc.connectedComponentsWithStats(image, labeled, rectComponents, centComponents);

//        print(rectComponents.rows());
        // Collect regions info
        int[] rectangleInfo = new int[5];
        double[] centroidInfo = new double[2];
        regions = new Region[rectComponents.rows() - 1];

        for(int i = 1; i < rectComponents.rows(); i++) {

            // Extract bounding box
            rectComponents.row(i).get(0, 0, rectangleInfo);
            Rect rectangle = new Rect(rectangleInfo[0], rectangleInfo[1], rectangleInfo[2], rectangleInfo[3]);

            // Extract centroids
            centComponents.row(i).get(0, 0, centroidInfo);
            Point centroid = new Point(centroidInfo[0], centroidInfo[1]);

            regions[i - 1] = new Region(rectangle, centroid);
        }

        // Free memory
        rectComponents.release();
        centComponents.release();
    }
    
    public Mat getRegionMask(Region region)
    {
        int i = Array.IndexOf(regions, region);
        Mat mask = new Mat(labeled.size(), labeled.type());
        Scalar color = new Scalar(i + 1, i + 1, i + 1);
        Core.inRange(labeled, color, color, mask);
        return mask;
    }

    public Region[] getRegions() {
        return regions;
    }

    public Region getRegion(int index) {
        return regions[index];
    }

    /**
     * Call this method to release private Mat member
     */
    public void release() {
        labeled.release();
    }

    
    /**
    * Extract original image of the region using the region mask
    * @param image Original image
    * 
    */
    public Mat getRegionImage(Mat image, Region region) {
        return new Mat(image, region.getBounding());;
    }
    
    /**
    * Extract original image of the region using the region mask
    * @param image Original image
    * 
    */
    public Mat getRegionImageWithMask(Mat image, Region region) {
        Mat mask = getRegionMask(region);
        Mat result = Mat.zeros(image.size(), image.type());
        result.setTo(new Scalar(255,255,255));
        image.copyTo(result, mask);
        Mat boxImage = new Mat(result, region.getBounding());
        mask.release();
        result.release();
        return boxImage;
    }
    
    public double resizeX;
    public double resizeY;

    
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
    
    
}
