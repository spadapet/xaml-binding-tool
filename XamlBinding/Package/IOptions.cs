using System;
using System.ComponentModel;

namespace XamlBinding.Package
{
    internal interface IOptions : INotifyPropertyChanged
    {
        bool PlaySoundOnError { get; }
        bool FlashWindowOnError { get; }
        bool ShowPaneOnError { get; }
        Guid UserId { get; }
    }
}
