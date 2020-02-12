import datetime
import geocoder
import ephem as ep

def GetImageData(az, alt):
    # get global position
    g = geocoder.ip('me')
    latitude = g.latlng[0]
    longitude = g.latlng[1]

    # convert to celstial coordinates
    return (latitude, longitude, az, alt)

def LocalSiderealTime(latitude, longitude):
    # get the current time
    timeNow = datetime.datetime.now()
    #format data to pass to ephem
    dateObj = ep.Observer()
    dateObj.lon, dateObj.lat = '{}'.format(longitude), '{}'.format(latitude)
    dateObj.date = '{}/{}/{} {}:{}'.format(timeNow.year, timeNow.month, timeNow.day, timeNow.hour, timeNow.minute)
    # find s
    lst = dateObj.sidereal_time()
    return str(lst)

