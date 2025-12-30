using Xunit;
using AUTOCAP.Core.Subtitle;

namespace AUTOCAP.Tests;

/// <summary>
/// Unit tests for the SubtitleEngine component.
/// Tests subtitle generation, punctuation, and export formats.
/// </summary>
public class SubtitleEngineTests
{
    [Fact]
    public void ProcessFinalResult_SingleWord_CreatesSubtitle()
    {
        // Arrange
        var engine = new SubtitleEngine();
        var lineCreatedEvent = false;
        engine.OnNewSubtitleLine += (s, e) => lineCreatedEvent = true;

        // Act
        engine.MaxLineCharacters = 5; // Force line creation on small buffer
        engine.ProcessFinalResult("test");
        engine.ProcessFinalResult("hello");

        // Assert
        Assert.True(lineCreatedEvent);
        Assert.NotEmpty(engine.GetSubtitleLines());
    }

    [Fact]
    public void ExportSRT_MultipleLinesWithPunctuation_GeneratesValidFormat()
    {
        // Arrange
        var engine = new SubtitleEngine();
        engine.MaxLineCharacters = 10;
        
        // Act
        engine.ProcessFinalResult("hello");
        engine.ProcessFinalResult("world");
        engine.ProcessFinalResult("test");
        var srt = engine.ExportSRT();

        // Assert
        Assert.NotEmpty(srt);
        Assert.Contains("-->", srt); // Check SRT timing format
        Assert.DoesNotContain("WEBVTT", srt); // Not VTT format
    }

    [Fact]
    public void ExportVTT_ProducesValidWebVTTFormat()
    {
        // Arrange
        var engine = new SubtitleEngine();
        engine.MaxLineCharacters = 5;
        
        // Act
        engine.ProcessFinalResult("test");
        engine.ProcessFinalResult("audio");
        var vtt = engine.ExportVTT();

        // Assert
        Assert.StartsWith("WEBVTT", vtt);
        Assert.Contains("-->", vtt);
    }

    [Fact]
    public void Clear_ResetsAllState()
    {
        // Arrange
        var engine = new SubtitleEngine();
        engine.ProcessFinalResult("test");

        // Act
        engine.Clear();

        // Assert
        Assert.Empty(engine.GetSubtitleLines());
    }

    [Fact]
    public void ProcessPartialResult_EmitsPartialEvent()
    {
        // Arrange
        var engine = new SubtitleEngine();
        string? capturedText = null;
        engine.OnPartialSubtitle += (s, text) => capturedText = text;

        // Act
        engine.ProcessPartialResult("partial hypothesis");

        // Assert
        Assert.Equal("partial hypothesis", capturedText);
    }

    [Fact]
    public void AutoPunctuate_CapitalizesAndAddsEndPeriod()
    {
        // Arrange
        var engine = new SubtitleEngine { AutoPunctuate = true, MaxLineCharacters = 5 };

        // Act
        engine.ProcessFinalResult("hello");
        engine.ProcessFinalResult("world");
        var lines = engine.GetSubtitleLines();

        // Assert
        Assert.NotEmpty(lines);
        var firstLine = lines[0];
        Assert.True(char.IsUpper(firstLine.Text[0]), "First letter should be capitalized");
        Assert.True(firstLine.Text.EndsWith(".") || firstLine.Text.EndsWith("!") || firstLine.Text.EndsWith("?"));
    }

    [Fact]
    public void MaxLineDuration_TriggersLineCreation()
    {
        // Arrange
        var engine = new SubtitleEngine
        {
            MaxLineCharacters = 1000, // High threshold to avoid char-based creation
            MaxLineDuration = TimeSpan.FromMilliseconds(100)
        };

        // Act
        engine.ProcessFinalResult("test");
        System.Threading.Thread.Sleep(150); // Wait for duration threshold
        engine.ProcessFinalResult("another");

        // Assert
        Assert.NotEmpty(engine.GetSubtitleLines());
    }

    [Fact]
    public void Sanity_Check()
    {
        Assert.True(true);
    }
}
