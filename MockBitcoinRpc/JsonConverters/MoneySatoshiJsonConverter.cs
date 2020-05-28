using NBitcoin;
using Newtonsoft.Json;
using System;

namespace MockBitcoinRpc.JsonConverters
{
	public class MoneySatoshiJsonConverter : JsonConverter
	{
		 
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(Money);
		}

		 
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var serialized = (long)reader.Value;

			return new Money(serialized);
		}

		 
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var money = (Money)value;

			writer.WriteValue(money.Satoshi);
		}
	}
}
