using System.Windows;
using System.Windows.Media;

namespace XamlBinding.Utility
{
    /// <summary>
    /// Just to make the XAML designer happy
    /// </summary>
    public class DesignTimeResources : ResourceDictionary
    {
        public DesignTimeResources()
        {
            if (Constants.IsXamlDesigner)
            {
                this["VsBrush.Window"] = new SolidColorBrush();
                this["VsBrush.WindowText"] = new SolidColorBrush();
                this["VsBrush.ToolWindowBackground"] = new SolidColorBrush();
            }
        }
    }
}
