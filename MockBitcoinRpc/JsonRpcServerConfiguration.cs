using NBitcoin;

namespace MockBitcoinRpc
{
	public class JsonRpcServerConfiguration
	{
		public JsonRpcServerConfiguration(string user, string password, string[] prefixes)
		{
			JsonRpcUser = user;
			JsonRpcPassword = password;
			Prefixes = prefixes;
		}

		public string JsonRpcUser { get; }
		public string JsonRpcPassword { get; }
		public string[] Prefixes { get; }
		public bool RequiresCredentials => !string.IsNullOrEmpty(JsonRpcUser) && !string.IsNullOrEmpty(JsonRpcPassword);
	}
}
