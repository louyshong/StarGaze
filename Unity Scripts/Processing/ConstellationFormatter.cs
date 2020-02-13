using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using STAR_GAZE;
using System.Linq;
using System;

public class ConstellationFormatter : EditorWindow
{
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //-------------------------------------------  CONSTELLATION_FORMATTER  ----------------------------------------------
    /*
     * This script does the following:
     *      - Read the constellation information from .csv file
     *      - Format data and save it inside a scriptable object
     * */

    //-------------- Save location ---------------
    public ConsellationManager dataStore;


    //-------------- Text file references ---------------
    public TextAsset ConstellationData;
    public TextAsset ConstellationNames;

    //--------------- Loaded Data ------------------
    private Dictionary<string, string> nameConversion = new Dictionary<string, string>();
    private Dictionary<int, Star> stars = new Dictionary<int, Star>();
    private List<int> starConnections = new List<int>();

    [MenuItem("Window/ConstellationFormatter")]
    public static void ShowWindow()
    {
        GetWindow<ConstellationFormatter>("ConstellationFormatter");
    }

    void OnGUI()
    {
        //------------------------GET THE PREFAB REFERENCES FROM THE WINDOW---------------------

        //ConstellationData 
        ScriptableObject scriptableObj_CHUTE = this;
        SerializedObject serialObj_CHUTE = new SerializedObject(scriptableObj_CHUTE);
        SerializedProperty serialProp_CHUTE = serialObj_CHUTE.FindProperty("ConstellationData");
        EditorGUILayout.PropertyField(serialProp_CHUTE, true);
        serialObj_CHUTE.ApplyModifiedProperties();

        //ConstellationNames
        ScriptableObject scriptableObj_CORNER = this;
        SerializedObject serialObj_CORNER = new SerializedObject(scriptableObj_CORNER);
        SerializedProperty serialProp_CORNER = serialObj_CORNER.FindProperty("ConstellationNames");
        EditorGUILayout.PropertyField(serialProp_CORNER, true);
        serialObj_CORNER.ApplyModifiedProperties();

        //dataStore
        ScriptableObject scriptableObj_DATA = this;
        SerializedObject serialObj_DATA = new SerializedObject(scriptableObj_DATA);
        SerializedProperty serialProp_DATA = serialObj_DATA.FindProperty("dataStore");
        EditorGUILayout.PropertyField(serialProp_DATA, true);
        serialObj_DATA.ApplyModifiedProperties();
        

        //---------------------LOAD THE DATA INTO THE PREFABS-----------------------
        if (GUILayout.Button("Load Data"))
        {
            nameConversion = new Dictionary<string, string>();
            stars = new Dictionary<int, Star>();
            starConnections = new List<int>();
            LoadConstellationNames();
            LoadConstellationLines();
            Debug.Log("LOADED DATA");

            //int sizeOfList = EditorExtentionMethods.FindAssetsByType<CustomShapeData>().Count;

        }

    }

    void LoadConstellationNames()
    {
        //each string contains a row
        string[] data = ConstellationNames.text.Split(new char[] { '\n' });

        for (int i = 1; i < data.Length - 1; i++)
        {
            //break each row into individual elements
            string[] row = data[i].Split(new char[] { ',' });

            if (row.Length == 2)
            {
                // add key value pairs to dictionary
                nameConversion.Add(row[0], row[1]);
                //Debug.Log("Key: " + row[0] + "   Value: " + row[1]);
            }
        }
    }

