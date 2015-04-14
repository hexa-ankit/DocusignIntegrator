<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Index.aspx.cs" Inherits="WebApplication1.Index" EnableEventValidation="false" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <style type="text/css">
        .auto-style1 {
            width: 100%;
            height: 498px;
        }
        .auto-style2 {
            width: 36px;
        }
        .auto-style3 {
            width: 169px;
        }
        .auto-style4 {
            width: 41px;
        }
        .auto-style5 {
            width: 820px;
            vertical-align:top;
            text-align:center;
        }
        .auto-style6 {
            width: 36px;
            height: 62px;
        }
        .auto-style7 {
            width: 41px;
            height: 62px;
        }
        .auto-style8 {
            height: 62px;
            text-align:center;
        }
        .auto-style10 {
            height: 62px;
        }
        .auto-style11 {
            margin:15px;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    
        <table class="auto-style1">
            <tr>
                <td class="auto-style6"></td>
                <td class="auto-style7"></td>
                <td class="auto-style8" colspan="2">
                    <asp:Label ID="Label1" runat="server" Font-Size="XX-Large" Text="DOCUSIGN DEMO"></asp:Label>
                </td>
                <td class="auto-style10"></td>
            </tr>
            <tr>
                <td class="auto-style2">&nbsp;</td>
                <td class="auto-style4">&nbsp;</td>
                <td class="auto-style5">
                    <asp:SqlDataSource ID="SqlDataSource1" runat="server"></asp:SqlDataSource>
                    <asp:GridView ID="GridDocument" runat="server" AutoGenerateColumns="False" CellPadding="4" ForeColor="#333333" GridLines="None" Width="789px" OnRowDataBound="GridDocument_RowDataBound">
                        <AlternatingRowStyle BackColor="White" />
                        <Columns>
                            <asp:BoundField DataField="EnvelopeID" HeaderText="EnvelopeID" ItemStyle-Width="50%" >
<ItemStyle Width="50%"></ItemStyle>
                            </asp:BoundField>
                            <asp:BoundField DataField="StatusChangedTime" HeaderText="StatusChangedDate" />
                            <asp:BoundField DataField="Status" HeaderText="Status" />
                            <asp:BoundField DataField="SignedDocument" HeaderText="SignedDocument" Visible="False" />
                            <asp:TemplateField>
                                <ItemTemplate>                                    
                                   <asp:Button runat="server" Text="Download" ID="btnDownload" CausesValidation="false" OnClick="btnDownload_Click" CommandArgument="MyVal" CommandName='<%# Eval("EnvelopeID") %>'  />                
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                        <EditRowStyle BackColor="#2461BF" />
                        <FooterStyle BackColor="#507CD1" Font-Bold="True" ForeColor="White" />
                        <HeaderStyle BackColor="#507CD1" Font-Bold="True" ForeColor="White" />
                        <PagerStyle BackColor="#2461BF" ForeColor="White" HorizontalAlign="Center" />
                        <RowStyle BackColor="#EFF3FB" />
                        <SelectedRowStyle BackColor="#D1DDF1" Font-Bold="True" ForeColor="#333333" />
                        <SortedAscendingCellStyle BackColor="#F5F7FB" />
                        <SortedAscendingHeaderStyle BackColor="#6D95E1" />
                        <SortedDescendingCellStyle BackColor="#E9EBEF" />
                        <SortedDescendingHeaderStyle BackColor="#4870BE" />
                    </asp:GridView>
                </td>
                <td class="auto-style3">
                    <asp:Panel ID="Panel1" runat="server" Height="426px">
                        &nbsp;&nbsp;&nbsp; Name:<asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="Name" ErrorMessage="*" ForeColor="Red"></asp:RequiredFieldValidator>
                        <br />
                        <asp:TextBox ID="Name" runat="server" CssClass="auto-style11" Width="136px"></asp:TextBox>
                        <br />
                        &nbsp;&nbsp;&nbsp; Email:<asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="Email" ErrorMessage="*" ForeColor="Red"></asp:RequiredFieldValidator>
                        <br />
                        <asp:TextBox ID="Email" runat="server" CssClass="auto-style11" Width="138px"></asp:TextBox>
                        <asp:Button ID="New" runat="server" Text="Send Document" Width="146px" CssClass="auto-style11" OnClick="New_Click" />
                        <asp:Button ID="Update" runat="server"  Text="Update All Status"  CssClass="auto-style11" Width="146px" CausesValidation="False" OnClick="Update_Click" />
                        <asp:Label ID="Status" runat="server" ForeColor="#009933"></asp:Label>
                    </asp:Panel>
                </td>
                <td>&nbsp;</td>
            </tr>
            
        </table>
    
    </div>
    </form>
</body>
</html>
