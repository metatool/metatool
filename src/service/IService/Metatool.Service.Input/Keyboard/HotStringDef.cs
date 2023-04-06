﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace Metatool.Service;

public class HotStringConverter : TypeConverter
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
			string str => new HotStringDef(){str},
			_ => base.ConvertFrom(context, culture, value)
		};
	}
}

[TypeConverter(typeof(HotStringConverter))]
public class HotStringDef : List<string>
{

}