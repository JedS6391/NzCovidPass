namespace NzCovidPass.Core.Verification
{
    public class KeyNotFoundException : Exception
    {
        public KeyNotFoundException(string message)
            : base(message)
        {

        }
    }
}
