using System;
using System.ComponentModel;
using System.Globalization;

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
}