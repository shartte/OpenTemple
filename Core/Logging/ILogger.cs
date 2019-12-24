using JetBrains.Annotations;

namespace SpicyTemple.Core.Logging
{
    public interface ILogger
    {
        void Error(string message);

        [StringFormatMethod("format")]
        void Error<T1>(string format, T1 arg1);

        [StringFormatMethod("format")]
        void Error<T1, T2>(string format, T1 arg1, T2 arg2);

        [StringFormatMethod("format")]
        void Error<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3);

        void Warn(string message);

        [StringFormatMethod("format")]
        void Warn<T1>(string format, T1 arg1);

        [StringFormatMethod("format")]
        void Warn<T1, T2>(string format, T1 arg1, T2 arg2);

        [StringFormatMethod("format")]
        void Warn<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3);

        [StringFormatMethod("format")]
        void Warn(string format, params object[] args);

        void Info(string message);

        [StringFormatMethod("format")]
        void Info<T1>(string format, T1 arg1);

        [StringFormatMethod("format")]
        void Info<T1, T2>(string format, T1 arg1, T2 arg2);

        [StringFormatMethod("format")]
        void Info<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3);

        [StringFormatMethod("format")]
        void Info(string format, params object[] args);

        void Debug(string message);

        [StringFormatMethod("format")]
        void Debug<T1>(string format, T1 arg1);

        [StringFormatMethod("format")]
        void Debug<T1, T2>(string format, T1 arg1, T2 arg2);

        [StringFormatMethod("format")]
        void Debug<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3);

        [StringFormatMethod("format")]
        void Debug(string format, params object[] args);
    }
}