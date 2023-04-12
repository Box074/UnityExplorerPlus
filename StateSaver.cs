using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityExplorerPlus
{
    internal class StateSaver<T> : StateSaver
    {
        public void SaveState(string name,T state) => SaveState<T>(name,state);
        public T LoadState(string name) => LoadState<T>(name);
        public bool TryLoadState(string name, out T state) => TryLoadState<T>(name, out state);
    }
    internal class StateSaver
    {
        public Dictionary<string, ICollection> states = new();
        private Stack<T> GetStates<T>(string name)
        {
            return (Stack<T>)states.TryGetOrAddValue(name, () => new Stack<T>());
        }
        public void SetState<T>(string name, T state)
        {
            var s = GetStates<T>(name);
            s.Clear();
            s.Push(state);
        }
        public void SaveState<T>(string name, T state)
        {
            GetStates<T>(name).Push(state);
        }
        public T LoadState<T>(string name)
        {
            return GetStates<T>(name).Pop();
        }
        public bool TryLoadState<T>(string name, out T state)
        {
            state = default;
            if(states.TryGetValue(name, out var s))
            {
                if(s is Stack<T> ss)
                {
                    return ss.TryPop(out state);
                }
            }
            return false;
        }
    }
}
