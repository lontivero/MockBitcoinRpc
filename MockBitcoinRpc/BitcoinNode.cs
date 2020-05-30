using System;
using System.Collections.Generic;
using System.Linq;
using NBitcoin;

namespace MockBitcoinRpc
{
	public class BitcoinNode
	{
		private static KeyPath DefaultAccountKeyPath = new KeyPath("m/84h/0h/0h");

		private ExtKey _extKey;
		private ExtPubKey _extPubKey;
		private uint _keyDepth;
		private List<Block> _blocks;
		private List<Transaction> _mempool;
		private Dictionary<uint256, Block> _blockIndex;
		private Dictionary<uint256, Transaction> _txIndex;
		public static Network Network { get; private set; }

		public BitcoinNode(Network network, RandomWithSeed rnd)
		{
			RandomUtils.Random = rnd;
			Network = network;

			var key = new Key(rnd.GetNextBytes(32));
			var code = rnd.GetNextBytes(32);
			_extKey = new ExtKey(key, code);
			_extPubKey = _extKey.Derive(DefaultAccountKeyPath).Neuter();
			_keyDepth = 0;
			////////////////

			_blocks = new List<Block>();
			_blockIndex = new Dictionary<uint256, Block>();
			_mempool = new List<Transaction>();
			_txIndex = new Dictionary<uint256, Transaction>();

			var genesisBlock = network.GetGenesis();
			AddBlock(genesisBlock);
		}

		public BitcoinAddress GetNewAddress()
		{
			return _extPubKey.Derive(_keyDepth++).GetPublicKey().GetAddress(ScriptPubKeyType.Segwit, Network);
		}

		public Block GetBestBlock()
		{
			return _blocks.Last();
		}

		public uint256 GetBestBlockHash()
		{
			return GetBestBlock().GetHash();
		}

		public uint GetBlockCount()
		{
			return (uint)_blocks.Count();
		}

		public uint256 GetBlockHash(uint height)
		{
			if (_blocks.Count() < height)
				throw new ArgumentException("Block height out of range");

			return _blocks[(int)height].GetHash();
		}

		public (Block, uint, uint) GetBlock(uint256 hash)
		{
			if (!_blockIndex.TryGetValue(hash, out var block))
				throw new NotFoundException("Block not found");

			var height = GetHeightOfBlock(block);
			var tipHeight = GetHeightOfBlock(GetBestBlock());
			return (block, height, tipHeight);
		}

		private uint GetHeightOfBlock(Block block)
		{
			return (uint)_blocks.IndexOf(block);
		}

		public double GetDifficulty()
		{
			return GetBestBlock().Header.Bits.Difficulty;
		}

		public Transaction[] GetMempool()
		{
			return _mempool.ToArray();
		}

		public Transaction GetRawTransaction(uint256 txid)
		{
			if (_txIndex.TryGetValue(txid, out var tx))
			{
				return tx;
			}
			throw new ArgumentException("");
		}

		public void SendRawTransaction(Transaction tx, bool allowhighfees)
		{
			AddTransaction(tx);
		}

		public Key DumpPrivateKey(BitcoinAddress address)
		{
			for(var i = 0u; i < 100; i++)
			{
				var pk = _extPubKey.Derive(i).GetPublicKey();
				if (pk.GetAddress(ScriptPubKeyType.Legacy, Network) == address ||
					pk.GetAddress(ScriptPubKeyType.Segwit, Network) == address ||
					pk.GetAddress(ScriptPubKeyType.SegwitP2SH, Network) == address)
					{
						return _extKey.Derive(i).PrivateKey;
					}
			}

			throw new NotFoundException("Address");
		}

		public uint256[] Generate(int nblocks, BitcoinAddress minerAddress)
		{
			var tipBlock = GetBestBlock();
			for(var i= 0; i < nblocks; i++)
			{
				var date = tipBlock.Header.BlockTime.AddMinutes(10);
				var newBlock = CreateNextBlockWithCoinbase(tipBlock, minerAddress, (int)GetHeightOfBlock(tipBlock) + 1, date);
				AddBlock(newBlock);
				tipBlock = newBlock;
			}
			return _blocks.TakeLast(nblocks).Select(x => x.GetHash()).ToArray();
		}

		public Block CreateNextBlockWithCoinbase(Block parentBlock, BitcoinAddress address, int height, DateTimeOffset now)
		{
			if (address == null)
				throw new ArgumentNullException(nameof(address));
			Block block = parentBlock.GetConsensusFactory().CreateBlock();
			var nonce = RandomUtils.GetUInt32();
			block.Header.Nonce = nonce;
			block.Header.HashPrevBlock = parentBlock.GetHash();
			block.Header.BlockTime = now;
			var coinbaseTx = block.AddTransaction(parentBlock.GetConsensusFactory().CreateTransaction());
			coinbaseTx.Inputs.Add(scriptSig: new Script(Op.GetPushOp(RandomUtils.GetBytes(30))));
			coinbaseTx.Outputs.Add(new TxOut(address.Network.GetReward(height), address)
			{
				Value = address.Network.GetReward(height)
			});
			var vsize = 0;
			foreach(var tx in _mempool.OrderByDescending(CalculateFeeRate).ToArray())
			{
				block.AddTransaction(tx);
				_mempool.Remove(tx);
				vsize += tx.GetSerializedSize();
				
				if (vsize >= 4_000_000)
				{
					break;
				}
			}
			return block;
		}

		private FeeRate CalculateFeeRate(Transaction tx)
		{
			var spendingCoins = new List<ICoin>();
			var outPoints = tx.Inputs.Select(x => x.PrevOut);
			foreach(var outPoint in outPoints)
			{
				if (_txIndex.TryGetValue(outPoint.Hash, out var coinTx))
				{
					spendingCoins.Add(new Coin(outPoint, coinTx.Outputs[outPoint.N]));
				}
			}
			return tx.GetFeeRate(spendingCoins.ToArray());
		}

		private void AddTransaction(Transaction tx)
		{
			_mempool.Add(tx);
			AddTransactionToIndex(tx);
		}
		
		private void AddTransactionToIndex(Transaction tx)
		{
			_txIndex.Add(tx.GetHash(), tx);
		}

		private void AddBlock(Block block)
		{
			_blocks.Add(block);
			_blockIndex.Add(block.GetHash(), block);
			AddTransactionToIndex(block.Transactions[0]);

			foreach(var tx in block.Transactions)
			{
				_mempool.Remove(tx);
			}
		}
	}
}
