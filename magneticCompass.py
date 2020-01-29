'''
    Find Heading by using HMC5883L interface with Raspberry Pi using Python
	http://www.electronicwings.com
'''
import smbus		# import SMBus module of I2C
import time as time
from time import sleep  # import sleep
import math

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
B_high_gain = 0xa0              # high gain output
M_continuous = 0                # condinuous output module


# calibration variables
x_offset = 0
y_offset = 0
z_offset = 0

x_scale = 1.0
y_scale = 1.0


# state monitoring
initialised = False
bus = smbus.SMBus(1) 	# or bus = smbus.SMBus(0) for older version boards


def Magnetometer_Init():
    # Write to Configuration Register A
    bus.write_byte_data(Device_Address, Register_A, A_accurate)

    # Write to Configuration Register B for gain
    bus.write_byte_data(Device_Address, Register_B, B_high_gain)

    # Write to mode Register for selecting mode
    bus.write_byte_data(Device_Address, Register_mode, M_continuous)

    initialised = True


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


def get_heading():
    if initialised is False:
        Magnetometer_Init()

    # Read sensor raw value
    x = (read_raw_data(X_axis_H) - x_offset) * x_scale
    # z = read_raw_data(Z_axis_H) - z_offset
    y = (read_raw_data(Y_axis_H) - y_offset) * y_scale

    heading = math.atan2(y, x) + declination

    # Due to declination check for >360 degree
    if(heading > 2*pi):
        heading = heading - 2*pi

    # check for sign
    if(heading < 0):
        heading = heading + 2*pi

    # convert into angle
    heading_angle = int(heading * 180/pi)

    return heading_angle


if __name__ == "__main__":
    while True:
        heading = get_heading()
        print("Heading is: %5.2f" % heading)
