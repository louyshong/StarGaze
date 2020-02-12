import datetime
import numpy as np
import ephem as ep
import geocoder

#----------------- Helper Methods ----------------

def Sin(x):
    return np.sin(x)

def Cos(x):
    return np.cos(x)

def ATan(x):
    return np.arctan(x)

def ASin(x):
    return np.arcsin(x)

def ACos(x):
    return np.arccos(x)



#-------------------- Main Methods --------------------

def CurrentRotation():

    # get current greenwich sidereal time
    greenwich_sidereal_time = LocalSiderealTime(51.4934, 0.0)

    # find the hour angle
    hour_angle = float(greenwich_sidereal_time)

    return hour_angle

def GetImageName(az, alt):
    # get global position
    g = geocoder.ip('me')
    latitude = g.latlng[0]
    longitude = g.latlng[1]

    # convert to celstial coordinates
    return ToCelestialCoordinates(latitude, longitude, az, alt)

# Name format:
#       Latitude_Longitude_dec_ra_(time).jpg
#       Time = 'hour' + h + 'minute' + m
def ToCelestialCoordinates(lat, long, az, alt):
    # Key:
    #     ------------ Local Coordinates -------------
    #     az = azimuth - angle around the horizon from due north to under POI (north has azimuth of 0m south has azimuth of 180)
    #     alt = altitude - angle from the horizon where the object is.
    #     lat = latitude
    #     long = Longitude
    #     lst = local sideral time - Greenwich meridian plus Longitude
    #     lha = local hour angle -  It is the angle between the meridian of your Assumed Position and the meridian of the geographical position of the celestial body.
    #     ------------ Equatorial Coordinates ---------------
    #     ra = right ascention
    #     dec = declination angle
    #     ha = hour angle - angle between local meridian projected on celestial sphere, and right ascention of body
    #
    #
    # CALCULATING: 'right ascension' (ra) and 'declination' (dec)
    # 1) sin(alt) = sin(dec)sin(lat) + cos(dec)cos(lat)cos(ha)
    # 2) sin(ha) =  tan(az)[cos(ha)sin(lat) - tan(dec)cos(lat)]
    # 3) ra = lst - ha
    #
    # Rearranging leads to:
    # ha = arctan(x,y)
    # x = sin(lat)cos(alt)cos(az) + cos(lat)sin(alt)
    # y = cos(alt)sin(az)
    #
    # dec = arctan(x', y')
    # x' = sin(az)cos(ha)sin(lat) - sin(ha)cos(az)
    # y' = cos(lat)sin(az)

    lst = LocalSiderealTime(lat, long)

    ha = Sin(lat)*Cos(alt)*Cos(az) + Cos(lat)*Sin(alt)
    ha /= Cos(alt)*Sin(az)
    ha = ATan(ha)

    dec = Sin(az)*Cos(ha)*Sin(lat) - Sin(ha)*Cos(az)
    dec /= Cos(lat)*Sin(az)
    dec = ATan(dec)

    ra = lst - ha

    return str(dec) + '_' + str(ra)


def LocalSiderealTime(latitude, longitude):
    # get the current time
    timeNow = datetime.datetime.now()

    #format data to pass to ephem
    dateObj = ep.Observer()
    dateObj.lon, dateObj.lat = '{}'.format(longitude), '{}'.format(latitude)
    dateObj.date = '{}/{}/{} {}:{}'.format(timeNow.year, timeNow.month, timeNow.day, timeNow.hour, timeNow.minute)

    # find s
    lst = dateObj.sidereal_time()
    return lst


answer = ToCelestialCoordinates(0,0)
