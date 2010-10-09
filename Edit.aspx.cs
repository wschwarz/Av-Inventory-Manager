using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Text;
using System.IO;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System.Collections.Generic;
using System.Data.Common;

// Allows the user to view the files and file attributes in a 
// treeview, delete files, move files, and edit file attributes
// Tied to a specific folder 

/* App is designed to allow additional management and meta data to
be applied to a folder of recorded material */
namespace Law.UI.AVInventory
{
	public partial class Edit: Law.UI.BasePage
	{
		#region Events
		protected void DataSubmit_Click( object sender, EventArgs e ) {
			StringBuilder XmlString = new StringBuilder();
			foreach( RepeaterItem TempItem in DataRepeater.Items ) {
				XmlString.Append( "<field type=\"" );
				XmlString.Append( ( (Label)TempItem.FindControl( "DataTag" ) ).Text );
				XmlString.Append( "\">" );
				XmlString.Append( "<value>" );
				XmlString.Append( ( (TextBox)TempItem.FindControl( "DataContent" ) ).Text );
				XmlString.Append( "</value>" );
				XmlString.Append( "</field>" );
			}
			StringBuilder path = new StringBuilder();
			path.Append( GetRootPath() );
			path.Append( TreeID.ToolTip );
			try {
				Database database = DatabaseFactory.CreateDatabase(/* Database Connection String */);
				using( DbCommand Store = database.GetStoredProcCommand("AvUpdateMetaData") ) {
					database.AddInParameter( Store, "@Data", DbType.Xml, XmlString.ToString() );
					database.AddInParameter( Store, "@FullPath", DbType.String, path.ToString() );
					database.ExecuteNonQuery( Store );
				}
			}
			catch {
				Fail.Visible = true;
			}
			EditMenu.Visible = false;
			Success.Visible = true;
		}

		protected void ItemDelete_Click( object sender, EventArgs e ) {
			DeleteConfirm.Visible = false;
			StringBuilder path = new StringBuilder();
			path.Append( GetRootPath() );
			path.Append( TreeID.ToolTip );
			FileInfo File = new FileInfo( path.ToString() );
			string FileType = null;
			try {
				Database database = DatabaseFactory.CreateDatabase(/* Database Connection String */);
				using( DbCommand Check = database.GetStoredProcCommand("AvCheckFileType") ) {
					database.AddInParameter( Check, "@FullPath", DbType.String, path.ToString() );
					FileType = database.ExecuteScalar( Check ).ToString();
				}
				if( FileType != "Directory" ) {
					using( DbCommand Delete = database.GetStoredProcCommand( "AvDeleteEntry") ) {
						database.AddInParameter( Delete, "@Fullpath", DbType.String, path.ToString() );
						database.ExecuteNonQuery( Delete );
					}
				}
				else {
					DeleteFail.Visible = true;
					return;
				}
			}
			catch {
				DeleteFail.Visible = true;
			}
			if( FileType != "Directory" ) {
				try {
					File.Delete();
					EditMenu.Visible = false;
					DeleteSuccess.Visible = true;
				}
				catch {
					DeleteFail.Visible = true;
				}
			}
		}

		protected void ItemDeleteConfirm_Click( object sender, EventArgs e ) {
			EditMenu.Visible = false;
			DeleteConfirm.Visible = true;
		}

		protected void ItemDeleteCancel_Click( object sender, EventArgs e ) {
			DeleteConfirm.Visible = false;
			EditMenu.Visible = true;
		}

		protected void Continue_Click( object sender, EventArgs e ) {
			Response.Redirect( Request.Url.AbsolutePath );
		}

