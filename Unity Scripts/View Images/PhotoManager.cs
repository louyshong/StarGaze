using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using STAR_GAZE;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.SceneManagement;

public class PhotoManager : MonoBehaviour
{
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //---------------------------------------------  PHOTO_MANAGER  -------------------------------------------------
    /*
     * This script does the following:
     *      - Spawn dots where there are images to display
     *      - Handles the on photo click events
     * 
     * */

    #region SINGLETON
    public static PhotoManager Instance { get; private set; }  //Singleton
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);

        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

    }
    #endregion

    //-------------- Properties ----------------
    public float ImageRadius = 10.0f;
    public float ImageRadiusVariance = 3.0f;
    int numberOfImages = 0;
    List<Sprite> imagesTaken = new List<Sprite>();


    //------------- References ----------------
    public GameObject ImagePOI;
    public Sprite TestImage;
    public Transform UniverseTransform;



    //------------- Save Data ---------------------
    public List<ImageData> PICTURES_TAKEN { get; private set; }

    public int CurrentViewImage { get; private set; }

    //--------- Start-up Methods ---------------
    private void Start()
    {
        //RestSaveData();
        //LoadData();

        SpawnSavedImages();
    }
    void SpawnSavedImages()
    {
        PICTURES_TAKEN = mqttFunctions.Instance.ALL_IMAGES;

        if(PICTURES_TAKEN == null)
        {
            PICTURES_TAKEN = new List<ImageData>();
        }
        
        //------------- FIREBASE IMPLEMENTATION ------------------
        for (int i = 0; i < PICTURES_TAKEN.Count; i++)
        {
            //add image to image list
            imagesTaken.Add(TestImage);


            //spawn ball
            SpawnNewImagePOI(PICTURES_TAKEN[i].Declination, PICTURES_TAKEN[i].RightAscension);
        }
        

        /*
        for (int i = 0; i < 10; i++)
        {
            //choose random location
            float dec = UnityEngine.Random.Range(-60.0f, 60.0f);
            float ra = UnityEngine.Random.Range(0, 360);

            //add images to list
            imagesTaken.Add(TestImage);

            //spawn ball
            SpawnNewImagePOI(dec, ra);
        }
        */

        

    }


 

    //--------- listen for clicks ---------------
    private void Update()
    {
        //check for image open trigger
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hitInfo = new RaycastHit();
            bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);
            if (hit)
            {
                //check hit information
                if(hitInfo.transform.gameObject.tag == "ImagePOI")
                {
                    //get the index
                    int index = hitInfo.transform.gameObject.GetComponent<ImageDot>().ImageIndex;

                    //open the image
                    OnOpenImage(index);
                    Debug.Log("Index is: " + index);
                }
            }
        }

        //check for new image
        if(mqttFunctions.Instance.ALL_IMAGES.Count > numberOfImages)
        {
            //update list
            PICTURES_TAKEN = mqttFunctions.Instance.ALL_IMAGES;
            FillImageList();
        }
    }

    void FillImageList()
    {
        
        for (int i = numberOfImages; i < PICTURES_TAKEN.Count; i++)
        {
            imagesTaken.Add(TestImage);
            SpawnNewImagePOI(PICTURES_TAKEN[i].Declination, PICTURES_TAKEN[i].RightAscension);
        }
        //numberOfImages = PICTURES_TAKEN.Count;
        Debug.Log("Number of images: " + numberOfImages);
    }


    //---------------- New Image ---------------
    public void OnReceivedNewImage(float dec, float ra, int yearTaken, int hourTaken, int minuteTaken, string URL)
    {
        //create a new saved instance
        ImageData newPic = new ImageData(dec, ra, yearTaken, hourTaken, minuteTaken, URL);
        PICTURES_TAKEN.Add(newPic);
        SaveData();

        //create a new dot on the screen
        SpawnNewImagePOI(dec, ra);
    }

    void SpawnNewImagePOI(float dec, float ra)
    {
        //get the world coordinates
        float radius = ImageRadius + UnityEngine.Random.Range(-ImageRadiusVariance, ImageRadius);
        Vector3 spawnPos = CoordinateTransformation.ToWorldCoordiantes(dec, ra, radius);

        //spawn new POI object
        GameObject newPOI = Instantiate(ImagePOI, spawnPos, Quaternion.identity);
        newPOI.transform.SetParent(UniverseTransform);
        newPOI.transform.localPosition = spawnPos;

        //assign index
        //int index = numberOfImages + 1;
        newPOI.GetComponent<ImageDot>().SetIndex(numberOfImages);
        numberOfImages += 1;
    }

    public void OnOpenImage(int index)
    {
        //set image index
        CurrentViewImage = index;

        //open photo page
        MenuTransitionManager.Instance.EnterPage(State.Photo);
    }


    //----------------- Save Data ------------------
    #region SAVING AND LOADING
    public void SaveData()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/stargazeData.dat");
        CustomSaveClass data = new CustomSaveClass();

        //format data
        CustomSaveClass CUSTOM_SAVE_CLASS = new CustomSaveClass();
        ImageData[] picTakenArr = PICTURES_TAKEN.ToArray();
        CUSTOM_SAVE_CLASS.picturesTaken = picTakenArr;
        data = CUSTOM_SAVE_CLASS;

        //save data
        bf.Serialize(file, data);
        file.Close();

    }

    public void LoadData()
    {
        if (File.Exists(Application.persistentDataPath + "/stargazeData.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/stargazeData.dat", FileMode.Open);

            try
            {
                CustomSaveClass data = (CustomSaveClass)bf.Deserialize(file);
                file.Close();
                CustomSaveClass CUSTOM_SAVE_CLASS = data;

                //load data into list
                PICTURES_TAKEN = new List<ImageData>();
                for (int i = 0; i < CUSTOM_SAVE_CLASS.picturesTaken.Length; i++)
                {
                    PICTURES_TAKEN.Add(CUSTOM_SAVE_CLASS.picturesTaken[i]);
                }
            }
            catch
            {
                file.Close();
                RestSaveData();


            }


        }

    }

    public void RestSaveData()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/stargazeData.dat");
        CustomSaveClass data = new CustomSaveClass();

        //save data
        bf.Serialize(file, data);
        file.Close();

        //reload scene
        //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    #endregion

}
/*
[System.Serializable]
public class PictureTaken
{
    public float Altitude;
    public float Azimuth;
    public float Latitude;
    public float Longitude;
    public string LocalSideralDay;   //fraction between 0 and 23.something
    public int YearTaken;
    public int MonthTaken;
    public int DayTaken;
    public int HourTaken;
    public int MinuteTaken;
    public string ImageName;

}

[System.Serializable]
public class ImageData
{
    public float Declination;
    public float RightAscension;
    public int YearTaken;
    public int MonthTaken;
    public int DayTaken;
    public int HourTaken;
    public int MinuteTaken;
    public string ImageName;

    public ImageData(float declination, float rightAscension,int yearTaken, int hourTaken, int minuteTaken, string imageName)
    {
        this.Declination = declination;
        this.RightAscension = rightAscension;
        this.YearTaken = yearTaken;
        this.HourTaken = hourTaken;
        this.MinuteTaken = minuteTaken;
        this.ImageName = imageName;
    }
}

[System.Serializable]
public class CustomSaveClass
{
    public ImageData[] picturesTaken;
}
*/
