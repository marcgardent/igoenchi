using System;
using System.Text;

namespace IGoEnchi
{
	public static class Network
	{
		public static byte[] ASCIIBytes(string line, out int bytes)
		{
			var encoding = System.Text.Encoding.ASCII;
			var text = line + '\n';
			var size = encoding.GetMaxByteCount(text.Length);
			var data = new byte[size];
			bytes = encoding.GetBytes(text, 0, text.Length, data, 0);
			return data;
		}
	}
}
