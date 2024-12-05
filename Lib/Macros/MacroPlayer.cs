using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#region юзинги для библиотеки
using SharpMacroPlayer.Utils;
using SharpMacroPlayer.Macros;
#endregion

#region статичные юзинги
using static SharpMacroPlayer.Utils.WinAPIFunctions;
using static SharpMacroPlayer.Utils.Constants;
#endregion

namespace SharpMacroPlayer.Macros
{
    public struct IndexCondition
    {
        public int Index;
        public long ConditionNum;
    }

    public class MacroPlayer
    {
        private bool _isRunning = false;
        public bool IsRunning
        {
            get => _isRunning;
        }

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
                throw new MacroPlayerException("Ошибка исполнения макроса: попытка получить индекс возврата из пустого стэка\n(Вероятно, элемент макроса \"Конец цикла\" неверно закрывает цикл!");
            }
        }

        private Macro _macro;
        public ref Macro Macro
        {
            get => ref _macro;
        }

        private int _currentIndex;
        public int CurrentIndex
        {
            get => _currentIndex;
            set => _currentIndex = value;
        }

        public MacroPlayer(Macro macro)
        {
            _returnIndexes = new Stack<IndexCondition>();
            _macro = macro;
            _currentIndex = 0;
        }

        /// <summary>
        /// Выполнить макросы в списке асинхронно (без блокировки UI)
        /// </summary>
        /// <returns></returns>
        public async Task RunMacroAsync()
        {
            if (!_isRunning)
            {
                _isRunning = true;
                while (_currentIndex < _macro.MacroElements.Count)
                {
                    await _macro.MacroElements[_currentIndex].Execute(this);
                    _currentIndex++;
                }
                _currentIndex = 0;
                _isRunning = false;
            }
        }
        /// <summary>
        /// Выполнить макросы в списке синхронно (возможна блокировка UI)
        /// </summary>
        public void RunMacro()
        {
            if (!_isRunning)
            {
                _isRunning = true;
                while (_currentIndex < _macro.MacroElements.Count)
                {
                    Task.Run(async () => { await _macro.MacroElements[_currentIndex].Execute(this); }).Wait();
                    _currentIndex++;
                }
                _currentIndex = 0;
                _isRunning = false;
            }
        }
    }


    [Serializable]
    public class MacroPlayerException : Exception
    {
        public MacroPlayerException() { }
        public MacroPlayerException(string message) : base(message) { }
        public MacroPlayerException(string message, Exception inner) : base(message, inner) { }
        protected MacroPlayerException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
