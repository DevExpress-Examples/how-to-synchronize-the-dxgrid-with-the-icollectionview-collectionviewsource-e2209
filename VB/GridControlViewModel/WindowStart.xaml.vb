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
Imports System.Windows.Shapes
Imports System.Collections

Namespace GridControlViewModel
	''' <summary>
	''' Interaction logic for WindowStart.xaml
	''' </summary>
	Partial Public Class WindowStart
		Inherits Window
		Public Sub New()
			InitializeComponent()
		End Sub
		Public Shared Function CreateList() As IList
			Dim list As New List(Of TestData)()
			For i As Integer = 0 To 99
				list.Add(New TestData() With {.Number1 = i, .Number2 = i * 10, .Text1 = "row " & i, .Text2 = "ROW " & i})
			Next i
			Return list
		End Function

		Private Sub button1_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
			CType(New Window1(), Window1).ShowDialog()
		End Sub

		Private Sub button2_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
			CType(New Window2(), Window2).ShowDialog()
		End Sub

	End Class
	Public Class TestData
		Private privateNumber1 As Integer
		Public Property Number1() As Integer
			Get
				Return privateNumber1
			End Get
			Set(ByVal value As Integer)
				privateNumber1 = value
			End Set
		End Property
		Private privateNumber2 As Integer
		Public Property Number2() As Integer
			Get
				Return privateNumber2
			End Get
			Set(ByVal value As Integer)
				privateNumber2 = value
			End Set
		End Property
		Private privateText1 As String
		Public Property Text1() As String
			Get
				Return privateText1
			End Get
			Set(ByVal value As String)
				privateText1 = value
			End Set
		End Property
		Private privateText2 As String
		Public Property Text2() As String
			Get
				Return privateText2
			End Get
			Set(ByVal value As String)
				privateText2 = value
			End Set
		End Property
	End Class
End Namespace
