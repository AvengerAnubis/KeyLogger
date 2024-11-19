using KeyLogger.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyLogger.Bindings
{
    public class BindingsPlayer
    {
        private bool _isRunning;
        public bool IsRunning
        {
            get => _isRunning;
        }

        private List<Binding> _bindings;
        public ref List<Binding> Bindings
        {
            get => ref _bindings;
        }

        private InputHooker _hooker;

        public BindingsPlayer(ref InputHooker hooker)
        {
            _hooker = hooker;
            _bindings = new List<Binding>();
        }

        public void Start()
        {
            if (!_isRunning)
            {
                _isRunning = true;

            }
        }

        public void Stop()
        {

        }


        private void OnMouseInputGiven(object sender, HookCallbackEventArgs args)
        {

        }
    }
}
