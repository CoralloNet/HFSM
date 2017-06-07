<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Form1
	Inherits System.Windows.Forms.Form

	'Form esegue l'override del metodo Dispose per pulire l'elenco dei componenti.
	<System.Diagnostics.DebuggerNonUserCode()>
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
	<System.Diagnostics.DebuggerStepThrough()>
	Private Sub InitializeComponent()
		Me.DockPanel1 = New WeifenLuo.WinFormsUI.Docking.DockPanel()
		Me.SuspendLayout()
		'
		'DockPanel1
		'
		Me.DockPanel1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None
		Me.DockPanel1.Dock = System.Windows.Forms.DockStyle.Fill
		Me.DockPanel1.DocumentStyle = WeifenLuo.WinFormsUI.Docking.DocumentStyle.DockingWindow
		Me.DockPanel1.Location = New System.Drawing.Point(0, 0)
		Me.DockPanel1.MinimumSize = New System.Drawing.Size(50, 0)
		Me.DockPanel1.Name = "DockPanel1"
		Me.DockPanel1.Size = New System.Drawing.Size(763, 653)
		Me.DockPanel1.TabIndex = 6
		'
		'Form1
		'
		Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
		Me.ClientSize = New System.Drawing.Size(763, 653)
		Me.Controls.Add(Me.DockPanel1)
		Me.Name = "Form1"
		Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
		Me.Text = "HFSM"
		Me.WindowState = System.Windows.Forms.FormWindowState.Maximized
		Me.ResumeLayout(False)

	End Sub

	Friend WithEvents DockPanel1 As WeifenLuo.WinFormsUI.Docking.DockPanel
End Class
