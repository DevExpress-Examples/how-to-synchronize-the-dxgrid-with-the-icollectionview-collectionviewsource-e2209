Imports Microsoft.VisualBasic
Imports System
Imports System.Collections.Generic
Imports System.Windows
Imports System.Windows.Data
Imports System.Windows.Documents
Imports System.ComponentModel
Imports DevExpress.Xpf.Grid
Imports NUnit.Framework
Imports System.Collections
Imports DevExpress.Data
Imports DevExpress.Data.Filtering
Imports DevExpress.Xpf.Core

Namespace GridControlViewModel.Tests
	Public Class TestData
		Private privateText As String
		Public Property Text() As String
			Get
				Return privateText
			End Get
			Set(ByVal value As String)
				privateText = value
			End Set
		End Property
		Private privateNumber As Integer
		Public Property Number() As Integer
			Get
				Return privateNumber
			End Get
			Set(ByVal value As Integer)
				privateNumber = value
			End Set
		End Property
	End Class
	<TestFixture> _
	Public Class GridControlDataModelTests
		Private window As Window
		Private grid As GridControl
		Private ReadOnly Property View() As GridViewBase
			Get
				Return grid.View
			End Get
		End Property
		<SetUp> _
		Public Sub SetUp()
			window = New Window()
			grid = New GridControl()
			window.Content = grid
		End Sub
		<TearDown> _
		Public Sub TearDown()
			DispatcherHelper.UpdateLayoutAndDoEvents(window)
			window.Close()
			window = Nothing
			grid = Nothing
		End Sub
		<Test> _
		Public Sub AssignNullDataModel()
			GridControlDataModel.SetGridControlDataModel(grid, Nothing)
			Assert.IsNull(grid.DataSource)
			Assert.AreEqual(0, grid.Columns.Count)
		End Sub
		<Test> _
		Public Sub AssignDataModelWithNullCollectionView()
			GridControlDataModel.SetGridControlDataModel(grid, New GridControlDataModel())
			Assert.IsNull(grid.DataSource)
			window.Show()
			Assert.AreEqual(0, grid.Columns.Count)
		End Sub
		<Test> _
		Public Sub AssignDataModelWithCollectionView()
			Dim list As IList = CreateList()
			GridControlDataModel.SetGridControlDataModel(grid, New GridControlDataModel() With {.CollectionView = New ListCollectionView(list)})
			Assert.AreEqual(list, grid.DataSource)
			window.Show()
			Assert.AreEqual(2, grid.Columns.Count)
			Assert.AreEqual("Text", grid.Columns(0).FieldName)
			Assert.AreEqual("Number", grid.Columns(1).FieldName)
		End Sub
		<Test> _
		Public Sub AutoPopulateColumnsFalse()
			Dim list As IList = CreateList()
			GridControlDataModel.SetGridControlDataModel(grid, New GridControlDataModel() With {.CollectionView = New ListCollectionView(list), .AutoPopulateColumns = False})
			Assert.AreEqual(list, grid.DataSource)
			window.Show()
			Assert.AreEqual(0, grid.Columns.Count)
		End Sub
		<Test> _
		Public Sub Filter()
			Dim list As IList = CreateList()
			Dim view As New ListCollectionView(list) With {.Filter = New Predicate(Of Object)(AddressOf EvenFilter)}
			GridControlDataModel.SetGridControlDataModel(grid, New GridControlDataModel() With {.CollectionView = view})
			window.Show()
			Assert.AreEqual(50, grid.VisibleRowCount)
			Assert.IsFalse(Me.View.AllowColumnFiltering)

			grid.View.FocusedRowHandle = 40
			Assert.AreEqual(40, view.CurrentPosition)

			view.MoveCurrentToPosition(30)
			Assert.AreEqual(30, grid.View.FocusedRowHandle)
		End Sub
		<Test> _
		Public Sub SortDescriptions()
			Dim list As IList = CreateList()
			Dim view As ICollectionView = New ListCollectionView(list)
			view.SortDescriptions.Add(New SortDescription() With {.Direction = ListSortDirection.Ascending, .PropertyName = "Text"})
			view.SortDescriptions.Add(New SortDescription() With {.Direction = ListSortDirection.Descending, .PropertyName = "Number"})
			GridControlDataModel.SetGridControlDataModel(grid, New GridControlDataModel() With {.CollectionView = view})
			window.Show()
			Assert.AreEqual(0, grid.Columns("Text").SortIndex)
			Assert.AreEqual(ColumnSortOrder.Ascending, grid.Columns("Text").SortOrder)
			Assert.AreEqual(1, grid.Columns("Number").SortIndex)
			Assert.AreEqual(ColumnSortOrder.Descending, grid.Columns("Number").SortOrder)
		End Sub
		<Test> _
		Public Sub PropertyGroupDescriptions()
			Dim list As IList = CreateList()
			Dim view As ICollectionView = New ListCollectionView(list)
			view.GroupDescriptions.Add(New PropertyGroupDescription() With {.PropertyName = "Text"})
			view.GroupDescriptions.Add(New PropertyGroupDescription() With {.PropertyName = "Number"})
			GridControlDataModel.SetGridControlDataModel(grid, New GridControlDataModel() With {.CollectionView = view})
			window.Show()
			Assert.AreEqual(0, grid.Columns("Text").GroupIndex)
			Assert.AreEqual(ColumnSortOrder.Ascending, grid.Columns("Text").SortOrder)
			Assert.AreEqual(1, grid.Columns("Number").GroupIndex)
			Assert.AreEqual(ColumnSortOrder.Ascending, grid.Columns("Number").SortOrder)
		End Sub
		<Test> _
		Public Sub SortPropertyGroupDescriptions()
			Dim list As IList = CreateList()
			Dim view As ICollectionView = New ListCollectionView(list)
			view.GroupDescriptions.Add(New SortPropertyGroupDescription() With {.PropertyName = "Text", .SortDirection = ListSortDirection.Ascending})
			view.GroupDescriptions.Add(New SortPropertyGroupDescription() With {.PropertyName = "Number", .SortDirection = ListSortDirection.Descending})
			GridControlDataModel.SetGridControlDataModel(grid, New GridControlDataModel() With {.CollectionView = view})
			window.Show()
			Assert.AreEqual(0, grid.Columns("Text").GroupIndex)
			Assert.AreEqual(ColumnSortOrder.Ascending, grid.Columns("Text").SortOrder)
			Assert.AreEqual(1, grid.Columns("Number").GroupIndex)
			Assert.AreEqual(ColumnSortOrder.Descending, grid.Columns("Number").SortOrder)
		End Sub
		<Test> _
		Public Sub UpdateFilter()
			Dim list As IList = CreateList()
			Dim view As ICollectionView = New ListCollectionView(list)
			GridControlDataModel.SetGridControlDataModel(grid, New GridControlDataModel() With {.CollectionView = view})
			window.Show()
			Assert.AreEqual(100, grid.VisibleRowCount)
			view.Filter = AddressOf EvenFilter
			Assert.AreEqual(50, grid.VisibleRowCount)
		End Sub
		<Test> _
		Public Sub AllowGroupSortFalse()
			Dim list As IList = CreateList()
			Dim view As ICollectionView = New CollectionView(list)
			GridControlDataModel.SetGridControlDataModel(grid, New GridControlDataModel() With {.CollectionView = view})
			Assert.IsFalse(Me.View.AllowSorting)
			Assert.IsFalse(Me.View.AllowGrouping)
		End Sub
		<Test> _
		Public Sub SyncSorting()
			Dim list As IList = CreateList()
			Dim view As ICollectionView = New ListCollectionView(list)
			GridControlDataModel.SetGridControlDataModel(grid, New GridControlDataModel() With {.CollectionView = view})
			window.Show()
			view.SortDescriptions.Add(New SortDescription("Text", ListSortDirection.Descending))
			Assert.AreEqual(0, grid.Columns("Text").SortIndex)
			Assert.AreEqual(ColumnSortOrder.Descending, grid.Columns("Text").SortOrder)

			grid.SortInfo(0).SortOrder = ListSortDirection.Ascending
			Assert.AreEqual(1, view.SortDescriptions.Count)
			Assert.AreEqual(ListSortDirection.Ascending, view.SortDescriptions(0).Direction)
			grid.SortInfo.Add(New GridSortInfo("Number", ListSortDirection.Descending))
			Assert.AreEqual(2, view.SortDescriptions.Count)
			Assert.AreEqual(ListSortDirection.Ascending, view.SortDescriptions(0).Direction)
			Assert.AreEqual(ListSortDirection.Descending, view.SortDescriptions(1).Direction)
		End Sub
		<Test> _
		Public Sub SyncSorting2()
			Dim list As IList = CreateList()
			Dim view As ICollectionView = New ListCollectionView(list)
			GridControlDataModel.SetGridControlDataModel(grid, New GridControlDataModel() With {.CollectionView = view})
			window.Show()
			view.SortDescriptions.Add(New SortDescription("Text", ListSortDirection.Descending))
			view.SortDescriptions.Add(New SortDescription("Number", ListSortDirection.Ascending))
			Assert.AreEqual(2, grid.SortInfo.Count)
			Assert.AreEqual("Text", grid.SortInfo(0).FieldName)
			Assert.AreEqual(ListSortDirection.Descending, grid.SortInfo(0).SortOrder)
			Assert.AreEqual("Number", grid.SortInfo(1).FieldName)
			Assert.AreEqual(ListSortDirection.Ascending, grid.SortInfo(1).SortOrder)
			view.SortDescriptions.Clear()
			Assert.AreEqual(0, grid.SortInfo.Count)
		End Sub
		<Test> _
		Public Sub SyncGrouping()
			Dim list As IList = CreateList()
			Dim view As ICollectionView = New ListCollectionView(list)
			GridControlDataModel.SetGridControlDataModel(grid, New GridControlDataModel() With {.CollectionView = view})
			window.Show()
			view.GroupDescriptions.Add(New PropertyGroupDescription("Text"))
			Assert.AreEqual(0, grid.Columns("Text").GroupIndex)

			grid.SortInfo(0).SortOrder = ListSortDirection.Descending
			Assert.AreEqual(1, view.GroupDescriptions.Count)
			Assert.AreEqual(ListSortDirection.Descending, (CType(view.GroupDescriptions(0), SortPropertyGroupDescription)).SortDirection)
			grid.GroupBy("Number")
			Assert.AreEqual(2, view.GroupDescriptions.Count)
			Assert.AreEqual(ListSortDirection.Descending, (CType(view.GroupDescriptions(0), SortPropertyGroupDescription)).SortDirection)
			Assert.AreEqual(ListSortDirection.Ascending, (CType(view.GroupDescriptions(1), SortPropertyGroupDescription)).SortDirection)
		End Sub
		<Test> _
		Public Sub SyncGrouping2()
			Dim list As IList = CreateList()
			Dim view As ICollectionView = New ListCollectionView(list)
			GridControlDataModel.SetGridControlDataModel(grid, New GridControlDataModel() With {.CollectionView = view})
			window.Show()
			view.GroupDescriptions.Add(New PropertyGroupDescription("Text"))
			view.GroupDescriptions.Add(New PropertyGroupDescription("Number"))
			Assert.AreEqual(2, grid.GroupCount)

			view.GroupDescriptions.Clear()
			Assert.AreEqual(0, grid.GroupCount)
		End Sub
		<Test> _
		Public Sub SyncFocusedRowTrue()
			Dim list As IList = CreateList()
			Dim view As ICollectionView = New ListCollectionView(list)
			view.MoveCurrentToPosition(3)
			GridControlDataModel.SetGridControlDataModel(grid, New GridControlDataModel() With {.CollectionView = view})
			window.Show()
			Assert.AreEqual(3, Me.View.FocusedRowHandle)
			view.MoveCurrentToPosition(5)
			Assert.AreEqual(5, Me.View.FocusedRowHandle)
			Me.View.FocusedRowHandle = 4
			Assert.AreEqual(4, view.CurrentPosition)
		End Sub
		<Test> _
		Public Sub SyncFocusedRowFalse()
			Dim list As IList = CreateList()
			Dim view As ICollectionView = New ListCollectionView(list)
			view.MoveCurrentToPosition(3)
			GridControlDataModel.SetIsSynchronizedWithCurrentItem(grid, False)
			GridControlDataModel.SetGridControlDataModel(grid, New GridControlDataModel() With {.CollectionView = view})
			window.Show()
			Assert.AreEqual(0, Me.View.FocusedRowHandle)
			view.MoveCurrentToPosition(5)
			Assert.AreEqual(0, Me.View.FocusedRowHandle)
			Me.View.FocusedRowHandle = 4
			Assert.AreEqual(5, view.CurrentPosition)
		End Sub
		<Test> _
		Public Sub FilterMode()
			Dim list As IList = CreateList()
			Dim dataModel As New GridControlDataModel() With {.CollectionView = New ListCollectionView(list) With {.Filter = New Predicate(Of Object)(AddressOf EvenFilter)}}
			dataModel.FilterCriteria = CriteriaOperator.Parse("Number < 30")
			GridControlDataModel.SetGridControlDataModel(grid, dataModel)
			window.Show()
			dataModel.FilterMode = ModelFilterMode.FilterCriteria
			Assert.IsTrue(grid.View.AllowColumnFiltering)
			Assert.AreEqual(30, grid.VisibleRowCount)
			dataModel.FilterCriteria = CriteriaOperator.Parse("Number < 20")
			Assert.AreEqual(20, grid.VisibleRowCount)
			dataModel.FilterMode = ModelFilterMode.CollectionViewFilterPredicate
			Assert.IsFalse(grid.View.AllowColumnFiltering)
			Assert.AreEqual(50, grid.VisibleRowCount)

			dataModel.FilterCriteria = Nothing
			dataModel.FilterMode = ModelFilterMode.FilterCriteria
			Assert.AreEqual(100, grid.VisibleRowCount)
		End Sub
		<Test> _
		Public Sub FilterMode2()
			Dim list As IList = CreateList()
			Dim dataModel As New GridControlDataModel() With {.CollectionView = New ListCollectionView(list)}
			GridControlDataModel.SetGridControlDataModel(grid, dataModel)
			window.Show()
			dataModel.CollectionView.Filter = AddressOf EvenFilter
			window.UpdateLayout()
			dataModel.FilterMode = ModelFilterMode.FilterCriteria
			window.UpdateLayout()
			Assert.AreEqual(100, grid.VisibleRowCount)
			dataModel.FilterCriteria = CriteriaOperator.Parse("Number < 20")
			dataModel.FilterMode = ModelFilterMode.CollectionViewFilterPredicate
			Assert.IsNull(grid.FilterCriteria)
			Assert.AreEqual(50, grid.VisibleRowCount)
		End Sub
		<Test> _
		Public Sub SyncFilterCriteria()
			Dim list As IList = CreateList()
			Dim dataModel As New GridControlDataModel() With {.FilterMode = ModelFilterMode.FilterCriteria, .CollectionView = New ListCollectionView(list)}
			dataModel.FilterCriteria = CriteriaOperator.Parse("Number < 30")
			GridControlDataModel.SetGridControlDataModel(grid, dataModel)
			window.Show()
			Assert.AreEqual(30, grid.VisibleRowCount)
			grid.FilterString = "Number < 20"
			Assert.AreEqual(20, grid.VisibleRowCount)
			Assert.AreEqual(CriteriaOperator.Parse("Number < 20"), dataModel.FilterCriteria)
		End Sub
		<Test> _
		Public Sub CollectionViewProperty()
			Dim list As IList = CreateList()
			Dim listViiew As New ListCollectionView(list)
			GridControlDataModel.SetCollectionView(grid, listViiew)
			Assert.AreSame(listViiew, GridControlDataModel.GetGridControlDataModel(grid).CollectionView)
		End Sub
		Private Function EvenFilter(ByVal obj As Object) As Boolean
			Dim testData As TestData = CType(obj, TestData)
			Return testData.Number Mod 2 = 0
		End Function
		Private Function CreateList() As IList
			Dim list As New List(Of TestData)()
			For i As Integer = 0 To 99
				list.Add(New TestData() With {.Number = i, .Text = "row " & i})
			Next i
			Return list
		End Function
	End Class
End Namespace