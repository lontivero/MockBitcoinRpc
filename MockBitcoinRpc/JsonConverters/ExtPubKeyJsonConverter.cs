using NBitcoin;
using Newtonsoft.Json;
using System;

namespace MockBitcoinRpc.JsonConverters
{
	public class ExtPubKeyJsonConverter : JsonConverter
	{
		 
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(ExtPubKey);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var extPubKeyString = (string)reader.Value;
			return ExtPubKey.Parse(extPubKeyString, BitcoinNode.Network);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var epk = (ExtPubKey)value;

			var xpub = epk.GetWif(BitcoinNode.Network).ToWif();
			writer.WriteValue(xpub);
		}
	}
}
