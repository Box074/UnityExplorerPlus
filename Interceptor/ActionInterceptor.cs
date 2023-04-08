using InControl;

namespace UnityExplorerPlus.Interceptor
{
    internal static class ActionInterceptor
    {
        private static GameObject mask;
        public static void StopGameInput()
        {
            InputManager.Enabled = false;

            if(mask == null)
            {

            }
            else
            {
                mask.SetActive(true);
            }
        }

        public static void StartGameInput()
        {
            InputManager.Enabled = true;
            if(mask != null)
            {
                mask.SetActive(false);
            }
        }
    }
}
