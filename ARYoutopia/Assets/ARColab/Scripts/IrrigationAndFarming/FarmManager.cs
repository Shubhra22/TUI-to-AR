using System;
using System.Collections.Generic;
using UnityEngine;


public class FarmManager : MonoBehaviour,IManager, IComparer<Collider>
{
    
    public int Compare(Collider x, Collider y) 
    { 
        // Compare x and y in reverse order. 
        return (transform.position - x.transform.position).sqrMagnitude.
            CompareTo ((transform.position - x.transform.position).sqrMagnitude);
    }
    
    public enum FarmType
    {
        Farm,
        Garden,
    }

    public FarmType typeOfFarm;

    private Vector3 debugPos;
    
    public ErrorMessage error;


    public bool DependencyManager(Vector3 position)
    {
        debugPos = position;
        float radius = 0.2f;
        int irrigationneeded;

        int irrigationCounter = 0;
           
        if (typeOfFarm == FarmType.Garden)
        {
               
            Collider[] colliders = Physics.OverlapSphere(position, radius,LayerMask.GetMask("Irrigation"));
            irrigationneeded = 1;// Load it from JSON
            irrigationCounter = 0;
            if (colliders.Length > 0)
            {
                print("Came"); 
                IrrigationManager irrigationManager = colliders[0].transform.GetComponent<IrrigationManager>();
                if (!irrigationManager.used)
                {
                    irrigationManager.used = true;
                    irrigationCounter++;
                    return true;
                }
            }
            
            return false;
        }
        
        else if (typeOfFarm == FarmType.Farm)
        {
            Collider[] colliders = Physics.OverlapSphere(position, radius,LayerMask.GetMask("Irrigation"));
            irrigationneeded = 1;
            if (colliders.Length >=2)
            {
                return true;
            }
        }

        return false;
    }

    public void ImpactManager()
    {
        
    }

    void OnDrawGizmosSelected()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 2.25f);
    }
    
    public void ShowError(Vector3 position)
    {
        GameObject errorObj = Instantiate(error.gameObject, position, error.transform.rotation);
        errorObj.GetComponent<ErrorMessage>().descriptionBox.text = "You need 3 Irrigation to create a farm";
        errorObj.GetComponent<ErrorMessage>().titleBox.text = "Insufficent resource";
        
        
    }
    
}
