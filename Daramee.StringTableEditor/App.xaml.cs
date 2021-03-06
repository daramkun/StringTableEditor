﻿using Daramee.DaramCommonLib;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace Daramee.StringTableEditor
{
	/// <summary>
	/// App.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class App : Application
	{
		Daramee.DaramCommonLib.StringTable ownLocalizer;

		public string LoadingFile { get; private set; } = null;

		public App ()
		{
			ProgramHelper.Initialize ( Assembly.GetExecutingAssembly (), "daramkun", "StringTableEditor" );
			ownLocalizer = new Daramee.DaramCommonLib.StringTable ();
		}

		protected override void OnStartup ( StartupEventArgs e )
		{
			if ( e.Args.Length > 0 )
				LoadingFile = e.Args [ 0 ];
			base.OnStartup ( e );
		}
	}
}
