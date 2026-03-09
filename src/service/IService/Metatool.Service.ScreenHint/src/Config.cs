namespace Metatool.Service.ScreenHint;
public class ScreenHintConfig
{
	public HintEncoderConfig HintEncoder { get; set; }
}
public class HintEncoderConfig
{
	public char[] HintKeys { get; set; }
	public char[] AdditionalSingleCodeKey { get; set; }
}