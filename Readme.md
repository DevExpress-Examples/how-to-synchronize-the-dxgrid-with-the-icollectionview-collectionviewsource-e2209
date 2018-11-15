<!-- default file list -->
*Files to look at*:

* [MainWindow.xaml](./CS/MainWindow.xaml)
* [MainWindow.xaml.cs](./CS/MainWindow.xaml.cs)
* [ViewModel.cs](./CS/ViewModel.cs) (VB: [WindowStart.xaml](./VB/GridControlViewModel/WindowStart.xaml))
<!-- default file list end -->
# How to synchronize the DXGrid with the ICollectionView (CollectionViewSource)


<p>The following example illustrates how to synchronize the DXGrid with the ICollectionView.</p>


<h3>Description</h3>

<p>Since the release of v2011 vol 2, the DXGrid supports ICollectionView binding out of the box. In previous versions, you had to manually create helper classes to synchronize the grid and collection view. Now this is not required. Assign the source collection to the grid&#39;s ItemSource property and enable the TableView&#39;s IsSynchronizedWithCurrentItem option. The grid automatically synchronizes its grouping, filtering, sorting, current item and can directly change the underlying collection.</p>

<br/>


