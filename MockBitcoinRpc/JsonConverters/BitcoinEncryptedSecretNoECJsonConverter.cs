using NBitcoin;
using Newtonsoft.Json;
using System;

namespace MockBitcoinRpc.JsonConverters
{
	public class BitcoinEncryptedSecretNoECJsonConverter : JsonConverter
	{
		 
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(BitcoinEncryptedSecretNoEC);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var value = reader.Value as string;

			if (string.IsNullOrWhiteSpace(value))
			{
				return null;
			}

			return new BitcoinEncryptedSecretNoEC(value);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			writer.WriteValue(((BitcoinEncryptedSecretNoEC)value).ToWif());
		}
	}
}
