using System;

namespace MockBitcoinRpc
{
	public class RpcException : Exception
	{
		public RpcException(JsonRpcErrorCodes code, string message)
			: base(message)
		{
			Code = code;
		}

		public JsonRpcErrorCodes Code { get; }
	}

	public class NotFoundException : RpcException
	{
		public NotFoundException(string message) 
			: base(JsonRpcErrorCodes.InvalidAddressOrKey, message)
		{
		}
	}
}