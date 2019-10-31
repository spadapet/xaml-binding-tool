using Microsoft.VisualStudio.Composition;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System;
using System.ComponentModel.Composition;
using Task = System.Threading.Tasks.Task;

namespace XamlBinding.Package
{
    /// <summary>
    /// Tells the package whenever a debug output text view is created
    /// </summary>
    [Export(typeof(ITextViewCreationListener))]
    [ContentType("DebugOutput")]
    [TextViewRole("OUTPUTWINDOW")]
    internal class DebugOutputTextViewCreationListener : ITextViewCreationListener
    {
        void ITextViewCreationListener.TextViewCreated(ITextView textView)
        {
            ThreadHelper.JoinableTaskFactory.RunAsync(async delegate
            {
                await this.TextViewCreatedAsync(textView);
            });
        }

        private async Task TextViewCreatedAsync(ITextView textView)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (await ServiceProvider.GetGlobalServiceAsync<SVsShell, IVsShell7>() is IVsShell7 shell)
            {
                Guid packageId = Constants.GuidPackage;
                if (await shell.LoadPackageAsync(ref packageId) is BindingPackage package)
                {
                    package.OnDebugOutputTextViewCreated(textView);
                }
            }
        }
    }
}
