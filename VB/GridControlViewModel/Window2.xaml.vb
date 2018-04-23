Imports Microsoft.VisualBasic
Imports System.Windows
Imports System.Windows.Data
Imports System.Collections

Namespace GridControlViewModel
	''' <summary>
	''' Interaction logic for Window2.xaml
	''' </summary>
	Partial Public Class Window2
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
			UpdateFilter()
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
