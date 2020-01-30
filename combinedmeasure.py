import smbus		
import time 
import math

#Magnetometer config
Magnetometer_Address = 0x1e

Register_A     = 0              # Address of Configuration register A
Register_B     = 0x01           # Address of configuration register B
Register_mode  = 0x02           # Address of mode register

X_axis_H    = 0x03              # Address of X-axis MSB data register
Z_axis_H    = 0x05              # Address of Z-axis MSB data register
Y_axis_H    = 0x07              # Address of Y-axis MSB data register

A_accurate = 0x70               # 8 sample avergage, at 15Hz output rate
B_high_gain = 0xa0              # high gain output
M_continuous = 0                # continuous output

#Accelerometer config
Accelerometer_Address = 0x18

Register_One     = 0x20          # Address of Control register 1
Register_Four    = 0x23          # Address of Control register 4

X_axis    = 0x28            # Address of X-axis MSB data register
Z_axis    = 0x2c            # Address of Z-axis MSB data register
Y_axis    = 0x2a            # Address of Y-axis MSB data register

Reg_One_Config = 0x27       # 8 sample avergage, at 15Hz output rate
Reg_Four_Config = 0x00


def magnetometer_init():
    # Write to Configuration Register A
    bus.write_byte_data(Magnetometer_Address, Register_A, A_accurate)

    # Write to Configuration Register B for gain
    bus.write_byte_data(Magnetometer_Address, Register_B, B_high_gain)

    # Write to mode Register for selecting mode
    bus.write_byte_data(Magnetometer_Address, Register_mode, M_continuous)


def read_magnetometer_data(addr):

    # Read raw 16-bit value
    high = bus.read_byte_data(Magnetometer_Address, addr)
    low = bus.read_byte_data(Magnetometer_Address, addr+1)

    # concatenate higher and lower value
    value = ((high << 8) | low)

    # to get signed value from module
    if(value > 32768):
        value = value - 65536
    return value

def calibrate_magnetometer():

    sampleDuration = 60.0    # test lasts for 60 seconds
    sampleRate = 3.0          # 3Hz sample rate
    sleep_duration = 1 / sampleRate

    magnetometer_init()     # initialize HMC5883L magnetometer

    start_time = time.time()    # note the start start_time
    compassValuesList = []  # initialse list to store data

    print("------------ Calibrating magnetometer ------------------")

    while (time.time() - start_time <= sampleDuration):

        # Read sensor raw value
        x = read_magnetometer_data(X_axis_H)
        z = read_magnetometer_data(Z_axis_H)
        y = read_magnetometer_data(Y_axis_H)

        compassValues = {}
        compassValues["X"] = x
        compassValues["Y"] = y
        compassValues["Z"] = z
        compassValuesList.append(compassValues)

        time.sleep(sleep_duration)

    print("------------ Done reading data ------------------")


    print("------------ Processing data ------------------")

    max_x = float("-inf")
    max_y = float("-inf")
    max_z = float("-inf")
    min_x = float("inf")
    min_y = float("inf")
    min_z = float("inf")

    for i in range(len(compassValuesList)):
        x = compassValuesList[i]['X']
        y = compassValuesList[i]['Y']
        z = compassValuesList[i]['Z']

        max_x = x if x > max_x else max_x
        max_y = y if y > max_y else max_y
        max_z = z if z > max_z else max_z
        min_x = x if x < min_x else min_x
        min_y = y if y < min_y else min_y
        min_z = z if z < min_z else min_z

    offset_x = (max_x + min_x) / 2
    offset_y = (max_y + min_y) / 2
    offset_z = (max_z + min_z) / 2
    range_x = max_x - min_x
    range_y = max_y - min_y
    range_z = max_z - min_z

    print("X offset is: %f" % offset_x)
    print("Y offset is: %f" % offset_y)
    print("Z offset is: %f" % offset_z)
    print("X Range is: %f" % range_x)
    print("Y Range is: %f" % range_y) 
    print("Z Range is: %f" % range_z) 

    print("------------ Done calibrating magnetometer ------------------")

    return(offset_x, offset_y, offset_z, range_x, range_y, range_z)

def get_heading(declination, x_offset, y_offset, z_offset, x_scale, y_scale, z_scale, angle_adjust):
    # Read sensor raw value
    x = (read_magnetometer_data(X_axis_H) - x_offset) * x_scale
    y = (read_magnetometer_data(Y_axis_H) - y_offset) * y_scale
    z = (read_magnetometer_data(Z_axis_H) - z_offset) * z_scale
   
   # Adjust using reading from accelerometer (angle_adjust is given in degrees)
    x_h = x * math.cos(angle_adjust * (math.pi/180))
    z_h = z * math.cos((math.pi/2) - (angle_adjust * (math.pi/180)))

    x_adj = x_h + z_h

    heading = math.atan2(y, x_adj) + declination

    # Due to declination check for >360 degree
    if(heading > 2 * (math.pi)):
        heading = heading - 2 * (math.pi)

    # check for sign
    if(heading < 0):
        heading = heading + 2 * (math.pi)

    # convert into degrees
    heading_angle = int(heading * 180/(math.pi))

    return heading_angle

