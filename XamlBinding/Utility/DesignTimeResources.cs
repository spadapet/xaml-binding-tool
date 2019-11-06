using System.Drawing;
using System.Windows;

namespace XamlBinding.Utility
{
    /// <summary>
    /// Just to make the XAML designer happy
    /// </summary>
    public sealed class DesignTimeResources : ResourceDictionary
    {
        public DesignTimeResources()
        {
            if (Constants.IsXamlDesigner)
            {
                this["VsBrush.Window"] = SystemBrushes.Window;
                this["VsBrush.WindowText"] = SystemBrushes.WindowText;
                this["VsBrush.ToolWindowBackground"] = SystemBrushes.Control;
            }
        }
    }
}
