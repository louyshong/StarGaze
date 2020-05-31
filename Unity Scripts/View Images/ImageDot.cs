using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageDot : MonoBehaviour
{
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //---------------------------------------------  IMAGE_DOT  -------------------------------------------------
    /*
     * This script does the following:
     *      - Hold information about image 
     * 
     * */

    //---------------- Properties ----------------
    public int ImageIndex { get; private set; }


    //--------------- Set Methods ----------------
    public void SetIndex(int index)
    {
        ImageIndex = index;
    }

}
