using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using STAR_GAZE;

public class TelescopeManager : MonoBehaviour
{
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //---------------------------------------------  TELESCOPE_MANAGER  -------------------------------------------------
    /*
     * This script does the following:
     *      - Updates the position and orientation of the telescope
     * 
     * */

    #region SINGLETON
    public static TelescopeManager Instance { get; private set; }  //Singleton
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

    }
    #endregion


    //-------------- References ----------------
    public Transform TelescopeLens;


    const float planetRadius = 2.75f;

    private void Start()
    {
        UpdateTelescopePosition(51.0f, 1f, 45, 0);
    }

    public void UpdateTelescopePosition(float lat, float lng, float alt, float az)
    {
        //find position
        Vector3 planetCoor = CoordinateTransformation.ToWorldCoordiantes(lat, lng, planetRadius);
        transform.localPosition = planetCoor;

        //find orientation 
        float x_rot = 0;
        float y_rot = -lng + az;
        float z_rot = lat - 90;
        transform.localRotation = Quaternion.Euler(new Vector3(x_rot, y_rot, z_rot));

        //update lens rotation
        float z_rot_lens = -alt;
        TelescopeLens.localRotation = Quaternion.Euler(new Vector3(0, 0, z_rot_lens));
    }

   
}
