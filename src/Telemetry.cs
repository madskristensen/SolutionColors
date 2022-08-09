#if !DEBUG
#define telemetry
#endif

using System.Diagnostics;
using Microsoft.VisualStudio.Telemetry;

namespace SolutionColors
{
    public class Telemetry
    {
        private const string _namespace = "VS/Extension/" + Vsix.Name + "/";

        [Conditional("telemetry")]
        public static void TrackUserTask(string name, TelemetryResult result = TelemetryResult.Success)
        {
            TelemetryService.DefaultSession.PostUserTask(CleanName(name), result);
        }

        [Conditional("telemetry")]
        public static void TrackOperation(string name, string details = "", TelemetryResult result = TelemetryResult.Success)
        {
            TelemetryService.DefaultSession.PostOperation(CleanName(name), result, details);
        }

        [Conditional("telemetry")]
        public static void TrackProperty(string name, string value)
        {
            TelemetryService.DefaultSession.PostProperty(CleanName(name), value);
        }

        [Conditional("telemetry")]
        public static void TrackException(string name, Exception exception)
        {
            if (string.IsNullOrWhiteSpace(name) || exception == null)
            {
                return;
            }

            TelemetryService.DefaultSession.PostFault(CleanName(name), exception.Message, exception);
        }

        private static string CleanName(string name)
        {
            return (_namespace + name).Replace(" ", "_");
        }
    }
}
