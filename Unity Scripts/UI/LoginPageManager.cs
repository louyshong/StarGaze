using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using STAR_GAZE;

public class LoginPageManager : MonoBehaviour, IPage
{
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //---------------------------------------------  LOGIN_PAGE_MANAGER  -------------------------------------------------
    /*
     * This script does the following:
     *      - Validates user input credentials
     *      - Transition to main page
     * 
     * */

    //-------------- Properties --------------
    private string USERNAME = "pi";
    private string PASSWORD = "friendgroup";
    private bool loggedIn = false;

    //------------- References ---------------
    [SerializeField] private Text usernameText;
    [SerializeField] private Text passwordText;
    [SerializeField] private Animator anim;

    public void OnPageEnter(State previousPage)
    {

    }
    public void OnPageExit(State page)
    {
        // execute animation

        //disable page
        MenuTransitionManager.Instance.OnDisablePage(State.Login);
    }

    public void OnLoginButton()
    {
        if (!loggedIn)
        {
            loggedIn = true;
            EventManager.TriggerEvent("OnLoggedIn", new EventParam());
            MenuTransitionManager.Instance.EnterPage(State.Main);
        }
    }

}
