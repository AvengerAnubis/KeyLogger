using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpMacroPlayer.ClientNew.Logger
{
	public class Logger
	{
		public static readonly string LogPath = @"logs\";
		private string _logFile;
		public static Logger? LoggerInstance { get; private set; }

		public Logger()
		{
			string dir = Path.GetFullPath(LogPath);
            _logFile = $"{dir}log{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.log";
			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);
		}

		public static void LogException(LogableException ex)
		{
			LoggerInstance ??= new Logger();
			LogMessage(ex.ToString());
		}
		public static void LogMessage(string message)
		{
            LoggerInstance ??= new Logger();
            try 
			{ 
				File.AppendAllText(LoggerInstance._logFile, $"{DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss")}: {message}\n");
			}
			catch { }
		}
	}
}
