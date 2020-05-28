using NBitcoin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MockBitcoinRpc.JsonConverters
{
	public class KeyPathJsonConverter : JsonConverter
	{
		 
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(KeyPath);
		}

		 
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var s = (string)reader.Value;
			if (string.IsNullOrWhiteSpace(s))
			{
				return null;
			}
			var kp = KeyPath.Parse(s.Trim());

			return kp;
		}

		 
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var kp = (KeyPath)value;

			var s = kp.ToString();
			writer.WriteValue(s);
		}
	}
}
