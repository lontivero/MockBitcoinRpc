using NBitcoin;
using Newtonsoft.Json;
using System;

namespace MockBitcoinRpc.JsonConverters
{
	public class PubKeyJsonConverter : JsonConverter
	{
		 
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(PubKey);
		}

		 
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			return new PubKey(((string)reader.Value).Trim());
		}

		 
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var pubKey = (PubKey)value;
			writer.WriteValue(pubKey.ToHex());
		}
	}
}
