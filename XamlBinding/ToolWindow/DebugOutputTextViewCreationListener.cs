using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Composition;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using System.Diagnostics;

namespace XamlBinding.ToolWindow
{
    /// <summary>
    /// Useful for knowing when the debug output text view is created
    /// </summary>
    [Export(typeof(IWpfTextViewCreationListener))]
    [Export(typeof(DebugOutputTextViewCreationListener))]
    [ContentType("DebugOutput")]
    [TextViewRole("OUTPUTWINDOW")]
    internal class DebugOutputTextViewCreationListener : IWpfTextViewCreationListener
    {
        public delegate void TextViewCreatedFunc(IWpfTextView textView);
        private event TextViewCreatedFunc textViewCreated;

        public static event TextViewCreatedFunc TextViewCreated
        {
            add
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                if (DebugOutputTextViewCreationListener.Import is DebugOutputTextViewCreationListener textViewListener)
                {
                    textViewListener.textViewCreated += value;
                }
            }

            remove
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                if (DebugOutputTextViewCreationListener.Import is DebugOutputTextViewCreationListener textViewListener)
                {
                    textViewListener.textViewCreated -= value;
                }
            }
        }

        private static DebugOutputTextViewCreationListener Import
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                if (ServiceProvider.GlobalProvider.GetService(typeof(SComponentModel)) is IComponentModel componentModel)
                {
                    try
                    {
                        return componentModel.GetService<DebugOutputTextViewCreationListener>();
                    }
                    catch (CompositionFailedException ex)
                    {
                        Debug.Fail(ex.Message);
                    }
                }

                return null;
            }
        }

        void IWpfTextViewCreationListener.TextViewCreated(IWpfTextView textView)
        {
            this.textViewCreated?.Invoke(textView);
        }
    }
}
