
# HFSM - Final State Machine
Put FSM into your .NET project
Automatically generate C source and GraphViz / GoJS documentation

You can write your own state machine in .NET or import it from meta language file.

## 1 Describe your FSM
First step is to describe your FSM.

### 1.1 Write directly in .NET
Define states and events:

	Private Enum EnStates
		PROCESS_INIT
		PROCESS_READY
		PROCESS_RUNNING
		PROCESS_WAITING
		PROCESS_TERMINATED
	End Enum

	Private Enum EnEvents
		EVENT_IO_READY
		EVENT_INTERRUPT
		EVENT_IO_WAIT
		EVENT_DISPATCH
		EVENT_EXIT
	End Enum

Write function callback:

	Friend Function ProcessInit(ByVal ExecuteState As EnStates, ByRef Parameters() As Object) As EnEvents
		System.Console.WriteLine(Date.Now.ToString("yyyyMMdd-HHmmss.fff") & " [" & ExecuteState.ToString() & "] MyInit")
		Return EnEvents.EVENT_DISPATCH
	End Function
	Friend Function ProcessReady(ByVal ExecuteState As EnStates, ByRef Parameters() As Object) As EnEvents
		System.Console.WriteLine(Date.Now.ToString("yyyyMMdd-HHmmss.fff") & " [" & ExecuteState.ToString() & "] MyReady")
		Return EnEvents.EVENT_DISPATCH
	End Function
	Friend Function ProcessRunning(ByVal ExecuteState As EnStates, ByRef Parameters() As Object) As EnEvents
		System.Console.WriteLine(Date.Now.ToString("yyyyMMdd-HHmmss.fff") & " [" & ExecuteState.ToString() & "] MyRunning")
		Return EnEvents.EVENT_EXIT
	End Function
	Friend Function ProcessWaiting(ByVal ExecuteState As EnStates, ByRef Parameters() As Object) As EnEvents
		System.Console.WriteLine(Date.Now.ToString("yyyyMMdd-HHmmss.fff") & " [" & ExecuteState.ToString() & "] MyWaiting")
		Return EnEvents.EVENT_IO_READY
	End Function

And create your FSM:

	Dim MyFSM As ClsFSM(Of EnStates, EnEvents)

	MyFSM = New ClsFSM(Of EnStates, EnEvents)("ProcessDemo", EnStates.PROCESS_INIT)

	MyFSM.Configure(EnStates.PROCESS_INIT) _
		.OnExecute(AddressOf ProcessInit) _
		.DefaultTransaction(EnStates.PROCESS_READY)

	MyFSM.Configure(EnStates.PROCESS_READY) _
		.OnExecute(AddressOf ProcessReady) _
		.Transaction(EnEvents.EVENT_DISPATCH, EnStates.PROCESS_RUNNING)

	MyFSM.Configure(EnStates.PROCESS_RUNNING) _
		.OnExecute(AddressOf ProcessRunning) _
		.Transaction(EnEvents.EVENT_INTERRUPT, EnStates.PROCESS_READY) _
		.Transaction(EnEvents.EVENT_IO_WAIT, EnStates.PROCESS_WAITING) _
		.Transaction(EnEvents.EVENT_EXIT, EnStates.PROCESS_TERMINATED)

	MyFSM.Configure(EnStates.PROCESS_WAITING) _
		.OnExecute(AddressOf ProcessWaiting) _
		.Transaction(EnEvents.EVENT_IO_READY, EnStates.PROCESS_READY)

### 1.2 Import from meta-language file
First of all, you must create a file like this:

process.fsm

	FSM: Process
	InitialState: PROCESS_INIT

	FsmHeader:
	FsmHeader: #include "myInclude.h"
	FsmHeader:

	State: PROCESS_INIT
		OnExecute: ProcessInit
		DefaultTransaction: PROCESS_READY

	State: PROCESS_READY
		OnExecute: ProcessReady
		Transaction On EVENT_DISPATCH: PROCESS_RUNNING

	State: PROCESS_RUNNING
		OnExecute: ProcessRunning
		Transaction On EVENT_INTERRUPT: PROCESS_READY
		Transaction On EVENT_IO_WAIT: PROCESS_WAITING
		Transaction On EVENT_EXIT: PROCESS_TERMINATED

	State: PROCESS_WAITING
		OnExecute: ProcessWaiting
		Transaction On EVENT_IO_READY: PROCESS_READY

