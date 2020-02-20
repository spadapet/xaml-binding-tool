using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.DiagnosticsHub.Views;
using XamlBinding.Resources;

namespace XamlBinding.DiagnosticTools
{
    internal sealed class BindingSummaryItem : ISummaryItem
    {
        public BindingSummaryItem()
        {
        }

#pragma warning disable CS0067
        public event EventHandler TextChanged;
        public event EventHandler IconChanged;
        public event EventHandler VisibleChanged;
        public event EventHandler EnabledChanged;
#pragma warning restore CS0067

        string ISummaryItem.Text => Resource.BindingSummaryItem_Name;
        string ISummaryItem.Icon => null;
        bool ISummaryItem.Visible => true;
        bool ISummaryItem.Enabled => true;
        SummaryItemStyle ISummaryItem.Style => null;

        IEnumerable<KeyValuePair<string, string>> ISummaryItem.GetIconPaths()
        {
            return Enumerable.Empty<KeyValuePair<string, string>>();
        }

        Action ISummaryItem.GetInvoke()
        {
            return null;
        }

        void ISummaryItem.OnFrequentDataUpdate()
        {
        }
    }
}
