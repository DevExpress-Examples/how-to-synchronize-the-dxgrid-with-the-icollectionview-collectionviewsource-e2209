using System;
using System.Windows;
using System.Windows.Data;
using System.ComponentModel;
using DevExpress.Wpf.Grid;
using DevExpress.Data;
using System.Collections.Specialized;
using DevExpress.Wpf.Core;
using DevExpress.Data.Filtering;

namespace GridControlViewModel {
    public enum ModelFilterMode { 
        CollectionViewFilterPredicate,
        FilterCriteria,
    }
    public class SortPropertyGroupDescription : PropertyGroupDescription {
        ListSortDirection sortDirection;
        public ListSortDirection SortDirection {
            get { return sortDirection; }
            set {
                if(sortDirection == value)
                    return;
                sortDirection = value;
                OnPropertyChanged(new PropertyChangedEventArgs("SortDirection"));
            }
        }
    }
    public class GridControlDataModel {
        public static GridControlDataModel GetGridControlDataModel(GridControl gridControl) {
            return (GridControlDataModel)gridControl.GetValue(GridControlDataModelProperty);
        }
        public static void SetGridControlDataModel(GridControl gridControl, GridControlDataModel value) {
            gridControl.SetValue(GridControlDataModelProperty, value);
        }
        public static bool GetIsSynchronizedWithCurrentItem(GridControl gridControl) {
            return (bool)gridControl.GetValue(IsSynchronizedWithCurrentItemProperty);
        }
        public static void SetIsSynchronizedWithCurrentItem(GridControl gridControl, bool value) {
            gridControl.SetValue(IsSynchronizedWithCurrentItemProperty, value);
        }
        public static ICollectionView GetCollectionView(GridControl gridControl) {
            return (ICollectionView)gridControl.GetValue(CollectionViewProperty);
        }
        public static void SetCollectionView(GridControl gridControl, ICollectionView value) {
            gridControl.SetValue(CollectionViewProperty, value);
        }

        public static readonly DependencyProperty GridControlDataModelProperty;
        public static readonly DependencyProperty IsSynchronizedWithCurrentItemProperty;
        public static readonly DependencyProperty CollectionViewProperty;

