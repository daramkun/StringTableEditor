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

		public Table ( IEnumerable<TableRecord> collection )
		{
			foreach ( var record in collection )
			{
				Add ( record.Key, record.Value );
			}
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

		public int KeysCount => tables.Count;

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
			tables.Add ( cultureInfo, new Table ( tables [ tables.Keys.FirstOrDefault () ] ) );
			Keys.DoNotifyCollectionChange ( new NotifyCollectionChangedEventArgs ( NotifyCollectionChangedAction.Add, cultureInfo ) );
		}

		public void Remove ( CultureInfo cultureInfo )
		{
			if ( !tables.ContainsKey ( cultureInfo ) )
				throw new KeyNotFoundException ();
			int index = IndexOf ( cultureInfo );
			if ( tables.Remove ( cultureInfo ) )
				Keys.DoNotifyCollectionChange ( new NotifyCollectionChangedEventArgs ( NotifyCollectionChangedAction.Remove,
					cultureInfo, index ) );
		}

		private int IndexOf ( CultureInfo cultureInfo )
		{
			IEnumerator<CultureInfo> c = tables.Keys.GetEnumerator ();
			for ( int i = 0; ; ++i )
			{
				if ( !c.MoveNext () )
					break;
				if ( c.Current == cultureInfo )
					return i;
			}
			return -1;
		}

		public bool ContainsKey ( string key )
		{
			return tables [ tables.Keys.FirstOrDefault () ].ContainsKey ( key );
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

#pragma warning disable CS0649
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
#pragma warning restore CS0649

		public string GetAndroidStringsXML ( CultureInfo cultureInfo, Stream stream )
		{
			StringBuilder builder = new StringBuilder ();
			builder.Append ( "<resources>\n" );
			foreach ( var kv in tables [ cultureInfo ] )
			{
				builder.Append ( "\t<string name=\"" ).Append ( kv.Key ).Append ( "\">" );
				builder.Append ( "<![CDATA[" ).Append ( kv.Value ).Append ( "]]>" );
				builder.Append ( "</string>\n" );
			}
			builder.Append ( "</resources>\n" );
			return builder.ToString ();
		}

		public string GetDotNetResX ( CultureInfo cultureInfo, Stream stream )
		{
			StringBuilder builder = new StringBuilder ();
			builder.Append ( "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" );
			builder.Append ( "<root>\n" );
			builder.Append ( "  <xsd:schema id=\"root\" xmlns=\"\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\">\n" );
			builder.Append ( "    <xsd:import namespace=\"http://www.w3.org/XML/1998/namespace\" />\n" );
			builder.Append ( "    <xsd:element name=\"root\" msdata:IsDataSet=\"true\">\n" );
			builder.Append ( "      <xsd:complexType>\n" );
			builder.Append ( "        <xsd:choice maxOccurs=\"unbounded\">\n" );
			builder.Append ( "          <xsd:element name=\"data\">\n" );
			builder.Append ( "            <xsd:complexType>\n" );
			builder.Append ( "              <xsd:sequence>\n" );
			builder.Append ( "                <xsd:element name=\"value\" type=\"xsd:string\" minOccurs=\"0\" msdata:Ordinal=\"1\" />\n" );
			builder.Append ( "                <xsd:element name=\"comment\" type=\"xsd:string\" minOccurs=\"0\" msdata:Ordinal=\"2\" />\n" );
			builder.Append ( "              </xsd:sequence>\n" );
			builder.Append ( "              <xsd:attribute name=\"name\" type=\"xsd:string\" use=\"required\" msdata:Ordinal=\"1\" />\n" );
			builder.Append ( "              <xsd:attribute name=\"type\" type=\"xsd:string\" msdata:Ordinal=\"3\" />\n" );
			builder.Append ( "              <xsd:attribute name=\"mimetype\" type=\"xsd:string\" msdata:Ordinal=\"4\" />\n" );
			builder.Append ( "              <xsd:attribute ref=\"xml:space\" />\n" );
			builder.Append ( "            </xsd:complexType>\n" );
			builder.Append ( "          </xsd:element>\n" );
			builder.Append ( "          <xsd:element name=\"resheader\">\n" );
			builder.Append ( "            <xsd:complexType>\n" );
			builder.Append ( "              <xsd:sequence>\n" );
			builder.Append ( "                <xsd:element name=\"value\" type=\"xsd:string\" minOccurs=\"0\" msdata:Ordinal=\"1\" />\n" );
			builder.Append ( "              </xsd:sequence>\n" );
			builder.Append ( "              <xsd:attribute name=\"name\" type=\"xsd:string\" use=\"required\" />\n" );
			builder.Append ( "            </xsd:complexType>\n" );
			builder.Append ( "          </xsd:element>\n" );
			builder.Append ( "        </xsd:choice>\n" );
			builder.Append ( "      </xsd:complexType>\n" );
			builder.Append ( "    </xsd:element>\n" );
			builder.Append ( "  </xsd:schema>\n" );
			builder.Append ( "  <resheader name=\"resmimetype\">\n" );
			builder.Append ( "    <value>text/microsoft-resx</value>\n" );
			builder.Append ( "  </resheader>\n" );
			builder.Append ( "  <resheader name=\"version\">\n" );
			builder.Append ( "    <value>2.0</value>\n" );
			builder.Append ( "  </resheader>\n" );
			builder.Append ( "  <resheader name=\"reader\">\n" );
			builder.Append ( "    <value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>\n" );
			builder.Append ( "  </resheader>\n" );
			builder.Append ( "  <resheader name=\"writer\">\n" );
			builder.Append ( "    <value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>\n" );
			builder.Append ( "  </resheader>\n" );
			foreach ( var kv in tables [ cultureInfo ] )
			{
				builder.Append ( "    <data	name=\"" ).Append ( kv.Key ).Append ( "\" xml:space=\"preserve\">\n" );
				builder.Append ( "      <value><![CDATA[" ).Append ( kv.Value ).Append ( "]]></value>\n" );
				builder.Append ( "    </data>\n" );
			}
			builder.Append ( "</root>" );
			return builder.ToString ();
		}

		public string GetXAMLResourceDictionary ( CultureInfo cultureInfo )
		{
			StringBuilder builder = new StringBuilder ();
			builder.Append ( "<ResourceDictionary xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\" xmlns:system=\"clr-namespace:System;assembly=mscorlib\">" );
			foreach ( var kv in tables [ cultureInfo ] )
			{
				builder.Append ( "\t<system:String x:Key=\"" ).Append ( kv.Key ).Append ( "\">" );
				builder.Append ( "<![CDATA[" ).Append ( kv.Value ).Append ( "]]>" );
				builder.Append ( "</system:String>\n" );
			}
			builder.Append ( "</ResourceDictionary>" );
			return builder.ToString ();
		}

		public string GetApplePropertyLists ( CultureInfo cultureInfo, Stream stream )
		{
			StringBuilder builder = new StringBuilder ();
			builder.Append ( "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" );
			builder.Append ( "<!DOCTYPE plist PUBLIC \"-//Apple//DTD PLIST 1.0//EN\" \"http://www.apple.com/DTDs/PropertyList-1.0.dtd\">\n" );
			builder.Append ( "<plist version=\"1.0\">\n" );
			builder.Append ( "<dict>\n" );
			foreach ( var kv in tables [ cultureInfo ] )
			{
				builder.Append ( "\t<key>" ).Append ( kv.Key ).Append ( "</key>\n" );
				builder.Append ( "\t<string><![CDATA[" ).Append ( kv.Value ).Append ( "]]></string>\n" );
			}
			builder.Append ( "</dict>\n" );
			builder.Append ( "</plist>\n" );
			return builder.ToString ();
		}
	}
}
