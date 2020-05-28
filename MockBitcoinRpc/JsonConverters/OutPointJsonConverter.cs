using NBitcoin;
using NBitcoin.DataEncoders;
using Newtonsoft.Json;
using System;

namespace MockBitcoinRpc.JsonConverters
{
	public class OutPointJsonConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(OutPoint);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var hex = (string)reader.Value;
			var op = new OutPoint();
			op.FromBytes(Encoders.Hex.DecodeData(hex));

			return op;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			string opHex = Encoders.Hex.EncodeData(((OutPoint)value).ToBytes());
			writer.WriteValue(opHex);
		}
	}
}
