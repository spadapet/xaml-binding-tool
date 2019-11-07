using System;
using System.ComponentModel;
using XamlBinding.Resources;

namespace XamlBinding.Utility
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    internal sealed class LocalizedDisplayNameAttribute : DisplayNameAttribute
    {
        private readonly string id;

        public LocalizedDisplayNameAttribute(string id)
        {
            this.id = id;
        }

        public override string DisplayName => Resource.ResourceManager.GetString(this.id) ?? string.Empty;
    }
}
