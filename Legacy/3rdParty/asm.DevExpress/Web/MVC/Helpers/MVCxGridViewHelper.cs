using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web.UI.WebControls;
using asm.Data.Helpers;
using asm.DevExpress.Properties;
using asm.Extensions;
using asm.Web.Routing;
using DevExpress.Utils;
using DevExpress.Web;
using DevExpress.Web.Mvc;
using JetBrains.Annotations;
using Image = System.Drawing.Image;
using PagerSettings = asm.Web.Mvc.PagerSettings;

namespace asm.DevExpress.Web.MVC.Helpers
{
	public class MVCxGridViewHelper
	{
		public static GridViewSettings Settings<T>([NotNull] RouteValues routeValues, string name = null, string keyFieldNames = null, GridViewVisibleElements elements = GridViewVisibleElements.None,
			PagerSettings pagerSettings = null, Action<GridViewSettings> onSettingsCreated = null, Action<GridViewClientSideEvents> onClientSideEvents = null, Predicate<PropertyInfo> columnFilter = null)
		{
			return Settings(typeof(T), routeValues, name, keyFieldNames, elements, pagerSettings, onSettingsCreated, onClientSideEvents, columnFilter);
		}

		public static GridViewSettings Settings<TColumn>(object obj, [NotNull] RouteValues routeValues, string name = null, string keyFieldNames = null, GridViewVisibleElements elements = GridViewVisibleElements.None, 
			PagerSettings pagerSettings = null, Action<GridViewSettings> onSettingsCreated = null, Action<GridViewClientSideEvents> onClientSideEvents = null, Predicate<TColumn> columnFilter = null)
		{

			name = name?.Trim();

			if (string.IsNullOrEmpty(name))
			{
				string templateTypeName;

				switch (obj)
				{
					case null:
						templateTypeName = string.Empty;
						break;
					case DataTable dt:
						templateTypeName = dt.TableName;
						break;
					default:
						templateTypeName = obj.AsType().Name;
						break;
				}

				name = $"{templateTypeName}GridView";
			}

			bool rtl = Thread.CurrentThread.CurrentUICulture.IsRightToLeft();

			GridViewSettings settings = new GridViewSettings
			{
				Name = name,
				EnableCallbackCompression = true,
				KeyboardSupport = true,
				RightToLeft = rtl ? DefaultBoolean.True : DefaultBoolean.False,
				Width = Unit.Percentage(100),
				Height = Unit.Percentage(100),
				ControlStyle = { CssClass = "table table-bordered table-hover dataTable no-footer dtr-inline" },
				CallbackRouteValues = routeValues.List,
				CustomActionRouteValues = routeValues.CustomAction,
				CustomDataActionRouteValues = routeValues.CustomDataAction,
				CommandColumn =
				{
					ShowInCustomizationForm = false,
					ShowNewButtonInHeader = true,
					SelectAllCheckboxMode = GridViewSelectAllCheckBoxMode.Page,
					ShowSelectCheckbox = elements.HasFlag(GridViewVisibleElements.Select),
					ShowNewButton = elements.HasFlag(GridViewVisibleElements.New) && routeValues.Create.PropertiesCount(Constants.BF_PUBLIC_INSTANCE | BindingFlags.DeclaredOnly) > 0,
					ShowEditButton = elements.HasFlag(GridViewVisibleElements.Edit) && routeValues.Update.PropertiesCount(Constants.BF_PUBLIC_INSTANCE | BindingFlags.DeclaredOnly) > 0,
					ShowDeleteButton = elements.HasFlag(GridViewVisibleElements.Delete) && routeValues.Delete.PropertiesCount(Constants.BF_PUBLIC_INSTANCE | BindingFlags.DeclaredOnly) > 0,
					ShowApplyFilterButton = elements.HasFlag(GridViewVisibleElements.Filter),
					ShowClearFilterButton = elements.HasFlag(GridViewVisibleElements.ClearFilter),
					HeaderStyle = { VerticalAlign = VerticalAlign.Middle }
				},
				Settings =
				{
					AutoFilterCondition = AutoFilterCondition.Contains,
					ColumnMinWidth = 0,
					ShowHeaderFilterButton = elements.HasFlag(GridViewVisibleElements.Filter),
					ShowFilterBar = GridViewStatusBarMode.Auto
				},
				SettingsCommandButton =
				{
					EncodeHtml = false,
					SelectButton =
					{
						Styles =
						{
							Native = true,
							Style = { CssClass = "btn btn-default" }
						},
						Text = @"<span class=""fa fa-check""></span>"
					},
					NewButton =
					{
						Styles =
						{
							Native = true,
							Style = { CssClass = "btn btn-default" }
						},
						Text = @"<span class=""fa fa-plus""></span>"
					},
					EditButton =
					{
						Styles =
						{
							Native = true,
							Style = { CssClass = "btn btn-default btn-xs" }
						},
						Text = @"<span class=""fa fa-pencil""></span>"
					},
					UpdateButton =
					{
						Styles =
						{
							Native = true,
							Style = { CssClass = "btn btn-primary btn-xs" }
						},
						Text = @"<span class=""glyphicon glyphicon-ok""></span>"
					},
					CancelButton =
					{
						Styles =
						{
							Native = true,
							Style = { CssClass = "btn btn-default btn-xs" }
						},
						Text = @"<span class=""fa fa-times""></span>"
					},
					DeleteButton =
					{
						Styles =
						{
							Native = true,
							Style = { CssClass = "btn btn-warning btn-xs" }
						},
						Text = @"<span class=""fa fa-times""></span>"
					},
					RecoverButton =
					{
						Styles =
						{
							Native = true,
							Style = { CssClass = "btn btn-default btn-xs" }
						},
						Text = @"<span class=""fa fa-undo""></span>"
					},
					ApplyFilterButton =
					{
						Styles =
						{
							Native = true,
							Style = { CssClass = "btn btn-default btn-xs" }
						},
						Text = @"<span class=""fa fa-filter""></span>"
					},
					ClearFilterButton =
					{
						Styles =
						{
							Native = true,
							Style = { CssClass = "btn btn-default btn-xs" }
						},
						Text = @"<span class=""fa fa-times""></span>"
					},
					ShowAdaptiveDetailButton =
					{
						Styles =
						{
							Native = true,
							Style = { CssClass = "btn btn-default btn-xs" }
						},
						Text = @"<span class=""fa fa-eye""></span>"
					},
					HideAdaptiveDetailButton =
					{
						Styles =
						{
							Native = true,
							Style = { CssClass = "btn btn-default btn-xs" }
						},
						Text = @"<span class=""fa fa-eye-slash""></span>"
					},
					SearchPanelApplyButton =
					{
						Styles =
						{
							Native = true,
							Style = { CssClass = "btn btn-default btn-xs" }
						},
						Text = @"<span class=""fa fa-search""></span>"
					},
					SearchPanelClearButton =
					{
						Styles =
						{
							Native = true,
							Style = { CssClass = "btn btn-default btn-xs" }
						},
						Text = @"<span class=""fa fa-times""></span>"
					},
					CustomizationDialogApplyButton =
					{
						Styles =
						{
							Native = true,
							Style = { CssClass = "btn btn-primary" }
						},
						Text = @"<span class=""fa fa-check""></span>"
					},
					CustomizationDialogCloseButton =
					{
						Styles =
						{
							Native = true,
							Style = { CssClass = "btn btn-default" }
						},
						Text = @"<span class=""fa fa-times""></span>"
					}
				},
				SettingsEditing =
				{
					Mode = GridViewEditingMode.PopupEditForm,
					ShowModelErrorsForEditors = true,
					NewItemRowPosition = GridViewNewItemRowPosition.Bottom,
					AddNewRowRouteValues = routeValues.Create,
					UpdateRowRouteValues = routeValues.Update,
					DeleteRowRouteValues = routeValues.Delete
				},
				DeleteSelectedRowsRouteValues = routeValues.DeleteSelected,
				SettingsAdaptivity =
				{
					AdaptivityMode = GridViewAdaptivityMode.HideDataCells
				},
				SettingsBehavior =
				{
					AllowSelectByRowClick = true
				},
				SettingsCustomizationDialog =
				{
					Enabled = true,
					ShowColumnChooserPage = true,
					ShowFilteringPage = elements.HasFlag(GridViewVisibleElements.Filter)
				},
				SettingsDetail =
				{
					AllowOnlyOneMasterRowExpanded = true
				},
				SettingsExport =
				{
					PaperKind = PaperKind.A4
				},
				SettingsLoadingPanel = { Text = Resources.LoadingText },
				SettingsPopup =
				{
					CustomizationWindow =
					{
						AllowResize = true,
						ResizingMode = ResizingMode.Postponed,
						CloseOnEscape = AutoBoolean.True,
						Width = Unit.Pixel(300),
						Height = Unit.Pixel(400),
						MinWidth = Unit.Pixel(200),
						MinHeight = Unit.Pixel(200),
						HorizontalAlign = PopupHorizontalAlign.OutsideRight,
						VerticalAlign = PopupVerticalAlign.TopSides,
						ShowCloseButton = true
					},
					EditForm =
					{
						AllowResize = true,
						ResizingMode = ResizingMode.Postponed,
						CloseOnEscape = AutoBoolean.False,
						Width = Unit.Pixel(300),
						Height = Unit.Pixel(400),
						MinWidth = Unit.Pixel(200),
						MinHeight = Unit.Pixel(200),
						HorizontalAlign = PopupHorizontalAlign.WindowCenter,
						VerticalAlign = PopupVerticalAlign.WindowCenter,
						ShowCloseButton = true,
						ShowHeader = true,
						Modal = true,
						PopupAnimationType = AnimationType.Fade
					},
					FilterControl =
					{
						AllowResize = true,
						ResizingMode = ResizingMode.Postponed
					},
					HeaderFilter =
					{
						CloseOnEscape = AutoBoolean.True,
						ResizingMode = ResizingMode.Postponed
					}
				},
				SettingsResizing =
				{
					ColumnResizeMode = ColumnResizeMode.NextColumn,
					Visualization = ResizingMode.Postponed
				},
				SettingsSearchPanel =
				{
					Visible = elements.HasFlag(GridViewVisibleElements.Search),
					//ColumnNames = string.Empty <-- adjust this when building the columns,
					HighlightResults = true,
					EditorNullTextDisplayMode = NullTextDisplayMode.Unfocused,
					GroupOperator = GridViewSearchPanelGroupOperator.Or,
					Delay = 1000,
					AllowTextInputTimer = !elements.HasFlag(GridViewVisibleElements.Search),
					ShowApplyButton = elements.HasFlag(GridViewVisibleElements.Search),
					ShowClearButton = elements.HasFlag(GridViewVisibleElements.ClearSearch)
				},
				SettingsText =
				{
					CommandApplyFilter = Resources.Apply,
					CommandApplySearchPanelFilter = Resources.Apply,
					CommandBatchEditCancel = Resources.Cancel,
					CommandBatchEditUpdate = Resources.Save,
					CommandCancel = Resources.Cancel,
					CommandClearFilter = Resources.Clear,
					CommandClearSearchPanelFilter = Resources.Clear,
					CommandDelete = Resources.Delete,
					CommandEdit = Resources.Edit,
					CommandHideAdaptiveDetail = Resources.Hide,
					CommandNew = Resources.New,
					CommandRecover = Resources.Recover,
					CommandSelect = Resources.Select,
					CommandShowAdaptiveDetail = Resources.Show,
					CommandUpdate = Resources.Save,
					ConfirmDelete = Resources.ConfirmDelete,
					ConfirmOnLosingBatchChanges = Resources.ConfirmOnLosingBatchChanges,
					EmptyDataRow = Resources.EmptyDataRow,
					EmptyHeaders = string.Empty,
					FilterBarClear = Resources.Clear,
					FilterBarCreateFilter = Resources.Filter,
					HeaderFilterCancelButton = Resources.Cancel,
					HeaderFilterLastYear = Resources.LastYear,
					HeaderFilterThisYear = Resources.ThisYear,
					HeaderFilterNextYear = Resources.NextYear,
					HeaderFilterLastMonth = Resources.LastMonth,
					HeaderFilterThisMonth = Resources.ThisMonth,
					HeaderFilterNextMonth = Resources.NextMonth,
					HeaderFilterLastWeek = Resources.LastWeek,
					HeaderFilterThisWeek = Resources.ThisWeek,
					HeaderFilterNextWeek = Resources.LastWeek,
					HeaderFilterYesterday = Resources.Yesterday,
					HeaderFilterToday = Resources.Today,
					HeaderFilterTomorrow = Resources.Tomorrow,
					HeaderFilterOkButton = Resources.OK,
					HeaderFilterSelectAll = Resources.SelectAll,
					HeaderFilterShowAll = Resources.ShowAll,
					HeaderFilterShowBlanks = Resources.Blanks,
					HeaderFilterShowNonBlanks = Resources.NonBlanks,
					HeaderFilterTo = Resources.To,
					SearchPanelEditorNullText = Resources.Search,
					SelectAllCheckBoxInAllPagesMode = Resources.SelectAll,
					SelectAllCheckBoxInPageMode = Resources.SelectAllInPage,
					ToolbarCancel = Resources.Cancel,
					ToolbarClearFilter = Resources.Clear,
					ToolbarClearGrouping = Resources.Clear,
					ToolbarClearSorting = Resources.Clear,
					ToolbarCollapseDetailRow = Resources.Collapse,
					ToolbarCollapseRow = Resources.Collapse,
					ToolbarDelete = Resources.Delete,
					ToolbarEdit = Resources.Edit,
					ToolbarExpandDetailRow = Resources.Expand,
					ToolbarExpandRow = Resources.Expand,
					ToolbarFullCollapse = Resources.FullCollapse,
					ToolbarFullExpand = Resources.FullExpand,
					ToolbarNew = Resources.New,
					ToolbarRefresh = Resources.Refresh,
					ToolbarShowCustomizationDialog = Resources.Customize,
					ToolbarShowCustomizationWindow = Resources.Customize,
					ToolbarShowFilterEditor = Resources.Edit,
					ToolbarShowFilterRow = Resources.Filter,
					ToolbarShowFooter = Resources.Footer,
					ToolbarShowGroupPanel = Resources.GroupPanel,
					ToolbarShowSearchPanel = Resources.Search,
					ToolbarUpdate = Resources.Save
				}
			};

			if (pagerSettings != null)
			{
				settings.SettingsPager.EnableAdaptivity = true;
				settings.SettingsPager.FirstPageButton.Visible = pagerSettings.UseFirstAndLast;
				settings.SettingsPager.LastPageButton.Visible = pagerSettings.UseFirstAndLast;
				settings.SettingsPager.PrevPageButton.Visible = pagerSettings.UsePreviousAndNext;
				settings.SettingsPager.NextPageButton.Visible = pagerSettings.UsePreviousAndNext;
				settings.SettingsPager.NumericButtonCount = pagerSettings.AdjacentPageCount;
				settings.SettingsPager.PageSize = pagerSettings.ItemsPerPage;
				settings.SettingsPager.Position = PagerPosition.Bottom;
				settings.SettingsPager.Summary.Position = rtl ? PagerButtonPosition.Right : PagerButtonPosition.Left;
			}
			else
			{
				settings.SettingsPager.Mode = GridViewPagerMode.ShowAllRecords;
				settings.SettingsPager.Visible = false;
			}

			settings.CommandColumn.ShowUpdateButton = settings.CommandColumn.ShowCancelButton = settings.CommandColumn.ShowEditButton;
			settings.CommandColumn.Visible = settings.CommandColumn.ShowSelectCheckbox ||
											settings.CommandColumn.ShowNewButton ||
											settings.CommandColumn.ShowEditButton ||
											settings.CommandColumn.ShowDeleteButton ||
											settings.CommandColumn.ShowApplyFilterButton ||
											settings.CommandColumn.ShowClearFilterButton;
			BuildColumns(settings, obj, keyFieldNames, columnFilter);
			onSettingsCreated?.Invoke(settings);
			onClientSideEvents?.Invoke(settings.ClientSideEvents);
			return settings;
		}