		protected void TreeView1_NodeSelected( object sender, ComponentArt.Web.UI.TreeViewNodeEventArgs e ) {
			Success.Visible = false;
			EditMenu.Visible = true;
			TreeID.Text = e.Node.Text;
			TreeID.ToolTip = BuildTreePath();
			string endPath = TreeID.ToolTip;
			string FullPath = GetRootPath() + endPath;
			DataSet ds = new DataSet();
			Database database = DatabaseFactory.CreateDatabase(/* Database Connection String */ );
			using( DbCommand command = database.GetStoredProcCommand("AvUpdateMetaData") ) {
				database.AddInParameter( command, "@FullPath", DbType.String, FullPath );
				using( XmlReader reader = ( (SqlDatabase)database ).ExecuteXmlReader( command ) ) {
					XmlDocument document = new XmlDocument();
          document.Load(reader);
				}
			}
			int temp = 0;
			if ( ds.Tables.Count != 0)
				temp = ( ds.Tables[0].Rows.Count );
			List<int> BindingList = new List<int>();
			for( int k = 0; k < temp; k++ ) {
				BindingList.Add( k );
			}
			DataRepeater.DataSource = BindingList;
			DataRepeater.DataBind();
			int i = 0;
			if( ds.Tables.Count != 0 ) {
				foreach( RepeaterItem TempItem in DataRepeater.Items ) {
					if( ds.Tables["field"].Rows[i]["type"] != null ) {
						( (Label)TempItem.FindControl( "DataTag" ) ).Text = ds.Tables[0].Rows[i]["type"].ToString();
					}
					if( ds.Tables["field"].Columns.Count > 1 )
						( (TextBox)TempItem.FindControl( "DataContent" ) ).Text = ds.Tables[0].Rows[i]["value"].ToString();
					i++;
				}
			}
		}

		protected void TreeView1_NodeMoved( object sender, ComponentArt.Web.UI.TreeViewNodeEventArgs e ) {
			ComponentArt.Web.UI.TreeViewNode targetNode = e.Node.ParentNode;
			ComponentArt.Web.UI.TreeViewNode sourceNode = e.Node;
			ComponentArt.Web.UI.TreeViewNode iterator = targetNode;
			ComponentArt.Web.UI.TreeViewNode SourcePath = TreeView1.Nodes[TreeView1.Nodes.Count - 1];
			string TargetFullPath = "";
			string SourceFullPath = SourcePath.Text;
			TreeView1.Nodes.Remove( SourcePath );
			string RootEntry = GetRootEntry();

			TargetFullPath = '\\' + iterator.Text + TargetFullPath;
			iterator = iterator.ParentNode;
			TargetFullPath = TargetFullPath + '\\' + sourceNode.Text;
			SourceFullPath = SourceFullPath + '\\' + sourceNode.Text;
			iterator = sourceNode;
			bool MoveOk = false;
			while( iterator != null ) {
				iterator = iterator.PreviousSibling;
				if( iterator == null )
					break;
				if( sourceNode.Text == iterator.Text ) {
					MoveOk = true;
				}
			}
			if( MoveOk == false ) {
				Response.Redirect( Request.Url.AbsolutePath );
			}
			string OldFullPath = GetRootPath() + PathWithoutRoot( SourceFullPath );
			string NewFullPath = GetRootPath() + TargetFullPath;
			try {
				Database database = DatabaseFactory.CreateDatabase(/* Database Connection String */ );
				using( DbCommand command = database.GetStoredProcCommand( "AvUpdateFileByOldFullPath" ) ) {
					database.AddInParameter( command, "@OldFullPath", DbType.String, OldFullPath );
					database.AddInParameter( command, "@FullPath", DbType.String, NewFullPath );
					database.ExecuteNonQuery( command );
				}
			}
			catch {
			}
			Response.Redirect( Request.Url.AbsolutePath );
		}

		#endregion

	 	#region Initialization

		private void Page_Load( object sender, System.EventArgs e ) {
			if( !Page.IsPostBack ) {
				BuildTree();
			}
		}
	 	#endregion

	 	#region Helper Function

	 private void BuildTree() {
			string RootName = GetRootEntry();
			ComponentArt.Web.UI.TreeViewNode newNode = CreateNode( RootName, "folder.gif", "", false );
			TreeView1.Nodes.Add( newNode );
			PopulateSubTree( GetRootPath(), newNode );
		}

		private void PopulateSubTree( string ParentPath, ComponentArt.Web.UI.TreeViewNode node ) {
			Database database = DatabaseFactory.CreateDatabase( /* Database Connection String */ );
			DataSet ds = new DataSet();
            bool directory = false;
			using( DbCommand command = database.GetStoredProcCommand( "AvGetFilesByParentPath" ) ) {
				database.AddInParameter( command, "@ParentPath", DbType.String, ParentPath );
				ds = database.ExecuteDataSet( command );
			}
			foreach( DataRow childRow in ds.Tables[0].Rows ) {
                directory = childRow["FileType"].Equals("Directory");
				ComponentArt.Web.UI.TreeViewNode childNode = CreateNode( childRow["Entry"].ToString(), childRow["FileType"].Equals( "Directory" ) ? "folder.gif" : "file.gif", childRow["Moved"].Equals( (bool)true ) ? "margin_x.gif" : "", false );
				childNode.DraggingEnabled = childRow["Moved"].Equals( (bool)true ) ? true : false;
                childNode.DroppingEnabled = directory;
				node.Nodes.Add( childNode );
                if (directory) {
                    PopulateSubTree(Path.Combine(childRow["ParentPath"].ToString(), childRow["Entry"].ToString()), childNode);
                }
			}
		}

