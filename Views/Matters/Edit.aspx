﻿<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<OpenLawOffice.WebClient.ViewModels.Matters.MatterViewModel>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Edit Matter
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MenuContent" runat="server">
    <li>Actions</li>
    <ul style="list-style: none outside none; padding-left: 1em;">
        <li>
            <%: Html.ActionLink("New Matter", "Create") %></li>
        <li>
            <%: Html.ActionLink("Details", "Details", new { id = Model.Id })%></li>
        <li>
            <%: Html.ActionLink("Delete ", "Delete", new { id = Model.Id })%></li>
        <li>
            <%: Html.ActionLink("List", "Index") %></li>
    </ul>
    <li>
        <%: Html.ActionLink("Tags", "Tags", "Matters") %></li>
    <li>
        <%: Html.ActionLink("Responsible Users", "Users", "Matters")%></li>
    <li>
        <%: Html.ActionLink("Contacts", "Contacts", "Matters")%></li>
    <li>
        <%: Html.ActionLink("Tasks", "Tasks", "Matters")%>
        (<%: Html.ActionLink("Add", "AddTask", new { controller = "Matters", id = Model.Id }) %>)</li>
    <li>
        <%: Html.ActionLink("Notes", "Notes", "Matters")%>
        (<%: Html.ActionLink("Add", "AddNote", new { controller = "Matters", id = Model.Id }) %>)</li>
    <li>
        <%: Html.ActionLink("Documents", "Documents", "Matters")%>
        (<%: Html.ActionLink("Add", "AddDocument", new { controller = "Matters", id = Model.Id }) %>)</li>
    <li>
        <%: Html.ActionLink("Time", "Time", "Matters")%>
        (<%: Html.ActionLink("Add", "AddTime", new { controller = "Matters", id = Model.Id }) %>)</li>
    <li>
        <%: Html.ActionLink("Permissions", "Acls", "Matters")%></li>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <script type="text/javascript" src="../../Scripts/jqGrid-4.6.0/grid.locale-en.js"></script>
    <script type="text/javascript" src="../../Scripts/jqGrid-4.6.0/jquery.jqGrid.min.js"></script>
    <style type="text/css">
        div.ui-jqgrid-titlebar
        {
            height: 16px;
        }
    </style>
    <h2>
        Edit Matter<a id="pageInfo" class="btn-question" style="padding-left: 15px;">Help</a></h2>
    <% using (Html.BeginForm())
       {%>
    <%: Html.ValidationSummary(true) %>
    <table class="detail_table">
        <tr>
            <td class="display-label">
                Id
            </td>
            <td class="display-field">
                <%: Model.Id %>
            </td>
        </tr>
        <tr>
            <td class="display-label">
                Title<span class="required-field" title="Required Field">*</span>
            </td>
            <td class="display-field">
                <%: Html.TextBoxFor(model => model.Title) %>
                <%: Html.ValidationMessageFor(model => model.Title) %>
            </td>
        </tr>
        <tr>
            <td class="display-label">
                Synopsis<span class="required-field" title="Required Field">*</span>
            </td>
            <td class="display-field">
                <%: Html.TextBoxFor(model => model.Synopsis) %>
                <%: Html.ValidationMessageFor(model => model.Synopsis) %>
            </td>
        </tr>
        <tr>
            <td class="display-label">
                Parent
            </td>
            <td class="display-field">
                Parent:
                <%: Html.TextBoxFor(model => model.Parent.Id, new { @readonly = true })%>
                <br />
                <br />
                <table id="list">
                </table>
                <div id="pager">
                </div>
                <input id="clear" type="button" style="width: 200px;" value="clear" />
                <script language="javascript">
                    $(function () {
                        $("#list").jqGrid({
                            treeGrid: true,
                            width: 350,
                            url: '../../Matters/ListChildrenJqGrid',
                            datatype: 'json',
                            jsonReader: {
                                root: 'Rows',
                                page: 'CurrentPage',
                                total: 'TotalRecords',
                                id: 'Id',
                                rows: 'Rows'
                            },
                            colNames: ['id', 'Title', 'Synopsis'],
                            colModel: [
                                    { name: 'Id', width: 1, hidden: true, key: true },
                                    { name: 'Title', width: 250 },
                                    { name: 'Synopsis', width: 250 }
                                ],
                            pager: '#pager',
                            gridview: true,
                            treedatatype: 'json',
                            treeGridModel: 'adjacency',
                            ExpandColumn: 'Title',
                            caption: 'Matters',
                            onSelectRow: function (id) {
                                $("#Parent_Id").val(id);
                            }
                        });
                    });

                    $("#clear").click(function () {
                        $("#list").jqGrid('resetSelection');
                        $("#Parent_Id").val(null);
                    });
                </script>
            </td>
        </tr>
    </table>
    <p>
        <input type="submit" value="Save" />
    </p>
    <% } %>
    <div id="pageInfoDialog" title="Help">
        <p>
        <span style="font-weight: bold; text-decoration: underline;">Info:</span>
        Fill in the information on this page to modify the matter.  Required fields are indicated with an
        <span style="color: #ee0000;font-size: 12px;cursor:help;" title="Required Field">*</span><br /><br />
        <span style="font-weight: bold; text-decoration: underline;">Usage:</span>
        Select a "parent" matter to make this matter be a "submatter" of another matter.  To deselect a parent matter, click "clear". 
        Fields marked with an <span style="color: #ee0000;font-size: 12px;cursor:help;" title="Required Field">*</span> are required.
        </p>
    </div>
</asp:Content>