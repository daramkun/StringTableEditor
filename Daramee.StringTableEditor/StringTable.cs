using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Daramee.StringTableEditor
{
	[Serializable]
	public class StringTableKeyCollection : IEnumerable<CultureInfo>, INotifyCollectionChanged
	{
		Dictionary<CultureInfo, Table> tables;

		[field:NonSerialized]
		public event NotifyCollectionChangedEventHandler CollectionChanged;

		public StringTableKeyCollection ( Dictionary<CultureInfo, Table> tables )
		{
			this.tables = tables;
		}

		public IEnumerator<CultureInfo> GetEnumerator ()
		{
			return tables.Keys.GetEnumerator ();
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return tables.Keys.GetEnumerator ();
		}

		public void DoNotifyCollectionChange ( NotifyCollectionChangedEventArgs args )
		{
			//PropertyChanged?.Invoke ( this, new PropertyChangedEventArgs ( nameof ( Count ) ) );
			CollectionChanged?.Invoke ( this, args );
		}
	}

	[Serializable]
	public class TableRecord : INotifyPropertyChanged
	{
		readonly string key;
		string value;

		public string Key => key;
		public string Value
		{
			get => value;
			set
			{
				this.value = value;
				PropertyChanged?.Invoke ( this, new PropertyChangedEventArgs ( nameof ( Value ) ) );
			}
		}

		[field: NonSerialized]
		public event PropertyChangedEventHandler PropertyChanged;

		public TableRecord ( string key, string value = "" )
		{
			this.key = key;
			this.value = value;
		}

		public override int GetHashCode () => Key.GetHashCode ();
		public override string ToString () => $"{{Key: {Key}, Value: {Value}}}";
	}

	[Serializable]
	public class Table : ObservableCollection<TableRecord>
	{
		public bool IsReadOnly => false;

		public string this [ string key ]
		{
			get
			{
				return GetRecord ( key ).Value ?? throw new KeyNotFoundException ();
			}
			set
			{
				var tuple = GetRecord ( key );
				if ( tuple == null )
					throw new KeyNotFoundException ();
				tuple.Value = value;
			}
		}

		TableRecord GetRecord ( string key )
		{
			foreach ( var tuple in this )
			{
				if ( tuple.Key.Equals ( key ) )
					return tuple;
			}
			return null;
		}

		public Table ()
		{

		}

		public void Add ( string key, string value )
		{
			Add ( new TableRecord ( key, value ) );
		}

		public bool Remove ( string key )
		{
			TableRecord found = null;

			foreach ( var tuple in this )
			{
				if ( tuple.Key.Equals ( key ) )
				{
					found = tuple;
					break;
				}
			}

			if ( found != null )
			{
				Remove ( found );
				return true;
			}

			return false;
		}

		public bool ContainsKey ( string key )
		{
			foreach ( var tuple in this )
			{
				if ( tuple.Key.Equals ( key ) )
					return true;
			}
			return false;
		}
	}

	[Serializable]
	public class StringTable : INotifyPropertyChanged
	{
		Dictionary<CultureInfo, Table> tables;
		string targetApp, targetVersion, author, copyright, contact;

		[NonSerialized]
		StringTableKeyCollection keys;

		[field: NonSerialized]
		public event PropertyChangedEventHandler PropertyChanged;

		public StringTableKeyCollection Keys => keys ?? ( keys = new StringTableKeyCollection ( tables ) );

		public Table this [ CultureInfo cultureInfo ] { get => tables [ cultureInfo ]; }

		public string TargetApp { get => targetApp; set { targetApp = value; PropertyChanged?.Invoke ( this, new PropertyChangedEventArgs ( nameof ( TargetApp ) ) ); } }
		public string TargetVersion { get => targetVersion; set { targetVersion = value; PropertyChanged?.Invoke ( this, new PropertyChangedEventArgs ( nameof ( TargetVersion ) ) ); } }
		public string Author { get => author; set { author = value; PropertyChanged?.Invoke ( this, new PropertyChangedEventArgs ( nameof ( Author ) ) ); } }
		public string Copyright { get => copyright; set { copyright = value; PropertyChanged?.Invoke ( this, new PropertyChangedEventArgs ( nameof ( Copyright ) ) ); } }
		public string Contact { get => contact; set { contact = value; PropertyChanged?.Invoke ( this, new PropertyChangedEventArgs ( nameof ( Contact ) ) ); } }

		public StringTable ()
		{
			tables = new Dictionary<CultureInfo, Table>
			{
				{ CultureInfo.InvariantCulture, new Table () },
				{ CultureInfo.CurrentCulture, new Table () }
			};
			keys = new StringTableKeyCollection ( tables );

			TargetApp = "";
			TargetVersion = "";
			Author = Environment.UserName;
			Copyright = $"Copyright ⓒ {DateTime.Today.Year} {Author}";
			Contact = "";
		}

		private StringTable ( IOContract contract )
		{
			TargetApp = contract.TargetApp;
			TargetVersion = contract.TargetVersion;
			Author = contract.Author;
			Copyright = contract.Copyright;
			Contact = contract.Contact;

			tables = new Dictionary<CultureInfo, Table> ();
			foreach ( var lang in contract.Languages )
			{
				Table table = new Table ();
				foreach ( var kv in lang.Table )
					table.Add ( kv.Key, kv.Value );
				tables.Add ( CultureInfo.GetCultureInfo ( lang.LanguageRegion ), table );
			}

			// Keys validation check and add automatically
			foreach ( var table in tables )
			{
				foreach ( var kv in table.Value )
				{
					foreach ( var table2 in tables )
					{
						if ( table.Key == table2.Key ) continue;
						if ( !table2.Value.ContainsKey ( kv.Key ) )
							table2.Value.Add ( kv.Key, "" );
					}
				}
			}

			keys = new StringTableKeyCollection ( tables );
		}

		public void Add ( CultureInfo cultureInfo )
		{
			if ( tables.ContainsKey ( cultureInfo ) || cultureInfo == CultureInfo.InvariantCulture )
				throw new ArgumentException ();
			tables.Add ( cultureInfo, tables [ CultureInfo.InvariantCulture ] );
			Keys.DoNotifyCollectionChange ( new NotifyCollectionChangedEventArgs ( NotifyCollectionChangedAction.Add, cultureInfo ) );
		}

		public void Remove ( CultureInfo cultureInfo )
		{
			if ( !tables.ContainsKey ( cultureInfo ) )
				throw new KeyNotFoundException ();
			tables.Remove ( cultureInfo );
			Keys.DoNotifyCollectionChange ( new NotifyCollectionChangedEventArgs ( NotifyCollectionChangedAction.Remove, cultureInfo ) );
		}

		public bool ContainsKey ( string key )
		{
			return tables [ CultureInfo.InvariantCulture ].ContainsKey ( key );
		}

		public bool AddKey ( string key )
		{
			if ( ContainsKey ( key ) )
				return false;
			foreach ( var table in tables )
				table.Value.Add ( key, "" );
			return true;
		}

		public bool RemoveKey ( string key )
		{
			if ( !ContainsKey ( key ) )
				return false;
			foreach ( var table in tables )
				table.Value.Remove ( key );
			return true;
		}

		static System.Runtime.Serialization.Json.DataContractJsonSerializer serializer
			 = new System.Runtime.Serialization.Json.DataContractJsonSerializer ( typeof ( IOContract ), new System.Runtime.Serialization.Json.DataContractJsonSerializerSettings ()
			 {
				 UseSimpleDictionaryFormat = true,
			 } );

		public static StringTable LoadFrom ( Stream stream )
		{
			try
			{
				using ( StreamReader reader = new StreamReader ( stream, Encoding.UTF8, true ) )
				{
					using ( Stream mem = new MemoryStream ( Encoding.UTF8.GetBytes ( reader.ReadToEnd () ) ) )
					{
						IOContract contract = serializer.ReadObject ( mem ) as IOContract;
						return new StringTable ( contract );
					}
				}
			}
			catch { return null; }
		}

		public bool Save ( Stream stream )
		{
			IOContract contract = new IOContract
			{
				TargetApp = TargetApp,
				TargetVersion = TargetVersion,
				Author = Author,
				Copyright = Copyright,
				Contact = Contact,
				Languages = new List<IOContract.Language> ()
			};

			foreach ( var table in tables )
			{
				var language = new IOContract.Language
				{
					LanguageRegion = table.Key.Name,
					Table = new Dictionary<string, string> ()
				};
				foreach ( var record in table.Value )
				{
					language.Table.Add ( record.Key, record.Value );
				}
				contract.Languages.Add ( language );
			}

			using ( MemoryStream mem = new MemoryStream () )
			{
				serializer.WriteObject ( mem, contract );
				using ( StreamWriter writer = new StreamWriter ( stream, Encoding.UTF8 ) )
				{
					writer.Write ( Encoding.UTF8.GetString ( mem.ToArray () ) );
				}
			}

			return true;
		}

		[DataContract]
		private class IOContract
		{
			[DataMember ( Name = "target", Order = 1 )]
			public string TargetApp;
			[DataMember ( Name = "version", Order = 2, IsRequired = false )]
			public string TargetVersion;
			[DataMember ( Name = "author", Order = 3, IsRequired = false )]
			public string Author;
			[DataMember ( Name = "copyright", Order = 4, IsRequired = false )]
			public string Copyright;
			[DataMember ( Name = "contact", Order = 5, IsRequired = false )]
			public string Contact;

			[DataContract]
			public class Language
			{
				[DataMember ( Name = "language", Order = 1 )]
				public string LanguageRegion;
				[DataMember ( Name = "table", Order = 2 )]
				public Dictionary<string, string> Table;
			}

			[DataMember ( Name = "languages", Order = 6 )]
			public List<Language> Languages;
		}
	}
}
