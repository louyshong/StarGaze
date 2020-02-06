from firebase import firebase
from google.cloud import storage
import os
from picamera import PiCamera
from time import sleep

# Real-time Database URl
real_time_url = "https://stargaze-embedded.firebaseio.com/"

# Storage Database URL
storage_url = "stargaze-embedded.appspot.com"

os.environ["GOOGLE_APPLICATION_CREDENTIALS"]="/home/pi/stargaze-firebase-adminsdk.json"
firebase = firebase.FirebaseApplication(real_time_url)
client = storage.Client()
bucket = client.get_bucket(storage_url) # Dont forget to remove gs:// from the storage database link

# posting to firebase storage
blob = bucket.get_blob('image.jpg')
blob.download_to_filename("/home/pi/firebase_testing/downloaded.jpg")

