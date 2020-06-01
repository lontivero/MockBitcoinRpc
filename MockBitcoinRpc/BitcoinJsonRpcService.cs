using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MockBitcoinRpc.Dtos;
using MockBitcoinRpc.Logging;
using NBitcoin;
using NBitcoin.DataEncoders;

namespace MockBitcoinRpc
{
	public class BitcoinJsonRpcService
	{
		private static readonly Log Logger = new Log(new TraceSource(nameof(BitcoinJsonRpcService), SourceLevels.Verbose));

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


		[JsonRpcMethod("createrawtransaction")]
		public Transaction CreateRawTransaction(InputDto[] inputs, Dictionary<string, string> outputs, uint locktime = 0xffffffff, bool rbf = false)
		{
			var txInputs = inputs.Select(i => new TxIn(new OutPoint(i.txid, i.vout)));
			var txOutputs= outputs.Select(kv => kv.Key == "data"
				? new TxOut(Money.Zero, TxNullDataTemplate.Instance.GenerateScriptPubKey(Encoders.Hex.DecodeData(kv.Value)))
				: new TxOut(Money.Satoshis(ulong.Parse(kv.Value)), BitcoinAddress.Create(kv.Key, BitcoinNode.Network)));
			var tx = BitcoinNode.Network.CreateTransaction();

			tx.Inputs.AddRange(txInputs);
			tx.Outputs.AddRange(txOutputs);
			tx.LockTime = locktime;

			if (rbf)
			{
				foreach(var txin in tx.Inputs)
				{
					txin.Sequence = Sequence.MAX_BIP125_RBF_SEQUENCE;
				}
			}
			return tx;
		}


		[JsonRpcMethod("listunspent")]
		public IEnumerable<UtxoDto> ListUnspentUtxo(int minconf = 0, int maxconf = int.MaxValue)
		{
			return _node.GetUnspentUtxo(minconf, maxconf);
		}

		[JsonRpcMethod("signrawtransactionwithkey")]
		public Transaction SignRawTransactionWithKey(Transaction tx, Key[] privatekeys)
		{
		//	return _node.SignRawTransactionWithKey(tx, privatekeys);
			return tx;
		}

		[JsonRpcMethod("dumpprivkey")]
		public Key DumpPrivateKey(BitcoinAddress address)
		{
			return _node.DumpPrivateKey(address);
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