		[NotNull]
		public static GridViewSettings BuildColumns<T>([NotNull] GridViewSettings settings, string keyFieldNames = null, Predicate<PropertyInfo> columnFilter = null)
		{
			return BuildColumns(settings, typeof(T), keyFieldNames, columnFilter);
		}

		[NotNull]
		public static GridViewSettings BuildColumns<TColumn>([NotNull] GridViewSettings settings, object obj, string keyFieldNames = null, Predicate<TColumn> columnFilter = null)
		{
			switch (obj)
			{
				case null:
					settings.KeyFieldName = keyFieldNames?.Trim();
					break;
				case DataTable dt:
					foreach (DataColumn column in ColumnsHelper.GetColumns(obj, columnFilter).Cast<DataColumn>())
						AddColumnFromDataColumn(settings, column);

					settings.KeyFieldName = dt.PrimaryKey.Length > 0 
						? string.Join(",", dt.PrimaryKey.Select(e => e.ColumnName)) 
						: keyFieldNames?.Trim();
					break;
				default:
					foreach (PropertyInfo property in ColumnsHelper.GetColumns(obj, columnFilter).Cast<PropertyInfo>())
						AddColumnFromProperty(settings, property);

					settings.KeyFieldName = keyFieldNames?.Trim();
					break;
			}

			settings.CommandColumn.Index = settings.Columns.Count - 1;
			return settings;
		}

