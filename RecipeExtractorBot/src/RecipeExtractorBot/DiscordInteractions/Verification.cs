using System.Text;
using NSec.Cryptography;

namespace RecipeExtractorBot.DiscordInteractions;

public static class Verification
{
    public static bool VerifySignature(string rawBody, string? signature, string? timestamp, string rawPublicKey)
    {
        if (signature is null || timestamp is null)
        {
            return false;
        }

        var algorithm = SignatureAlgorithm.Ed25519;
        var publicKey = PublicKey.Import(algorithm, Convert.FromHexString(rawPublicKey), KeyBlobFormat.RawPublicKey);
        var data = Encoding.UTF8.GetBytes(timestamp + rawBody);

        return algorithm.Verify(publicKey, data, Convert.FromHexString(signature));
    }
}
