import paho.mqtt.client as mqtt
import ssl
import json
from time import sleep
from raspicam import take_photo 
from upload_img import upload
import Sensors as s
import datetime
from coordinates import GetImageData, LocalSiderealTime

def on_message(client, userdata, message) :
    print("Received message:{} on topic {}".format(message.payload, message.topic))
    if(message.payload == b'take photo'):
        sensor_data = sensors.get_readings()
        image_data = GetImageData(sensor_data[1],sensor_data[0])
        d=datetime.datetime.today()
        image_name = d.strftime("%d-%b-%Y (%H:%M:%S.%f)")
        localsideralday =  LocalSiderealTime(image_data[0],image_data[1])
        image_data_json = {
            'Altitude': image_data[3],
            'Azimuth': image_data[2],
            'Latitude': image_data[0],
            'Longitude': image_data[1],
            'LocalSideralDay': localsideralday,
            'YearTaken':d.year,
            'MonthTaken': d.month,
            'DayTaken': d.day,
            'HourTaken': d.hour,
            'MinuteTaken': d.minute,
            'ImageName': image_name
        }
        image_data_string = json.dumps(image_data_json)
        take_photo(image_name)
        mqtt_publish('camera',image_data_string)
        upload(image_name)
        print("image uploaded successfully")

def mqtt_setup ():
    client.tls_set(ca_certs="mosquitto.org.crt", certfile="client.crt",keyfile="client.key",tls_version=ssl.PROTOCOL_TLSv1_2)
    client.connect("test.mosquitto.org",port=8884)
    client.subscribe("IC.embedded/friends/pi")
    
def mqtt_start ():
    client.loop_start()
    print("listening for messages")
    
def mqtt_stop ():
    client.loop_stop()
    print("stopped listening for messages")
    
def mqtt_publish (topic, message):
    client.publish("IC.embedded/friends/unity/"+topic,message)
    mqtt.error_string(mqtt.MQTTMessageInfo.rc)

client = mqtt.Client()
client.on_message = on_message

mqtt_setup()
mqtt_start()

sensors = s.Sensors()

while True: 
    sensor_data = sensors.get_readings()
    sensor_data_json = {
        'Altitude': sensor_data[0],
        'Azimuth': sensor_data[1]
    }
    mqtt_publish ('sensors',json.dumps(sensor_data_json))
    sleep(60)
