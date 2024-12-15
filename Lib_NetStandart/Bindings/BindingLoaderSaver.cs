using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

#region юзинги для библиотеки
using SharpMacroPlayer.Utils;
using SharpMacroPlayer.Macros;
#endregion

#region статичные юзинги
using static SharpMacroPlayer.Utils.WinAPIFunctions;
using static SharpMacroPlayer.Utils.Constants;
#endregion

namespace SharpMacroPlayer.Bindings
{
    // todo:    MacroLoaderSaver и BindingLoaderSaver - практически идентичные классы. 
    //          переделать с использованием интерфейсов мб

    /// <summary>
    /// Предоставляет статические методы для сохранения/загрузки биндингов в JSON файлы
    /// </summary>
    public static class BindingLoaderSaver
    {
        // все биндинги пишутся в один файл, но можно записать несколько файлов - наборов биндингов
        // таким образом можно реализовать переключение профилей
        public static string PathOfBindings = "bindings";

        /// <summary>
        /// Сохранить биндинг в файл
        /// </summary>
        /// <param name="macros">Биндинг, который будет сохранен в файл</param>
        /// <param name="filepath">Путь и название файла, который необходимо создать/перезаписать</param>
        /// <param name="isRelative">Является ли filepath относительным путем? (рекомендуется true)</param>
        /// <exception cref="BindingSaveException"></exception>
        public static void SaveBindings(BindingContainer binding, string filepath, bool isRelative = true)
        {
            try
            {
                if (isRelative)
                {
                    if (!Directory.Exists(Path.GetFullPath(PathOfBindings)))
                    {
                        Directory.CreateDirectory(Path.GetFullPath(PathOfBindings));
                    }
                    filepath = Path.GetFullPath(PathOfBindings + "\\" + filepath);
                }
                using (FileStream file = new FileStream(filepath, FileMode.Create, FileAccess.Write))
                {
                    Utf8JsonWriter writer = new Utf8JsonWriter(file);
                    binding.WriteToJson(ref writer);
                    writer.Flush();
                }
            }
            catch (Exception ex)
            {
                throw new BindingSaveException("Ошибка сохранения биндинга:\n" + ex.Message);
            }
        }
        /// <summary>
        /// Загрузить биндинги из файла
        /// </summary>
        /// <param name="filepath">Путь к файлу</param>
        /// <param name="isRelative">Является ли filepath относительным путем? (рекомендуется true)</param>
        /// <returns>Биндинги</returns>
        /// <exception cref="BindingLoadException"></exception>
        public static BindingContainer LoadBindings(string filepath, bool isRelative = true)
        {
            try
            {
                if (isRelative)
                {
                    if (!Directory.Exists(Path.GetFullPath(PathOfBindings)))
                    {
                        Directory.CreateDirectory(Path.GetFullPath(PathOfBindings));
                    }
                    filepath = Path.GetFullPath(PathOfBindings + "\\" + filepath);
                }
                using (FileStream file = new FileStream(filepath, FileMode.Open, FileAccess.Read))
                {
                    return new BindingContainer(JsonDocument.Parse(file));
                }
            }
            catch (Exception ex)
            {
                throw new BindingLoadException("Ошибка загрузки биндинга:" + ex.Message);
            }
        }
        /// <summary>
        /// Удалить биндинг из библиотеки макросов
        /// </summary>
        /// <param name="filepath">Относительный путь (удаление случайного файла по абсолютному пути не разрешено)</param>
        public static void DeleteBinding(string filepath)
        {
            try
            {
                if (!Directory.Exists(Path.GetFullPath(PathOfBindings)))
                {
                    Directory.CreateDirectory(Path.GetFullPath(PathOfBindings));
                }
                filepath = Path.GetFullPath(PathOfBindings + "\\" + filepath);
                File.Delete(filepath);
            }
            catch (Exception ex)
            {
                throw new MacroDeleteException("Ошибка удаления макроса:\n" + ex.Message);
            }
        }
        /// <summary>
        /// Возвращает все сохраненные файлы биндингов
        /// Рекомендуется брать названия файлов из этого метода
        /// </summary>
        /// <returns>Названия/пути ко всем файлам</returns>
        public static string[] GetAllBindings()
        {
            if (!Directory.Exists(Path.GetFullPath(PathOfBindings)))
            {
                Directory.CreateDirectory(Path.GetFullPath(PathOfBindings));
            }
            string[] files = Directory.EnumerateFiles(Path.GetFullPath(PathOfBindings)).Select(el => Path.GetFileName(el)).ToArray();
            if (files.Length == 0)
            {
                SaveBindings(new BindingContainer(), "default.json");
                files = Directory.EnumerateFiles(Path.GetFullPath(PathOfBindings)).Select(el => Path.GetFileName(el)).ToArray();
            }
            // возрващаем только имена файлов
            return files;
        }
    }


    [Serializable]
    public class BindingLoadException : Exception
    {
        public BindingLoadException() { }
        public BindingLoadException(string message) : base(message) { }
        public BindingLoadException(string message, Exception inner) : base(message, inner) { }
        protected BindingLoadException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }


    [Serializable]
    public class BindingSaveException : Exception
    {
        public BindingSaveException() { }
        public BindingSaveException(string message) : base(message) { }
        public BindingSaveException(string message, Exception inner) : base(message, inner) { }
        protected BindingSaveException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
