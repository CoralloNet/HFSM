
Public Class ToolOutC

	Friend Sub SetText(ByVal StTextC As String)
		TxtC.Invoke(
			Sub()
				TxtC.Text = StTextC
			End Sub
			)

	End Sub

End Class
