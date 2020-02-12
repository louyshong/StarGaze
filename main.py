import Sensors as s
import time

sensors = s.Sensors()

while True: 
    sensors.get_readings()
    time.sleep(2)
