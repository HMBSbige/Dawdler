namespace Dawdler.Bilibili;

public class BilibiliNoLoginException : Exception
{
	public BilibiliNoLoginException() : base(@"未登录") { }
	public BilibiliNoLoginException(string message) : base(message) { }
}
