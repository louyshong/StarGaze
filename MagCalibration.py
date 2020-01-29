'''
    Find Heading by using HMC5883L interface with Raspberry Pi using Python
	http://www.electronicwings.com
'''
import smbus		# import SMBus module of I2C
import time as time
from time import sleep  # import sleep
import math
import csv

# HMC5883L magnetometer device address
Device_Address = 0x1e

# some MPU6050 Registers and their Address
Register_A     = 0              # Address of Configuration register A
Register_B     = 0x01           # Address of configuration register B
Register_mode  = 0x02           # Address of mode register

X_axis_H    = 0x03              # Address of X-axis MSB data register
Z_axis_H    = 0x05              # Address of Z-axis MSB data register
Y_axis_H    = 0x07              # Address of Y-axis MSB data register
declination = -0.00669          # define declination angle of location where measurement going to be done
pi          = 3.14159265359     # define pi value


# device configureation
A_accurate = 0x70                   # 8 sample avergage, at 15Hz output rate
A_fast = 0x38

B_high_gain = 0xa0              # high gain output

M_continuous = 0                # condinuous output module


def Magnetometer_Init():
    # Write to Configuration Register A
    bus.write_byte_data(Device_Address, Register_A, A_accurate)

    # Write to Configuration Register B for gain
    bus.write_byte_data(Device_Address, Register_B, B_high_gain)

    # Write to mode Register for selecting mode
    bus.write_byte_data(Device_Address, Register_mode, M_continuous)



def read_raw_data(addr):

    # Read raw 16-bit value
    high = bus.read_byte_data(Device_Address, addr)
    low = bus.read_byte_data(Device_Address, addr+1)

    # concatenate higher and lower value
    value = ((high << 8) | low)

    # to get signed value from module
    if(value > 32768):
        value = value - 65536
    return value



if __name__ == "__main__":
    bus = smbus.SMBus(1) 	# or bus = smbus.SMBus(0) for older version boards

    sampleDuration = 60.0    # test lasts for 60,000ms (60 seconds)
    sampleRate = 3.0          # 3Hz sample rate
    sleep_duration = 1 / sampleRate

    Magnetometer_Init()     # initialize HMC5883L magnetometer

    start_time = time.time()    # note the start start_time
    compassValuesList = []  # initialse list to store data

    print("------------Reading sensor data------------------")

    while (time.time() - start_time <= sampleDuration):

        # Read sensor raw value
        x = read_raw_data(X_axis_H)
        z = read_raw_data(Z_axis_H)
        y = read_raw_data(Y_axis_H)

        compassValues = {}
        compassValues["X"] = x
        compassValues["Y"] = y
        compassValues["Z"] = z
        compassValuesList.append(compassValues)

        sleep(sleep_duration)

    print("------------- Done reading data -------------")


    print("------------- Processing Data --------------")

    max_x = float("-inf")
    max_y = float("-inf")
    min_x = float("inf")
    min_y = float("inf")

    for i in range(len(compassValuesList)):
        x = compassValuesList[i]['X']
        y = compassValuesList[i]['Y']
        z = compassValuesList[i]['Z']

        max_x = x if x > max_x else max_x
        max_y = y if y > max_y else max_y
        min_x = x if x < min_x else min_x
        min_y = y if y < min_y else min_y

    offset_x = (max_x + min_x) / 2
    offset_y = (max_y + min_y) / 2
    range_x = max_x - min_x
    range_y = max_y - min_y

    print("X offset is: %f" % offset_x)
    print("Y offset is: %f" % offset_y)
    print("X Range is: %f" % range_x)
    print("Y Range is: %f" % range_y)

    outputFilename = "Compass_values_{0}.csv".format(int(time.time()))
    with open(outputFilename, "w") as csvFile:
        dictWriter = csv.DictWriter(csvFile, ["X", "Y", "Z"])
        dictWriter.writeheader()
        dictWriter.writerows(compassValuesList)
        print("----------- Saved to CVS ------------")
