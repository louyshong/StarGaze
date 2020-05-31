from picamera import PiCamera
import time

camera = PiCamera()

def take_photo(image_name) :
    # assign camera
    camera.start_preview()
    time.sleep(2)
    camera.capture("images/"+image_name+".jpg")
    camera.stop_preview()
    print("photo taken")
