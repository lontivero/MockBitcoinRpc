using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace MockBitcoinRpc.JsonConverters
{
	public class DateTimeOffsetUnixSecondsConverter : JsonConverter
	{
		 
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(DateTimeOffset);
		}

		 
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var stringValue = reader.Value as string;
			if (string.IsNullOrWhiteSpace(stringValue))
			{
				return default(DateTimeOffset);
			}
			else
			{
				return DateTimeOffset.FromUnixTimeSeconds(long.Parse(stringValue));
			}
		}

		 
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			writer.WriteValue(((DateTimeOffset)value).ToUnixTimeSeconds());
		}
	}
}
