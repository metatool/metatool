using System.Collections.Generic;
using System.Linq;
using Metaseed.Hint;
using Metatool.Service;
using Metatool.Service.ScreenHint;
using Microsoft.Extensions.Logging;

namespace Metatool.ScreenPoint;

public class HintsBuilderNew : IHintsBuilder
{
    private readonly ILogger<HintsBuilderNew> _logger;
    private readonly string[] _codeChars;
    private readonly string[] _additionalSingleCodeChars;

    public HintsBuilderNew(ILogger<HintsBuilderNew> logger, IConfig<HintEncoderConfig> config)
    {
        _logger = logger;
        var v = config.CurrentValue;
        _codeChars = v.HintKeys.Select(c => $"{c}").ToArray();
        _additionalSingleCodeChars = v.AdditionalSingleCodeKey.Select(c => $"{c}").ToArray();
    }

    public Dictionary<string, IUIElement> GenerateKeys(List<IUIElement> elementRects)
    {
        using var _ = _logger.Time("GetKeyPointPairs");

        var count = elementRects.Count;
        if (count == 0) return new();

        var codes = HintEncoder.GetJumpCodes(count, _codeChars, _additionalSingleCodeChars);

        var result = new Dictionary<string, IUIElement>(count);
        for (var i = 0; i < count; i++)
        {
            result.Add(codes[i], elementRects[i]);
        }

        return result;
    }
}
