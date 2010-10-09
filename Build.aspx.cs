using System.Data;
using System.Text;
using System.IO;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Collections.Generic;
using System.Data.Common;
using System;


// This page is run from the menu system. No views will hit build.aspx directly as they will be redirected to edit.aspx as soon
// as processing finishes.
namespace Law.UI.AVInventory
{
	public partial class Build: Law.UI.BasePage
	{
		private void Page_Load( object sender, System.EventArgs e ) {
			SubmitBuild_Click();
		}

		protected void SubmitBuild_Click() {
			// To change your root folder place the url or path of the folder you wish
			// in the RootFolder variable (Note: Please truncate the DB table before changing the root folder)
			string RootFolder = /* Path to root folder */;
			DirectoryInfo directory = new DirectoryInfo( RootFolder );
			List<string> FileList = new List<string>();
			foreach( FileSystemInfo temp in directory.GetFileSystemInfos() ) {
			   FileList.Add( temp.Name.ToString() );
			}

			Database database = DatabaseFactory.CreateDatabase(/* General Db Connection string */);
			using( DbCommand command = database.GetStoredProcCommand( "AvLoadInventoryData"" ) ) {
				database.AddInParameter( command, "@Data", DbType.Xml, xmlgenerator( FileList, RootFolder ) );
				database.ExecuteNonQuery( command );
			}
			DataSet ds = new DataSet();
			using( DbCommand command = database.GetStoredProcCommand( "AvGetDataTypes" ) ) {
				ds = database.ExecuteDataSet( command );
			}
			StringBuilder xmldata = new StringBuilder();
			foreach( DataRow row in ds.Tables[0].Rows ) {
				xmldata.Append( "<field type=\"" );
				xmldata.Append( row["DataType"] );
				xmldata.Append( "\">" );
				xmldata.Append( "<value> </value>" );
				xmldata.Append( "</field>" );
			}
			using( DbCommand command = database.GetStoredProcCommand("AvUpdateAllMetaData" ) ) {
				database.AddInParameter( command, "@Data", DbType.Xml, xmldata.ToString() );
				database.ExecuteNonQuery( command );
			}
			Response.Redirect( "Edit.aspx" ); 
		}

		protected List<string> CreateFileList( DirectoryInfo parent ) {
			List<string> FileList = new List<string>();
			foreach( DirectoryInfo temp in parent.GetDirectories() ) {
					FileList.AddRange( CreateFileList( temp ) );
			}
			foreach( FileSystemInfo temp in parent.GetFileSystemInfos() ) {
				if( !temp.Name.StartsWith( "." ))
					FileList.Add( temp.FullName );
			}
			return FileList;
		}

		private string xmlgenerator( List<string> FileList, string RootFolder ) {
			StringBuilder files = new StringBuilder();


			string[] RootFolderSplit = RootFolder.Split( '\\' );
			string RootFolderName = RootFolderSplit[RootFolderSplit.Length - 1];

			files.Append( "<data>" );
			files.Append( "<files>" );
			files.Append( "<file FullPath=\"" );
			files.Append( RootFolder );
			files.Append( "\"" );
			files.Append( " Parent=\"Root\"" );
			files.Append( " ParentPath=\"" );
			files.Append( RootFolder.Replace( ( '\\' + RootFolderName ), string.Empty ) );
			files.Append( "\"" );
			files.Append( " FileType=\"" );
			files.Append( "Directory" );
			files.Append( "\">" );
			files.Append( RootFolderName );
			files.Append( "</file>" );
			foreach( string FileName in FileList ) {
				if( !string.IsNullOrEmpty( Path.GetFileName( FileName ) ) ) {
					files.Append( "<file FullPath=\"" );
					files.Append( Path.GetFullPath( FileName ).Replace( "&", "&amp;" ) );
					files.Append( "\"" );
					files.Append( " Parent=\"" );
					string[] parts = FileName.Split( '\\' );
					string name = parts[parts.Length - 2];
					files.Append( name.Replace( "&", "&amp;" ) );
					files.Append( "\"" );
					files.Append( " ParentPath=\"" );
					files.Append( Path.GetDirectoryName( FileName ).Replace( "&", "&amp;" ) );
					files.Append( "\"" );
					files.Append( " FileType=\"" );
					FileAttributes attr = File.GetAttributes( FileName );
					if( ( attr & FileAttributes.Directory ) == FileAttributes.Directory ) {
						files.Append( "Directory" );
					}
					else
						files.Append( "File" );
					files.Append( "\">" );
					files.Append( Path.GetFileName( FileName ).Replace( "&", "&amp;" ) );
					files.Append( "</file>" );
				}
			}
			files.Append( "</files>" );
			files.Append( "</data>" );
			string output = files.ToString();

			return files.ToString();
		}
	}
}