def accelerometer_init():
    # Write to Control Register 1
    bus.write_byte_data(Accelerometer_Address, Register_One, Reg_One_Config)

    # Write to Control Register 4
    bus.write_byte_data(Accelerometer_Address, Register_Four, Reg_Four_Config)

def read_accelerometer_data():

    # LIS3DHTR address, 0x18(24)
    # Read data back from 0x28(40), 2 bytes
    # X-Axis LSB, X-Axis MSB
    data0 = bus.read_byte_data(0x18, 0x28)
    data1 = bus.read_byte_data(0x18, 0x29)

    # Convert the data
    xAccl = data1 * 256 + data0
    if xAccl > 32767 :
    	xAccl -= 65536

    # LIS3DHTR address, 0x18(24)
    # Read data back from 0x2A(42), 2 bytes
    # Y-Axis LSB, Y-Axis MSB
    data0 = bus.read_byte_data(0x18, 0x2A)
    data1 = bus.read_byte_data(0x18, 0x2B)

    yAccl = data1 * 256 + data0
    if yAccl > 32767 :
    	yAccl -= 65536

    # LIS3DHTR address, 0x18(24)
    # Read data back from 0x2C(44), 2 bytes
    # Z-Axis LSB, Z-Axis MSB
    data0 = bus.read_byte_data(0x18, 0x2C)
    data1 = bus.read_byte_data(0x18, 0x2D)

    zAccl = data1 * 256 + data0
    if zAccl > 32767 :
    	zAccl -= 65536

    return (xAccl, yAccl, zAccl)

def calibrate_accelerometer():

    sampleDuration = 20.0    # test lasts for 60,000ms (20 seconds)
    sampleRate = 3.0         # 3Hz sample rate
    sleep_duration = 1 / sampleRate

    accelerometer_init()     

    start_time = time.time()    
    accelerationValuesList = []  # initialse list to store data

    print("------------ Calibrating accelerometer ------------------")

    while (time.time() - start_time <= sampleDuration):

        # Read sensor raw value
        accelerationValue = read_accelerometer_data()

        accelerationValuesList.append(accelerationValue)

        time.sleep(sleep_duration)

    print("------------ Done reading data ------------------")


    print("------------ Processing data ------------------")

    x = 0
    y = 0
    z = 0

    for i in range(len(accelerationValuesList)):
        x = accelerationValuesList[i][0]
        y = accelerationValuesList[i][1]
        z = accelerationValuesList[i][2]

        sum_x = x + x
        sum_y = y + y
        sum_z = z + z

    baseline_x_value = sum_x / (len(accelerationValuesList))
    baseline_y_value = sum_y / (len(accelerationValuesList))
    baseline_z_value = sum_z / (len(accelerationValuesList))

    print("X baseline is: %f" % baseline_x_value)
    print("Y baseline is: %f" % baseline_y_value)
    print("Z baseline is : %f" % baseline_z_value)
    print("------------ Done calibrating accelerometer ------------------")

    return (baseline_x_value, baseline_y_value, baseline_z_value)

#get_angle returns angle in radians 
def get_angle(baselinex, baseliney, baselinez):

    Accl = read_accelerometer_data()

    #calculate angle using formula
    x_val = Accl[0] - baselinex
    y_val = Accl[1] - baseliney
    z_val = Accl[2] - baselinez

    y2 = y_val * y_val
    z2 = z_val * z_val

    #X Axis
    result = math.sqrt(y2 + z2)
    result = x_val / result
    accel_angle_x = math.atan(result)

    #convert angle to degrees 
    angle_degrees = accel_angle_x * (180 / math.pi)
    
    #Output angle
    return (angle_degrees)

if __name__ == "__main__":

        #Bus instance
        bus = smbus.SMBus(1)    

        mag_cal_offsets = calibrate_magnetometer()
        offset_x = mag_cal_offsets[0]
        offset_y = mag_cal_offsets[1]
        offset_z = mag_cal_offsets[2]
        range_x = mag_cal_offsets[3]
        range_y = mag_cal_offsets[4]
        range_z = mag_cal_offsets[5]

        declination = 0.000872665

        print("put the pi down")
        time.sleep(5) #5 seconds to put pi down (for accelerometer to calibrate)

        acc_cal_baseline = calibrate_accelerometer()

        while True:
            angle = get_angle(acc_cal_baseline[0], acc_cal_baseline[1], acc_cal_baseline[2])

            time.sleep(1)
            heading = get_heading(declination, offset_x, offset_y, offset_z, 360/range_x, 360/range_y, 360/range_z, angle)

            print("Heading is: %5.2f" %heading)
            print("Angle is: %5.2f" %angle)
            time.sleep(2)
