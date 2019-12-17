using System;
using System.Collections;
using System.Collections.Generic;
//using Graphs;
using UnityEngine;

public class IrrigationManager : MonoBehaviour,IManager,IComparer<Collider>
{
    public bool used;
    public LayerMask layerMask;

    public ErrorMessage error;

    //private Graph<IrrigationManager> irrigationGraph;
    private void Start()
    {
        //irrigationGraph = GraphManager.instance.irrigationGraph;
        ContentHandler(transform.position,10);
       // print(irrigationGraph.Find(this).Neighbors.Count);
    }

    public int Compare(Collider x, Collider y) 
    { 
        // Compare x and y in reverse order. 
        return (transform.position - x.transform.position).sqrMagnitude.
            CompareTo ((transform.position - x.transform.position).sqrMagnitude);
    }
    public bool DependencyManager(Vector3 position)
    {
        // look into the data table and find the required max distance (d)
        // distance from the river should be less then "d" unit
        float radius = 0.1f;
        float riverHealth = 100;
        Collider[] collisions = Physics.OverlapSphere(position, radius,layerMask);
        if (collisions.Length > 0 && riverHealth>0)
        {
            return true;
        }        
        return false;
    }

    public void ImpactManager()
    {
        // River water gets low     
        Debug.Log("IrrigationImpact");
        ContentHandler(transform.position,10);
    }

    public void ContentHandler(Vector3 position, float radius)
    {
        //irrigationGraph.AddNode(this);
        Collider[] cols = Physics.OverlapSphere(position, radius,LayerMask.GetMask("Irrigation"));
        if (cols.Length <= 1)
        {
            //irrigationGraph = new Graph<IrrigationManager>();
            //irrigationGraph.AddNode(this);            
        }
        else
        {
            Array.Sort(cols,Compare);
            //irrigationGraph.AddEdge(this, cols[1].GetComponent<IrrigationManager>());
        }
        
    }

    public void ShowError(Vector3 position)
    {
        GameObject errorObj = Instantiate(error.gameObject, position, error.transform.rotation);
        errorObj.GetComponent<ErrorMessage>().descriptionBox.text = "Irrigation can only be placed near a water source.";
        errorObj.GetComponent<ErrorMessage>().titleBox.text = "Wrong placement.";
        
        
    }
}
