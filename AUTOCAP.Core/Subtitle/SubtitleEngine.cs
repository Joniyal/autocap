namespace AUTOCAP.Core.Subtitle;

/// <summary>
/// Represents a single subtitle line with timing and text.
/// </summary>
public class SubtitleLine
{
    public int Index { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Text { get; set; } = string.Empty;
}

/// <summary>
/// Manages subtitle generation from ASR results.
/// Buffers partial/final results, applies punctuation heuristics, and generates timed subtitle lines.
/// </summary>
public class SubtitleEngine
{
    private List<SubtitleLine> _subtitleLines = new();
    private StringBuilder _currentBuffer = new();
    private DateTime _currentSegmentStart = DateTime.UtcNow;
    private int _lineIndex = 0;

    // Settings
    public int MaxLineCharacters { get; set; } = 50;
    public TimeSpan MaxLineDuration { get; set; } = TimeSpan.FromMilliseconds(2500);
    public TimeSpan PauseThresholdForPunctuation { get; set; } = TimeSpan.FromMilliseconds(800);
    public bool AutoPunctuate { get; set; } = true;

    public event EventHandler<SubtitleLine>? OnNewSubtitleLine;
    public event EventHandler<string>? OnPartialSubtitle;

    /// <summary>
    /// Process a final ASR result and potentially create a subtitle line.
    /// </summary>
    public void ProcessFinalResult(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        _currentBuffer.Append(" ").Append(text);
        
        // Check if we should create a line (buffer size, duration, or explicit end)
        if (ShouldCreateLine())
        {
            CreateSubtitleLine();
        }
    }

    /// <summary>
    /// Process a partial (interim) ASR result for live display.
    /// </summary>
    public void ProcessPartialResult(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        // Emit partial for real-time display
        OnPartialSubtitle?.Invoke(this, text);
    }

    /// <summary>
    /// Force creation of a subtitle line (useful at the end of a segment or user interrupt).
    /// </summary>
    public void FlushCurrentLine()
    {
        if (_currentBuffer.Length > 0)
        {
            CreateSubtitleLine();
        }
    }

    /// <summary>
    /// Get all subtitle lines generated so far.
    /// </summary>
    public List<SubtitleLine> GetSubtitleLines() => new(_subtitleLines);

    /// <summary>
    /// Clear all subtitles and reset state.
    /// </summary>
    public void Clear()
    {
        _subtitleLines.Clear();
        _currentBuffer.Clear();
        _currentSegmentStart = DateTime.UtcNow;
        _lineIndex = 0;
    }

    /// <summary>
    /// Export subtitles in SRT (SubRip) format.
    /// </summary>
    public string ExportSRT()
    {
        var sb = new StringBuilder();
        foreach (var line in _subtitleLines)
        {
            sb.AppendLine(line.Index.ToString());
            sb.AppendLine($"{FormatTime(line.StartTime)} --> {FormatTime(line.EndTime)}");
            sb.AppendLine(line.Text);
            sb.AppendLine();
        }
        return sb.ToString();
    }

    /// <summary>
    /// Export subtitles in VTT (WebVTT) format.
    /// </summary>
    public string ExportVTT()
    {
        var sb = new StringBuilder();
        sb.AppendLine("WEBVTT");
        sb.AppendLine();

        foreach (var line in _subtitleLines)
        {
            sb.AppendLine($"{FormatTimeVTT(line.StartTime)} --> {FormatTimeVTT(line.EndTime)}");
            sb.AppendLine(line.Text);
            sb.AppendLine();
        }
        return sb.ToString();
    }

    private bool ShouldCreateLine()
    {
        if (_currentBuffer.Length == 0)
            return false;

        // Check if buffer exceeded max characters
        if (_currentBuffer.Length >= MaxLineCharacters)
            return true;

        // Check if line duration exceeded
        var duration = DateTime.UtcNow - _currentSegmentStart;
        if (duration >= MaxLineDuration)
            return true;

        return false;
    }

    private void CreateSubtitleLine()
    {
        if (_currentBuffer.Length == 0)
            return;

        _lineIndex++;
        string text = _currentBuffer.ToString().Trim();

        // Apply basic punctuation
        if (AutoPunctuate)
        {
            text = ApplyPunctuation(text);
        }

        var endTime = DateTime.UtcNow;
        var line = new SubtitleLine
        {
            Index = _lineIndex,
            StartTime = TimeSpan.Zero, // Will be calculated from actual timing context
            EndTime = TimeSpan.Zero,
            Text = text
        };

        _subtitleLines.Add(line);
        OnNewSubtitleLine?.Invoke(this, line);

        _currentBuffer.Clear();
        _currentSegmentStart = DateTime.UtcNow;
    }

    private string ApplyPunctuation(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        // Capitalize first letter
        text = char.ToUpper(text[0]) + text[1..];

        // Add period at end if not already punctuated
        if (!text.EndsWith('.') && !text.EndsWith('!') && !text.EndsWith('?'))
        {
            text += ".";
        }

        return text;
    }

    private string FormatTime(TimeSpan time)
    {
        // SRT format: HH:MM:SS,mmm
        int hours = (int)time.TotalHours;
        int minutes = time.Minutes;
        int seconds = time.Seconds;
        int milliseconds = time.Milliseconds;

        return $"{hours:D2}:{minutes:D2}:{seconds:D2},{milliseconds:D3}";
    }

    private string FormatTimeVTT(TimeSpan time)
    {
        // VTT format: HH:MM:SS.mmm
        int hours = (int)time.TotalHours;
        int minutes = time.Minutes;
        int seconds = time.Seconds;
        int milliseconds = time.Milliseconds;

        return $"{hours:D2}:{minutes:D2}:{seconds:D2}.{milliseconds:D3}";
    }
}
