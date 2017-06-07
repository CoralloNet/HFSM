
Public Class Form1

	Private Sub ToolEditor_TextChanged(ByVal NewText As String) Handles PrvToolEditor.TextChanged
		Dim NewFSM As HFSM_lib.HFSM(Of String, String)

		'Generate new FSM
		NewFSM = HFSM_lib.HFSM(Of String, String).LoadFromString(NewText, Nothing, Nothing, Nothing)
		If NewFSM Is Nothing Then Exit Sub

		'Print C source code to windows
		Dim StC As String

		StC = NewFSM.ToC_FSMSource(System.Text.Encoding.ASCII.GetChars({&HD}) & System.Text.Encoding.ASCII.GetChars({&HA}))
		PrvToolOutC.SetText(StC)

	End Sub

#Region " LAYOUT MANAGEMENT "

	Friend WithEvents PrvToolEditor As ToolEditor
	Friend PrvToolOutC As ToolOutC

	Private LayoutToolEditor As Boolean
	Private LayoutToolOutC As Boolean

	Private Sub Form1_Load(sender As Object, e As System.EventArgs) Handles MyBase.Load
		PrvToolEditor = New ToolEditor()
		PrvToolOutC = New ToolOutC()

		Try
			DockPanel1.LoadFromXml("layout.xml", New WeifenLuo.WinFormsUI.Docking.DeserializeDockContent(AddressOf GetContentFromPersistString))

		Catch ex As System.Exception
			'Layout predefinito nel caso di file mancante
			PrvToolEditor.Show(DockPanel1, WeifenLuo.WinFormsUI.Docking.DockState.DockLeft)
			PrvToolOutC.Show(DockPanel1, WeifenLuo.WinFormsUI.Docking.DockState.DockRight)
		End Try

		'Layout predefinito nel caso di pannello non visibile
		If LayoutToolEditor = False Then PrvToolEditor.Show(DockPanel1, WeifenLuo.WinFormsUI.Docking.DockState.DockLeft)
		If LayoutToolOutC = False Then PrvToolOutC.Show(DockPanel1, WeifenLuo.WinFormsUI.Docking.DockState.DockRight)

	End Sub

	Private Sub Form1_FormClosing(sender As Object, e As System.Windows.Forms.FormClosingEventArgs) Handles MyBase.FormClosing
		DockPanel1.SaveAsXml("layout.xml")

	End Sub

	Private Function GetContentFromPersistString(ByVal persistString As String) As WeifenLuo.WinFormsUI.Docking.IDockContent

		Select Case persistString
			Case PrvToolEditor.GetType().ToString()
				LayoutToolEditor = True
				Return PrvToolEditor
		End Select

		Return Nothing

	End Function

#End Region

End Class

