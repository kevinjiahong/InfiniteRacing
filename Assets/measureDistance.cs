using UnityEngine;
using System.Collections;

public class measureDistance : MonoBehaviour {
    public GameObject copCar;
    public GameObject playerCar;
    public GameObject guiDistance;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        guiDistance.guiText.text = 
                        (playerCar.transform.position - copCar.transform.position).magnitude.ToString();
	}
}
