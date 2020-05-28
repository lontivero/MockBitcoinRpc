using NBitcoin;
using Newtonsoft.Json;
using System;

namespace MockBitcoinRpc.JsonConverters
{
	public class NetworkJsonConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(Network);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			// check additional strings that are not checked by GetNetwork
			string networkString = ((string)reader.Value).Trim();
			if ("regression".Equals(networkString, StringComparison.OrdinalIgnoreCase))
			{
				return Network.RegTest;
			}

			return Network.GetNetwork(networkString);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			writer.WriteValue(((Network)value).ToString());
		}
	}
}
