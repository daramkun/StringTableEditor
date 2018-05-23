using System;
using System.Collections.Generic;
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
    /// EditInfoWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class EditInfoWindow : Window
    {
		StringTable stringTable;

		public EditInfoWindow ( StringTable stringTable )
		{
			this.stringTable = stringTable;

			InitializeComponent ();

			textBoxAuthor.Text = stringTable.Author;
			textBoxCopyright.Text = stringTable.Copyright;
			textBoxContact.Text = stringTable.Contact;
			textBoxTargetApp.Text = stringTable.TargetApp;
			textBoxTargetVersion.Text = stringTable.TargetVersion;
		}

		private void ApplyButton_Click ( object sender, RoutedEventArgs e )
		{
			stringTable.Author = textBoxAuthor.Text;
			stringTable.Copyright = textBoxCopyright.Text;
			stringTable.Contact = textBoxContact.Text;
			stringTable.TargetApp = textBoxTargetApp.Text;
			stringTable.TargetVersion = textBoxTargetVersion.Text;

			DialogResult = true;
		}

		private void CancelButton_Click ( object sender, RoutedEventArgs e )
		{
			DialogResult = false;
		}
    }
}
