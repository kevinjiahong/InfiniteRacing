using UnityEngine;
using System.Collections;

public class colliderTrig : MonoBehaviour {
    private GameObject terrainGen;
    private GameObject AICar;
    private bool triggered = false;
    private bool finishedInit = false;
	// Use this for initialization
	void Start () {
        terrainGen = GameObject.Find("TerrainGenerator");
        AICar = GameObject.Find("Aston");
	}

    void OnTriggerEnter(Collider coll)
    {
        GameObject component = null;
        // the car hits the trigger(enter the start point)
        if (!triggered && finishedInit)
        {
            triggered = true;
            if (coll.name.ToString().Equals("Austin_Body") ||
                coll.name.ToString().Equals("CopCar"))
            {
                Debug.Log("create " + triggered.ToString());
                terrainGen.SendMessage("loadNextTerrain", SendMessageOptions.DontRequireReceiver);

                component = transform.parent.gameObject;
                AICar.SendMessage("updateComponent", component, SendMessageOptions.DontRequireReceiver);
            }
        }        
    }

    void OnTriggerExit(Collider coll)
    {
        if (triggered)
        {
            Debug.Log("left");
            triggered = false;
        }
    }

    void SetFinishInit()
    {
        if (finishedInit == false)
        {
            finishedInit = true;
        }
    }
	// Update is called once per frame
	void Update () {
       
       
       
	}


}