    void LoadConstellationLines()
    {
        //references to scriptable object(s)
        List<ConstellationStore> storeRef = EditorExtentionMethods.FindAssetsByType<ConstellationStore>();

        //each string contains a row
        string[] data = ConstellationData.text.Split(new char[] { '\n' });

        for (int i = 0; i < data.Length - 1; i++)
        {
            //break each row into individual elements
            string[] row = data[i].Split(new char[] { ',' });

            if (row.Length > 5)
            {
                // parse data
                string CON;
                nameConversion.TryGetValue(row[0], out CON);

                int HR1;
                int.TryParse(row[1], out HR1);

                float RA1;
                float.TryParse(row[2], out RA1);

                float DEC1;
                float.TryParse(row[3], out DEC1);

                float MAG1;
                float.TryParse(row[4], out MAG1);

                string GREEK1 = row[5];

                string NAME1 = GetElement(row[6], '|', 0);
                if (NAME1 == null)
                    NAME1 = row[6];

                int HR2;
                int.TryParse(row[7], out HR2);

                float RA2;
                float.TryParse(row[8], out RA2);

                float DEC2;
                float.TryParse(row[9], out DEC2);

                float MAG2;
                float.TryParse(row[10], out MAG2);

                string GREEK2 = row[11];

                string NAME2 = GetElement(row[12], '|', 0);
                if (NAME2 == null)
                    NAME2 = row[12];

                //add to star dictionary is not already present
                if (!stars.ContainsKey(HR1))
                {
                    //create star object
                    Star newStar = new Star();
                    newStar.StarName = NAME1;
                    newStar.HR_Number = HR1;
                    newStar.Brightness = MAG1;
                    newStar.Declination_RightAcsension = new Vector2(DEC1, RA1);
                    newStar.WorldPosition = CoordinateTransformation.ToWorldCoordiantes(DEC1, RA1, dataStore.CelsestialSphereRadius);
                    stars.Add(HR1, newStar);
                }
                if (!stars.ContainsKey(HR2))
                {
                    //create star object
                    Star newStar = new Star();
                    newStar.StarName = NAME2;
                    newStar.HR_Number = HR2;
                    newStar.Brightness = MAG2;
                    newStar.Declination_RightAcsension = new Vector2(DEC2, RA2);
                    newStar.WorldPosition = CoordinateTransformation.ToWorldCoordiantes(DEC2, RA2, dataStore.CelsestialSphereRadius);
                    stars.Add(HR2, newStar);
                }

                starConnections.Add(HR1);
                starConnections.Add(HR2);
            }
        }

        Star[] StarArray = ConvertToArray(stars);
        storeRef[0].allStars = StarArray;
        storeRef[0].starGroups = GetConstellations(starConnections, StarArray);
    }

    string GetElement(string list, char delminator, int index)
    {
        string[] data = list.Split(new char[] { delminator });
        if (data[index] != null)
            return data[index];
        else
            return null;
    }
    Star[] ConvertToArray(Dictionary<int, Star> dict)
    {
        Debug.Log("Number of stars: " + dict.Count);
        List<Star> starList = new List<Star>();
        for (int i = 0; i < dict.Count; i++)
        {
            starList.Add(dict.ElementAt(i).Value);
        }
        return starList.ToArray();
    }
    StarGroup[] GetConstellations(List<int> HRs, Star[] stars)
    {
        //find index of star with corresponding HR number
        List<int> orderedIndexs = new List<int>();
        for (int i = 0; i < HRs.Count; i++)
        {
            int index = Array.IndexOf(stars, Array.Find(stars, s => s.HR_Number == HRs[i]));
            orderedIndexs.Add(index);
        }

        Debug.Log("Number of Links: " + orderedIndexs.Count/2);

        //group together nodes
        List<List<int>> constellationGroups = new List<List<int>>();
        for (int i = 0; i < orderedIndexs.Count; i+=2)
        {
            //find if any of the lists contains as its last element, any of the two points
            int index = -1;
            int matchIndex = -1;
            for (int j = 0; j < constellationGroups.Count; j++)
            {
                int lastValue = constellationGroups[j][constellationGroups[j].Count - 1];
                if (lastValue == orderedIndexs[i])
                {
                    index = j;
                    matchIndex = 0;
                }
                if (lastValue == orderedIndexs[i+1])
                {
                    index = j;
                    matchIndex = 1;
                }

            }

            if(index != -1)
            {
                if(matchIndex == 0)
                {
                    constellationGroups[index].Add(orderedIndexs[i]);
                    constellationGroups[index].Add(orderedIndexs[i+1]);
                }
                if(matchIndex == 1)
                {
                    constellationGroups[index].Add(orderedIndexs[i+1]);
                    constellationGroups[index].Add(orderedIndexs[i]);
                }
            }
            else //else make a new list
            {
                List<int> newList = new List<int>();
                newList.Add(orderedIndexs[i]);
                newList.Add(orderedIndexs[i+1]);
                constellationGroups.Add(newList);
            }

        }

        Debug.Log("Number of groups: " + constellationGroups.Count);

        //convert each list to world space coordinates
        List<StarGroup> output = new List<StarGroup>();
        for (int i = 0; i < constellationGroups.Count; i++)
        {
            List<Vector3> group = new List<Vector3>();
            for (int j = 0; j < constellationGroups[i].Count; j++)
            {
                group.Add(stars[constellationGroups[i][j]].WorldPosition);
            }
            StarGroup newStarGroup = new StarGroup();
            newStarGroup.starConnections = group.ToArray();
            output.Add(newStarGroup);
            
        }


        return output.ToArray();
    }

}

public static class EditorExtentionMethods
{
    public static List<T> FindAssetsByType<T>() where T : UnityEngine.Object
    {
        List<T> assets = new List<T>();
        string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(T)));
        for (int i = 0; i < guids.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset != null)
            {
                assets.Add(asset);
            }
        }
        return assets;
    }
}
