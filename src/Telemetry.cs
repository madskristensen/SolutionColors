using Microsoft.VisualStudio.Telemetry;

namespace SolutionColors
{
    public class Telemetry
    {
        private const string _namespace = "VS/Extension/" + Vsix.Name + "/";

        public static void TrackUserTask(string name, TelemetryResult result = TelemetryResult.Success)
        {
            string actualName = name.Replace(" ", "_");
            TelemetryService.DefaultSession.PostUserTask(_namespace + actualName, result);
        }
        
        public static void TrackOperation(string name, string details = "", TelemetryResult result = TelemetryResult.Success)
        {
            string actualName = name.Replace(" ", "_");
            TelemetryService.DefaultSession.PostOperation(_namespace + actualName, result, details);
        }

        public static void TrackProperty(string name, string value)
        {
            string actualName = name.Replace(" ", "_");
            TelemetryService.DefaultSession.PostProperty(_namespace + actualName, value);
        }

        public static void TrackException(string name, Exception exception)
        {
            if (string.IsNullOrWhiteSpace(name) || exception == null)
            {
                return;
            }

            string actualName = name.Replace(" ", "_");
            TelemetryService.DefaultSession.PostFault(_namespace + actualName, exception.Message, exception);
        }
    }
}
