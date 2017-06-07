
Public Class ToolEditor

	Friend Shadows Event TextChanged(ByVal NewText As String)

	Private WithEvents TmrEdit As System.Windows.Forms.Timer = New System.Windows.Forms.Timer() With {
			.Enabled = False,
			.Interval = 1000
		}

	Private Sub TxtSource_TextChanged(sender As Object, e As System.EventArgs) Handles TxtFSM.TextChanged
		'Reset the timer
		TmrEdit.Stop()
		TmrEdit.Start()

	End Sub

	Private Sub Tmr_Elapsed() Handles TmrEdit.Tick
		TmrEdit.Stop()
		RaiseEvent TextChanged(TxtFSM.Text)

	End Sub

End Class
