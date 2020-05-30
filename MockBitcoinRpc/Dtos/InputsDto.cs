using NBitcoin;

namespace MockBitcoinRpc.Dtos
{
	public class InputDto
	{
		public uint256 txid { get; set; }
		public uint vout { get; set; }
		public uint sequence { get; set; }
	}
}