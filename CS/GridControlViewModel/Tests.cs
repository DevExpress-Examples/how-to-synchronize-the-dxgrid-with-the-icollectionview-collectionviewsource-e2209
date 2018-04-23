using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.ComponentModel;
using DevExpress.Xpf.Grid;
using NUnit.Framework;
using System.Collections;
using DevExpress.Data;
using DevExpress.Data.Filtering;
using DevExpress.Xpf.Core;

namespace GridControlViewModel.Tests {
    public class TestData {
        public string Text { get; set; }
        public int Number { get; set; }
    }
    [TestFixture]
    public class GridControlDataModelTests {
        Window window;
        GridControl grid;
        GridViewBase View { get { return grid.View; } }
        [SetUp]
        public void SetUp() {
            window = new Window();
            grid = new GridControl();
            window.Content = grid;
        }
        [TearDown]
        public void TearDown() {
            DispatcherHelper.UpdateLayoutAndDoEvents(window);
            window.Close();
            window = null;
            grid = null;
        }
        [Test]
        public void AssignNullDataModel() {
            GridControlDataModel.SetGridControlDataModel(grid, null);
            Assert.IsNull(grid.DataSource);
            Assert.AreEqual(0, grid.Columns.Count);
        }
        [Test]
        public void AssignDataModelWithNullCollectionView() {
            GridControlDataModel.SetGridControlDataModel(grid, new GridControlDataModel());
            Assert.IsNull(grid.DataSource);
            window.Show();
            Assert.AreEqual(0, grid.Columns.Count);
        }
        [Test]
        public void AssignDataModelWithCollectionView() {
            IList list = CreateList();
            GridControlDataModel.SetGridControlDataModel(grid, new GridControlDataModel() { CollectionView = new ListCollectionView(list) });
            Assert.AreEqual(list, grid.DataSource);
            window.Show();
            Assert.AreEqual(2, grid.Columns.Count);
            Assert.AreEqual("Text", grid.Columns[0].FieldName);
            Assert.AreEqual("Number", grid.Columns[1].FieldName);
        }
        [Test]
        public void AutoPopulateColumnsFalse() {
            IList list = CreateList();
            GridControlDataModel.SetGridControlDataModel(grid, new GridControlDataModel() { CollectionView = new ListCollectionView(list), AutoPopulateColumns = false });
            Assert.AreEqual(list, grid.DataSource);
            window.Show();
            Assert.AreEqual(0, grid.Columns.Count);
        }
        [Test]
        public void Filter() {
            IList list = CreateList();
            ListCollectionView view = new ListCollectionView(list) { Filter = new Predicate<object>(EvenFilter) };
            GridControlDataModel.SetGridControlDataModel(grid, new GridControlDataModel() { CollectionView = view  });
            window.Show();
            Assert.AreEqual(50, grid.VisibleRowCount);
            Assert.IsFalse(View.AllowColumnFiltering);

            grid.View.FocusedRowHandle = 40;
            Assert.AreEqual(40, view.CurrentPosition);

            view.MoveCurrentToPosition(30);
            Assert.AreEqual(30, grid.View.FocusedRowHandle);
        }
        [Test]
        public void SortDescriptions() { 
            IList list = CreateList();
            ICollectionView view = new ListCollectionView(list);
            view.SortDescriptions.Add(new SortDescription() { Direction = ListSortDirection.Ascending, PropertyName = "Text" });
            view.SortDescriptions.Add(new SortDescription() { Direction = ListSortDirection.Descending, PropertyName = "Number" });
            GridControlDataModel.SetGridControlDataModel(grid, new GridControlDataModel() { CollectionView = view } );
            window.Show();
            Assert.AreEqual(0, grid.Columns["Text"].SortIndex);
            Assert.AreEqual(ColumnSortOrder.Ascending, grid.Columns["Text"].SortOrder);
            Assert.AreEqual(1, grid.Columns["Number"].SortIndex);
            Assert.AreEqual(ColumnSortOrder.Descending, grid.Columns["Number"].SortOrder);
        }
        [Test]
        public void PropertyGroupDescriptions() {
            IList list = CreateList();
            ICollectionView view = new ListCollectionView(list);
            view.GroupDescriptions.Add(new PropertyGroupDescription() { PropertyName = "Text" });
            view.GroupDescriptions.Add(new PropertyGroupDescription() { PropertyName = "Number" });
            GridControlDataModel.SetGridControlDataModel(grid, new GridControlDataModel() { CollectionView = view });
            window.Show();
            Assert.AreEqual(0, grid.Columns["Text"].GroupIndex);
            Assert.AreEqual(ColumnSortOrder.Ascending, grid.Columns["Text"].SortOrder);
            Assert.AreEqual(1, grid.Columns["Number"].GroupIndex);
            Assert.AreEqual(ColumnSortOrder.Ascending, grid.Columns["Number"].SortOrder);
        }
        [Test]
        public void SortPropertyGroupDescriptions() {
            IList list = CreateList();
            ICollectionView view = new ListCollectionView(list);
            view.GroupDescriptions.Add(new SortPropertyGroupDescription() { PropertyName = "Text", SortDirection = ListSortDirection.Ascending });
            view.GroupDescriptions.Add(new SortPropertyGroupDescription() { PropertyName = "Number", SortDirection = ListSortDirection.Descending });
            GridControlDataModel.SetGridControlDataModel(grid, new GridControlDataModel() { CollectionView = view });
            window.Show();
            Assert.AreEqual(0, grid.Columns["Text"].GroupIndex);
            Assert.AreEqual(ColumnSortOrder.Ascending, grid.Columns["Text"].SortOrder);
            Assert.AreEqual(1, grid.Columns["Number"].GroupIndex);
            Assert.AreEqual(ColumnSortOrder.Descending, grid.Columns["Number"].SortOrder);
        }
        [Test]
        public void UpdateFilter() {
            IList list = CreateList();
            ICollectionView view = new ListCollectionView(list);
            GridControlDataModel.SetGridControlDataModel(grid, new GridControlDataModel() { CollectionView = view });
            window.Show();
            Assert.AreEqual(100, grid.VisibleRowCount);
            view.Filter = EvenFilter;
            Assert.AreEqual(50, grid.VisibleRowCount);
        }
        [Test]
        public void AllowGroupSortFalse() {
            IList list = CreateList();
            ICollectionView view = new CollectionView(list);
            GridControlDataModel.SetGridControlDataModel(grid, new GridControlDataModel() { CollectionView = view });
            Assert.IsFalse(View.AllowSorting);
            Assert.IsFalse(View.AllowGrouping);
        }
        [Test]
        public void SyncSorting() {
            IList list = CreateList();
            ICollectionView view = new ListCollectionView(list);
            GridControlDataModel.SetGridControlDataModel(grid, new GridControlDataModel() { CollectionView = view });
            window.Show();
            view.SortDescriptions.Add(new SortDescription("Text", ListSortDirection.Descending));
            Assert.AreEqual(0, grid.Columns["Text"].SortIndex);
            Assert.AreEqual(ColumnSortOrder.Descending, grid.Columns["Text"].SortOrder);

            grid.SortInfo[0].SortOrder = ListSortDirection.Ascending;
            Assert.AreEqual(1, view.SortDescriptions.Count);
            Assert.AreEqual(ListSortDirection.Ascending, view.SortDescriptions[0].Direction);
            grid.SortInfo.Add(new GridSortInfo("Number", ListSortDirection.Descending));
            Assert.AreEqual(2, view.SortDescriptions.Count);
            Assert.AreEqual(ListSortDirection.Ascending, view.SortDescriptions[0].Direction);
            Assert.AreEqual(ListSortDirection.Descending, view.SortDescriptions[1].Direction);
        }
        [Test]
        public void SyncSorting2() {
            IList list = CreateList();
            ICollectionView view = new ListCollectionView(list);
            GridControlDataModel.SetGridControlDataModel(grid, new GridControlDataModel() { CollectionView = view });
            window.Show();
            view.SortDescriptions.Add(new SortDescription("Text", ListSortDirection.Descending));
            view.SortDescriptions.Add(new SortDescription("Number", ListSortDirection.Ascending));
            Assert.AreEqual(2, grid.SortInfo.Count);
            Assert.AreEqual("Text", grid.SortInfo[0].FieldName);
            Assert.AreEqual(ListSortDirection.Descending, grid.SortInfo[0].SortOrder);
            Assert.AreEqual("Number", grid.SortInfo[1].FieldName);
            Assert.AreEqual(ListSortDirection.Ascending, grid.SortInfo[1].SortOrder);
            view.SortDescriptions.Clear();
            Assert.AreEqual(0, grid.SortInfo.Count);
        }
        [Test]
        public void SyncGrouping() {
            IList list = CreateList();
            ICollectionView view = new ListCollectionView(list);
            GridControlDataModel.SetGridControlDataModel(grid, new GridControlDataModel() { CollectionView = view });
            window.Show();
            view.GroupDescriptions.Add(new PropertyGroupDescription("Text"));
            Assert.AreEqual(0, grid.Columns["Text"].GroupIndex);

            grid.SortInfo[0].SortOrder = ListSortDirection.Descending;
            Assert.AreEqual(1, view.GroupDescriptions.Count);
            Assert.AreEqual(ListSortDirection.Descending, ((SortPropertyGroupDescription)view.GroupDescriptions[0]).SortDirection);
            grid.GroupBy("Number");
            Assert.AreEqual(2, view.GroupDescriptions.Count);
            Assert.AreEqual(ListSortDirection.Descending, ((SortPropertyGroupDescription)view.GroupDescriptions[0]).SortDirection);
            Assert.AreEqual(ListSortDirection.Ascending, ((SortPropertyGroupDescription)view.GroupDescriptions[1]).SortDirection);
        }
        [Test]
        public void SyncGrouping2() {
            IList list = CreateList();
            ICollectionView view = new ListCollectionView(list);
            GridControlDataModel.SetGridControlDataModel(grid, new GridControlDataModel() { CollectionView = view });
            window.Show();
            view.GroupDescriptions.Add(new PropertyGroupDescription("Text"));
            view.GroupDescriptions.Add(new PropertyGroupDescription("Number"));
            Assert.AreEqual(2, grid.GroupCount);

            view.GroupDescriptions.Clear();
            Assert.AreEqual(0, grid.GroupCount);
        }
        [Test]
        public void SyncFocusedRowTrue() {
            IList list = CreateList();
            ICollectionView view = new ListCollectionView(list);
            view.MoveCurrentToPosition(3);
            GridControlDataModel.SetGridControlDataModel(grid, new GridControlDataModel() { CollectionView = view });
            window.Show();
            Assert.AreEqual(3, View.FocusedRowHandle);
            view.MoveCurrentToPosition(5);
            Assert.AreEqual(5, View.FocusedRowHandle);
            View.FocusedRowHandle = 4;
            Assert.AreEqual(4, view.CurrentPosition);
        }
        [Test]
        public void SyncFocusedRowFalse() {
            IList list = CreateList();
            ICollectionView view = new ListCollectionView(list);
            view.MoveCurrentToPosition(3);
            GridControlDataModel.SetIsSynchronizedWithCurrentItem(grid, false);
            GridControlDataModel.SetGridControlDataModel(grid, new GridControlDataModel() { CollectionView = view });
            window.Show();
            Assert.AreEqual(0, View.FocusedRowHandle);
            view.MoveCurrentToPosition(5);
            Assert.AreEqual(0, View.FocusedRowHandle);
            View.FocusedRowHandle = 4;
            Assert.AreEqual(5, view.CurrentPosition);
        }
        [Test]
        public void FilterMode() {
            IList list = CreateList();
            GridControlDataModel dataModel = new GridControlDataModel() { CollectionView = new ListCollectionView(list) { Filter = new Predicate<object>(EvenFilter) } };
            dataModel.FilterCriteria = CriteriaOperator.Parse("Number < 30");
            GridControlDataModel.SetGridControlDataModel(grid, dataModel);
            window.Show();
            dataModel.FilterMode = ModelFilterMode.FilterCriteria;
            Assert.IsTrue(grid.View.AllowColumnFiltering);
            Assert.AreEqual(30, grid.VisibleRowCount);
            dataModel.FilterCriteria = CriteriaOperator.Parse("Number < 20");
            Assert.AreEqual(20, grid.VisibleRowCount);
            dataModel.FilterMode = ModelFilterMode.CollectionViewFilterPredicate;
            Assert.IsFalse(grid.View.AllowColumnFiltering);
            Assert.AreEqual(50, grid.VisibleRowCount);

            dataModel.FilterCriteria = null;
            dataModel.FilterMode = ModelFilterMode.FilterCriteria;
            Assert.AreEqual(100, grid.VisibleRowCount);
        }
        [Test]
        public void FilterMode2() {
            IList list = CreateList();
            GridControlDataModel dataModel = new GridControlDataModel() { CollectionView = new ListCollectionView(list) };
            GridControlDataModel.SetGridControlDataModel(grid, dataModel);
            window.Show();
            dataModel.CollectionView.Filter = EvenFilter;
            window.UpdateLayout();
            dataModel.FilterMode = ModelFilterMode.FilterCriteria;
            window.UpdateLayout();
            Assert.AreEqual(100, grid.VisibleRowCount);
            dataModel.FilterCriteria = CriteriaOperator.Parse("Number < 20");
            dataModel.FilterMode = ModelFilterMode.CollectionViewFilterPredicate;
            Assert.IsNull(grid.FilterCriteria);
            Assert.AreEqual(50, grid.VisibleRowCount);
        }
        [Test]
        public void SyncFilterCriteria() {
            IList list = CreateList();
            GridControlDataModel dataModel = new GridControlDataModel() { FilterMode = ModelFilterMode.FilterCriteria, CollectionView = new ListCollectionView(list) };
            dataModel.FilterCriteria = CriteriaOperator.Parse("Number < 30");
            GridControlDataModel.SetGridControlDataModel(grid, dataModel);
            window.Show();
            Assert.AreEqual(30, grid.VisibleRowCount);
            grid.FilterString = "Number < 20";
            Assert.AreEqual(20, grid.VisibleRowCount);
            Assert.AreEqual(CriteriaOperator.Parse("Number < 20"), dataModel.FilterCriteria);
        }
        [Test]
        public void CollectionViewProperty() {
            IList list = CreateList();
            ListCollectionView listViiew = new ListCollectionView(list);
            GridControlDataModel.SetCollectionView(grid, listViiew);
            Assert.AreSame(listViiew, GridControlDataModel.GetGridControlDataModel(grid).CollectionView);
        }
        bool EvenFilter(object obj) {
            TestData testData = (TestData)obj;
            return testData.Number % 2 == 0;
        }
        IList CreateList() {
            List<TestData> list = new List<TestData>();
            for(int i = 0; i < 100; i++) {
                list.Add(new TestData() { Number = i, Text = "row " + i });
            }
            return list;
        }
    }
}