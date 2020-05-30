using NBitcoin;
using Newtonsoft.Json;
using System;

namespace MockBitcoinRpc.JsonConverters
{
	public class TransactionJsonConverter : JsonConverter
	{
		private Network network;

		public TransactionJsonConverter(Network network)
		{
			this.network = network;
		}

		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(Transaction);
		}

		 
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var txHex = reader.Value.ToString();
			var tx = Transaction.Parse(txHex, network);
			return tx;
		}

		 
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			writer.WriteValue(((Transaction)value).ToHex());
		}
	}
}
