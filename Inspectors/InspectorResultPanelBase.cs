using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniverseLib.UI.Widgets.ButtonList;

namespace UnityExplorerPlus.Inspectors
{
    internal class InspectorResultPanelBase : UEPanel
    {
        public override UUIManager.Panels PanelType
        {
            get
            {
                return UUIManager.Panels.UIInspectorResults;
            }
        }
        public virtual List<GameObject> Result { get; } = new();

		public override string Name
		{
			get
			{
				return "UI Inspector Results";
			}
		}

		public override int MinWidth
		{
			get
			{
				return 500;
			}
		}

		public override int MinHeight
		{
			get
			{
				return 500;
			}
		}

		public override Vector2 DefaultAnchorMin
		{
			get
			{
				return new Vector2(0.5f, 0.5f);
			}
		}

		public override Vector2 DefaultAnchorMax
		{
			get
			{
				return new Vector2(0.5f, 0.5f);
			}
		}

		public override bool CanDragAndResize
		{
			get
			{
				return true;
			}
		}

		public override bool NavButtonWanted
		{
			get
			{
				return false;
			}
		}

		public override bool ShouldSaveActiveState
		{
			get
			{
				return false;
			}
		}

		public override bool ShowByDefault
		{
			get
			{
				return false;
			}
		}

		public InspectorResultPanelBase()
			: base(UUIManagerR.UiBase)
		{
		}

		public void ShowResults()
		{
			dataHandler.RefreshData();
			buttonScrollPool.Refresh(true, true);
		}

		private List<GameObject> GetEntries()
		{
			return Result;
		}

		private bool ShouldDisplayCell(object cell, string filter)
		{
			return true;
		}

		private void OnCellClicked(int index)
		{
			bool flag = index >= Result.Count;
			if (!flag)
			{
				InspectorManager.Inspect(Result[index], null);
			}
		}

		private void SetCell(ButtonCell cell, int index)
		{
			bool flag = index >= Result.Count;
			if (!flag)
			{
				GameObject gameObject = Result[index];
				cell.Button.ButtonText.text = string.Concat(new string[]
				{
					"<color=cyan>",
					gameObject.name,
					"</color> (",
					gameObject.transform.GetTransformPath(true),
					")"
				});
			}
		}

		public override void SetDefaultSizeAndPosition()
		{
			base.SetDefaultSizeAndPosition();
			base.Rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 500f);
			base.Rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 500f);
		}

		protected override void ConstructPanelContent()
		{
			dataHandler = new ButtonListHandler<GameObject, ButtonCell>(buttonScrollPool, 
				new Func<List<GameObject>>(GetEntries), 
				new Action<ButtonCell, int>(SetCell), new Func<GameObject, string, bool>(ShouldDisplayCell), new Action<int>(OnCellClicked));
            buttonScrollPool = UIFactory.CreateScrollPool<ButtonCell>(base.ContentRoot, "ResultsList", out GameObject gameObject, out GameObject gameObject2, null);
            buttonScrollPool.Initialize(dataHandler, null);
			GameObject gameObject3 = gameObject;
			int? num = new int?(9999);
			UIFactory.SetLayoutElement(gameObject3, null, null, null, num, null, null, null);
		}

		private ButtonListHandler<GameObject, ButtonCell> dataHandler;

		private ScrollPool<ButtonCell> buttonScrollPool;
    }
}
