using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MockBitcoinRpc.Logging;
using Mono.Options;
using NBitcoin;

namespace MockBitcoinRpc
{
	class Program
	{
		private static readonly Log Logger = new Log(new TraceSource(nameof(Program), SourceLevels.Verbose));
		private static JsonRpcServer RpcServer;
		private static CancellationTokenSource Cancel = new CancellationTokenSource();

		static async Task Main(string[] args)
		{
			var rpcNetwork = "TestNet";
			var rpcPort = 0;
			var rpcUser = string.Empty;
			var rpcPassword = string.Empty;
			var randomSeed = 123456;
			var banner = true; 
			var help = false;

			var os = new OptionSet()
			{
				{ "h|?|help", _ => help = true},
				{ "port=",
					"Rpc port where the server is listening.",
					(int port) => rpcPort = port },
				{ "n=|network=",
					"Bitcoin network to use (Main, TestNet, RegTest)",
					(string network) => rpcNetwork = network },
				{ "u=|user=",
					"Username allowed to make requests (default: empty)",
					(string user) => rpcUser = user },
				{ "p=|password=",
					"Password for the specified username. (default: empty)",
					(string password) => rpcPassword = password },
				{ "s=|seed=",
					"numeric seed used to make the session reproducible (default: 123456)",
					(int seed) => randomSeed = seed },
				{ "no-banner",
					"Do not display the welcome banner.",
					_ => banner = false },
			};
			os.Parse(args);
			
			var network = Network.GetNetwork(rpcNetwork);
			rpcPort = rpcPort == 0 ? network.RPCPort : rpcPort;
			var prefixes = new[] { $"http://*:{rpcPort}/" };

			if (help)
			{
				ShowHelp(os);
			}
			if (banner)
			{
				Banner(rpcNetwork, rpcPort, rpcUser, rpcPassword);
			}

			RpcServer = new JsonRpcServer(
				new BitcoinJsonRpcService(
					new BitcoinNode(
						network,
						new RandomWithSeed(randomSeed))),
				 new JsonRpcServerConfiguration(rpcUser, rpcPassword, prefixes));

			try
			{
				await RpcServer.StartAsync(Cancel.Token).ConfigureAwait(false);
			}
			catch (System.Net.HttpListenerException e)
			{
				Logger.Error($"Failed to start {nameof(JsonRpcServer)} with error: {e.Message}.");
				RpcServer = null;
			}

			while (true)
			{
				await Task.Delay(1_000);
			}
		}

		private static void ShowHelp(OptionSet os)
		{
			Console.WriteLine("Start a fake Bitcoin RPC server that mimics a real bitcoin core node.");
			Console.WriteLine($"usage: {nameof(MockBitcoinRpc)} [options]");
			Console.WriteLine("");
			Console.WriteLine($"eg: {nameof(MockBitcoinRpc)} --network:TestNet --user:user --password:password");
			Console.WriteLine("");
			os.WriteOptionDescriptions(Console.Out);
			Console.WriteLine("");

			Environment.Exit(0);
		}

		private static void Banner(string rpcNetwork, int rpcPort, string rpcUser, string rpcPassword)
		{
			var defaultConsoleColor = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine(@"   __  ___         __    ___  _ __           _      ___  ___  _____");
			Console.WriteLine(@"  /  |/  /__  ____/ /__ / _ )(_) /________  (_)__  / _ \/ _ \/ ___/");
			Console.WriteLine(@" / /|_/ / _ \/ __/  '_// _  / / __/ __/ _ \/ / _ \/ , _/ ___/ /__  ");
			Console.WriteLine(@"/_/  /_/\___/\__/_/\_\/____/_/\__/\__/\___/_/_//_/_/|_/_/   \___/  v.0.0.1-alpha");                                                                   
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("");
			Console.WriteLine($"RPC server listening on http://127.0.0.1:{rpcPort} for {rpcNetwork} network.");
			Console.WriteLine("");

			var credentials = !string.IsNullOrEmpty(rpcUser) ? $"{rpcUser}:{rpcPassword}@" : string.Empty;
			Console.ForegroundColor = ConsoleColor.DarkGreen;
			Console.WriteLine($"Experiment with the terminal like a pro! Try with:");
			Console.WriteLine($"$ curl -s --data-binary '{{\"jsonrpc\": \"2.0\", \"id\":\"1\", \"method\": \"getbestblockhash\", \"params\": [] }}' -H 'content-type: text/plain;' http://{credentials}127.0.0.1:{rpcPort}");
			Console.WriteLine("");

			Console.ForegroundColor = defaultConsoleColor;
			Console.WriteLine("Press CTRL+C to finish.");
		}
	}
}
