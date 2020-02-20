using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using XamlBinding.Package;

namespace XamlBinding.Utility
{
    /// <summary>
    /// Sends anonymous event data to Application Insights
    /// </summary>
    internal sealed class Telemetry : IDisposable
    {
        private readonly SolutionOptions solutionOptions;
        private readonly TelemetryConfiguration config;
        private readonly TelemetryClient client;
        private bool disposed;

        public Telemetry(IOptions options, SolutionOptions solutionOptions)
        {
            this.solutionOptions = solutionOptions;

            try
            {
                this.config = new TelemetryConfiguration(Constants.ApplicationInsightsInstrumentationKeyString);
#if DEBUG
                this.config.TelemetryChannel.DeveloperMode = true;
#endif
                this.client = new TelemetryClient(this.config);
                this.client.Context.Cloud.RoleInstance = "null";
                this.client.Context.Component.Version = this.GetType().Assembly.GetName().Version.ToString();
                this.client.Context.Session.Id = Guid.NewGuid().ToString();
                this.client.Context.User.Id = options.UserId.ToString();
                this.client.Context.GlobalProperties[Constants.PropertySolutionId] = this.GetSolutionId();

                this.solutionOptions.PropertyChanged += this.OnSolutionOptionChanged;
            }
            catch
            {
                // Can't log telemetry if App Insights fails to load, pretend that I have been disposed
                this.disposed = true;
            }
        }

        public void Dispose()
        {
            if (!this.disposed)
            {
                this.disposed = true;

                this.solutionOptions.PropertyChanged -= this.OnSolutionOptionChanged;

                this.client.Flush();
                this.config.Dispose();
            }
        }

        private string GetSolutionId()
        {
            if (!this.solutionOptions.TryGet(Constants.OptionSolutionId, out Guid id))
            {
                id = Guid.NewGuid();
                this.solutionOptions.Set(Constants.OptionSolutionId, id);
            }

            return id.ToString();
        }

        private void OnSolutionOptionChanged(object sender, PropertyChangedEventArgs args)
        {
            this.client.Context.GlobalProperties[Constants.PropertySolutionId] = this.GetSolutionId();
        }

        public void TrackEvent(string eventName, IEnumerable<KeyValuePair<string, object>> properties = null)
        {
            if (!this.disposed && !string.IsNullOrEmpty(eventName))
            {
                Telemetry.ConvertProperties(properties,
                    out Dictionary<string, string> eventProperties,
                    out Dictionary<string, double> eventMetrics);

                this.client.TrackEvent(eventName,
                    (eventProperties != null && eventProperties.Count > 0) ? eventProperties : null,
                    (eventMetrics != null && eventMetrics.Count > 0) ? eventMetrics : null);
            }
        }

        public void TrackException(Exception exception, IEnumerable<KeyValuePair<string, object>> properties = null)
        {
            if (!this.disposed && exception != null)
            {
                Telemetry.ConvertProperties(properties,
                    out Dictionary<string, string> eventProperties,
                    out Dictionary<string, double> eventMetrics);

                this.client.TrackException(exception,
                    (eventProperties != null && eventProperties.Count > 0) ? eventProperties : null,
                    (eventMetrics != null && eventMetrics.Count > 0) ? eventMetrics : null);
            }
        }

        private static void ConvertProperties(IEnumerable<KeyValuePair<string, object>> properties, out Dictionary<string, string> eventProperties, out Dictionary<string, double> eventMetrics)
        {
            if (properties == null)
            {
                eventProperties = null;
                eventMetrics = null;
                return;
            }

            eventProperties = new Dictionary<string, string>();
            eventMetrics = new Dictionary<string, double>();

            foreach (KeyValuePair<string, object> pair in properties.Where(p => !string.IsNullOrEmpty(p.Key) && p.Value != null))
            {
                if (!(pair.Value is IConvertible convertible))
                {
                    if (pair.Value is TimeSpan timeSpan)
                    {
                        eventMetrics[pair.Key] = timeSpan.TotalMilliseconds;
                    }
                    else if (pair.Value.ToString() is string stringValue)
                    {
                        eventProperties[pair.Key] = stringValue;
                    }

                    continue;
                }

                switch (convertible.GetTypeCode())
                {
                    case TypeCode.DateTime:
                        eventProperties[pair.Key] = ((DateTime)pair.Value).ToString("u", CultureInfo.InvariantCulture);
                        break;

                    case TypeCode.Boolean:
                    case TypeCode.Char:
                    case TypeCode.String:
                    case TypeCode.Object:
                        try
                        {
                            if (convertible.ToString(CultureInfo.InvariantCulture) is string stringValue)
                            {
                                eventProperties[pair.Key] = stringValue;
                            }
                        }
                        catch
                        {
                            // ignore it
                        }
                        break;

                    case TypeCode.Byte:
                    case TypeCode.Decimal:
                    case TypeCode.Double:
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                    case TypeCode.SByte:
                    case TypeCode.Single:
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                    case TypeCode.UInt64:
                        if (pair.Value.GetType().IsEnum)
                        {
                            eventProperties[pair.Key] = Enum.GetName(pair.Value.GetType(), pair.Value);
                        }
                        else
                        {
                            try
                            {
                                if (convertible.ToDouble(CultureInfo.InvariantCulture) is double doubleValue)
                                {
                                    eventMetrics[pair.Key] = doubleValue;
                                }
                            }
                            catch
                            {
                                // ignore it
                            }
                        }
                        break;
                }
            }
        }
    }
}
