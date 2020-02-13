using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using STAR_GAZE;
using UnityEngine.UI;

public class MainPageManager : MonoBehaviour, IPage
{
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //---------------------------------------------  MAIN_PAGE_MANAGER  -------------------------------------------------
    /*
     * This script does the following:
     *      - Controls the buttons on the main page
     *      - Handles the animations and transitions of the main page
     * 
     * */

    #region SINGLETON
    public static MainPageManager Instance { get; private set; }  //Singleton
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


    //-------------------- Properties ----------------------
    [Header("References")]
    [SerializeField]
    private Animator anim;
    public GameObject DebugScreen;
    bool debugState = false;


    [Header("Button References")]
    public Button CamButton;
    public Image CamIcon;
    public Color CamDisableCol;


    //------------ Internal Variables ---------------
    private bool settingsState = false;
    private float lastTimePictureTaken = 0;
    private float photoDelay = 5.0f;


    //---------------- Implementing IPage interface
    public void OnPageEnter(State previousPage)
    {
        anim = GetComponent<Animator>();

    }

    public void OnPageExit(State page)
    {

    }


    //--------------- Settings ----------------------
    public void SetCameraState(bool state)
    {
        if (state)
        {
            CamButton.interactable = true;
            CamIcon.color = Color.white;
        }
        else
        {
            CamButton.interactable = false;
            CamIcon.color = CamDisableCol;
        }

    }

    //--------------- Page Buttons -----------------
    public void OnCentreCamera()
    {
        CameraControls.Instance.ToggleAxisFreeze();
    }

    public void OnFreezeAxis()
    {
        CameraControls.Instance.ToggleAxisFreeze();
    }

    public void OnOpenImage()
    {

    }

    public void OnTakeImage()
    {
        if(Time.time > lastTimePictureTaken + photoDelay)
        {
            mqttFunctions.Instance.TakePicture();
            lastTimePictureTaken = Time.time;
            StartCoroutine(DisableCamDelay());
        }
    }


    //------------------ Srttings Button -------------------
    public void OnSettingButton()
    {
        settingsState = !settingsState;

        if (settingsState)
            anim.SetInteger("SettingsState", 1);
        else
            anim.SetInteger("SettingsState", 0);
    }

    public void OnDebug()
    {
        debugState = !debugState;

        if (!debugState)
            DebugScreen.SetActive(false);
        else
            DebugScreen.SetActive(true);
    }

    IEnumerator DisableCamDelay()
    {
        SetCameraState(false);
        yield return new WaitForSeconds(photoDelay);
        SetCameraState(true);
    }
}
