namespace Metatool.Service.ScreenHint;
public class ScreenHintConfig
{
	public HintEncoderConfig HintEncoder { get; set; }
}
public class HintEncoderConfig
{
	public string HintKeys { get; set; }
	public string AdditionalSingleCodeKey { get; set; }
}