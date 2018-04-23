Imports Microsoft.VisualBasic
Imports System
Imports System.Windows
Imports System.Windows.Data
Imports System.ComponentModel
Imports DevExpress.Xpf.Grid
Imports DevExpress.Data
Imports System.Collections.Specialized
Imports DevExpress.Xpf.Core
Imports DevExpress.Data.Filtering

Namespace GridControlViewModel
	Public Enum ModelFilterMode
		CollectionViewFilterPredicate
		FilterCriteria
	End Enum
	Public Class SortPropertyGroupDescription
		Inherits PropertyGroupDescription
		Private sortDirection_Renamed As ListSortDirection
		Public Property SortDirection() As ListSortDirection
			Get
				Return sortDirection_Renamed
			End Get
			Set(ByVal value As ListSortDirection)
				If sortDirection_Renamed = value Then
					Return
				End If
				sortDirection_Renamed = value
				OnPropertyChanged(New PropertyChangedEventArgs("SortDirection"))
			End Set
		End Property
	End Class
	Public Class GridControlDataModel
		Public Shared Function GetGridControlDataModel(ByVal gridControl As GridControl) As GridControlDataModel
			Return CType(gridControl.GetValue(GridControlDataModelProperty), GridControlDataModel)
		End Function
		Public Shared Sub SetGridControlDataModel(ByVal gridControl As GridControl, ByVal value As GridControlDataModel)
			gridControl.SetValue(GridControlDataModelProperty, value)
		End Sub
		Public Shared Function GetIsSynchronizedWithCurrentItem(ByVal gridControl As GridControl) As Boolean
			Return CBool(gridControl.GetValue(IsSynchronizedWithCurrentItemProperty))
		End Function
		Public Shared Sub SetIsSynchronizedWithCurrentItem(ByVal gridControl As GridControl, ByVal value As Boolean)
			gridControl.SetValue(IsSynchronizedWithCurrentItemProperty, value)
		End Sub
		Public Shared Function GetCollectionView(ByVal gridControl As GridControl) As ICollectionView
			Return CType(gridControl.GetValue(CollectionViewProperty), ICollectionView)
		End Function
		Public Shared Sub SetCollectionView(ByVal gridControl As GridControl, ByVal value As ICollectionView)
			gridControl.SetValue(CollectionViewProperty, value)
		End Sub

		Public Shared ReadOnly GridControlDataModelProperty As DependencyProperty
		Public Shared ReadOnly IsSynchronizedWithCurrentItemProperty As DependencyProperty
		Public Shared ReadOnly CollectionViewProperty As DependencyProperty

		Shared Sub New()
			GridControlDataModelProperty = DependencyProperty.RegisterAttached("GridControlDataModel", GetType(GridControlDataModel), GetType(GridControlDataModel), New UIPropertyMetadata(Nothing, AddressOf OnGridControlDataModelChanged))
			IsSynchronizedWithCurrentItemProperty = DependencyProperty.RegisterAttached("IsSynchronizedWithCurrentItem", GetType(Boolean), GetType(GridControlDataModel), New UIPropertyMetadata(True))
			CollectionViewProperty = DependencyProperty.RegisterAttached("CollectionView", GetType(ICollectionView), GetType(GridControlDataModel), New UIPropertyMetadata(Nothing, AddressOf OnCollectionViewChanged))
		End Sub
		Private Shared Sub OnGridControlDataModelChanged(ByVal d As DependencyObject, ByVal e As DependencyPropertyChangedEventArgs)
			Dim gridControl As GridControl = CType(d, GridControl)
			Dim model As GridControlDataModel = CType(e.NewValue, GridControlDataModel)
			If model IsNot Nothing Then
				model.ConnectToGridControl(gridControl)
			End If
		End Sub
		Private Shared Sub OnCollectionViewChanged(ByVal d As DependencyObject, ByVal e As DependencyPropertyChangedEventArgs)
			Dim gridControl As GridControl = CType(d, GridControl)
			Dim view As ICollectionView = CType(e.NewValue, ICollectionView)
			SetGridControlDataModel(gridControl, New GridControlDataModel() With {.CollectionView = view})
		End Sub
		Private collectionView_Renamed As ICollectionView
		Private gridControl As GridControl
		Private syncGroupSortLocker As New Locker()
		Private filterMode_Renamed As ModelFilterMode
		Private filterCriteria_Renamed As CriteriaOperator
		Public Sub New()
			AutoPopulateColumns = True
		End Sub
		Private privateAutoPopulateColumns As Boolean
		Public Property AutoPopulateColumns() As Boolean
			Get
				Return privateAutoPopulateColumns
			End Get
			Set(ByVal value As Boolean)
				privateAutoPopulateColumns = value
			End Set
		End Property
		Public Property FilterMode() As ModelFilterMode
			Get
				Return filterMode_Renamed
			End Get
			Set(ByVal value As ModelFilterMode)
				If filterMode_Renamed = value Then
					Return
				End If
				filterMode_Renamed = value
				SyncFilter()
				If gridControl IsNot Nothing Then
					gridControl.RefreshData()
				End If
			End Set
		End Property
		Public Property FilterCriteria() As CriteriaOperator
			Get
				Return filterCriteria_Renamed
			End Get
			Set(ByVal value As CriteriaOperator)
				If Object.ReferenceEquals(filterCriteria_Renamed, value) Then
					Return
				End If
				filterCriteria_Renamed = value
				SyncFilter()
			End Set
		End Property
		Public Property CollectionView() As ICollectionView
			Get
				Return collectionView_Renamed
			End Get
			Set(ByVal value As ICollectionView)
				If collectionView_Renamed Is value Then
					Return
				End If
				If collectionView_Renamed IsNot Nothing Then
					Dim notifyPropertyChanged As INotifyPropertyChanged = TryCast(collectionView_Renamed, INotifyPropertyChanged)
					If notifyPropertyChanged IsNot Nothing Then
						RemoveHandler notifyPropertyChanged.PropertyChanged, AddressOf OnCollectionViewPropertyChanged
					End If
					If collectionView_Renamed.SortDescriptions IsNot Nothing Then
						RemoveHandler (CType(collectionView_Renamed.SortDescriptions, INotifyCollectionChanged)).CollectionChanged, AddressOf OnSortDescriptionsCollectionChanged
					End If
					If collectionView_Renamed.GroupDescriptions IsNot Nothing Then
						RemoveHandler collectionView_Renamed.GroupDescriptions.CollectionChanged, AddressOf OnGroupDescriptionsCollectionChanged
					End If
					RemoveHandler collectionView_Renamed.CurrentChanged, AddressOf OnCollectionViewCurrentChanged
				End If
				collectionView_Renamed = value
				If collectionView_Renamed IsNot Nothing Then
					Dim notifyPropertyChanged As INotifyPropertyChanged = TryCast(collectionView_Renamed, INotifyPropertyChanged)
					If notifyPropertyChanged IsNot Nothing Then
						AddHandler notifyPropertyChanged.PropertyChanged, AddressOf OnCollectionViewPropertyChanged
					End If
					If collectionView_Renamed.SortDescriptions IsNot Nothing Then
						AddHandler (CType(collectionView_Renamed.SortDescriptions, INotifyCollectionChanged)).CollectionChanged, AddressOf OnSortDescriptionsCollectionChanged
					End If
					If collectionView_Renamed.GroupDescriptions IsNot Nothing Then
						AddHandler collectionView_Renamed.GroupDescriptions.CollectionChanged, AddressOf OnGroupDescriptionsCollectionChanged
					End If
					AddHandler collectionView_Renamed.CurrentChanged, AddressOf OnCollectionViewCurrentChanged
				End If
			End Set
		End Property

		Private Sub OnCollectionViewCurrentChanged(ByVal sender As Object, ByVal e As EventArgs)
			SyncFocusedRowHandle()
		End Sub
		Private Sub SyncFocusedRowHandle()
			If CanSyncCurrentRow() Then
				gridControl.View.FocusedRow = collectionView_Renamed.CurrentItem
			End If
		End Sub
		Private Sub OnGridControlFocusedRowChanged(ByVal sender As Object, ByVal e As FocusedRowChangedEventArgs)
			If CanSyncCurrentRow() AndAlso gridControl.View.FocusedRowHandle <> GridControl.InvalidRowHandle Then
				collectionView_Renamed.MoveCurrentTo(gridControl.View.FocusedRow)
			End If
		End Sub
		Private Function CanSyncCurrentRow() As Boolean
			Return gridControl IsNot Nothing AndAlso GetIsSynchronizedWithCurrentItem(gridControl)
		End Function
		Private Sub OnGroupDescriptionsCollectionChanged(ByVal sender As Object, ByVal e As NotifyCollectionChangedEventArgs)
			If gridControl Is Nothing Then
				Return
			End If
			SyncGrouping()
		End Sub
		Private Sub OnSortDescriptionsCollectionChanged(ByVal sender As Object, ByVal e As NotifyCollectionChangedEventArgs)
			If gridControl Is Nothing Then
				Return
			End If
			SyncSorting()
		End Sub
		Private Sub OnCollectionViewPropertyChanged(ByVal sender As Object, ByVal e As PropertyChangedEventArgs)
			If e.PropertyName = "Count" Then
				SyncFilter()
			End If
		End Sub
		Private Sub SyncFilter()
			If gridControl Is Nothing Then
				Return
			End If
			If FilterMode = ModelFilterMode.FilterCriteria Then
				gridControl.FilterCriteria = FilterCriteria
				gridControl.View.AllowColumnFiltering = True
			Else
				gridControl.FilterCriteria = Nothing
				gridControl.RefreshData()
				gridControl.View.AllowColumnFiltering = False
			End If
		End Sub
		Private Sub ConnectToGridControl(ByVal gridControl As GridControl)
			If Me.gridControl IsNot Nothing Then
				RemoveHandler gridControl.CustomRowFilter, AddressOf gridControl_CustomRowFilter
				RemoveHandler gridControl.SortInfo.CollectionChanged, AddressOf OnSortInfoCollectionChanged
				RemoveHandler gridControl.View.FocusedRowChanged, AddressOf OnGridControlFocusedRowChanged
				TypeDescriptor.GetProperties(GetType(GridControl))(GridControl.FilterCriteriaProperty.Name).RemoveValueChanged(gridControl, AddressOf OnGridControlFilterCriteriaChanged)
			End If
			Me.gridControl = gridControl
			If gridControl Is Nothing Then
				Return
			End If
			gridControl.AutoPopulateColumns = AutoPopulateColumns
			If CollectionView Is Nothing Then
				Return
			End If
			gridControl.DataSource = CollectionView.SourceCollection
			gridControl.BeginInit()
			Try
				AddHandler gridControl.CustomRowFilter, AddressOf gridControl_CustomRowFilter
				AddHandler gridControl.SortInfo.CollectionChanged, AddressOf OnSortInfoCollectionChanged
				AddHandler gridControl.View.FocusedRowChanged, AddressOf OnGridControlFocusedRowChanged
				TypeDescriptor.GetProperties(GetType(GridControl))(GridControl.FilterCriteriaProperty.Name).AddValueChanged(gridControl, AddressOf OnGridControlFilterCriteriaChanged)
				SyncSorting()
				SyncGrouping()
				SyncFocusedRowHandle()
				SyncFilter()
				CType(gridControl.View, TableView).AllowGrouping = collectionView_Renamed.CanGroup
				gridControl.View.AllowSorting = collectionView_Renamed.CanSort
			Finally
				gridControl.EndInit()
			End Try
		End Sub
		Private Sub OnGridControlFilterCriteriaChanged(ByVal sender As Object, ByVal e As EventArgs)
			FilterCriteria = gridControl.FilterCriteria
		End Sub
		Private Sub OnSortInfoCollectionChanged(ByVal sender As Object, ByVal e As NotifyCollectionChangedEventArgs)
			If syncGroupSortLocker.IsLocked Then
				Return
			End If
			syncGroupSortLocker.DoLockedAction(AddressOf SyncSortInfo)
		End Sub
		Private Sub SyncSortInfo()
			If CollectionView Is Nothing Then
				Return
			End If
			If CollectionView.SortDescriptions IsNot Nothing Then
				CollectionView.SortDescriptions.Clear()
				CollectionView.GroupDescriptions.Clear()
				For i As Integer = 0 To gridControl.GroupCount - 1
					Dim info As GridSortInfo = gridControl.SortInfo(i)
					CollectionView.GroupDescriptions.Add(New SortPropertyGroupDescription() With {.PropertyName = info.FieldName, .SortDirection = info.SortOrder})
				Next i
				For i As Integer = gridControl.GroupCount To gridControl.SortInfo.Count - 1
					Dim info As GridSortInfo = gridControl.SortInfo(i)
					CollectionView.SortDescriptions.Add(New SortDescription(info.FieldName, info.SortOrder))
				Next i
			End If
		End Sub
		Private Sub SyncSorting()
			If syncGroupSortLocker.IsLocked Then
				Return
			End If
			syncGroupSortLocker.DoLockedAction(AddressOf AnonymousMethod1)
		End Sub
		
		Private Sub AnonymousMethod1()
			If CollectionView.SortDescriptions IsNot Nothing Then
				gridControl.SortInfo.BeginUpdate()
				Try
					gridControl.ClearSorting()
					For Each sortDescription As SortDescription In CollectionView.SortDescriptions
						If sortDescription.Direction = ListSortDirection.Ascending Then
							gridControl.SortBy(gridControl.Columns(sortDescription.PropertyName),ColumnSortOrder.Ascending)
						Else
							gridControl.SortBy(gridControl.Columns(sortDescription.PropertyName),ColumnSortOrder.Descending)
						End If
					Next sortDescription
				Finally
					gridControl.SortInfo.EndUpdate()
				End Try
			End If
		End Sub
		Private Sub SyncGrouping()
			If syncGroupSortLocker.IsLocked Then
				Return
			End If
			syncGroupSortLocker.DoLockedAction(AddressOf AnonymousMethod2)
		End Sub
		
		Private Sub AnonymousMethod2()
			If CollectionView.GroupDescriptions IsNot Nothing Then
				gridControl.SortInfo.BeginUpdate()
				Try
					gridControl.ClearGrouping()
					For Each groupDescription As PropertyGroupDescription In CollectionView.GroupDescriptions
						Dim sortGroupDescription As SortPropertyGroupDescription = TryCast(groupDescription, SortPropertyGroupDescription)
						Dim sortDirection As ListSortDirection
						If sortGroupDescription IsNot Nothing Then
							sortDirection = sortGroupDescription.SortDirection
						Else
							sortDirection = ListSortDirection.Ascending
						End If
						If sortDirection = ListSortDirection.Ascending Then
							gridControl.GroupBy(gridControl.Columns(groupDescription.PropertyName),ColumnSortOrder.Ascending)
						Else
							gridControl.GroupBy(gridControl.Columns(groupDescription.PropertyName),ColumnSortOrder.Descending)
						End If
					Next groupDescription
				Finally
					gridControl.SortInfo.EndUpdate()
				End Try
			End If
		End Sub
		Private Sub gridControl_CustomRowFilter(ByVal sender As Object, ByVal e As RowFilterEventArgs)
			If CollectionView.Filter Is Nothing OrElse FilterMode = ModelFilterMode.FilterCriteria Then
				Return
			End If
			Dim rowHandle As Integer = gridControl.GetRowHandleByListIndex(e.ListSourceRowIndex)
			e.Visible = CollectionView.Filter(gridControl.GetRow(rowHandle))
			e.Handled = True
		End Sub
	End Class
End Namespace
