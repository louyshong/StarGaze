using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FirebaseFunctions : MonoBehaviour
{
	
	// Start is called before the first frame update
	void Start()
	{
		//Test();
	}


	void Test()
	{
		Debug.Log("downloading image");
		Firebase.Storage.FirebaseStorage storage = Firebase.Storage.FirebaseStorage.DefaultInstance;
		Firebase.Storage.StorageReference storage_ref = storage.GetReferenceFromUrl("gs://stargaze-embedded.appspot.com/image.jpg");

		// Create local filesystem URL
		string local_url = "file:///local/images/image.jpg";

		// Download to the local filesystem
		storage_ref.GetFileAsync(local_url).ContinueWith(task => {
			if (!task.IsFaulted && !task.IsCanceled)
			{
				Debug.Log("File downloaded.");
			}
		});
	}

	void DownloadImage()
	{
		Firebase.Storage.StorageReference storageReference =
		Firebase.Storage.FirebaseStorage.DefaultInstance.GetReferenceFromUrl("gs://stargaze-embedded.appspot.com/");

		storageReference.Child("image.jpg").GetBytesAsync(1024 * 1024).
			ContinueWith((System.Threading.Tasks.Task<byte[]> task) =>
			{
				if (task.IsFaulted || task.IsCanceled)
				{
					Debug.Log(task.Exception.ToString());
				}
				else
				{
					byte[] fileContents = task.Result;
					Texture2D texture = new Texture2D(1, 1);
					texture.LoadImage(fileContents);
					//if you need sprite for SpriteRenderer or Image
					Sprite sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width,
					texture.height), new Vector2(0.5f, 0.5f), 100.0f);
					Debug.Log("Finished downloading!");
				}
			});
	}
	
}