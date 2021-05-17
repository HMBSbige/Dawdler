using CryptoBase;
using CryptoBase.Abstractions.Digests;
using CryptoBase.Digests.MD5;
using System;
using System.Buffers;
using System.Text;

namespace Dawdler.Utils
{
	public static class Md5
	{
		public static string ToHexString(in string str)
		{
			var buffer = ArrayPool<byte>.Shared.Rent(Encoding.UTF8.GetMaxByteCount(str.Length));
			try
			{
				var length = Encoding.UTF8.GetBytes(str, buffer);

				Span<byte> hash = stackalloc byte[HashConstants.Md5Length];

				MD5Utils.Default(buffer.AsSpan(0, length), hash);

				return hash.ToHex();
			}
			finally
			{
				ArrayPool<byte>.Shared.Return(buffer);
			}
		}
	}
}
