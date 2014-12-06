using UnityEngine;
using System.Collections;

public class GenerateTerrain : MonoBehaviour {
    const int prefabContain = 10;
    const int loadArrayCapacity = 30;
    //space to save preloaded components
    public GameObject[] loadedRoadComponent = new GameObject[30];

    public GameObject[] roadComponentPrefab;
    public GUIText guiLoading;
    private GameObject raceCar;
    private GameObject copCar;
    private int prefabCount = 0;
    private Queue roadComponentQueue;
    private GameObject lastComponent;
    private GameObject currentComponent;
    private int currentGetLoadedIndex = 0;  //select the next one from the loadedroadcomponent
	// Use this for initialization
	void Start () {
        //load the prefabs from resource files
        roadComponentPrefab = new GameObject[prefabContain];
        roadComponentPrefab[prefabCount] = Resources.Load<GameObject>("sroad");
        prefabCount++;
        roadComponentPrefab[prefabCount] = Resources.Load<GameObject>("leftturn");
        prefabCount++;
        roadComponentPrefab[prefabCount] = Resources.Load<GameObject>("rightturn");
        prefabCount++;
        //pre-initiate multiple components
        for (int i = 0; i < loadArrayCapacity; ++i)
        {
        //    loadedRoadComponent[i] = (GameObject)Instantiate(roadComponentPrefab[Random.Range(0, 2)]
         //                                  , new Vector3(-10000f, -10000f, -10000f)
          //                                  , transform.rotation);
            loadedRoadComponent[i] = (GameObject)Instantiate(roadComponentPrefab[Random.Range(0, prefabCount)]
                                            , new Vector3(-10000f, -10000f, -10000f)
                                            , transform.rotation);
            guiLoading.text = "Loading" + i.ToString() + "/" + loadArrayCapacity.ToString();

        }
        guiLoading.active = false;

            //initialize the queue
        roadComponentQueue = new Queue();
        //create the first object
        GameObject tempGameObject = loadedRoadComponent[currentGetLoadedIndex];
        currentGetLoadedIndex++;    //load the next one at the next time
        tempGameObject.transform.position = new Vector3(0f, 0f, 0f);

        lastComponent = tempGameObject;
        currentComponent = tempGameObject;
        roadComponentQueue.Enqueue(tempGameObject);

        loadNextTerrain();  //load the very next terrain because the start of first component will not be triggered
        loadNextTerrain();
       
        
        raceCar = GameObject.Find("Aston");
        //get the path of the first component
        raceCar.SendMessage("updateComponent", tempGameObject, SendMessageOptions.DontRequireReceiver);

        copCar = GameObject.Find("CopCar");
        copCar.SendMessage("updateComponent", tempGameObject, SendMessageOptions.DontRequireReceiver);
	}

    void resortPreload()
    {
        //rerandom the loadedRoadComponents
        Debug.Log("resort");
        int randNum = 0;
        GameObject temp;
        for (int i = 0; i < loadArrayCapacity; ++i)
        {
            while (randNum != i)
            {
                randNum = Random.Range(0, loadArrayCapacity);
            }
            temp = loadedRoadComponent[i];
            loadedRoadComponent[i] = loadedRoadComponent[randNum];
            loadedRoadComponent[randNum] = temp;            
        }
    }
    void loadNextTerrain()
    {

        //int randomTerrainPrefab = 0;
        
        currentGetLoadedIndex++;

        if (currentGetLoadedIndex >= loadArrayCapacity)
        {
            resortPreload();    //random them
            currentGetLoadedIndex = 0;
        }

        currentComponent = loadedRoadComponent[currentGetLoadedIndex];  //current component is the new comp
        //connect the current component to last component
        //find the end of last componenet
        Transform endPoint = null;
        Transform startPoint = null;
        Vector3 lastEndGlobal = new Vector3();
        Vector3 currentStartGlobal = new Vector3();
        Vector3 deltaStartEndPoint = new Vector3();
        Quaternion currentStartRotation = new Quaternion();
        Quaternion lastEndRotation = new Quaternion();
        Vector3 deltaRotation;
        for (int i = 0; i < lastComponent.transform.childCount; ++i)
        {
            if (lastComponent.transform.GetChild(i).name.Equals("Endpoint"))
            {
                endPoint = lastComponent.transform.GetChild(i);              
                break;
            }
        }
        //get the start of current component
        for (int i = 0; i < currentComponent.transform.childCount; ++i)
        {
            if (currentComponent.transform.GetChild(i).name.Equals("Startpoint"))
            {
                startPoint = currentComponent.transform.GetChild(i);               
                break;
            }
        }

        lastEndRotation = endPoint.rotation;
        currentStartRotation = startPoint.rotation;
        //rotate to connect correctly
        deltaRotation = lastEndRotation.eulerAngles - currentStartRotation.eulerAngles;
        currentComponent.transform.eulerAngles = deltaRotation;

        lastEndGlobal = endPoint.position;
        currentStartGlobal = startPoint.position;
        //get the move delta       
        deltaStartEndPoint = lastEndGlobal - currentStartGlobal;
    
        //move to the new place
        currentComponent.transform.position += deltaStartEndPoint;

      
        
        startPoint.SendMessage("SetFinishInit", SendMessageOptions.DontRequireReceiver);
        roadComponentQueue.Enqueue(currentComponent);   //add to the queue
        if (roadComponentQueue.Count >= 5)
        {
            GameObject oldComponent = (GameObject)roadComponentQueue.Dequeue();
            oldComponent.transform.position = new Vector3(-10000f, -10000f, -10000f); //back to the spare area
            oldComponent.transform.eulerAngles = new Vector3(0, 0, 0);
            //Destroy(oldComponent);
        }
        lastComponent = currentComponent;
        /*switch (terrainCount)
        {
            case 1:
                Debug.Log("New Terrian\n");
                               
                terrainTiles[terrainCount] = (GameObject)Instantiate(terrainPrefab
                                                                        , new Vector3(100f, 0f, 0f)
                                                                        , transform.rotation);
                //terrainTiles[terrainCount].transform.
                terrainCount++;
                break;
        }*/
       
    }

    
	
	// Update is called once per frame
	void Update () {
	    
	}
}
