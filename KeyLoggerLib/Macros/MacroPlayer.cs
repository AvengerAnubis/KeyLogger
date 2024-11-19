using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
    public struct IndexCondition
    {
        public int Index;
        public long ConditionNum;
    }

    public class MacroPlayer
    {
        private bool _isRunning = false;
        private Stack<IndexCondition> _returnIndexes;

        public void PushIndexCondition(int index, long conditionNumber)
        {
            _returnIndexes.Push(new IndexCondition() { Index = index, ConditionNum = conditionNumber });
        }

        public IndexCondition PopIndex()
        {
            if (_returnIndexes.Count > 0)
            {
                return _returnIndexes.Pop();
            }
            else
            {
                throw new MacrosPlayerException("Ошибка исполнения макроса: попытка получить индекс возврата из пустого стэка\n(Вероятно, элемент макроса \"Конец цикла\" неверно закрывает цикл!");
            }
        }

        private Macros _macros;
        public ref Macros Macros
        {
            get => ref _macros;
        }

        private int _currentIndex;
        public int CurrentIndex
        {
            get => _currentIndex;
            set => _currentIndex = value;
        }

        public MacroPlayer(Macros macros)
        {
            _returnIndexes = new Stack<IndexCondition>();
            _macros = macros;
            _currentIndex = 0;
        }

        /// <summary>
        /// Выполнить макросы в списке асинхронно (без блокировки UI)
        /// </summary>
        /// <returns></returns>
        public async Task RunMacrosAsync()
        {
            if (!_isRunning)
            {
                _isRunning = true;
                while (_currentIndex < _macros.MacrosElements.Count)
                {
                    await _macros.MacrosElements[_currentIndex].Execute(this);
                    _currentIndex++;
                }
                _isRunning = false;
            }
        }
        /// <summary>
        /// Выполнить макросы в списке синхронно (возможно блокировка UI)
        /// </summary>
        public void RunMacros()
        {
            if (!_isRunning)
            {
                _isRunning = true;
                while (_currentIndex < _macros.MacrosElements.Count)
                {
                    Task.Run(async () => { await _macros.MacrosElements[_currentIndex].Execute(this); }).Wait();
                    _currentIndex++;
                }
                _isRunning = false;
            }
        }
    }


    [Serializable]
    public class MacrosPlayerException : Exception
    {
        public MacrosPlayerException() { }
        public MacrosPlayerException(string message) : base(message) { }
        public MacrosPlayerException(string message, Exception inner) : base(message, inner) { }
        protected MacrosPlayerException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
