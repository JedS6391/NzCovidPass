using NzCovidPass.Core.Shared;

namespace NzCovidPass.Core.Cbor
{
    public class CborTokenReaderContext : Context
    {
        public byte[] Data { get; }

        public static Context.FailureReason TokenReadFailed => new FailureReason("", "");
    }
}