using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityExplorerPlus.LineDrawing
{
    public record class LineData(Vector2 Start, Vector2 End, Color Color, int Depth, float Width = 0.7f);
    internal interface ILineProvider
    {
        List<LineData> Lines { get; }
    }
}
