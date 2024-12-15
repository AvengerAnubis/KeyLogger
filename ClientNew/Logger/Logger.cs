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

		public Logger()
		{
			string dir = Path.GetFullPath(LogPath);
            _logFile = $"{dir}log{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.log";
			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);
		}

		public void LogException(LogableException ex) => LogMessage(ex.ToString());
		public void LogMessage(string message)
		{
			try 
			{ 
				File.AppendAllText(_logFile, $"{DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss")}: {message}\n");
			}
			catch { }
		}
	}
}
