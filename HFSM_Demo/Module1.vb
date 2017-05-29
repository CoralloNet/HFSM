
Module Module1

	Sub Main()
		Dim FsmName As String = "process"

		'Debug string to console
		System.Console.WriteLine(Date.Now.ToString("yyyyMMdd-HHmmss.fff") & " Load from file")

		'Create FSM from file definition
		Dim VtStates() As String = {}
		Dim VtEvents() As String = {}
		Dim NewFsm As HFSM_lib.HFSM(Of String, String) = HFSM_lib.HFSM(Of String, String).LoadFromFile(
			FsmName & ".fsm", New MyProcessCallback(), VtStates, VtEvents
		)

		If NewFsm Is Nothing Then
			System.Console.WriteLine(Date.Now.ToString("yyyyMMdd-HHmmss.fff") & " Error loading FSM")
			PressKeyToExit()
		End If

		'Debug string to console
		System.Console.WriteLine(Date.Now.ToString("yyyyMMdd-HHmmss.fff") & " Start")

		'Run FSM until end
		Do Until NewFsm.Evolve() = "PROCESS_TERMINATED"
		Loop

		'Export FSM in other languages
		With System.IO.File.CreateText(FsmName & ".dot")
			.Write(NewFsm.ToGraphviz(""))
			.Close()
		End With
		With System.IO.File.CreateText(FsmName & ".h")
			.Write(NewFsm.ToC_FSMHeader(""))
			.Close()
		End With
		With System.IO.File.CreateText(FsmName & ".c")
			.Write(NewFsm.ToC_FSMSource(""))
			.Close()
		End With

		'Convert graphviz drwaing in pdf and open it
		System.Diagnostics.Process.Start("C:\Organize\Utility\graphviz\bin\dot.exe", "-Tpdf -o " & FsmName & ".pdf " & FsmName & ".dot").WaitForExit()
		System.Diagnostics.Process.Start("cmd.exe", "/c start " & FsmName & ".pdf")

		PressKeyToExit()

	End Sub

	Private Sub PressKeyToExit()
		System.Console.WriteLine(Date.Now.ToString("yyyyMMdd-HHmmss.fff") & " Press any key to exit")
		System.Console.ReadKey()
		End

	End Sub

	Private Class MyProcessCallback
		Friend Function ProcessInit(ByVal ExecuteState As String, ByRef Parameters() As Object) As String
			System.Console.WriteLine(Date.Now.ToString("yyyyMMdd-HHmmss.fff") & " [" & ExecuteState.ToString() & "] MyInit")
			Return ""
		End Function
		Friend Function ProcessReady(ByVal ExecuteState As String, ByRef Parameters() As Object) As String
			System.Console.WriteLine(Date.Now.ToString("yyyyMMdd-HHmmss.fff") & " [" & ExecuteState.ToString() & "] MyReady")
			Return "EVENT_DISPATCH"
		End Function
		Friend Function ProcessRunning(ByVal ExecuteState As String, ByRef Parameters() As Object) As String
			System.Console.WriteLine(Date.Now.ToString("yyyyMMdd-HHmmss.fff") & " [" & ExecuteState.ToString() & "] MyRunning")
			Return "EVENT_EXIT"
		End Function
		Friend Function ProcessWaiting(ByVal ExecuteState As String, ByRef Parameters() As Object) As String
			System.Console.WriteLine(Date.Now.ToString("yyyyMMdd-HHmmss.fff") & " [" & ExecuteState.ToString() & "] MyWaiting")
			Return "EVENT_IO_READY"
		End Function

		'SUBFSM: PROCESS_RUN_DETAIL
		Friend Function ProcessDetailInit(ByVal ExecuteState As String, ByRef Parameters() As Object) As String
			System.Console.WriteLine(Date.Now.ToString("yyyyMMdd-HHmmss.fff") & " [" & ExecuteState.ToString() & "] MyProcessDetailInit")
			Return ""
		End Function
		Friend Function ProcessDetailRunning(ByVal ExecuteState As String, ByRef Parameters() As Object) As String
			System.Console.WriteLine(Date.Now.ToString("yyyyMMdd-HHmmss.fff") & " [" & ExecuteState.ToString() & "] MyProcessDetailRunning")
			Return "EVENT_EXIT"
		End Function

	End Class

End Module
