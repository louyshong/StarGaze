using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColourGenerator
{
    ColourSettings settings;
    Texture2D texture;
    const int texture_resolution = 50;


    public void UpdateSettings(ColourSettings settings)
    {
        this.settings = settings;

        if(texture == null)
        {
            texture = new Texture2D(texture_resolution, 1);

        }
    }

    public void UpdateElevation(MinMax elevationMinMax)
    {
        settings.planetMaterial.SetVector("_elevationMinMax", new Vector4(elevationMinMax.Min, elevationMinMax.Max));
    }

    public void UpdateColours()
    {
        Color[] colours = new Color[texture_resolution];
        for (int i = 0; i < texture_resolution; i++)
        {
            colours[i] = settings.gradient.Evaluate(i / (texture_resolution - 1f));

        }
        texture.SetPixels(colours);
        texture.Apply();
        settings.planetMaterial.SetTexture("_texture", texture);
    }
}

