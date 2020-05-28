
using NBitcoin;

namespace System
{
	public class RandomWithSeed : IRandom
	{
		private Random _random;

		public RandomWithSeed(int seed)
		{
			this._random =  new Random(seed);
		}

		public void GetBytes(byte[] output)
		{
			_random.NextBytes(output);
		}

		public void GetBytes(Span<byte> output)
		{
			_random.NextBytes(output);
		}

		public byte[] GetNextBytes(int size)
		{
			var buffer = new byte[size];
			GetBytes(buffer);
			return buffer;
		}
	}
}
