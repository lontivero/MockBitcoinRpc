using NBitcoin;
using Newtonsoft.Json;
using System;

namespace MockBitcoinRpc.JsonConverters
{
	public class BitcoinAddressJsonConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return typeof(BitcoinAddress).IsAssignableFrom(objectType);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var bitcoinAddressString = reader.Value as string;
			return BitcoinAddress.Create(bitcoinAddressString, BitcoinNode.Network);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var bitcoinAddress = value as BitcoinAddress;

			writer.WriteValue(bitcoinAddress.ToString());
		}
	}
}
