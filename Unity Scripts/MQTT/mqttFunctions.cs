using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using uPLibrary.Networking.M2Mqtt.Utility;
using uPLibrary.Networking.M2Mqtt.Exceptions;
using System.Text;
using System;
using STAR_GAZE;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.SceneManagement;

public class mqttFunctions : MonoBehaviour 
{
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//---------------------------------------------  MQTT_FUNCTIONS  -------------------------------------------------
	/*
     * This script does the following:
     *      - Established and manages MQTT Connection
	 *      - Processess income/outgoing data
	 *      
	 * General Message format:
	 *		<Topic>_<Information>
	 * 
	 * Type of data than can be received by app:
	 *		- 'telPos'	<lat>_<long>_<azimuth>_<altitude>
	 *		- 'recPic'  <dec>_<ra>_<timeTaken>_<URL>
	 *		- <timeTaken> = hh#mm
	 *		
	 * Type of data than can be sent:
	 *		- 'takePic'  <request>
	 *		
	 *	
     * 
     * */

	//list of all current images
	public List<ImageData> ALL_IMAGES { get; private set; }


	#region SINGLETON
	public static mqttFunctions Instance { get; private set; }  //Singleton
	
	#endregion


	public string tel_timetaken = "";
	public string tel_alt = "";
	public string tel_az = "";
	public string tel_lat = "";
	public string tel_long = "";
	public string tel_minute = "";
	public string tel_hour = "";

	public string pic_timetaken = "";
	public string pic_dec = "";
	public string pic_ra = "";
	public string pic_minute = "";
	public string pic_hour = "";


	public string TelsecopeUpdate = "";
	public string ImageUpdate = "";

	public GameObject DebugObj;

	public bool MQTT_CONNECTION = false;

	//------------------- References -----------------------
	private MqttClient client;



	//------------- Initialise Connections with broker -----------------
	void Awake () {

		// Singleton instance
		if (Instance == null)
		{
			Instance = this;

		}
		else if (Instance != this)
		{
			Destroy(gameObject);
		}

		try
		{
			// create client instance 
			client = new MqttClient("test.mosquitto.org",1883 , false , null ); 
		
			// register to message received 
			client.MqttMsgPublishReceived += client_MqttMsgPublishReceived; 
		
			//establishing connection to broker
			string clientId = Guid.NewGuid().ToString(); 
			client.Connect(clientId); 
		
			//for testing purposes, publishes a message to IC.embedded/friends
			//client.Publish("IC.embedded/friends", System.Text.Encoding.UTF8.GetBytes("Sending from Unity3D!!!"), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, true);
		
			// subscribe to the topic "IC.embedded/friends/sensors"
			client.Subscribe(new string[] { "IC.embedded/friends/unity/#" }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });

			Debug.Log("Connected to MQTT Broker");
			MQTT_CONNECTION = true;

		}
		catch
		{
			Debug.Log("Failed to connect to MQTT Broker");
			MQTT_CONNECTION = false;
		}
		

		// load save data
		LoadData();

		if(ALL_IMAGES == null)
			ALL_IMAGES = new List<ImageData>();


