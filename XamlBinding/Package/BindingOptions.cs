using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using XamlBinding.Resources;
using XamlBinding.Utility;

namespace XamlBinding.Package
{
    [Guid(Constants.BindingOptionsString)]
    internal sealed class BindingOptions : DialogPage, IOptions
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private bool playSoundOnError;
        private bool flashWindowOnError;
        private bool showPaneOnError;
        private Guid userId;

        [LocalizedCategory(nameof(Resource.OptionsCategoryForNewFailures))]
        [LocalizedDisplayName(nameof(Resource.OptionsNamePlaySound))]
        [LocalizedDescription(nameof(Resource.OptionsDescriptionPlaySound))]
        [DefaultValue(false)]
        public bool PlaySoundOnError
        {
            get => this.playSoundOnError;
            set => this.SetValue(ref this.playSoundOnError, value);
        }

        [LocalizedCategory(nameof(Resource.OptionsCategoryForNewFailures))]
        [LocalizedDisplayName(nameof(Resource.OptionsNameFlashWindow))]
        [LocalizedDescription(nameof(Resource.OptionsDescriptionFlashWindow))]
        [DefaultValue(false)]
        public bool FlashWindowOnError
        {
            get => this.flashWindowOnError;
            set => this.SetValue(ref this.flashWindowOnError, value);
        }

        [LocalizedCategory(nameof(Resource.OptionsCategoryForNewFailures))]
        [LocalizedDisplayName(nameof(Resource.OptionsNameShowPane))]
        [LocalizedDescription(nameof(Resource.OptionsDescriptionShowPane))]
        [DefaultValue(false)]
        public bool ShowPaneOnError
        {
            get => this.showPaneOnError;
            set => this.SetValue(ref this.showPaneOnError, value);
        }

        [Browsable(false)]
        public Guid UserId
        {
            get => this.userId;
            set => this.SetValue(ref this.userId, value);
        }

        private void OnPropertyChanged(string name = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name ?? string.Empty));
        }

        private bool SetValue<T>(ref T prop, T value, [CallerMemberName] string name = null)
        {
            if (!object.Equals(prop, value))
            {
                prop = value;
                this.OnPropertyChanged(name);
                return true;
            }

            return false;
        }

        public override void LoadSettingsFromStorage()
        {
            base.LoadSettingsFromStorage();
            this.LoadUserId();
        }

        private void LoadUserId()
        {
            try
            {
                using (RegistryKey rootKey = VSRegistry.RegistryRoot(__VsLocalRegistryType.RegType_UserSettings, writable: true))
                using (RegistryKey pageKey = rootKey.CreateSubKey(Constants.UserSettingsKey, writable: true))
                {
                    if (pageKey.GetValue(Constants.UserIdValue) is string userIdString && Guid.TryParse(userIdString, out Guid userId))
                    {
                        this.UserId = userId;
                    }
                    else
                    {
                        this.UserId = Guid.NewGuid();
                        pageKey.SetValue(Constants.UserIdValue, this.UserId.ToString());
                    }
                }
            }
            catch
            {
                // Not critical, just ignore registry errors here
            }
        }
    }
}
