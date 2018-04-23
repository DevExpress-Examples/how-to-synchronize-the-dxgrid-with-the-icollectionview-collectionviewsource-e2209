Imports Microsoft.VisualBasic
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Data
Imports System.Windows.Documents
Imports System.Windows.Input
Imports System.Windows.Media
Imports System.Windows.Media.Imaging
Imports System.Windows.Navigation
Imports System.Windows.Shapes
Imports System.ComponentModel
Imports DevExpress.Xpf.Grid
Imports DevExpress.Data
Imports System.Collections.Specialized
Imports DevExpress.Xpf.Core
Imports System.Collections
Imports System.Windows.Threading
Imports System.Threading

Namespace GridControlViewModel
	''' <summary>
	''' Interaction logic for Window3.xaml
	''' </summary>
	Partial Public Class Window3
		Inherits Window
		Private view As ListCollectionView
		Public Sub New()
			InitializeComponent()
			Dim list As IList = WindowStart.CreateList()
			view = New ListCollectionView(list)
			DataContext = view
			AddHandler filterComboBox.SelectionChanged, AddressOf ComboBox_SelectionChanged
		End Sub

		Private Sub ComboBox_SelectionChanged(ByVal sender As Object, ByVal e As System.Windows.Controls.SelectionChangedEventArgs)
			Dispatcher.BeginInvoke(New ThreadStart(AddressOf UpdateFilter), DispatcherPriority.Background)
		End Sub

		Private Sub UpdateFilter()
			Select Case filterComboBox.SelectedIndex
				Case 0
					view.Filter = Nothing
				Case 1
					view.Filter = AddressOf EvenFilter
				Case 2
					view.Filter = AddressOf OddFilter
				Case Else
			End Select
		End Sub
		Private Function EvenFilter(ByVal obj As Object) As Boolean
			Dim testData As TestData = CType(obj, TestData)
			Return testData.Number1 Mod 2 = 0
		End Function
		Private Function OddFilter(ByVal obj As Object) As Boolean
			Dim testData As TestData = CType(obj, TestData)
			Return testData.Number1 Mod 2 = 1
		End Function
	End Class
End Namespace
