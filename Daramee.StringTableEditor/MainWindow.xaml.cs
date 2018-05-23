﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Daramee.StringTableEditor
{
	/// <summary>
	/// MainWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class MainWindow : Window
	{
		StringTable stringTable = new StringTable ();
		string savePath = "";
		bool isSaved = true;

		public string SavePath
		{
			get => savePath;
			set
			{
				savePath = value;
				Title = $"{( isSaved ? "" : "*" )}{( string.IsNullOrEmpty ( savePath ) ? "Untitled.json" : System.IO.Path.GetFileName ( savePath ) )} - String Table Editor";
			}
		}
		public bool IsSaved
		{
			get => isSaved;
			set
			{
				isSaved = value;
				Title = $"{( isSaved ? "" : "*" )}{( string.IsNullOrEmpty ( savePath ) ? "Untitled.json" : System.IO.Path.GetFileName ( savePath ) )} - String Table Editor";
			}
		}

		private bool CheckSavedState ()
		{
			if ( IsSaved ) return true;
			var button = MessageBox.Show ( "Are you want to save?", "String Table Editor", MessageBoxButton.YesNoCancel, MessageBoxImage.Question );
			switch ( button )
			{
				case MessageBoxResult.Yes:
					return SaveFile ();

				case MessageBoxResult.Cancel:
					return false;
			}

			return true;
		}

		private void OpenFile ()
		{
			if ( !CheckSavedState () )
				return;

			OpenFileDialog ofd = new OpenFileDialog ()
			{
				Filter = "JSON File(*.json)|*.json",
			};
			if ( ofd.ShowDialog () == false )
				return;

			using ( Stream stream = new FileStream ( ofd.FileName, FileMode.Open ) )
			{
				var st = StringTable.LoadFrom ( stream );
				if ( st != null )
				{
					stringTable = st;
					SavePath = ofd.FileName;
					IsSaved = true;

					ResetControlBinding ();
				}
				else
					MessageBox.Show ( "It is not String Table JSON Formatted file.",
						"String Table Editor", MessageBoxButton.OK, MessageBoxImage.Error );
			}
		}

		private bool SaveFile ( bool saveas = false )
		{
			if ( string.IsNullOrEmpty ( SavePath ) || saveas )
			{
				SaveFileDialog sfd = new SaveFileDialog ()
				{
					Filter = "JSON File(*.json)|*.json",
				};
				if ( sfd.ShowDialog () == false )
					return false;

				using ( Stream stream = new FileStream ( sfd.FileName, FileMode.Create ) )
					if ( !stringTable.Save ( stream ) )
						return false;

				SavePath = sfd.FileName;
			}
			else
			{
				using ( Stream stream = new FileStream ( SavePath, FileMode.Create ) )
					if ( !stringTable.Save ( stream ) )
						return false;
			}

			ResetControlBinding ();

			IsSaved = true;
			return true;
		}

		private void ResetControlBinding ()
		{
			listBoxCultureInfos.ItemsSource = stringTable.Keys;
			dataGridTable.ItemsSource = null;
		}

		public MainWindow ()
		{
			InitializeComponent ();

			ResetControlBinding ();
		}

		private void NewFile_Click ( object sender, RoutedEventArgs e )
		{
			if ( !CheckSavedState () )
				return;

			stringTable = new StringTable ();
			SavePath = "";
			IsSaved = true;

			ResetControlBinding ();
		}

		private void OpenFile_Click ( object sender, RoutedEventArgs e )
		{
			OpenFile ();
		}

		private void SaveFile_Click ( object sender, RoutedEventArgs e )
		{
			SaveFile ();
		}

		private void SaveAs_Click ( object sender, RoutedEventArgs e )
		{
			SaveFile ( true );
		}

		private void Exit_Click ( object sender, RoutedEventArgs e )
		{
			this.Close ();
		}

		private void Window_Closing ( object sender, System.ComponentModel.CancelEventArgs e )
		{
			if ( !CheckSavedState () )
				e.Cancel = true;
		}

		private void ListBoxItem_MouseDoubleClick ( object sender, MouseButtonEventArgs e )
		{
			dataGridTable.ItemsSource = stringTable [ ( sender as ListBoxItem ).Content as CultureInfo ];
		}

		private void listBoxCultureInfos_SelectionChanged ( object sender, SelectionChangedEventArgs e )
		{
			if ( listBoxCultureInfos.SelectedItems.Count == 1 )
				dataGridTable.ItemsSource = stringTable [ listBoxCultureInfos.SelectedItem as CultureInfo ];
			else
				dataGridTable.ItemsSource = null;
		}

		private void AddLanguage_Click ( object sender, RoutedEventArgs e )
		{
			AddLanguageWindow window = new AddLanguageWindow ( stringTable ) { Owner = this, WindowStartupLocation = WindowStartupLocation.CenterOwner };
			if ( window.ShowDialog () == false )
				return;
			stringTable.Add ( window.SelectedCultureInfo );
			IsSaved = false;
		}

		private void RemoveLanguage_Click ( object sender, RoutedEventArgs e )
		{
			if ( listBoxCultureInfos.SelectedItems.Count == 0 )
				return;

			if ( MessageBox.Show ( "Are you want to remove selected languages?",
				"String Table Editor", MessageBoxButton.YesNo ) == MessageBoxResult.No )
				return;

			foreach ( CultureInfo cultureInfo in listBoxCultureInfos.SelectedItems )
			{
				stringTable.Remove ( cultureInfo );
			}
		}

		private void AddString_Click ( object sender, RoutedEventArgs e )
		{
			AddStringWindow window = new AddStringWindow ( stringTable ) { Owner = this, WindowStartupLocation = WindowStartupLocation.CenterOwner };
			if ( window.ShowDialog () == false )
				return;
			stringTable.AddKey ( window.Key );
			IsSaved = false;
		}

		private void RemoveString_Click ( object sender, RoutedEventArgs e )
		{
			if ( dataGridTable.SelectedItems.Count == 0 )
				return;

			if ( MessageBox.Show ( "Are you want to remove selected strings?",
				"String Table Editor", MessageBoxButton.YesNo ) == MessageBoxResult.No )
				return;

			foreach ( TableRecord record in dataGridTable.SelectedItems )
			{
				stringTable.RemoveKey ( record.Key );
			}
		}

		private void DataGridTable_CellEditEnding ( object sender, DataGridCellEditEndingEventArgs e )
		{
			IsSaved = false;
		}

		private void EditInfo_Click ( object sender, RoutedEventArgs e )
		{
			new EditInfoWindow ( stringTable ) { Owner = this, WindowStartupLocation = WindowStartupLocation.CenterOwner }.ShowDialog ();
		}

		private void ListBox_KeyUp ( object sender, KeyEventArgs e )
		{
			if ( listBoxCultureInfos.SelectedItems.Count > 0 && e.Key == Key.Delete )
			{
				RemoveLanguage_Click ( sender, e );
			}
		}

		private void DataGrid_KeyUp ( object sender, KeyEventArgs e )
		{
			if ( dataGridTable.SelectedItems.Count > 0 && e.Key == Key.Delete )
			{
				RemoveString_Click ( sender, e );
			}
		}
	}
}