using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using AASharp;

namespace STAR_GAZE
{
    public interface IPage
    {
        void OnPageEnter(State previousPage);
        void OnPageExit(State page);
        //void OnPageRefresh();
    }

    public enum State
    {
        Login, Main, Photo
    }

    public static class CoordinateTransformation
    {
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //-----------------------------------------  COORDINATE_TRANSFORMATION  ---------------------------------------------
        /*
         * This script does the following:
         *      - Transform from horizontal to celestial coordinates
         *      
         * 
         * */

        // returns the sidereal time at the designated longitude
        public static float LocalSiderealTime(float lng)
        {
            // get the current time
            DateTime now = DateTime.Now;

            //create time custom time object
            AASDate dateSunCalc = new AASDate(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, true);

            //get greenwich time
            double greenwich_sidereal_time = AASSidereal.MeanGreenwichSiderealTime(dateSunCalc.Julian);

            //adjust for longitude
            double adj_st = greenwich_sidereal_time + (lng * (24.0 / 360.0));

            if (adj_st >= 24.0)
                adj_st -= 24.0;
            if (adj_st < 0)
                adj_st += 24.0;

            return (float)adj_st;
        }

        // returns the current rotation of the Earth from the Vernal Equinox, assumming Earth zero is at Greenwich
        public static Vector3 CurrentRotation()
        {
            //get the current sidereal day
            float current_sideral_day = LocalSiderealTime(0.0f);

            // calculate the sideral days (23.934hrs) passed since equinox
            float rotation = current_sideral_day * (360f / 24f);

            return new Vector3(0, rotation, 0);
        }

        // converts horizontal coordinates to equitorial coordinates
        public static AAS2DCoordinate ToCelestialCoordinates(float lat, float lng, float az, float alt)
        {
            /*
            Key:
            ------------ Local Coordinates -------------
            az = azimuth - angle around the horizon from due north to under POI (north has azimuth of 0m south has azimuth of 180)
            alt = altitude - angle from the horizon where the object is.
            lat = latitude
            long = Longitude
            lst = local sideral time - Greenwich meridian plus Longitude
            lha = local hour angle -  It is the angle between the meridian of your Assumed Position and the meridian of the geographical position of the celestial body.
            ------------ Equatorial Coordinates ---------------
            ra = right ascention
            dec = declination angle
            ha = hour angle - angle between local meridian projected on celestial sphere, and right ascention of body


            CALCULATING: 'right ascension' (ra) and 'declination' (dec)
            1) sin(alt) = sin(dec)sin(lat) + cos(dec)cos(lat)cos(ha)
            2) sin(ha) =  tan(az)[cos(ha)sin(lat) - tan(dec)cos(lat)]
            3) ra = lst - ha

            Rearranging leads to:
            ha = arctan(x,y)
            x = sin(lat)cos(alt)cos(az) + cos(lat)sin(alt)
            y = cos(alt)sin(az)

            dec = arctan(x', y')
            x' = sin(az)cos(ha)sin(lat) - sin(ha)cos(az)
            y' = cos(lat)sin(az)
             */

            //find coordinates assuming sidereal_time is 00::00::00
            AAS2DCoordinate converted = AASCoordinateTransformation.Horizontal2Equatorial(az, alt, lat);

            //adjust for local sideral_time
            converted.X += (LocalSiderealTime(lng) * (360f / 24f));

            return converted;


        }

        // convert celestial coordinates to world coordinates (in degrees)
        public static Vector3 ToWorldCoordiantes(float dec, float ra, float rad)
        {
            float dec_adj = (90f - dec) * Mathf.Deg2Rad;
            float ra_adj = ra * Mathf.Deg2Rad;
            float x = rad * Mathf.Sin(dec_adj) * Mathf.Cos(ra_adj);
            float z = rad * Mathf.Sin(dec_adj) * Mathf.Sin(ra_adj);
            float y = rad * Mathf.Cos(dec_adj);
            return new Vector3(x, y, z);
        }


    }

    /*
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
    */
}
