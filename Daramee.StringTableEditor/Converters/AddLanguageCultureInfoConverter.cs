using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Daramee.StringTableEditor.Converters
{
	class AddLanguageCultureInfoConverter : IValueConverter
	{
		public object Convert ( object value, Type targetType, object parameter, CultureInfo culture )
		{
			CultureInfo c = value as CultureInfo;
			return c != null ? $"{c.DisplayName} - {c.Name}" : "{Null}";
		}

		public object ConvertBack ( object value, Type targetType, object parameter, CultureInfo culture )
		{
			throw new NotImplementedException ();
		}
	}
}
