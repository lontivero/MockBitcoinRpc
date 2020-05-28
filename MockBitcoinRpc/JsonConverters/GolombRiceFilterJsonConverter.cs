using NBitcoin;
using NBitcoin.DataEncoders;
using Newtonsoft.Json;
using System;

namespace MockBitcoinRpc.JsonConverters
{
	public class GolombRiceFilterJsonConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(GolombRiceFilter);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var value = reader.ReadAsString();

			var data = Encoders.Hex.DecodeData(value);
			return data.Length == 0 ? null : new GolombRiceFilter(data, 20, 1 << 20);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			writer.WriteValue(((GolombRiceFilter)value).ToString());
		}
	}
}
