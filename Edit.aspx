<%@ Page Language="C#" MasterPageFile="~/Templates/Application.master" AutoEventWireup="True" Inherits="Law.UI.AVInventory.Edit" Title="AV File Editor" Codebehind="Edit.aspx.cs" %>

<%@ Register TagPrefix="ajx" Namespace="AjaxControlToolkit" Assembly="AjaxControlToolkit" %>
<%@ Register Assembly="ComponentArt.Web.UI" Namespace="ComponentArt.Web.UI" TagPrefix="ComponentArt" %>
<asp:Content runat="server" ContentPlaceHolderID="HeaderContent">

	<script type="text/javascript">
	function OnNode_BeforeMove(sender, EventArgs) {
		var Node = EventArgs.get_node();	
		var SourceNode = EventArgs.get_node().get_parentNode();
		var path = "";
		while (SourceNode != null) {
			path =  '\\' + SourceNode.get_text() + path;
			SourceNode = SourceNode.get_parentNode();
		}
		document.getElementById(HdnLblid).Value = path;
		var newNode = new ComponentArt.Web.UI.TreeViewNode(); 
		newNode.set_text(path);
		var Treeview = Node.get_parentTreeView();
		Treeview.get_nodes().add(newNode);
		return;
	}
	</script>

</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="Content" runat="Server">
	<h1>
		AV Inventory Editor</h1>
	<div class="left">
		<label>
			Item View Panel</label>
		<ComponentArt:TreeView ID="TreeView1" Height="450" Width="450" DragAndDropEnabled="true" 
			NodeEditingEnabled="false" KeyboardEnabled="true" CssClass="TreeViewMargin"
			NodeCssClass="TreeNode" SelectedNodeCssClass="SelectedTreeNode" HoverNodeCssClass="HoverTreeNode"
			NodeEditCssClass="NodeEdit" LineImageWidth="19" LineImageHeight="20" DefaultImageWidth="16"
			DefaultImageHeight="16" ItemSpacing="0" ImagesBaseUrl="/App_Themes/Generic/ComponentArtTreeView/images/"
			NodeLabelPadding="3" ParentNodeImageUrl="folder.gif" ExpandedParentNodeImageUrl="folder_open.gif"
			LeafNodeImageUrl="file.gif" ShowLines="true" LineImagesFolderUrl="/App_Themes/Generic/ComponentArtTreeView/images/lines/"
			EnableViewState="true" runat="server" AutoPostBackOnSelect="true" OnNodeSelected="TreeView1_NodeSelected"
			AutoAssignNodeIDs="true" DisplayMargin="true" DefaultMarginImageWidth="24" DefaultMarginImageHeight="20"
			AutoPostBackOnNodeMove="true" OnNodeMoved="TreeView1_NodeMoved">
			<ClientEvents>
				<NodeBeforeMove EventHandler="OnNode_BeforeMove" />
			</ClientEvents>
		</ComponentArt:TreeView>
		<input type="hidden" id="HdnLbl" runat="server" />
	</div>
	<div class="right">
		<label>
			Selected Item :
			<asp:Label Style="font-style: normal;" ID="TreeID" runat="server"></asp:Label>
		</label>
		<asp:Panel ID="EditMenu" runat='server' Visible="false">
			<asp:Repeater ID="DataRepeater" runat="server">
				<ItemTemplate>
					<table>
						<tr>
							<td style="width: 150px;">
								<asp:Label runat="server" ID="DataTag"></asp:Label>
							</td>
							<td>
								<asp:TextBox ID="DataContent" runat="server"></asp:TextBox>
							</td>
						</tr>
					</table>
				</ItemTemplate>
			</asp:Repeater>
			<asp:Button ID="DataSubmit" CssClass="button" runat="server" Text="Submit" OnClick="DataSubmit_Click" />
			<asp:Button ID="ItemDelete" CssClass="button" runat="server" Text="Delete Item" OnClick="ItemDeleteConfirm_Click" />
		</asp:Panel>
		<asp:Panel ID="DeleteConfirm" runat="server" Visible="false">
			<br />
			<label style="font-size: 18px; text-align: center;">
				Are you sure you want to delete this item?</label>
			<br />
			<label style="font-size: 12px; text-align: center; font-weight: normal;">
				This will delete the data associated with this item AND delete the file permanently.</label>
			<br />
			<div style="margin-left: 41%;">
				<asp:Button ID="Confirm" runat="server" Text="Yes" CssClass="button" OnClick="ItemDelete_Click" />
				<asp:Button ID="Cancel" runat="server" Text="No" CssClass="button" OnClick="ItemDeleteCancel_Click" />
			</div>
		</asp:Panel>
		<asp:Panel ID="Fail" runat="server" Visible="false">
			<br>
			<label style="font-size: 18px;">
				Your changes could not be completed.</label>
			<br />
			<asp:Button runat="server" Text="Continue" CssClass="button" OnClick="Continue_Click" />
		</asp:Panel>
		<asp:Panel ID="DeleteFail" runat="server" Visible="false">
			<br />
			<label style="font-size: 18px;">
				Your delete request could not be completed.</label>
			<br />
			<asp:Button ID="Button1" runat="server" Text="Continue" CssClass="button" OnClick="Continue_Click" />
		</asp:Panel>
		<asp:Panel ID="DeleteSuccess" runat="server" Visible="false">
			<br />
			<label style="font-size: 18px;">
				Your delete request has been successfully completed.</label>
			<br />
			<asp:Button ID="Button2" runat="server" Text="Continue" CssClass="button" OnClick="Continue_Click" />
		</asp:Panel>
		<asp:Panel ID="Success" runat="server" Visible="false">
			<br />
			<label style="font-size: 18px;">
				Your changes have been successfully submitted.</label>
			<br />
		</asp:Panel>
		<label style="font-weight: normal; border-width: thin; border-spacing: 10%; border-style: solid;">
			<label style="font-weight: bold; font-size: 16pt; text-align: center;">
				Instructions:
			</label>
			To edit an item's data from the view panel, please select an item by clicking on
			it.
			<br />
			<br />
			The x's signify that the files marked are no longer located where the view panel
			shows them.
			<br />
			<br />
			To update the view panel to display the most current data please click rebuild in
			the upper right hand corner.
			<br />
			<br />
			The view panel will not allow moving files unless an X appears to the right of the
			file name. If the X appears please drag the filename to the folder that the file
			was moved to.
			<br />
			<br />
			Deleting an item will delete the data stored for that item AND delete the file from
			the folder it resides in.
		</label>

		<script type="text/javascript">
			var HdnLblid = '<%=HdnLbl.ClientID%>';
		</script>

	</div>
	<p />
</asp:Content>
