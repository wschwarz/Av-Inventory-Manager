using System;
using System.IO;
using System.Data;
using System.Web.UI;
using System.Xml;
using System.Xml.Xsl;
using System.Data.Common;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using Microsoft.Practices.EnterpriseLibrary.Data;

// Displays all Files and Attributes
namespace Law.UI.AVInventory
{
	public partial class Default: Law.UI.BasePage
	{

		private void Page_Load( object sender, System.EventArgs e ) {
            Page.Form.Action = Request.Url.ToString();
			if( !Page.IsPostBack ) {
				DataSet ds = new DataSet();
				Database database = DatabaseFactory.CreateDatabase(/* General Db Connection string */ );
				using( DbCommand command = database.GetStoredProcCommand("AvGetFileListing") ) {
					using( XmlReader reader = ( (SqlDatabase)database ).ExecuteXmlReader( command ) ) {
						XMLBind( reader );
					}
				}
			}
		}

		protected void Click_toFileEditing( object sender, EventArgs e ) {
			Response.Redirect( Server.MapPath( "~\\AvInventory\\AvDisplay" ) );
		}

		protected void XMLBind( XmlReader xml ) {
			XslCompiledTransform Xsl = new XslCompiledTransform();
			Xsl.Load( Server.MapPath( "~/AvInventory/Report.xslt" ) );
			StringWriter output = new StringWriter();
			XsltArgumentList args = new XsltArgumentList();
			Xsl.Transform( xml, args, output );
			MetaDataResults.Text = output.ToString().Replace( "Owner_", "2nd Owner" );
		}
	}
}
