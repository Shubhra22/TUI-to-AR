using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TensorFlow;

public class Classifier : MonoBehaviour
{

    public TextAsset graphModel;                            // The trained TensorFlow graph
    float[,] recurrentTensor;
    float[,,,] inputImg;

    float confidence = 0;
    float sum = 0f;
    int index = 0;

    
    public static Classifier instance;
    private void Awake()
    {
        instance = this;
        
    }
    TFSession session =null;

    public char Evaluate(float[,,,] inputImage)
    {

        
        this.inputImg = inputImage;

        using (var graph = new TFGraph())
        {
            graph.Import(graphModel.bytes);

            // We declare and run a session with our graph.
            //if (session == null)
            //{
                session = new TFSession(graph);

             //}
            var runner = session.GetRunner();

            // Here I implicitly convert my inputTensor (array) to 
            // TensorFlow tensor. The TFTensor will take on the dimensions
            // of the array.
            TFTensor input = inputImg;

            // We tell the session to sub in our input tensor for our 
            // graph's placeholder tensor and fetch the predictions from the
            // output node.
            runner.AddInput(graph["x"][0], input);
            runner.Fetch(graph["y_pred"][0]);

            // We run the graph and store the probability of each result in 
            // recurrentTensor.
            recurrentTensor = runner.Run()[0].GetValue() as float[,];

            // We dispose of resources so our graph doesn't break down over
            // time. IMPORTANT if you will repeatedly call the graph.
            session.Dispose();
            graph.Dispose();
        }
        // get the option with the highest probability
        float highVal = 0;
        int highInd = -1;
        sum = 0f;

        // Debug.Log()
        for (int j = 0; j < 26; j++)
        {
            confidence = recurrentTensor[0, j];
            //String.Format("{0:0.00}", confidence*100);
            //confidenceMatrix.text += "Confidence[" + j + "] = " + String.Format("{0:0.00}", confidence * 100) + "% \n";
            if (highInd > -1)
            {
                if (recurrentTensor[0, j] > highVal)
                {
                    highVal = confidence;
                    highInd = j;
                }
            }
            else
            {
                highVal = confidence;
                highInd = j;
            }

            // debugging - sum should = 1 at the end
            sum += confidence;
        }

        // save the index of the highest value for later use!
        index = highInd;
        char output = (char)(index + 97);
        // Display the answer to the screen
        //        label.text = "Answer: " + output;
        return output;
    }
}
