<%@ Page Language="C#" MasterPageFile="~/Templates/Application.master" AutoEventWireup="True" Inherits="Law.UI.AVInventory.Default" Title="AV File Listing" Codebehind="Default.aspx.cs" %>

<%@ Register TagPrefix="ajx" Namespace="AjaxControlToolkit" Assembly="AjaxControlToolkit" %>
<%@ Register Assembly="ComponentArt.Web.UI" Namespace="ComponentArt.Web.UI" TagPrefix="ComponentArt" %>
<asp:Content ID="Content3" ContentPlaceHolderID="Content" runat="Server">
	<h1>AV Inventory</h1>
	<asp:Literal ID="MetaDataResults" runat="server"></asp:Literal>
</asp:Content>