		/*
		//create new image data 
		for (int i = 0; i < 2; i++)
		{
			ImageData newImageData = new ImageData(20, 20 + (i * 20), 2020, 20, 20, "randdomImage");
			ALL_IMAGES.Add(newImageData);
		}
		*/


	}


	public void OnAddImage()
	{
		//create new image data 
		ImageData newImageData = new ImageData(20, 20 + (2 * 20), 2020, 20, 20, "newImage");

		ALL_IMAGES.Add(newImageData);
	}

	//--------------- Processing Information -----------------------
	void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e) 
	{
		string message = Encoding.Default.GetString(e.Message);
		Debug.Log(message);

		Debug.Log(e.Topic);
		//Debug.Log("Altitude is: " + pos.Altitude);
		//Debug.Log("Azimuth is: " + pos.Azimuth);

		if(e.Topic == "IC.embedded/friends/unity/sensors")
		{
			ReceivedTelescopePos pos = JsonUtility.FromJson<ReceivedTelescopePos>(message);
			Debug.Log("Altitude is: " + pos.Altitude);
			Debug.Log("Azimuth is: " + pos.Azimuth);
			OnReceivePosUpdate(pos);


			TelsecopeUpdate = message;


		}
		else if(e.Topic == "IC.embedded/friends/unity/camera")
		{
			PictureTaken pic = JsonUtility.FromJson<PictureTaken>(message);
			Debug.Log(pic.Altitude);
			Debug.Log(pic.Azimuth);
			Debug.Log(pic.DayTaken);
			Debug.Log(pic.HourTaken);

			AASharp.AAS2DCoordinate coor =  CoordinateTransformation.ToCelestialCoordinates(pic.Latitude, pic.Longitude, pic.Azimuth, pic.Altitude);

			OnReceivePic(pic, (float)coor.Y, (float)coor.X);

			ImageUpdate = message;


			//create new image data 
			ImageData newImageData = new ImageData((float)coor.Y, (float)coor.X, pic.YearTaken, pic.HourTaken, pic.MinuteTaken, pic.ImageName);

			ALL_IMAGES.Add(newImageData);
			SaveData();

		}


	} 


	//------------------ CallBack Functions ------------------------
	void OnReceivePic(PictureTaken data, float dec, float ra)
	{

		//PhotoManager.Instance.OnReceivedNewImage(dec, ra, data.DayTaken, data.HourTaken, data.MinuteTaken, data.ImageName);
		
	}

	void OnReceivePosUpdate(ReceivedTelescopePos data)
	{

		//TelescopeManager.Instance.UpdateTelescopePosition(data.Latitude, data.Longitude, data.Altitude, data.Azimuth);

		//enable camera
		MainPageManager.Instance.SetCameraState(true);
	}



	public void TakePicture() 
	{
		client.Publish("IC.embedded/friends/pi", System.Text.Encoding.UTF8.GetBytes("take photo"), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, true);
		
		Debug.Log("sent");
	}


	//----------------- Save Data ------------------
	#region SAVING AND LOADING
	public void SaveData()
	{
		BinaryFormatter bf = new BinaryFormatter();
		FileStream file = File.Create(Application.persistentDataPath + "/stargazeData.dat");
		CustomSaveClass data = new CustomSaveClass();

		//format data
		CustomSaveClass CUSTOM_SAVE_CLASS = new CustomSaveClass();
		ImageData[] picTakenArr = ALL_IMAGES.ToArray();
		CUSTOM_SAVE_CLASS.picturesTaken = picTakenArr;
		data = CUSTOM_SAVE_CLASS;

		//save data
		bf.Serialize(file, data);
		file.Close();

	}

	public void LoadData()
	{
		if (File.Exists(Application.persistentDataPath + "/stargazeData.dat"))
		{
			BinaryFormatter bf = new BinaryFormatter();
			FileStream file = File.Open(Application.persistentDataPath + "/stargazeData.dat", FileMode.Open);

			try
			{
				CustomSaveClass data = (CustomSaveClass)bf.Deserialize(file);
				file.Close();
				CustomSaveClass CUSTOM_SAVE_CLASS = data;

				//load data into list
				ALL_IMAGES = new List<ImageData>();
				for (int i = 0; i < CUSTOM_SAVE_CLASS.picturesTaken.Length; i++)
				{
					ALL_IMAGES.Add(CUSTOM_SAVE_CLASS.picturesTaken[i]);
				}
			}
			catch
			{
				file.Close();
				RestSaveData();
			}


		}

	}

	public void RestSaveData()
	{
		BinaryFormatter bf = new BinaryFormatter();
		FileStream file = File.Create(Application.persistentDataPath + "/stargazeData.dat");
		CustomSaveClass data = new CustomSaveClass();

		//save data
		bf.Serialize(file, data);
		file.Close();

		//reload scene
		//SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	#endregion
	/*
	void OnGUI(){
		if ( GUI.Button (new Rect (20,40,80,20), "Take picture")) {
			TakePicture();
		}
	}
	*/

}


public class ReceivedTelescopePos
{
	public float Altitude;
	public float Azimuth;
	public float Latitude;
	public float Longitude;
}

[System.Serializable]
public class PictureTaken
{
	public float Altitude;
	public float Azimuth;
	public float Latitude;
	public float Longitude;
	public string LocalSideralDay;   //fraction between 0 and 23.something
	public int YearTaken;
	public int MonthTaken;
	public int DayTaken;
	public int HourTaken;
	public int MinuteTaken;
	public string ImageName;

}

[System.Serializable]
public class ImageData
{
	public float Declination;
	public float RightAscension;
	public int YearTaken;
	public int MonthTaken;
	public int DayTaken;
	public int HourTaken;
	public int MinuteTaken;
	public string ImageName;

	public ImageData(float declination, float rightAscension, int yearTaken, int hourTaken, int minuteTaken, string imageName)
	{
		this.Declination = declination;
		this.RightAscension = rightAscension;
		this.YearTaken = yearTaken;
		this.HourTaken = hourTaken;
		this.MinuteTaken = minuteTaken;
		this.ImageName = imageName;
	}
}

[System.Serializable]
public class CustomSaveClass
{
	public ImageData[] picturesTaken;
}
