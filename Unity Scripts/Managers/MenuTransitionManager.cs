using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using STAR_GAZE;

public class MenuTransitionManager : MonoBehaviour
{
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //---------------------------------------------  MENU_TRANSITION_MANAGER  -------------------------------------------------
    /*
     * This script does the following:
     *      - Manages the pages which are currently open
     *      - Handles the transitions animations between screen
     *      - Will not handle Weather and shape menu pop-ups (only main pages)
     * 
     * */

    #region SINGLETON
    public static MenuTransitionManager Instance { get; private set; }  //Singleton
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

        //set IPage references
        pageScripts = new IPage[MenuCanvas.Length];
        for (int i = 0; i < pageScripts.Length; i++)
        {
            pageScripts[i] = MenuCanvas[i].GetComponent<IPage>();
        }

        OnAppBoot();

    }
    #endregion

    private void OnEnable()
    {
        //EventManager.StartListening("GameStarted", OnGameStarted);
    }

    private void OnDisable()
    {
        //EventManager.StopListening("GameStarted", OnGameStarted);

    }

    //-----------------Script References-----------------------
    [Header("Page References")]
    [SerializeField]
    private GameObject[] MenuCanvas;
    private IPage[] pageScripts;

    

    //STACK TO HOLD PAGE STATE HISTORY
    private Stack<State> visitedPages = new Stack<State>();

    //------------- Public accessors ---------------
    public State CurrentPage { get { return visitedPages.Peek(); } }

    public void EnterPage(State state)
    {
        if (visitedPages.Count >0)// || state == State.End)
        {
            pageScripts[(int)visitedPages.Peek()].OnPageExit(state);    //exit currentPage
        }

        visitedPages.Push(state);

        //enable canvas
        MenuCanvas[(int)state].SetActive(true);

        //call appropriate 'OnPageEnter()'
        pageScripts[(int)state].OnPageEnter(state);

    }

    public void OnLeavePage()
    {
        //Leave current page
        State currentPage = visitedPages.Pop();
        pageScripts[(int)currentPage].OnPageExit(currentPage);

        //Enter previous page (letting the previous page know which page we are transitioning from)
        MenuCanvas[(int)visitedPages.Peek()].SetActive(true);
        pageScripts[(int)visitedPages.Peek()].OnPageEnter(currentPage);

    }
    public void OnDisablePage(State state)
    {
        //Disable the page signified by 'state' when this function is called
        MenuCanvas[(int)state].SetActive(false);
    }
    public void OnLeaveDiscretePage(State state)
    {
        if (MenuCanvas[(int)state].activeSelf)
            pageScripts[(int)state].OnPageExit(visitedPages.Peek());
    }

    #region Events
    void OnAppBoot()
    {
        EnterPage(State.Login);
        //disable all canvas' except start menu
        MenuCanvas[0].SetActive(true);
        for (int i = 1; i < MenuCanvas.Length; i++)
        {
            MenuCanvas[i].SetActive(false);
        }
    }
    
    #endregion

}
