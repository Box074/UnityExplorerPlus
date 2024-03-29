﻿namespace UnityExplorerPlus.Inspectors
{
    internal class Collider2DsInspector : MouseInspectorBase
    {
        public Collider2DsInspectorResultPanel resultPanel = new();
        private List<GameObject> currentGameObjects = new List<GameObject>();
        
        public override void OnBeginMouseInspect()
        {
            currentGameObjects.Clear();
        }
        private IEnumerator SetPanelActiveCoro()
        {
            yield return null;
            resultPanel.SetActive(true);
            resultPanel.ShowResults();
            yield break;
        }
        public override void ClearHitData()
        {
            currentGameObjects.Clear();
        }

        public override void OnSelectMouseInspect()
        {
            resultPanel.Result.Clear();
            resultPanel.Result.AddRange(currentGameObjects);
            RuntimeHelper.StartCoroutine(SetPanelActiveCoro());
        }

        public override void UpdateMouseInspect(Vector2 _)
        {
            var cam = CameraSwitcher.GetCurrentCamera();
            if (cam == null)
            {
                MouseInspector.Instance.StopInspect();
                return;
            }
            currentGameObjects.Clear();

            var p = CameraSwitcher.GetCurrentMousePosition();

            currentGameObjects.AddRange(Physics2D.OverlapPointAll(p, -1).Select(x => x.gameObject));

            if (currentGameObjects.Count > 0)
            {
                MouseInspectorR.Instance.objNameLabel.text = $"Click to view collider2Ds under mouse{(Vector2)p}: {currentGameObjects.Count}";
            }
            else
            {
                MouseInspectorR.Instance.objNameLabel.text = "No collider2Ds under mouse.";
            }

        }

        public override void OnEndInspect()
        {
        }

    }
}
