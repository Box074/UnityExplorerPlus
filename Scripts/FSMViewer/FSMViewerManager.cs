
namespace UnityExplorerPlusMod;

static class FSMViewerManager
{
    public static WebSocketServer server;
    public static bool isConnected => service != null;
    private static List<Action<FsmViewerService>> queue = new();
    private static FsmViewerService service;
    public static void OpenJsonFsm(string text)
    {
        TryOpenFSMViewer(service =>
        {
            service.Send("--JSONRAW\n" + Convert.ToBase64String(Encoding.UTF8.GetBytes(text)));
        });
    }
    public static bool TryOpenFSMViewer(Action<FsmViewerService> cb)
    {
        if (string.IsNullOrEmpty(UnityExplorerPlus.Instance.globalSettings.fsmViewerPath)) return false;
        if (server == null)
        {
            server = new(UnityExplorerPlus.Instance.globalSettings.fsmViewerPort);
            server.WebSocketServices.AddService<FsmViewerService>("/fsmviewer", _ => { });
            new Thread(() => server.Start()).Start();
        }
        lock (queue)
        {
            if (isConnected)
            {
                cb(service);
                return true;
            }
            else
            {
                queue.Add(cb);
            }
        }
        try
        {
            Process.Start(UnityExplorerPlus.Instance.globalSettings.fsmViewerPath, "--UEP " + UnityExplorerPlus.Instance.globalSettings.fsmViewerPort);
        }
        catch (Exception e)
        {
            UnityExplorerPlus.Instance.LogError(e);
            return false;
        }
        return true;
    }
    public class FsmViewerService : WebSocketBehavior
    {
        protected override void OnOpen()
        {
            service = this;
            lock (queue)
            {
                foreach (var v in queue) v.Invoke(this);
                queue.Clear();
            }
        }
        protected override void OnClose(CloseEventArgs e)
        {
            service = null;
        }
        public new void Send(string str) => base.Send(str);
        protected override void OnMessage(MessageEventArgs e)
        {
            var cmds = e.Data.Split('\n');
            Executer.Invoke(() =>
            {
                if (cmds[0].Equals("INSPECT-UOBJ", StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(cmds[1], out var id))
                    {
                        var uobj = Resources.InstanceIDToObject(id);
                        if (uobj != null)
                        {
                            InspectorManager.Inspect(uobj);
                        }
                    }
                }
                else if (cmds[0].Equals("INSPECT-ACTION", StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(cmds[1], out var id))
                    {
                        var uobj = Resources.InstanceIDToObject(id);
                        if (uobj is PlayMakerFSM pm)
                        {
                            var state = pm.Fsm.GetState(cmds[2]);
                            if (int.TryParse(cmds[3], out var aid))
                            {
                                InspectorManager.Inspect(state.Actions[aid]);
                            }
                        }
                    }
                }
            });
        }
    }
}
