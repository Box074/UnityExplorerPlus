using Modding.Utils;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityExplorerPlus.STW
{
    internal static class STWCore
    {
        private static readonly List<string> methodNames = new()
        {
            "Update",
            "FixedUpdate"
        };
        private static readonly List<Assembly> ignoreAssembly = new()
        {
            typeof(STWCore).Assembly,
            typeof(ExplorerCore).Assembly,
            typeof(UIFactory).Assembly,
            typeof(Button).Assembly
        };
        private static readonly List<string> ignoreNamespace = new()
        {
            "UnityEngine"
        };
        private static readonly List<Type> ignoreTypes = new()
        {

        };
        private static StateSaver state = new();
        public static bool IsIgnore(Type type)
        {
            if(ignoreTypes.Contains(type)) return true;
            if(ignoreAssembly.Contains(type.Assembly)) return true;
            return ignoreNamespace.Any(x => type.FullName.StartsWith(x + "."));
        }
        private static Dictionary<MethodBase, Detour> detours = new();
        private static void EmptyMethod()
        {

        }
        public static void Init()
        {
            foreach(var v in AppDomain.CurrentDomain.GetAssemblies())
            {
                AttachAssembly(v);
            }

            AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;

            On.UnityEngine.SetupCoroutine.InvokeMoveNext += SetupCoroutine_InvokeMoveNext;
        }

        private static unsafe void SetupCoroutine_InvokeMoveNext(On.UnityEngine.SetupCoroutine.orig_InvokeMoveNext orig, 
            IEnumerator enumerator, IntPtr returnValueAddress)
        {
            if(enumerator != null && returnValueAddress != IntPtr.Zero && isFrozen)
            {
                var type = enumerator.GetType();
                if(!IsIgnore(type))
                {
                    *(byte*)(void*)returnValueAddress = 1;
                    return;
                }
            }
            orig(enumerator, returnValueAddress);
        }

        public static bool isFrozen = false;
        public static void Apply()
        {
            isFrozen = true;
            TimeScaleWidgetR.Instance.timeInput.Component.interactable = false;
            TimeScaleWidgetR.Instance.lockBtn.Component.interactable = false;

            
            state.SaveState("locked", TimeScaleWidgetR.Instance.locked);
            state.SaveState("timescale", TimeScaleWidgetR.Instance.desiredTime);

            TimeScaleWidgetR.Instance.locked = true;
            TimeScaleWidgetR.Instance.SetTimeScale(0);
            TimeScaleWidgetR.Instance.desiredTime = 0;

            lock (detours)
            {
                foreach(var detour in detours.Values)
                {
                    detour.Apply();
                }
            }
        }

        public static void Undo()
        {
            isFrozen = false;
            TimeScaleWidgetR.Instance.timeInput.Component.interactable = true;
            TimeScaleWidgetR.Instance.lockBtn.Component.interactable = true;
            state.TryLoadState<float>("timescale", out TimeScaleWidgetR.Instance.desiredTime);

            state.TryLoadState("locked", out TimeScaleWidgetR.Instance.locked);
            TimeScaleWidgetR.Instance.SetTimeScale(TimeScaleWidgetR.Instance.desiredTime);

            lock (detours)
            {
                foreach (var detour in detours.Values)
                {
                    detour.Undo();
                }
            }
        }

        private static void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            AttachAssembly(args.LoadedAssembly);
        }

        private static void AttachAssembly(Assembly assembly)
        {
            if (ignoreAssembly.Contains(assembly)) return;
            foreach(var t in assembly.GetTypesSafely())
            {
                if(t.IsSubclassOf(typeof(Component)) && !t.ContainsGenericParameters)
                {
                    if(IsIgnore(t)) continue;
                    foreach(var m in t.GetMethods(HReflectionHelper.Instance))
                    {
                        lock (detours)
                        {
                            if (methodNames.Contains(m.Name) && !detours.ContainsKey(m) 
                                && !m.ContainsGenericParameters
                                && m.ReturnType == typeof(void) && m.GetParameters().Length == 0)
                            {
                                
                                var detour = new Detour(m, new Action(EmptyMethod).Method, new()
                                {
                                    ManualApply = true,
                                    Priority = 10000
                                });
                                detours[m] = detour;
                            }
                        }
                    }
                }
            }
        }
    }
}