		[NotNull]
		public static GridViewSettings AddColumnFromDataColumn([NotNull] GridViewSettings settings, [NotNull] DataColumn column)
		{
			MVCxGridViewColumnType columnType;
			Type type = column.DataType.ResolveType();

			if (column.AutoIncrement || column.ReadOnly)
				columnType = MVCxGridViewColumnType.Default;
			else if (type.IsArray || type.IsEnum)
				columnType = MVCxGridViewColumnType.DropDownEdit;
			else if (type.IsEnumerable())
				columnType = MVCxGridViewColumnType.ComboBox;
			else if (type.Is<string>())
				columnType = column.MaxLength > 256 ? MVCxGridViewColumnType.Memo : MVCxGridViewColumnType.TextBox;
			else if (type.Is<bool>())
				columnType = MVCxGridViewColumnType.CheckBox;
			else if (type.IsNumeric())
				columnType = MVCxGridViewColumnType.SpinEdit;
			else if (type.Is<DateTime>())
				columnType = MVCxGridViewColumnType.DateEdit;
			else if (type.Is<TimeSpan>())
				columnType = MVCxGridViewColumnType.TimeEdit;
			else if (type.Is<Color>())
				columnType = MVCxGridViewColumnType.ColorEdit;
			else if (type.Is<IEnumerable<byte>>() || type.Is<Image>())
				columnType = MVCxGridViewColumnType.BinaryImage;
			else if (type.Is<System.Web.UI.WebControls.Image>())
				columnType = MVCxGridViewColumnType.Image;
			else if (type.Is<Uri>() || type.Is<UriBuilder>())
				columnType = MVCxGridViewColumnType.HyperLink;
			else
				columnType = MVCxGridViewColumnType.Default;

			string caption = string.IsNullOrEmpty(column.Caption) ? column.ColumnName : column.Caption;
			MVCxGridViewColumn gridViewColumn = new MVCxGridViewColumn(column.ColumnName, caption, columnType);
			settings.Columns.Add(gridViewColumn);
			return settings;
		}

