using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpMacroPlayer.ClientNew.Logger;

namespace SharpMacroPlayer.ClientNew.Exceptions
{
    [Serializable]
    public class LogableException : Exception
    {
        public string Tag { get; init; } = "unknown";
        public static Logger.Logger? Logger { get; private set; }

        public LogableException() { }
        public LogableException(string message) : base(message)
        {
            if (Logger == null)
                Logger = new Logger.Logger();
            Logger.LogException(this);
        }
        public LogableException(string message, Exception inner) : base(message, inner)
        {
            if (Logger == null)
                Logger = new Logger.Logger();
            Logger.LogException(this);
        }

        public override string ToString() => $"{DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss")}: <{Tag.ToUpper()}> {Message};\n";
    }

    [Serializable]
    public class NotificationException : LogableException
    {
        public new string Tag {  get; init; } = "notification";

        public NotificationException() { }
        public NotificationException(string message) : base(message) { }
        public NotificationException(string message, Exception inner) : base(message, inner) { }
    }
    [Serializable]
	public class WarningException : LogableException
    {
        public new string Tag { get; init; } = "warning";

        public WarningException() { }
		public WarningException(string message) : base(message) { }
		public WarningException(string message, Exception inner) : base(message, inner) { }
	}
    [Serializable]
    public class ErrorException : LogableException
    {
        public new string Tag { get; init; } = "error";

        public ErrorException() { }
        public ErrorException(string message) : base(message) { }
        public ErrorException(string message, Exception inner) : base(message, inner) { }
    }
    [Serializable]
    public class FatalException : LogableException
    {
        public new string Tag { get; init; } = "fatal error";

        public FatalException() { }
        public FatalException(string message) : base(message) { }
        public FatalException(string message, Exception inner) : base(message, inner) { }
    }
}
