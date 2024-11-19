using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

#region юзинги для библиотеки
using KeyLogger.Utils;
using KeyLogger.Macros;
#endregion

#region статичные юзинги
using static KeyLogger.Utils.WinAPIFunctions;
using static KeyLogger.Utils.Constants;
#endregion

namespace KeyLogger.Macros
{
    /// <summary>
    /// Предоставляет статические методы для сохранения/загрузки макросов в JSON файлы
    /// </summary>
    public static class MacroLoaderSaver
    {
        public static string PathOfJsonFiles = "macros";

        /// <summary>
        /// Сохранить макрос в файл
        /// </summary>
        /// <param name="macros">Макрос, который будет сохранен в файл</param>
        /// <param name="filepath">Путь и название файла, который необходимо создать/перезаписать</param>
        /// <param name="isRelative">Является ли filepath относительным путем? (рекомендуется true)</param>
        /// <exception cref="MacrosSaveException"></exception>
        public static void SaveMacros(Macros macros, string filepath, bool isRelative = true)
        {
            try
            {
                if (isRelative)
                {
                    filepath = Path.GetFullPath(PathOfJsonFiles + "\\" + filepath);
                }
                using (FileStream file = new FileStream(filepath, FileMode.Create, FileAccess.Write))
                {
                    Utf8JsonWriter writer = new Utf8JsonWriter(file);
                    macros.WriteToJson(ref writer);
                    writer.Flush();
                }
            }
            catch (Exception ex)
            {
                throw new MacrosSaveException("Ошибка сохранения макроса:\n" + ex.Message);
            }
        }
        /// <summary>
        /// Загрузить макрос из файла
        /// </summary>
        /// <param name="filepath">Путь к файлу</param>
        /// <param name="isRelative">Является ли filepath относительным путем? (рекомендуется true)</param>
        /// <returns>Макрос</returns>
        /// <exception cref="MacrosLoadException"></exception>
        public static Macros LoadMacros(string filepath, bool isRelative = true)
        {
            try
            {
                if (isRelative)
                {
                    filepath = Path.GetFullPath(PathOfJsonFiles + "\\" + filepath);
                }
                using (FileStream file = new FileStream(filepath, FileMode.Open, FileAccess.Read))
                {
                    return new Macros(JsonDocument.Parse(file));
                }
            }
            catch (Exception ex)
            {
                throw new MacrosLoadException("Ошибка загрузки макроса:\n" + ex.Message);
            }
        }
        /// <summary>
        /// Возвращает все сохраненные файлы макросов
        /// Рекомендуется брать названия файлов из этого метода
        /// </summary>
        /// <returns>Названия/пути ко всем файлам</returns>
        public static string[] GetAllMacroses() => Directory.EnumerateFiles(Path.GetFullPath(PathOfJsonFiles)).ToArray();
    }


    [Serializable]
    public class MacrosLoadException : Exception
    {
        public MacrosLoadException() { }
        public MacrosLoadException(string message) : base(message) { }
        public MacrosLoadException(string message, Exception inner) : base(message, inner) { }
        protected MacrosLoadException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }


    [Serializable]
    public class MacrosSaveException : Exception
    {
        public MacrosSaveException() { }
        public MacrosSaveException(string message) : base(message) { }
        public MacrosSaveException(string message, Exception inner) : base(message, inner) { }
        protected MacrosSaveException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
