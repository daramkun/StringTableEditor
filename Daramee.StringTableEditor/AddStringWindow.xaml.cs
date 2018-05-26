using Daramee.DaramCommonLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// AddStringWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class AddStringWindow : Window
    {
		StringTable stringTable;

		public string Key { get; private set; }

		public AddStringWindow ( StringTable stringTable )
		{
			InitializeComponent ();
			this.stringTable = stringTable;
		}

		protected override void OnSourceInitialized ( EventArgs e )
		{
			WindowUtility.RemoveWindowIcon ( this );
		}

		private void AddButtn_Click ( object sender, RoutedEventArgs e )
		{
			if ( !Regex.IsMatch(textBoxKey.Text, "[a-zA-Z0-9_]+"))
			{
				MessageBox.Show ( "Key must assemble alphabet, '_', numbers." );
				return;
			}
			if ( stringTable.ContainsKey ( textBoxKey.Text ) )
			{
				MessageBox.Show ( "Key already added." );
				return;
			}
			
			Key = textBoxKey.Text;
			DialogResult = true;
		}

		private void CancelButton_Click ( object sender, RoutedEventArgs e )
		{
			DialogResult = false;
		}
	}
}