		private ComponentArt.Web.UI.TreeViewNode CreateNode( string text, string imageurl, string MarginImageUrl, bool expanded ) {
			ComponentArt.Web.UI.TreeViewNode node = new ComponentArt.Web.UI.TreeViewNode();
			if( MarginImageUrl != "" ) {
				node.MarginImageUrl = MarginImageUrl;
			}
			node.Text = text;
			node.ImageUrl = imageurl;
			node.Expanded = expanded;
			return node;
		}

		private string PathWithoutRoot( string path ) {
			return path.Replace( '\\' + GetRootEntry(), string.Empty );
		}

		private string GetRootPath() {
			string temp = "";
			Database database = DatabaseFactory.CreateDatabase( /* Database Connection String */ );
			using( DbCommand GetRoot = database.GetStoredProcCommand( "AvGetRootPath" ) ) {
				temp = (string)database.ExecuteScalar( GetRoot );
			}
			return temp;
		}

		private string GetRootEntry() {
			string temp = "";
			Database database = DatabaseFactory.CreateDatabase( /* Database Connection String */ );
			using( DbCommand GetRootEntry = database.GetStoredProcCommand( "AvGetRootEntry" ) ) {
				temp = (string)database.ExecuteScalar( GetRootEntry );
			}
			return temp;
		}

		private DataSet GetDataTypes() {
			DataSet DataTypes = new DataSet();
			Database database = DatabaseFactory.CreateDatabase( /* Database Connection String */ );
			using( DbCommand command = database.GetStoredProcCommand( "AvGetDataTypes" ) ) {
				DataTypes = database.ExecuteDataSet( command );
			}
			return DataTypes;
		}

		private string BuildTreePath() {
			string temp = "";
			if( TreeView1.SelectedNode != null ) {
				ComponentArt.Web.UI.TreeViewNode curNode = TreeView1.SelectedNode;
				string RootEntry = GetRootEntry();
				while( curNode.Text != RootEntry ) {
					temp = "\\" + curNode.Text + temp;
					curNode = curNode.ParentNode;
				}
			}
			return temp;
		}

	 	#endregion

	 	#region Build
	 protected void BuildClick( object sender, EventArgs e ) {
			// To change your root folder place the url or path of the folder you wish
			// in the RootFolder variable (Note: Please truncate the DB table before changing the root folder)
			string RootFolder = /* Set Root Folder Path (local machine) */;
			DirectoryInfo directory = new DirectoryInfo( RootFolder );
			List<string> FileList = new List<string>();
			foreach( DirectoryInfo temp in directory.GetDirectories() ) {
				FileList.AddRange( CreateFileList( temp ) );
			}
			foreach( FileSystemInfo temp in directory.GetFileSystemInfos() ) {
				FileList.Add( temp.FullName );
			}
			try {
				Database database = DatabaseFactory.CreateDatabase( /* Database Connection String */ );
				using( DbCommand command = database.GetStoredProcCommand( "AvLoadInventoryData" ) ) {
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
				using( DbCommand command = database.GetStoredProcCommand( "AvUpdateAllMetaData" ) ) {
					database.AddInParameter( command, "@Data", DbType.Xml, xmldata.ToString() );
					database.ExecuteNonQuery( command );
				}
			}
			catch {
			}
			Response.Redirect( Request.Url.AbsolutePath );
		}

		protected List<string> CreateFileList( DirectoryInfo parent ) {
			List<string> FileList = new List<string>();
			foreach( DirectoryInfo temp in parent.GetDirectories() ) {
				FileList.AddRange( CreateFileList( temp ) );
			}
			foreach( FileSystemInfo temp in parent.GetFileSystemInfos() ) {
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
	 	#endregion
	}
}
