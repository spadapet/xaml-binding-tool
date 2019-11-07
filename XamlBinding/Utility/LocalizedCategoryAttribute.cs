using System;
using System.ComponentModel;
using XamlBinding.Resources;

namespace XamlBinding.Utility
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    internal sealed class LocalizedCategory : CategoryAttribute
    {
        private readonly string id;

        public LocalizedCategory(string id)
        {
            this.id = id;
        }

        protected override string GetLocalizedString(string _) => Resource.ResourceManager.GetString(this.id) ?? string.Empty;
    }
}
