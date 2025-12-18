using System.Text;
using Whisper.net;

namespace IntoHear.Core;

public static class SubtitleHelper
{
    public static string FormatToSrt(IEnumerable<SegmentData> segments)
    {
        var sb = new StringBuilder();
        int counter = 1;

        foreach (var segment in segments)
        {
            sb.AppendLine(counter.ToString());
            sb.AppendLine($"{FormatTime(segment.Start)} --> {FormatTime(segment.End)}");
            sb.AppendLine(segment.Text.Trim());
            sb.AppendLine();
            counter++;
        }

        return sb.ToString();
    }

    private static string FormatTime(TimeSpan time)
    {
        return time.ToString(@"hh\:mm\:ss\,fff");
    }
}
