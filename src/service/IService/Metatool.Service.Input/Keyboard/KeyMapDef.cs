using System;
using System.ComponentModel;
using System.Globalization;

namespace Metatool.Service;

public class KeyMapConverter : TypeConverter
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
			string str => new KeyMapDef() { Target = str},
			_ => base.ConvertFrom(context, culture, value)
		};
	}
}

[TypeConverter(typeof(KeyMapConverter))]
public class KeyMapDef
{
	public KeyMaps Type { get; set; } = KeyMaps.MapOnDownUp;
	public string Target { get; set; }
	public string Description { get; set; }

}