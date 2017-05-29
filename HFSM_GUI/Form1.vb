
Public Class Form1

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
		Dim NewFSM As HFSM_lib.HFSM(Of String, String)

		TmrEdit.Stop()

		'Generate new FSM
		NewFSM = HFSM_lib.HFSM(Of String, String).LoadFromString(TxtFSM.Text, Nothing, Nothing, Nothing)
		If NewFSM Is Nothing Then Exit Sub

		'Print C source code to windows
		TxtC.Text = NewFSM.ToC_FSMSource(System.Text.Encoding.ASCII.GetChars({&HD}) & System.Text.Encoding.ASCII.GetChars({&HA}))

	End Sub

End Class

