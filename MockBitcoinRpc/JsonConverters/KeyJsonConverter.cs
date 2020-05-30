using NBitcoin;
using Newtonsoft.Json;
using System;

namespace MockBitcoinRpc.JsonConverters
{
	public class KeyJsonConverter : JsonConverter
	{
		private Network _network;

		public KeyJsonConverter(Network network)
		{
			_network = network;
		}

		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(Key);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var keyString = reader.Value as string;
			return Key.Parse(keyString, _network);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var key = (Key)value;
			writer.WriteValue(key.GetWif(_network));
		}
	}
}
