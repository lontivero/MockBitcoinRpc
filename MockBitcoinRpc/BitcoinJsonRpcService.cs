using System;
using System.Linq;
using System.Threading.Tasks;
using NBitcoin;
using NBitcoin.DataEncoders;
using Newtonsoft.Json.Linq;

namespace MockBitcoinRpc
{
	public class BitcoinJsonRpcService
	{
		private BitcoinNode _node;

		public BitcoinJsonRpcService(BitcoinNode node)
		{
			_node = node;
		}

		[JsonRpcMethod("getnewaddress")]
		public BitcoinAddress GetNewAddress()
		{
			return _node.GetNewAddress();
		}

		[JsonRpcMethod("getbestblockhash")]
		public uint256 GetBestBlockHash()
		{
			return _node.GetBestBlockHash();
		}

		[JsonRpcMethod("getblockcount")]
		public ulong GetBlockCount()
		{
			return _node.GetBlockCount();
		}

		[JsonRpcMethod("getblockhash")]
		public uint256 GetBlockHash(uint height)
		{
			return _node.GetBlockHash(height);
		}

		[JsonRpcMethod("getblock")]
		public object GetBlock(uint256 hash, int verbosity = 1)
		{
			var (block, height, tipHeight) =  _node.GetBlock(hash);
			var nextblockhash = (height == tipHeight) ? uint256.Zero : _node.GetBlockHash(height + 1);

			object ret = verbosity switch
			{
				0 => Encoders.Hex.EncodeData(block.ToBytes()),
				1 => new 
				{
					hash = block.GetHash(),
					confirmations = tipHeight - height + 1,
					size = block.GetSerializedSize(),
					strippedsize = block.WithOptions(TransactionOptions.None).GetSerializedSize(),
					weight = block.GetWeight(),
					height = height,
					version = block.Header.Version,
					versionHex = Encoders.Hex.EncodeData(BitConverter.GetBytes(block.Header.Version).Reverse().ToArray()),
					merkleroot = block.Header.HashMerkleRoot.ToString(),
					tx = block.Transactions.Select(x => x.GetHash()).ToArray(),
					time = block.Header.BlockTime.ToUnixTimeSeconds(),
					mediantime = block.Header.BlockTime.ToUnixTimeSeconds(),
					nonce = block.Header.Nonce,
					bits = Encoders.Hex.EncodeData(BitConverter.GetBytes(block.Header.Bits.ToCompact()).Reverse().ToArray()),
					//difficulty = block.Header.Bits.Difficulty,
					chainwork = block.Header.GetPoWHash(),
					nTx = block.Transactions.Count,
					previousblockhash = block.Header.HashPrevBlock,
					nextblockhash = nextblockhash 
				},
				_ => throw new ArgumentException("Verbosity")
			};
			return ret;
		}

		[JsonRpcMethod("getblockheader")]
		public object GetBlockHeader(uint256 hash, int verbosity = 1)
		{
			if (verbosity == 0)
			{
				var (block, _, _) =  _node.GetBlock(hash);
				return Encoders.Hex.EncodeData(block.Header.ToBytes());
			}
			else if (verbosity == 1)
			{
				dynamic blockJson = GetBlock(hash, 1);
				return new {
					blockJson.hash,
					blockJson.confirmations,
					blockJson.size,
					blockJson.strippedsize,
					blockJson.weight,
					blockJson.height,
					blockJson.version,
					blockJson.versionHex,
					blockJson.merkleroot,
					blockJson.time,
					blockJson.mediantime,
					blockJson.nonce,
					blockJson.bits,
					//blockJson.difficulty,
					blockJson.chainwork,
					blockJson.nTx,
					blockJson.previousblockhash,
					blockJson.nextblockhash,
				};
			}

			throw new ArgumentException("Verbosity");
		}

		[JsonRpcMethod("getblockchaininfo")]
		public object GetBlockchaIninfo()
		{
			var blockCount = _node.GetBlockCount();
			return new {
				chain = BitcoinNode.Network.ToString(),
				blocks = blockCount,
				headers = blockCount,
				bestblockhash = _node.GetBestBlockHash(),
				difficulty = _node.GetDifficulty(),
				mediantime = 1577129716,
				verificationprogress = 1,
				initialblockdownload = true,
				chainwork = uint256.One,
				size_on_disk = blockCount * (1_000 * 1_000),
				pruned = false,
				softforks = new[]{
					new {
						id = "bip34",
						version = 2,
						reject = new {
							status = false
						}
					},
					new {
						id = "bip66",
						version = 3,
						reject = new {
							status = true
						}
					},
					new {
						id = "bip65",
						version = 4,
						reject = new {
							status = true
						}
					}
				},
  				bip9_softforks = new {
					csv = new {
						status = "active",
						startTime = 0,
						timeout = 9223372036854775807,
						since = 432
					},
					segwit = new {
						status = "active",
						startTime = -1,
						timeout = 9223372036854775807,
						since = 0
					}
  				},
  				warnings = ""
			};
		}

		[JsonRpcMethod("getrawmempool")]
		public object GetRawMempool(bool verbose = false)
		{
			var mempool = _node.GetMempool();
			if (!verbose)
			{
				return mempool.Select(x =>x.GetHash()).ToArray();
			}
			else
			{
				return mempool; // TODO
			}
		}

		[JsonRpcMethod("getrawtransaction")]
		public Transaction GetRawTransaction(uint256 txid)
		{
			return _node.GetRawTransaction(txid);
		}

		[JsonRpcMethod("sendrawtransaction")]
		public uint256 SendRawTransaction(Transaction tx, bool allowhighfees = true)
		{
			_node.SendRawTransaction(tx, allowhighfees);
			return tx.GetHash();
		}

		[JsonRpcMethod("signrawtransaction")]
		public Transaction SignRawTransaction(Transaction tx)
		{
			return _node.SignRawTransaction(tx);
		}

		[JsonRpcMethod("createrawtransaction")]
		public Transaction CreateRawTransaction(string inputs, string outputs, uint locktime = 0xffffffff, bool rbf = false)
		{
			var inputsJson = JArray.Parse(inputs);
			var outputsJson= JArray.Parse(outputs);

			return BitcoinNode.Network.CreateTransaction(); //_node.CreateRawTransaction();
		}

		[JsonRpcMethod("generate")]
		public uint256[] Generate(int nblocks, int maxtries = 0)
		{
			var minerAddress = _node.GetNewAddress();
			return _node.Generate(nblocks, minerAddress);
		}

		[JsonRpcMethod("generatetoaddress")]
		public uint256[] GenerateToAddress(int nblocks, BitcoinAddress address, int maxtries = 0)
		{
			return _node.Generate(nblocks, address);
		}

		[JsonRpcMethod("stop")]
		public void Stop()
		{
			Environment.Exit(0);
		}
	}
}
