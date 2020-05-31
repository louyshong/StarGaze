from firebase import firebase
from google.cloud import storage
import os
    
# Real-time Database URl
real_time_url = "https://stargaze-embedded.firebaseio.com/"

# Storage Database URL
storage_url = "stargaze-embedded.appspot.com"

os.environ["GOOGLE_APPLICATION_CREDENTIALS"]="/home/pi/stargaze-firebase-adminsdk.json"
firebase = firebase.FirebaseApplication(real_time_url)
client = storage.Client()
bucket = client.get_bucket(storage_url) # Dont forget to remove gs:// from the storage database link

def upload (image_name):
    # imagePath = [os.path.join(self.path,f) for f in os.listdir(self.path)]
    imagePath = ("/home/pi/images/" + image_name + ".jpg") #where your image is saved
    imageBlob = bucket.blob("images/"+ image_name + ".jpg") # the name you want to save your image as on firebase
    imageBlob.upload_from_filename(imagePath)
