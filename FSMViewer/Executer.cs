namespace UnityExplorerPlus.FSMViewer;

class Executer : SingleMonoBehaviour<Executer>
{
    public static System.Collections.Concurrent.ConcurrentQueue<Action> actions = new();
    private void Update()
    {
        while (actions.TryDequeue(out var action))
        {
            try
            {
                action.Invoke();
            }
            catch (Exception e)
            {
                UnityExplorerPlus.Instance.LogError(e);
            }
        }
    }
    public static void Invoke(Action a) => actions.Enqueue(a);
}