        static GridControlDataModel() {
            GridControlDataModelProperty = DependencyProperty.RegisterAttached("GridControlDataModel", typeof(GridControlDataModel), typeof(GridControlDataModel), new UIPropertyMetadata(null, OnGridControlDataModelChanged));
            IsSynchronizedWithCurrentItemProperty = DependencyProperty.RegisterAttached("IsSynchronizedWithCurrentItem", typeof(bool), typeof(GridControlDataModel), new UIPropertyMetadata(true));
            CollectionViewProperty = DependencyProperty.RegisterAttached("CollectionView", typeof(ICollectionView), typeof(GridControlDataModel), new UIPropertyMetadata(null, OnCollectionViewChanged));
        }
        static void OnGridControlDataModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            GridControl gridControl = (GridControl)d;
            GridControlDataModel model = (GridControlDataModel)e.NewValue;
            if(model != null)
                model.ConnectToGridControl(gridControl);
        }
        static void OnCollectionViewChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            GridControl gridControl = (GridControl)d;
            ICollectionView view = (ICollectionView)e.NewValue;
            SetGridControlDataModel(gridControl, new GridControlDataModel() { CollectionView = view });
        }
        ICollectionView collectionView;
        GridControl gridControl;
        Locker syncGroupSortLocker = new Locker();
        ModelFilterMode filterMode;
        CriteriaOperator filterCriteria;
        public GridControlDataModel() {
            AutoPopulateColumns = true;
        }
        public bool AutoPopulateColumns { get; set; }
        public ModelFilterMode FilterMode {
            get { return filterMode; }
            set {
                if(filterMode == value)
                    return;
                filterMode = value;
                SyncFilter();
                if(gridControl != null)
                    gridControl.RefreshData();
            }
        }
        public CriteriaOperator FilterCriteria {
            get { return filterCriteria; }
            set {
                if(object.ReferenceEquals(filterCriteria, value))
                    return;
                filterCriteria = value;
                SyncFilter();
            }
        }
        public ICollectionView CollectionView {
            get { return collectionView; }
            set {
                if(collectionView == value)
                    return;
                if(collectionView != null) {
                    INotifyPropertyChanged notifyPropertyChanged = collectionView as INotifyPropertyChanged;
                    if(notifyPropertyChanged != null)
                        notifyPropertyChanged.PropertyChanged -= new PropertyChangedEventHandler(OnCollectionViewPropertyChanged);
                    if(collectionView.SortDescriptions != null)
                        ((INotifyCollectionChanged)collectionView.SortDescriptions).CollectionChanged -= new NotifyCollectionChangedEventHandler(OnSortDescriptionsCollectionChanged);
                    if(collectionView.GroupDescriptions != null)
                        collectionView.GroupDescriptions.CollectionChanged -= new NotifyCollectionChangedEventHandler(OnGroupDescriptionsCollectionChanged);
                    collectionView.CurrentChanged -= new EventHandler(OnCollectionViewCurrentChanged);
                }
                collectionView = value;
                if(collectionView != null) {
                    INotifyPropertyChanged notifyPropertyChanged = collectionView as INotifyPropertyChanged;
                    if(notifyPropertyChanged != null)
                        notifyPropertyChanged.PropertyChanged += new PropertyChangedEventHandler(OnCollectionViewPropertyChanged);
                    if(collectionView.SortDescriptions != null)
                        ((INotifyCollectionChanged)collectionView.SortDescriptions).CollectionChanged += new NotifyCollectionChangedEventHandler(OnSortDescriptionsCollectionChanged);
                    if(collectionView.GroupDescriptions != null)
                        collectionView.GroupDescriptions.CollectionChanged += new NotifyCollectionChangedEventHandler(OnGroupDescriptionsCollectionChanged);
                    collectionView.CurrentChanged += new EventHandler(OnCollectionViewCurrentChanged);
                }
            }
        }

        void OnCollectionViewCurrentChanged(object sender, EventArgs e) {
            SyncFocusedRowHandle();
        }
        void SyncFocusedRowHandle() {
            if(CanSyncCurrentRow())
                gridControl.View.FocusedRow = collectionView.CurrentItem;
        }
        void OnGridControlFocusedRowChanged(object sender, FocusedRowChangedEventArgs e) {
            if(CanSyncCurrentRow() && gridControl.View.FocusedRowHandle != GridControl.InvalidRowHandle)
                collectionView.MoveCurrentTo(gridControl.View.FocusedRow);
        }
        bool CanSyncCurrentRow() {
            return gridControl != null && GetIsSynchronizedWithCurrentItem(gridControl);
        }
        void OnGroupDescriptionsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            if(gridControl == null)
                return;
            SyncGrouping();
        }
        void OnSortDescriptionsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            if(gridControl == null)
                return;
            SyncSorting();
        }
        void OnCollectionViewPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if(e.PropertyName == "Count")
                SyncFilter();
        }
        void SyncFilter() {
            if(gridControl == null)
                return;
            if(FilterMode == ModelFilterMode.FilterCriteria) {
                gridControl.FilterCriteria = FilterCriteria;
                gridControl.View.AllowColumnFiltering = true;
            } else {
                gridControl.FilterCriteria = null;
                gridControl.RefreshData();
                gridControl.View.AllowColumnFiltering = false;
            }
        }
        void ConnectToGridControl(GridControl gridControl) {
            if(this.gridControl != null) {
                this.gridControl.CustomRowFilter -= new RowFilterEventHandler(gridControl_CustomRowFilter);
                this.gridControl.SortInfo.CollectionChanged -= new NotifyCollectionChangedEventHandler(OnSortInfoCollectionChanged);
                this.gridControl.View.FocusedRowChanged -= new FocusedRowChangedEventHandler(OnGridControlFocusedRowChanged);
                TypeDescriptor.GetProperties(typeof(GridControl))[GridControl.FilterCriteriaProperty.Name].RemoveValueChanged(gridControl, OnGridControlFilterCriteriaChanged);
            }
            this.gridControl = gridControl;
            if(gridControl == null)
                return;
            gridControl.AutoPopulateColumns = AutoPopulateColumns;
            if(CollectionView == null)
                return;
            gridControl.DataSource = CollectionView.SourceCollection;
            gridControl.BeginInit();
            try {
                gridControl.CustomRowFilter += new RowFilterEventHandler(gridControl_CustomRowFilter);
                gridControl.SortInfo.CollectionChanged += new NotifyCollectionChangedEventHandler(OnSortInfoCollectionChanged);
                gridControl.View.FocusedRowChanged += new FocusedRowChangedEventHandler(OnGridControlFocusedRowChanged);
                TypeDescriptor.GetProperties(typeof(GridControl))[GridControl.FilterCriteriaProperty.Name].AddValueChanged(gridControl, OnGridControlFilterCriteriaChanged);
                SyncSorting();
                SyncGrouping();
                SyncFocusedRowHandle();
                SyncFilter();
                gridControl.View.AllowGrouping = collectionView.CanGroup;
                gridControl.View.AllowSorting = collectionView.CanSort;
            } finally {
                gridControl.EndInit();
            }
        }
        void OnGridControlFilterCriteriaChanged(object sender, EventArgs e) {
            FilterCriteria = gridControl.FilterCriteria;
        }
        void OnSortInfoCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            if(syncGroupSortLocker.IsLocked)
                return;
            syncGroupSortLocker.DoLockedAction(SyncSortInfo);
        }
        void SyncSortInfo() {
            if(CollectionView == null)
                return;
            if(CollectionView.SortDescriptions != null) {
                CollectionView.SortDescriptions.Clear();
                CollectionView.GroupDescriptions.Clear();
                for(int i = 0; i < gridControl.GroupCount; i++) {
                    GridSortInfo info = gridControl.SortInfo[i];
                    CollectionView.GroupDescriptions.Add(new SortPropertyGroupDescription() { PropertyName = info.FieldName, SortDirection = info.SortOrder });
                }
                for(int i = gridControl.GroupCount; i < gridControl.SortInfo.Count; i++) {
                    GridSortInfo info = gridControl.SortInfo[i];
                    CollectionView.SortDescriptions.Add(new SortDescription(info.FieldName, info.SortOrder));
                }
            }
        }
        void SyncSorting() {
            if(syncGroupSortLocker.IsLocked)
                return;
            syncGroupSortLocker.DoLockedAction(delegate() {
                if(CollectionView.SortDescriptions != null) {
                    gridControl.SortInfo.BeginUpdate();
                    try {
                        gridControl.ClearSorting();
                        foreach(SortDescription sortDescription in CollectionView.SortDescriptions) {
                            gridControl.SortBy(gridControl.Columns[sortDescription.PropertyName], sortDescription.Direction == ListSortDirection.Ascending ? ColumnSortOrder.Ascending : ColumnSortOrder.Descending);
                        }
                    } finally {
                        gridControl.SortInfo.EndUpdate();
                    }
                }
            });
        }
        void SyncGrouping() {
            if(syncGroupSortLocker.IsLocked)
                return;
            syncGroupSortLocker.DoLockedAction(delegate() {
                if(CollectionView.GroupDescriptions != null) {
                    gridControl.SortInfo.BeginUpdate();
                    try {
                        gridControl.ClearGrouping();
                        foreach(PropertyGroupDescription groupDescription in CollectionView.GroupDescriptions) {
                            SortPropertyGroupDescription sortGroupDescription = groupDescription as SortPropertyGroupDescription;
                            ListSortDirection sortDirection = sortGroupDescription != null ? sortGroupDescription.SortDirection : ListSortDirection.Ascending;
                            gridControl.GroupBy(gridControl.Columns[groupDescription.PropertyName], sortDirection == ListSortDirection.Ascending ? ColumnSortOrder.Ascending : ColumnSortOrder.Descending);
                        }
                    } finally {
                        gridControl.SortInfo.EndUpdate();
                    }
                }
            });
        }
        void gridControl_CustomRowFilter(object sender, RowFilterEventArgs e) {
            if(CollectionView.Filter == null || FilterMode == ModelFilterMode.FilterCriteria)
                return;
            int rowHandle = gridControl.GetRowHandleByListIndex(e.ListSourceRowIndex);
            e.Visible = CollectionView.Filter(gridControl.GetRow(rowHandle));
            e.Handled = true;
        }
    }
}
