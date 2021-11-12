using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft;

namespace NzCovidPass.Core.Shared
{
    public static class Requires
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