		[NotNull]
		public static GridViewSettings AddColumnFromProperty([NotNull] GridViewSettings settings, [NotNull] PropertyInfo property)
		{
			MVCxGridViewColumnType columnType;

			// todo: how am i suppose to handle this? TypeName in ColumnAttribute columnAttribute = property.GetAttribute<ColumnAttribute>();
			DatabaseGeneratedAttribute databaseGeneratedAttribute = property.GetAttribute<DatabaseGeneratedAttribute>();
			Type type = property.PropertyType.ResolveType();

			if (databaseGeneratedAttribute.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity || databaseGeneratedAttribute.DatabaseGeneratedOption == DatabaseGeneratedOption.Computed)
				columnType = MVCxGridViewColumnType.Default;
			else if (type.IsArray || type.IsEnum)
				columnType = MVCxGridViewColumnType.DropDownEdit;
			else if (type.IsEnumerable())
				columnType = MVCxGridViewColumnType.ComboBox;
			else if (type.Is<string>())
				columnType = property.StringPropertyLength() > 256 ? MVCxGridViewColumnType.Memo : MVCxGridViewColumnType.TextBox;
			else if (type.Is<bool>())
				columnType = MVCxGridViewColumnType.CheckBox;
			else if (type.IsNumeric())
				columnType = MVCxGridViewColumnType.SpinEdit;
			else if (type.Is<DateTime>())
				columnType = MVCxGridViewColumnType.DateEdit;
			else if (type.Is<TimeSpan>())
				columnType = MVCxGridViewColumnType.TimeEdit;
			else if (type.Is<Color>())
				columnType = MVCxGridViewColumnType.ColorEdit;
			else if (type.Is<IEnumerable<byte>>() || type.Is<Image>())
				columnType = MVCxGridViewColumnType.BinaryImage;
			else if (type.Is<System.Web.UI.WebControls.Image>())
				columnType = MVCxGridViewColumnType.Image;
			else if (type.Is<Uri>() || type.Is<UriBuilder>())
				columnType = MVCxGridViewColumnType.HyperLink;
			else
				columnType = MVCxGridViewColumnType.Default;

			MVCxGridViewColumn gridViewColumn = new MVCxGridViewColumn(property.Name, property.GetDisplayName(property.Name), columnType);
			settings.Columns.Add(gridViewColumn);
			return settings;
		}
	}
}
