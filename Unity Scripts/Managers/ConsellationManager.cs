using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
public class ConsellationManager : MonoBehaviour
{
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //---------------------------------------------  CONSTELLATION_MANAGER  -------------------------------------------------
    /*
     * This script does the following:
     *      - Displays the stars and constellations
     * 
     * */

    #region SINGLETON
    public static ConsellationManager Instance { get; private set; }  //Singleton
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


    //-------------- Parameters ----------------
    public float CelsestialSphereRadius = 200f;

    //--------------- Data References ----------------
    public ConstellationStore constellationInfo;
    public Sprite[] imagesTaken;


    //--------------- Prefab References ------------
    public GameObject STAR_PREFAB;
    public GameObject LINE_OBJECT;

    //--------------- Object References --------------
    public Transform UniverseTransform;


    private void Start()
    {
        //spawn all stars
        SpawnStars();

        //spawn connections
        SpawnConnections();            
    }


    void SpawnStars()
    {
        for (int i = 0; i < constellationInfo.allStars.Length; i++)
        {
            //find the position
            Vector3 pos = constellationInfo.allStars[i].WorldPosition;
            GameObject newStar = Instantiate(STAR_PREFAB, pos, Quaternion.identity);
            newStar.transform.SetParent(UniverseTransform);
        }
    }


    void SpawnConnections()
    {
        int spawnProp = constellationInfo.starGroups.Length / 3;
        for (int i = 0; i < spawnProp; i++)
        {
            //draw line between points
            GameObject newLine = Instantiate(LINE_OBJECT, UniverseTransform);

            LineRenderer lr = newLine.GetComponent<LineRenderer>();

            lr.SetPositions(constellationInfo.starGroups[i].starConnections);
        }
    }

    


}


