using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using Metatool.Input.implementation;
using Microsoft.Extensions.Logging;

namespace Metatool.Service
{
    public class SequenceConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context,
            Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }

            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context,
            CultureInfo culture, object value)
        {
            return value switch
            {
                string str => Sequence.Parse(str),
                _ => base.ConvertFrom(context, culture, value)
            };
        }
    }

    /// <summary>
    ///     Describes key or key combination sequences. e.g. Control+Z,Z
    /// </summary>
    [TypeConverter(typeof(SequenceConverter))]
    public partial class Sequence : SequenceBase<ICombination>, ISequence
    {
        public Sequence(params ICombination[] combinations) : base(combinations)
        {
        }

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

        public static Sequence FromString(string str)
        {
            var sequence = str.Select(Helper.CharToKey);
            return new Sequence(sequence.ToArray());
        }

        public static Sequence Parse(string sequence)
        {
            if (sequence == null) throw new ArgumentNullException(nameof(sequence));

            var parts = sequence
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => Combination.Parse(p.Trim()));
            return new Sequence(parts.ToArray());
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
}