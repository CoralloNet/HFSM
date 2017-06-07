<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ToolEditor
	Inherits WeifenLuo.WinFormsUI.Docking.DockContent

	'Form esegue l'override del metodo Dispose per pulire l'elenco dei componenti.
	<System.Diagnostics.DebuggerNonUserCode()> _
	Protected Overrides Sub Dispose(ByVal disposing As Boolean)
		Try
			If disposing AndAlso components IsNot Nothing Then
				components.Dispose()
			End If
		Finally
			MyBase.Dispose(disposing)
		End Try
	End Sub

	'Richiesto da Progettazione Windows Form
	Private components As System.ComponentModel.IContainer

	'NOTA: la procedura che segue è richiesta da Progettazione Windows Form
	'Può essere modificata in Progettazione Windows Form.  
	'Non modificarla mediante l'editor del codice.
	<System.Diagnostics.DebuggerStepThrough()> _
	Private Sub InitializeComponent()
		Me.TxtFSM = New System.Windows.Forms.TextBox()
		Me.SuspendLayout()
		'
		'TxtFSM
		'
		Me.TxtFSM.Dock = System.Windows.Forms.DockStyle.Fill
		Me.TxtFSM.Font = New System.Drawing.Font("Courier New", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.TxtFSM.Location = New System.Drawing.Point(0, 0)
		Me.TxtFSM.Multiline = True
		Me.TxtFSM.Name = "TxtFSM"
		Me.TxtFSM.ScrollBars = System.Windows.Forms.ScrollBars.Both
		Me.TxtFSM.Size = New System.Drawing.Size(435, 315)
		Me.TxtFSM.TabIndex = 1
		'
		'ToolEditor
		'
		Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
		Me.ClientSize = New System.Drawing.Size(435, 315)
		Me.Controls.Add(Me.TxtFSM)
		Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
		Me.MaximizeBox = False
		Me.MinimizeBox = False
		Me.Name = "ToolEditor"
		Me.ShowInTaskbar = False
		Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
		Me.Text = "ToolEditor"
		Me.ResumeLayout(False)
		Me.PerformLayout()

	End Sub

	Friend WithEvents TxtFSM As System.Windows.Forms.TextBox
End Class
