using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Daramee.StringTableEditor.Converters
{
	class CultureInfoToFlagImageConverter : IValueConverter
	{
		public object Convert ( object value, Type targetType, object parameter, CultureInfo culture )
		{
			var ci = value as CultureInfo;
			if ( ci == null || ci == CultureInfo.InvariantCulture ) return null;

			string targetName = $"Daramee.StringTableEditor.Resources.flags.{ci.Name.Substring ( ci.Name.IndexOf ( '-' ) + 1 ).ToLower ()}.gif";

			Assembly assembly = Assembly.GetExecutingAssembly ();
			foreach ( string name in assembly.GetManifestResourceNames () )
			{
				if ( name == targetName )
				{
					BitmapImage image = new BitmapImage ();
					image.BeginInit ();
					image.StreamSource = assembly.GetManifestResourceStream ( name );
					image.EndInit ();
					return image;
				}
			}

			return null;
		}

		public object ConvertBack ( object value, Type targetType, object parameter, CultureInfo culture )
		{
			throw new NotImplementedException ();
		}
	}
}
