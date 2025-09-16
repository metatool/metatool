using System;
using System.ComponentModel;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Metatool.Service.MouseKey;

/// <summary>
///     Describes key or key combination sequences. e.g. Control+Z,Z
/// </summary>
[TypeConverter(typeof(SequenceConverter))]
public partial class Sequence(params ICombination[] combinations) : SequenceList<ICombination>(combinations), ISequence
{
    public bool Disabled
	{
		get => this.Last()?.Disabled ?? false;
		set
		{
			if (Count > 0)
				this.Last().Disabled = value;
		}
	}

	public object Context { get; set; }

	public static Sequence FromHotString(string str)
	{
		var sequence = str.Select(c => new Combination(c));
		return new Sequence([.. sequence]);
	}

	public static Sequence Parse(string sequence)
	{
		if (sequence == null) throw new ArgumentNullException(nameof(sequence));

		var parts = sequence
			.Split(',', StringSplitOptions.RemoveEmptyEntries)
			.Select(p => Combination.Parse(p.Trim()));
		return new Sequence([..parts]);
	}

	public static bool TryParse(string str, out Sequence value, bool log = true)
	{
		try
		{
			value = Parse(str);
			return true;
		}
		catch (Exception e)
		{
			if(log) Services.CommonLogger?.LogError(e.Message);
			value = null;
			return false;
		}
	}
}