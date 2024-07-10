using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;

[DataContract]
public class ImageInfo
{
	public ImageInfo(string name, int x, int y, int heihgt, int width)
	{
		this.name = name;
		this.x = x;
		this.y = y;
		this.height = height;
		this.width = width;
	}

	[DataMember(Order = 1)]
	public string name { get; set; }

	[DataMember(Order = 2)]
	public int x { get; set; }

	[DataMember(Order = 3)]
	public int y { get; set; }

	[DataMember(Order = 4)]
	public int height { get; set; }

	[DataMember(Order = 5)]
	public int width { get; set; }
}

[DataContract]
public class ImageInfoCollection
{
	[DataMember]
	public ImageInfo[] Images { get; set; }

	public static ImageInfoCollection FromJsonString(string jsonString)
	{
		ImageInfoCollection retVal = null;
		using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonString)))
		{
			DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(ImageInfoCollection));
			retVal = (ImageInfoCollection)ser.ReadObject(ms);
		}
		return retVal;
	}

	public static Dictionary<string,ImageInfo> CreateAsDictionary(ImageInfoCollection imageInfoCollection)
	{
		Dictionary<string,ImageInfo> retVal = new Dictionary<string, ImageInfo> (imageInfoCollection.Images.Length);
		foreach (ImageInfo imageInfo in imageInfoCollection.Images) 
		{
			if(! retVal.ContainsKey(imageInfo.name))
				retVal.Add(imageInfo.name, imageInfo);
		}
		return retVal;
	}

	public static string ToJsonString(ImageInfoCollection ic)
	{
		using (MemoryStream ms = new MemoryStream())
		{
			DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(ImageInfoCollection));
			ser.WriteObject(ms, ic);
			return Encoding.UTF8.GetString(ms.ToArray());
		}
	}
}