Write function callback:

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
	End Class

And than you can load your FSM:

	Dim VtStates() As String = {}
	Dim VtEvents() As String = {}
	Dim NewFsm As HFSM(Of String, String) = HFSM(Of String, String).LoadFromFile(
		"process.fsm", New MyProcessCallback(), VtStates, VtEvents
	)

Optionally you can check if all callback will be resolved runtime:

	If NewFsm.CheckCallback() = False Then
		System.Console.WriteLine("Wrong callback linking")
		PressKeyToExit()
	End If

## 2 Debug FSM
You can execute your FSM code:

	Do Until NewFsm.Evolve() = "PROCESS_TERMINATED"
	Loop

Each time the Evolve() function is called, the callback of the current state is called.

## 3 Export source code
The generated FSM can be exported in ohter languages.

### 3.1 Export C source code
Example of exporting C source code:

	With System.IO.File.CreateText("process.h")
		.Write(NewFsm.ToC_FSMHeader(""))
		.Close()
	End With
	With System.IO.File.CreateText("process.c")
		.Write(NewFsm.ToC_FSMSource(""))
		.Close()
	End With

The library will generate ready-to-build C rource code:

process.h

	typedef enum {
		PROCESS_INIT,
		PROCESS_READY,
		PROCESS_RUNNING,
		PROCESS_WAITING,
	} EnStates;

	typedef enum {
		EVENT_DISPATCH,
		EVENT_INTERRUPT,
		EVENT_IO_WAIT,
		EVENT_EXIT,
		EVENT_IO_READY,
	} EnEvents;

	void FSM_Process_Restart(void);
	EnStates FSM_Process(void * Parameters);

process.c

	#include "myInclude.h"


	static unsigned char StateEntry = 1;
	static EnStates CurrState;

	void FSM_Process_Restart(void){
		CurrState = PROCESS_INIT;
	}

	EnStates FSM_Process(void * Parameters){
		switch (CurrState){

			case PROCESS_INIT:{
				EnEvents TmpEvent;
				TmpEvent = FSM_Process_ProcessInit(Parameters);
				switch (TmpEvent){
					default:{ CurrState = PROCESS_READY; break; }
				}

				if (CurrState != PROCESS_INIT){
					StateEntry = 1;
				}

				break;
			}

			case PROCESS_READY:{
				EnEvents TmpEvent;
				TmpEvent = FSM_Process_ProcessReady(Parameters);
				switch (TmpEvent){
					case EVENT_DISPATCH:{ CurrState = PROCESS_RUNNING; break; }
				}

				if (CurrState != PROCESS_READY){
					StateEntry = 1;
				}

				break;
			}

			case PROCESS_RUNNING:{
				EnEvents TmpEvent;
				TmpEvent = FSM_Process_ProcessRunning(Parameters);
				switch (TmpEvent){
					case EVENT_INTERRUPT:{ CurrState = PROCESS_READY; break; }
					case EVENT_IO_WAIT:{ CurrState = PROCESS_WAITING; break; }
					case EVENT_EXIT:{ CurrState = PROCESS_TERMINATED; break; }
				}

				if (CurrState != PROCESS_RUNNING){
					StateEntry = 1;
				}

				break;
			}

			case PROCESS_WAITING:{
				EnEvents TmpEvent;
				TmpEvent = FSM_Process_ProcessWaiting(Parameters);
				switch (TmpEvent){
					case EVENT_IO_READY:{ CurrState = PROCESS_READY; break; }
				}

				if (CurrState != PROCESS_WAITING){
					StateEntry = 1;
				}

				break;
			}

		}
		return CurrState;
	}

## 4 Export documentation

### 4.1 Documentation for Graphviz

	With System.IO.File.CreateText("process.dot")
		.Write(NewFsm.ToGraphviz(""))
		.Close()
	End With

If you want you can convert the dot file in pdf directly from .NET:

	System.Diagnostics.Process.Start("C:\Programs\graphviz\bin\dot.exe", "-Tpdf -o process.pdf process.dot")
	System.Diagnostics.Process.Start("cmd.exe", "/c start process.pdf")

### 4.2 Documentation for GoJS

	With System.IO.File.CreateText("process.js")
		.Write(NewFsm.ToGoJs(""))
		.Close()
	End With

## Thanks
Thanks to 
[simple state machine](http://simplestatemachine.codeplex.com/)
[stateless](https://github.com/dotnet-state-machine/stateless)

