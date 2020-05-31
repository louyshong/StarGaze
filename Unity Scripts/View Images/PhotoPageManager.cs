using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using STAR_GAZE;

public class PhotoPageManager : MonoBehaviour, IPage
{
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //---------------------------------------------  PHOTO_PAGE_MANAGER  -------------------------------------------------
    /*
     * This script does the following:
     *      - Displays images 
     *      - Displays other information
     * 
     * */

    //------------ References -------------
    public Animator anim;
    public Image displayImage;
    public Text constellationName;
    public Text Declination;
    public Text RightAscension;
    public Text LocalTime;
    public Text ImageName;

    public void OnPageEnter(State previousPage)
    {
        
        //get information about image
        int index = PhotoManager.Instance.CurrentViewImage;
        Debug.Log("Selected Index is: " + index);
        ImageData data = PhotoManager.Instance.PICTURES_TAKEN[index];

        //update GUI
        Declination.text = "Declination: " + data.Declination.ToString();
        RightAscension.text = "Right Ascensions: " + data.RightAscension.ToString();
        LocalTime.text = "Time Taken: " + data.DayTaken.ToString() + "/" + data.MonthTaken.ToString() + "/" + data.YearTaken + "   " + data.HourTaken.ToString() + ":" + data.MinuteTaken.ToString();
        ImageName.text = "Image Name: " + data.ImageName;
    }

    public void OnPageExit(State page)
    {
        //fade out

        //disable page
        MenuTransitionManager.Instance.OnDisablePage(State.Photo);

    }

    public void OnExitButton()
    {
        MenuTransitionManager.Instance.OnLeavePage();
    }
}
