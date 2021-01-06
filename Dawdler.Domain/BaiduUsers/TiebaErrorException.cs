using BilibiliLiveRecordDownLoader.Shared.Utils;
using Dawdler.Tieba;
using System;

namespace Dawdler.BaiduUsers
{
	public class TiebaErrorException : Exception
	{
		public ErrorMessage Error { get; }

		public TiebaErrorException(ErrorMessage error)
			: base($@"[{Timestamp.GetTime(error.time).ToLocalTime()}] {error.error_code} {error.error_msg}")
		{
			Error = error;
		}
	}
}
