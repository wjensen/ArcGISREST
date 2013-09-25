var geometry = new PermitRenewalLocation.PermitLocationGeometry
					{
						x = permitLocation.Longitude,
						y = permitLocation.Latitude,
						spatialReference = new PermitRenewalLocation.PermitLocationSpatialReference {wkid = 4326}
					};

				double[] point = GISHelper.ToWebMercator(permitLocation.Longitude, permitLocation.Latitude);

				var mapextent = new MapExtent
					{
						xmin = point[0] - 686.7389262001960,
						ymin = point[1] - 764.3702829899260,
						xmax = point[0] + 686.7389262001960,
						ymax = point[1] + 764.3702829899260,
						spatialReference = new MapExtent.MapextentSpatialReference {wkid = 102100}
					};
				IdentifyResultList irl = Identify(geometry, mapextent);

private static IdentifyResultList Identify(object geometry, object mapextent)
		{
			var gjson = GISHelper.SerializeToJson(geometry);
			var mejson = GISHelper.SerializeToJson(mapextent);
			var query = string.Format(ConfigurationManager.AppSettings["AGSRESTService"] +
				"identify?" +
			                             "f=json" +
			                             "&geometry={0}" +
			                             "&tolerance=1" +
			                             "&returnGeometry=false" +
			                             "&mapExtent={1}" +
			                             "&imageDisplay=400%2C400%2C96" +
			                             "&geometryType=esriGeometryPoint" +
			                             "&sr=4326" +
			                             "&layers=all", HttpUtility.UrlEncode(gjson), HttpUtility.UrlEncode(mejson));
			var webrequest = WebRequest.Create(query);
			var objStream = webrequest.GetResponse().GetResponseStream();
			var objReader = new StreamReader(objStream);
			var identify = objReader.ReadToEnd();
			identify = identify.Replace("\"results\":", "\"IdentifyResult\":");
			var js = new JavaScriptSerializer();
			return js.Deserialize<IdentifyResultList>(identify);
		}
		
public static class GISHelper
	{
		public static string SerializeToJson(object obj)
		{
			var jser = new JavaScriptSerializer();
			return jser.Serialize(obj);
		}

		public static double[] ToGeographic(ref double mercatorXLon, ref double mercatorYLat)
		{
			var point = new double[2];
			if (Math.Abs(mercatorXLon) < 180 && Math.Abs(mercatorYLat) < 90)
				return new double[] {};

			if ((Math.Abs(mercatorXLon) > 20037508.3427892) || (Math.Abs(mercatorYLat) > 20037508.3427892))
				return new double[] {};

			double x = mercatorXLon;
			double y = mercatorYLat;
			double num3 = x/6378137.0;
			double num4 = num3*57.295779513082323;
			double num5 = Math.Floor(((num4 + 180.0)/360.0));
			double num6 = num4 - (num5*360.0);
			double num7 = 1.5707963267948966 - (2.0*Math.Atan(Math.Exp((-1.0*y)/6378137.0)));
			point[0] = num6;
			point[1] = num7*57.295779513082323;
			return point;
		}

		public static double[] ToWebMercator(double mercatorXLon, double mercatorYLat)
		{
			var point = new double[2];
			if ((Math.Abs(mercatorXLon) > 180 || Math.Abs(mercatorYLat) > 90))
				return new double[] {};

			double num = mercatorXLon*0.017453292519943295;
			double x = 6378137.0*num;
			double a = mercatorYLat*0.017453292519943295;

			point[0] = x;
			point[1] = 3189068.5*Math.Log((1.0 + Math.Sin(a))/(1.0 - Math.Sin(a)));
			return point;
		}
	}