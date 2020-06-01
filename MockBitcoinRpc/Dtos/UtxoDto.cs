using NBitcoin;

namespace MockBitcoinRpc.Dtos
{
	public class UtxoDto
	{
		public uint256 txid { get; set; }
		public uint vout { get; set; }
		public BitcoinAddress address { get; set; }
		public string label { get; set; }
		public Script scriptPubKey { get; set; }
		public Money amount { get; set; }
		public uint confirmations { get; set; }
		public bool spendable { get; set; }
		public bool solvable { get; set; }
		public bool safe { get; set; }
	}
}