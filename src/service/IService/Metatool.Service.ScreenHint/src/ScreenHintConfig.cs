namespace Metatool.Service.ScreenHint;
public class ScreenHintConfig
{
	public HintEncoderConfig HintEncoder { get; set; }
	public string HintForeground { get; set; } = "#FFD700";
	public string HintSingleCharBackground { get; set; } = "#B80033CC";
	public string HintTwoCharBackground { get; set; } = "#B8339933";
	public string HintBackground { get; set; } = "#B8CC3333";
	public string HintMatchedColor { get; set; } = "#9090A0";
}
public class HintEncoderConfig
{
	public string HintKeys { get; set; }
	public string AdditionalSingleCodeKey { get; set; }
}