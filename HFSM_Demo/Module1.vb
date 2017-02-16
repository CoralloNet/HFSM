
Module Module1

	Sub Main()

		'Debug string to console
		System.Console.WriteLine(Date.Now.ToString("yyyyMMdd-HHmmss.fff") & " Start")

		'Create FSM from file definition
		Dim VtStates() As String = {}
		Dim VtEvents() As String = {}
		Dim NewFsm As HFSM_lib.HFSM(Of String, String) = HFSM_lib.HFSM(Of String, String).LoadFromFile(
			"process.fsm", New MyProcessCallback(), VtStates, VtEvents
		)

		'Run FSM until end
		Do Until NewFsm.Evolve() = "PROCESS_TERMINATED"
		Loop

		'Export FSM in other languages
		With System.IO.File.CreateText("process.dot")
			.Write(NewFsm.ToGraphviz(""))
			.Close()
		End With
		With System.IO.File.CreateText("process.h")
			.Write(NewFsm.ToC_FSMHeader(""))
			.Close()
		End With
		With System.IO.File.CreateText("process.c")
			.Write(NewFsm.ToC_FSMSource(""))
			.Close()
		End With

		'Convert graphviz drwaing in pdf and open it
		System.Diagnostics.Process.Start("C:\Organize\Utility\graphviz\bin\dot.exe", "-Tpdf -o process.pdf process.dot").WaitForExit()
		System.Diagnostics.Process.Start("cmd.exe", "/c start process.pdf")

		PressKeyToExit()

	End Sub

	Private Sub PressKeyToExit()
		System.Console.WriteLine(Date.Now.ToString("yyyyMMdd-HHmmss.fff") & " Premi un tasto per uscire")
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
		Friend Function ProcessDetailReady(ByVal ExecuteState As String, ByRef Parameters() As Object) As String
			System.Console.WriteLine(Date.Now.ToString("yyyyMMdd-HHmmss.fff") & " [" & ExecuteState.ToString() & "] MyProcessDetailReady")
			Return "EVENT_EXIT"
		End Function
	End Class

End Module
