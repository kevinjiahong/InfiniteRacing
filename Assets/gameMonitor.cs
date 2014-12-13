using UnityEngine;
using System.Collections;

public class gameMonitor : MonoBehaviour {
    public GameObject copCar;
    public GameObject playerCar;
    public GameObject guiPolice;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        float distance = (playerCar.transform.position - copCar.transform.position).magnitude;
      /*  if (Vector3.Dot(playerCar.transform.forward, copCar.transform.position) < 0)
        {
            //cop car is in front of the player's car
            Application.LoadLevel("lose");
        }*/
        if (distance > 100)
        {
            //reset the cop car
            copCar.transform.position = playerCar.transform.position
                                        - 5f * playerCar.transform.forward;
            GameObject tempComponent = playerCar.GetComponent<CarAI>().currentComponent;
            //regenerate path for AI car
            copCar.SendMessage("updateComponent", tempComponent, SendMessageOptions.DontRequireReceiver);
            
        }
        else
        {
            guiPolice.transform.position
                    = new Vector3(Mathf.Clamp(0.9f - 0.0015f * distance, 0.75f, 0.90f), 0.9f, 0);
          
        }
        

	}
}
