using System.Security.Cryptography;

namespace JwtKeyGen
{
	public class JwtKeyGen
	{
		[Fact]
		public void Generate()
		{
			var rsaKey = RSA.Create();
			var privateKey = rsaKey.ExportRSAPrivateKey();
			File.WriteAllBytes("key", privateKey);
		}
	}
}