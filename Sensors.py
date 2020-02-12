import smbus
import time
import math

class Sensors:

    def __init__(self): 
        self.bus = smbus.SMBus(1)

        #Magnetometer config
        self.Magnetometer_Address = 0x1e
        self.Register_A = 0              #Address of Configuration register A
        self.Register_B = 0x01           #Address of configuration register B
        self.Register_mode = 0x02        #Address of mode register
        self.X_axis_H = 0x03             #Address of X-axis MSB data register
        self.Z_axis_H = 0x05             #Address of Z-axis MSB data register
        self.Y_axis_H = 0x07             #Address of Y-axis MSB data register
        self.A_accurate = 0x70           #8 sample avergage, at 15Hz output rate
        self.B_high_gain = 0xa0          #High gain output
        self.M_continuous = 0            #Continuous output

        #Accelerometer config
        self.Accelerometer_Address = 0x18
        self.Register_One = 0x20         #Address of Control register 1
        self.Register_Four = 0x23        #Address of Control register 4
        self.X_axis    = 0x28            #Address of X-axis MSB data register
        self.Z_axis    = 0x2c            #Address of Z-axis MSB data register
        self.Y_axis    = 0x2a            #Address of Y-axis MSB data register
        self.Reg_One_Config = 0x27       #8 sample avergage, at 15Hz output rate
        self.Reg_Four_Config = 0x00

        mag_cal_offsets = self.calibrate_magnetometer()
        self.offset_x = mag_cal_offsets[0]
        self.offset_y = mag_cal_offsets[1]
        self.offset_z = mag_cal_offsets[2]
        self.range_x = mag_cal_offsets[3]
        self.range_y = mag_cal_offsets[4]
        self.range_z = mag_cal_offsets[5]

        self.declination = 0.000581776

        print("put the pi down")
        time.sleep(5) #5 seconds to put pi on a level surface (for accelerometer to calibrate)

        self.acc_cal_baseline = self.calibrate_accelerometer()


    def magnetometer_init(self):
        self.bus.write_byte_data(self.Magnetometer_Address, self.Register_A, self.A_accurate)
        self.bus.write_byte_data(self.Magnetometer_Address, self.Register_B, self.B_high_gain)
        self.bus.write_byte_data(self.Magnetometer_Address, self.Register_mode, self.M_continuous)

    def read_magnetometer_data(self, addr):

        high = self.bus.read_byte_data(self.Magnetometer_Address, addr)
        low = self.bus.read_byte_data(self.Magnetometer_Address, addr+1)

        #Concatenate higher and lower value (by shifting MSB 8 bits then OR with the LSB bits)
        value = ((high << 8) | low)
        #To get signed value from module
        if(value > 32768):
            value = value - 65536

        return value

    def calibrate_magnetometer(self):

        sampleDuration = 60.0    #Test lasts for 60 seconds
        sampleRate = 3.0         #3Hz sample rate
        sleep_duration = 1 / sampleRate
        self.magnetometer_init() #Initialize HMC5883L magnetometer

        start_time = time.time()
        compassValuesList = []
        print("------------ Calibrating magnetometer ------------------")
        while (time.time() - start_time <= sampleDuration):
            x = self.read_magnetometer_data(self.X_axis_H)
            z = self.read_magnetometer_data(self.Z_axis_H)
            y = self.read_magnetometer_data(self.Y_axis_H)

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

    def get_heading(self, declination, x_offset, y_offset, z_offset, x_scale, y_scale, z_scale, angle_adjust):
        
        x = (self.read_magnetometer_data(self.X_axis_H) - x_offset) * x_scale
        y = (self.read_magnetometer_data(self.Y_axis_H) - y_offset) * y_scale
        z = (self.read_magnetometer_data(self.Z_axis_H) - z_offset) * z_scale

       #Adjust using reading from accelerometer (angle_adjust (pitch, roll) tuple is given in degrees)
       #Note that this configuration is such that the y axis on the HMC5883L is aligned with the x axis on the LIS3DH
       #(This is due to limitations of the two chips being mounted on separate modules with different orientations)

        (pitch, roll) = angle_adjust

        x_adj = y * math.cos(pitch * (math.pi/180)) - x * math.sin(roll * (math.pi/180)) * math.sin(pitch * (math.pi/180)) + z * math.cos(roll * (math.pi/180)) * math.sin(pitch * (math.pi/180))
        y_adj = - x * math.cos(roll * (math.pi/180)) - z * math.sin(roll * (math.pi/180))

        heading = math.atan2(y_adj, x_adj) + declination

        if(heading > 2 * (math.pi)):
            heading = heading - 2 * (math.pi)

        if(heading < 0):
            heading = heading + 2 * (math.pi)

        #Convert into degrees
        heading_angle = int(heading * 180/(math.pi))

        return heading_angle

    def accelerometer_init(self):
        self.bus.write_byte_data(self.Accelerometer_Address, self.Register_One, self.Reg_One_Config)
        self.bus.write_byte_data(self.Accelerometer_Address, self.Register_Four, self.Reg_Four_Config)

    def read_accelerometer_data(self):
        #X-Axis LSB, X-Axis MSB
        data0 = self.bus.read_byte_data(0x18, 0x28)
        data1 = self.bus.read_byte_data(0x18, 0x29)

        #Convert the data
        xAccl = data1 * 256 + data0
        if xAccl > 32767 :
        	xAccl -= 65536

        data0 = self.bus.read_byte_data(0x18, 0x2A)
        data1 = self.bus.read_byte_data(0x18, 0x2B)

        yAccl = data1 * 256 + data0
        if yAccl > 32767 :
        	yAccl -= 65536

        data0 = self.bus.read_byte_data(0x18, 0x2C)
        data1 = self.bus.read_byte_data(0x18, 0x2D)

        zAccl = data1 * 256 + data0
        if zAccl > 32767 :
        	zAccl -= 65536

        return (xAccl, yAccl, zAccl)

    def calibrate_accelerometer(self):

        sampleDuration = 20.0    #Test lasts for 20 seconds
        sampleRate = 3.0         #3Hz sample rate
        sleep_duration = 1 / sampleRate

        self.accelerometer_init()

        start_time = time.time()
        accelerationValuesList = []

        print("------------ Calibrating accelerometer ------------------")

        while (time.time() - start_time <= sampleDuration):
            accelerationValue = self.read_accelerometer_data()
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
    def get_angle(self, baselinex, baseliney, baselinez):

        Accl = self.read_accelerometer_data()

        #Calculate angle using formula
        x_val = Accl[0] - baselinex
        y_val = Accl[1] - baseliney
        z_val = Accl[2] - baselinez

        y2 = y_val * y_val
        z2 = z_val * z_val
        x2 = x_val * x_val

        #X Axis
        result = math.sqrt(y2 + z2)
        result = -x_val / result
        pitch = math.atan(result)

        #Y Axis
        result = math.sqrt(x2 + z2)
        result = y_val / result
        roll = math.atan(result)

        #Convert angle to degrees
        pitch = pitch * (180 / math.pi)
        roll = roll * (180 / math.pi)

        #Output angle
        return (pitch, roll)

    def get_readings(self):

        angle = self.get_angle(self.acc_cal_baseline[0], self.acc_cal_baseline[1], self.acc_cal_baseline[2])
        heading = self.get_heading(self.declination, self.offset_x, self.offset_y, self.offset_z, 360/self.range_x, 360/self.range_y, 360/self.range_z, angle)

        tilt = -angle[0]
        print("Heading is: %5.2f" %heading)
        print("Angle is: %5.2f" %tilt)
        return (tilt, heading)