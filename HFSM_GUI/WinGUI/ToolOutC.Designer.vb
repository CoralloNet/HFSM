<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ToolOutC
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
		Me.TxtC = New System.Windows.Forms.TextBox()
		Me.SuspendLayout()
		'
		'TxtC
		'
		Me.TxtC.Dock = System.Windows.Forms.DockStyle.Fill
		Me.TxtC.Font = New System.Drawing.Font("Courier New", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.TxtC.Location = New System.Drawing.Point(0, 0)
		Me.TxtC.Multiline = True
		Me.TxtC.Name = "TxtC"
		Me.TxtC.ScrollBars = System.Windows.Forms.ScrollBars.Both
		Me.TxtC.Size = New System.Drawing.Size(435, 315)
		Me.TxtC.TabIndex = 2
		'
		'ToolOutC
		'
		Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
		Me.ClientSize = New System.Drawing.Size(435, 315)
		Me.Controls.Add(Me.TxtC)
		Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
		Me.MaximizeBox = False
		Me.MinimizeBox = False
		Me.Name = "ToolOutC"
		Me.ShowInTaskbar = False
		Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
		Me.Text = "ToolOutC"
		Me.ResumeLayout(False)
		Me.PerformLayout()

	End Sub

	Friend WithEvents TxtC As System.Windows.Forms.TextBox
End Class
