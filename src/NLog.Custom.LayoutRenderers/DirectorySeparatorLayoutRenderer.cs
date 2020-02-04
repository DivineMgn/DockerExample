using NLog.LayoutRenderers;
using System.IO;
using System.Text;

namespace NLog.Custom.LayoutRenderers
{
    [LayoutRenderer("ds")]
    public class DirectorySeparatorLayoutRenderer : LayoutRenderer
    {
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            builder.Append(Path.DirectorySeparatorChar);
        }
    }
}
