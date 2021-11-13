using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace NzCovidPass.Core.Shared
{
    internal static class Requires
    {
        [DebuggerStepThrough]
        public static T NotNull<T>([NotNull] T value, [CallerArgumentExpression("value")] string? paramName = null)
            where T : class
        {
            ArgumentNullException.ThrowIfNull(value, paramName);

            return value;
        }
    }
}