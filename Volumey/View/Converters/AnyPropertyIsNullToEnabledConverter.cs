﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace Volumey.View.Converters
{
	public class AnyPropertyIsNullToEnabledConverter : IMultiValueConverter
	{
		public object Convert(object[] properties, Type targetType, object parameter, CultureInfo culture)
		{
			if(properties == null)
				return false;
			foreach(var property in properties)
			{
				if(property == null)
					return false;
			}
			return true;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
			null;
	}
}