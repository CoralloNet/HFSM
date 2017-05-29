<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
	Inherits System.Windows.Forms.Form

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
		Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
		Me.TxtC = New System.Windows.Forms.TextBox()
		Me.TxtFSM = New System.Windows.Forms.TextBox()
		Me.TableLayoutPanel1.SuspendLayout()
		Me.SuspendLayout()
		'
		'TableLayoutPanel1
		'
		Me.TableLayoutPanel1.ColumnCount = 2
		Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
		Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
		Me.TableLayoutPanel1.Controls.Add(Me.TxtC, 0, 0)
		Me.TableLayoutPanel1.Controls.Add(Me.TxtFSM, 0, 0)
		Me.TableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill
		Me.TableLayoutPanel1.Location = New System.Drawing.Point(0, 0)
		Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
		Me.TableLayoutPanel1.RowCount = 1
		Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
		Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
		Me.TableLayoutPanel1.Size = New System.Drawing.Size(763, 653)
		Me.TableLayoutPanel1.TabIndex = 0
		'
		'TxtC
		'
		Me.TxtC.Dock = System.Windows.Forms.DockStyle.Fill
		Me.TxtC.Font = New System.Drawing.Font("Courier New", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.TxtC.Location = New System.Drawing.Point(384, 3)
		Me.TxtC.Multiline = True
		Me.TxtC.Name = "TxtC"
		Me.TxtC.ScrollBars = System.Windows.Forms.ScrollBars.Both
		Me.TxtC.Size = New System.Drawing.Size(376, 647)
		Me.TxtC.TabIndex = 1
		'
		'TxtFSM
		'
		Me.TxtFSM.Dock = System.Windows.Forms.DockStyle.Fill
		Me.TxtFSM.Font = New System.Drawing.Font("Courier New", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.TxtFSM.Location = New System.Drawing.Point(3, 3)
		Me.TxtFSM.Multiline = True
		Me.TxtFSM.Name = "TxtFSM"
		Me.TxtFSM.ScrollBars = System.Windows.Forms.ScrollBars.Both
		Me.TxtFSM.Size = New System.Drawing.Size(375, 647)
		Me.TxtFSM.TabIndex = 0
		'
		'Form1
		'
		Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
		Me.ClientSize = New System.Drawing.Size(763, 653)
		Me.Controls.Add(Me.TableLayoutPanel1)
		Me.Name = "Form1"
		Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
		Me.Text = "HFSM"
		Me.WindowState = System.Windows.Forms.FormWindowState.Maximized
		Me.TableLayoutPanel1.ResumeLayout(False)
		Me.TableLayoutPanel1.PerformLayout()
		Me.ResumeLayout(False)

	End Sub

	Friend WithEvents TableLayoutPanel1 As System.Windows.Forms.TableLayoutPanel
	Friend WithEvents TxtFSM As System.Windows.Forms.TextBox
	Friend WithEvents TxtC As System.Windows.Forms.TextBox
End Class
