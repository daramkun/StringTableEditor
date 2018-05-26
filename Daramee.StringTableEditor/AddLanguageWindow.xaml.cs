using Daramee.DaramCommonLib;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Daramee.StringTableEditor
{
    /// <summary>
    /// AddLanguageWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class AddLanguageWindow : Window
    {
		public CultureInfo SelectedCultureInfo { get; private set; }

        public AddLanguageWindow( StringTable stringTable )
        {
            InitializeComponent();

			comboBoxLanguageRegion.ItemsSource = from c in CultureInfo.GetCultures ( CultureTypes.SpecificCultures ) where !stringTable.Keys.Contains ( c ) orderby c.DisplayName select c;
        }

		protected override void OnSourceInitialized ( EventArgs e )
		{
			WindowUtility.RemoveWindowIcon ( this );
		}

		private void AddButtn_Click ( object sender, RoutedEventArgs e )
		{
			SelectedCultureInfo = comboBoxLanguageRegion.SelectedItem as CultureInfo;
			DialogResult = true;
		}

		private void CancelButton_Click ( object sender, RoutedEventArgs e )
		{
			DialogResult = false;
		}
	}
}
