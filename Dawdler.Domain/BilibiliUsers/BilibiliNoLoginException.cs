using System;

namespace Dawdler.BilibiliUsers
{
	public class BilibiliNoLoginException : Exception
	{
		public BilibiliNoLoginException() { }
		public BilibiliNoLoginException(string message) : base(message) { }
	}
}
