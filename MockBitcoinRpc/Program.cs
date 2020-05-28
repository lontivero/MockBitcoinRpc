using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MockBitcoinRpc.Logging;
using NBitcoin;

namespace MockBitcoinRpc
{
	class Program
	{
		private static readonly Log Logger = new Log(new TraceSource(nameof(Program), SourceLevels.Verbose));
		private static JsonRpcServer RpcServer;
		private static CancellationTokenSource Cancel = new CancellationTokenSource();

		public 

		static void Main2(string[] args)
		{
//			var json = @"[{""jsonrpc"": ""2.0"", ""id"":""1"", ""method"": ""getnewaddress"", ""params"": [] },{""jsonrpc"":""2.0"", ""id"":""2"", ""method"": ""getblock"", ""params"": []}]";
//			var json = @"{""jsonrpc"": ""2.0"", ""id"":""1"", ""method"": ""getnewaddress"", ""params"": [] }";
//			JsonRpcRequest.TryParse2(json, out JsonRpcRequest[] list);
		}

		static async Task Main(string[] args)
		{
			var network = "TestNet";
			var rpcUser = string.Empty;
			var rpcPassword = string.Empty;
			var prefixes = new [] { "http://*:14883/" };
			var randomSeed = 123456;

			RpcServer = new JsonRpcServer(
				new BitcoinJsonRpcService(
					new BitcoinNode(
						Network.GetNetwork(network),
						new RandomWithSeed(randomSeed))),
				 new JsonRpcServerConfiguration(rpcUser, rpcPassword, prefixes));

			try
			{
				await RpcServer.StartAsync(Cancel.Token).ConfigureAwait(false);
			}
			catch (System.Net.HttpListenerException e)
			{
				Logger.Warn($"Failed to start {nameof(JsonRpcServer)} with error: {e.Message}.");
				RpcServer = null;
			}

			while(true)
			{
				await Task.Delay(1_000);
			}
		}
	}
}
