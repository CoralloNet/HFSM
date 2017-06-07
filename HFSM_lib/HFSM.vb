
#Const GENERATE_EVENTS = 0

'FSM Manager
'	Put FSM into your .NET project
'	Automatically generate C source and GraphViz / GoJS documentation
'
'	Original idea from
'		http://simplestatemachine.codeplex.com/
'		and
'		https://github.com/dotnet-state-machine/stateless
'
'		#----------------------------------------
'		workflow "Telephone Call"
'
'		trigger @CallDialed
'		trigger @HungUp
'		trigger @CallConnected
'		trigger @LeftMessage
'
'		trigger @PlacedOnHold:
'					on_event @PlayMuzak
'
'		trigger @TakenOffHold
'					 on_event @StopMuzak
'
'		trigger @PhoneHurledAgainstWall
'
'		state @OffHook:
'			when @CallDialed  >> @Ringing
'
'		state @Ringing:
'			when @HungUp  >> @OffHook
'			when @CallConnected  >> @Connected
'
'		state @Connected:
'			when @LeftMessage  >> @OffHook
'			when @HungUp   >> @OffHook
'			when @PlacedOnHold  >> @OnHold
'
'		state @OnHold:
'			when @TakenOffHold >> @Connected
'			when @HungUp  >> @OffHook
'			when @PhoneHurledAgainstWall >> @PhoneDestroyed
'
'		state @PhoneDestroyed
'		#----------------------------------------

Public Class HFSM(Of TState, TEvent)

	Friend Delegate Function VerifyCallback(ByVal Parameters() As Object) As Boolean
	Friend Delegate Sub EntryCallback(ByVal EntryState As TState)
	Friend Delegate Sub ExitCallback(ByVal ExitState As TState)
	Friend Delegate Function ExecuteCallback(ByVal ExecuteState As TState, ByRef Parameters() As Object) As TEvent

#If GENERATE_EVENTS Then
	Friend Event OnEntry(ByVal EntryState As TState)
	Friend Event OnExit(ByVal ExitState As TState)
	Friend Event OnExecute(ByVal ExecuteState As TState)
#End If

	Private VtStates As System.Collections.Generic.List(Of ClsState)
	Private CurrState As TState
	Private PrvInitialState As TState
	Private PrvFsmName As String
	Private PrvFsmHeader As String
	Private PrvClassCallback As Object
	Private VtSubFSM As System.Collections.Generic.List(Of HFSM(Of TState, TEvent))

	Friend Sub New(ByVal FsmName As String, ByVal InitialState As TState)
		PrvFsmName = FsmName
		VtStates = New System.Collections.Generic.List(Of ClsState)
		VtSubFSM = New System.Collections.Generic.List(Of HFSM(Of TState, TEvent))
		CurrState = InitialState
		PrvInitialState = InitialState

	End Sub

#Region " STATE MANAGEMENT "

	Private Class ClsTransaction
		Friend TrEvent As TEvent
		Friend TrStateEvent As TState
		Friend TrDestState As TState
		Friend TrVerifyCallback As VerifyCallback
	End Class

	Private Class ClsState
		Friend CurrState As TState
		Friend OnEntry As EntryCallback
		Friend OnExit As ExitCallback
		Friend OnExecute As ExecuteCallback
		Friend OnSubFsm As HFSM(Of TState, TEvent)
		Friend TransactionList As System.Collections.Generic.List(Of ClsTransaction)
		Friend DefaultTransaction As ClsTransaction

		Friend Sub New()
			TransactionList = New System.Collections.Generic.List(Of ClsTransaction)
			DefaultTransaction = Nothing
		End Sub

	End Class

	Private Function GetState(ByVal SearchState As TState) As ClsState
		Dim NewState As ClsState

		For Each NewState In VtStates
			If NewState.CurrState.Equals(SearchState) Then
				Return NewState
			End If
		Next

		NewState = New ClsState() With {
			.CurrState = SearchState
		}
		VtStates.Add(NewState)

		Return NewState

	End Function

#End Region

#Region " CONFIGURATION "

	Public Class StateConfiguration

		Friend ConfigState As TState
		Friend ParentFSM As HFSM(Of TState, TEvent)

		Friend Function OnEntry(ByVal FunctionCallback As EntryCallback) As StateConfiguration
			ParentFSM.GetState(ConfigState).OnEntry = FunctionCallback
			Return Me
		End Function
		Friend Function OnEntry(ByVal FunctionCallback As String, ByVal ClassCallback As Object) As StateConfiguration
			Try
				ParentFSM.GetState(ConfigState).OnEntry = TryCast(System.Delegate.CreateDelegate(GetType(EntryCallback), ClassCallback, FunctionCallback), EntryCallback)
			Catch
				System.Diagnostics.Debug.WriteLine("Cannot find OnEntry function '" & FunctionCallback & "'")
				Return Nothing
			End Try
			Return Me
		End Function

		Friend Function OnExit(ByVal FunctionCallback As ExitCallback) As StateConfiguration
			ParentFSM.GetState(Me.ConfigState).OnExit = FunctionCallback
			Return Me
		End Function
		Friend Function OnExit(ByVal FunctionCallback As String, ByVal ClassCallback As Object) As StateConfiguration
			Try
				ParentFSM.GetState(ConfigState).OnExit = TryCast(System.Delegate.CreateDelegate(GetType(ExitCallback), Nothing, FunctionCallback), ExitCallback)
			Catch
				System.Diagnostics.Debug.WriteLine("Cannot find OnExit function '" & FunctionCallback & "'")
				Return Nothing
			End Try
			Return Me
		End Function

		Friend Function OnExecute(ByVal FunctionCallback As ExecuteCallback) As StateConfiguration
			ParentFSM.GetState(Me.ConfigState).OnExecute = FunctionCallback
			Return Me
		End Function
		Friend Function OnExecute(ByVal FunctionCallback As String, ByVal ClassCallback As Object) As StateConfiguration
			If ClassCallback Is Nothing Then
				'Create a dummy callback, only to mantain function name for C source code generation
				Dim DummyCallback As New System.Reflection.Emit.DynamicMethod(FunctionCallback, GetType(TEvent), {GetType(TState), GetType(Object()).MakeByRefType()})
				With DummyCallback.GetILGenerator
					.Emit(System.Reflection.Emit.OpCodes.Ret)
				End With
				ParentFSM.GetState(ConfigState).OnExecute = TryCast(DummyCallback.CreateDelegate(GetType(ExecuteCallback)), ExecuteCallback)

			Else
				Try
					ParentFSM.GetState(ConfigState).OnExecute = TryCast(System.Delegate.CreateDelegate(GetType(ExecuteCallback), ClassCallback, FunctionCallback), ExecuteCallback)
				Catch
					System.Diagnostics.Debug.WriteLine("Cannot find OnExecute function '" & FunctionCallback & "'")
					Return Nothing
				End Try
			End If
			Return Me
		End Function

		Friend Function OnSubFSM(ByVal FsmCallback As HFSM(Of TState, TEvent)) As StateConfiguration
			ParentFSM.GetState(Me.ConfigState).OnSubFsm = FsmCallback
			Return Me
		End Function

		Friend Function Transaction(ByVal EventTrigger As TEvent, ByVal DestinationState As TState, Optional VerifyFunction As VerifyCallback = Nothing) As StateConfiguration
			With ParentFSM.GetState(Me.ConfigState).TransactionList
				.RemoveAll(Function(T) T.TrEvent.Equals(EventTrigger))
				.Add(New ClsTransaction With {
					 .TrEvent = EventTrigger,
					 .TrDestState = DestinationState,
					 .TrVerifyCallback = VerifyFunction
				})
			End With
			Return Me

		End Function
		'Used fot SubFSM transaction
		Friend Function StateTransaction(ByVal StateTrigger As TState, ByVal DestinationState As TState, Optional VerifyFunction As VerifyCallback = Nothing) As StateConfiguration
			With ParentFSM.GetState(Me.ConfigState).TransactionList
				.RemoveAll(Function(T) T.TrStateEvent.Equals(StateTrigger))
				.Add(New ClsTransaction With {
					 .TrStateEvent = StateTrigger,
					 .TrDestState = DestinationState,
					 .TrVerifyCallback = VerifyFunction
				})
			End With
			Return Me

		End Function

		Friend Function DefaultTransaction(ByVal DestinationState As TState) As StateConfiguration
			ParentFSM.GetState(Me.ConfigState).DefaultTransaction = New ClsTransaction With {
				.TrDestState = DestinationState
			}
			Return Me
		End Function

	End Class

	Public Function Configure(ByVal ConfigState As TState) As StateConfiguration
		Dim NewStateConfiguration As StateConfiguration

		NewStateConfiguration = New StateConfiguration() With {
			.ConfigState = ConfigState,
			.ParentFSM = Me
		}

		'Add new state, if necessary
		NewStateConfiguration.ParentFSM.GetState(ConfigState)

		Return NewStateConfiguration

	End Function

#End Region

#Region " EVENTS "

	Friend Sub Fire(ByVal EventTrigger As TEvent, Optional ByVal EventParameters() As Object = Nothing)
		Dim PrvState As ClsState
		Dim PrvTransaction As ClsTransaction

		PrvState = VtStates.Find(Function(T) T.CurrState.Equals(CurrState))
		If Not PrvState Is Nothing Then

			'Search a transaction
			If Not PrvState.OnSubFsm Is Nothing Then
				'SubFSM transaction
				PrvTransaction = PrvState.TransactionList.Find(Function(T) T.TrStateEvent.Equals(PrvState.OnSubFsm.CurrState))
			Else
				'Standard transaction
				PrvTransaction = PrvState.TransactionList.Find(Function(T) T.TrEvent.Equals(EventTrigger))
				If PrvTransaction Is Nothing Then
					PrvTransaction = PrvState.DefaultTransaction
				End If
			End If

			If Not PrvTransaction Is Nothing Then
				Dim ChangeState As Boolean

				'Verify if I must change state (or not)
				If PrvTransaction.TrVerifyCallback Is Nothing Then
					ChangeState = True
				Else
					If PrvTransaction.TrVerifyCallback(EventParameters) Then
						ChangeState = PrvTransaction.TrVerifyCallback(EventParameters)
					End If
				End If

				If ChangeState Then
					'Execute Exit callback
#If GENERATE_EVENTS Then
					RaiseEvent OnExit(CurrState)
#End If
					If Not PrvState.OnExit Is Nothing Then
						PrvState.OnExit(CurrState)
					End If

					'Change state
					CurrState = PrvTransaction.TrDestState

					'Execute Entry callback
#If GENERATE_EVENTS Then
					RaiseEvent OnEntry(CurrState)
#End If
					PrvState = VtStates.Find(Function(T) T.CurrState.Equals(CurrState))
					If Not PrvState Is Nothing Then
						If Not PrvState.OnEntry Is Nothing Then
							PrvState.OnEntry(CurrState)
						End If
						'Reset SubFSM
						If Not PrvState.OnSubFsm Is Nothing Then
							PrvState.OnSubFsm.CurrState = PrvState.OnSubFsm.PrvInitialState
						End If
					End If
				End If

			End If
		End If
	End Sub

	Public Function Evolve() As TState
		Dim PrvState As ClsState
		Dim PrvEvent As TEvent
		Dim Parameters() As Object

		'1) Call the state callback
		'2) Use the callback return value to fire new state transaction
		'
		'If state callback is nothing, try to use default transaction

		PrvState = VtStates.Find(Function(T) T.CurrState.Equals(CurrState))
		If Not PrvState Is Nothing Then

#If GENERATE_EVENTS Then
			RaiseEvent OnExecute(CurrState)
#End If
			If Not PrvState.OnExecute Is Nothing Then
				Parameters = Nothing
				PrvEvent = PrvState.OnExecute(CurrState, Parameters)
				Fire(PrvEvent, Parameters)
			Else
				If Not PrvState.OnSubFsm Is Nothing Then
					PrvState.OnSubFsm.Evolve()
				End If
				'Try to use default transaction
				Fire(Nothing)
			End If

		End If

		Return CurrState

	End Function

	Public ReadOnly Property CurrentState As TState
		Get
			Return CurrState
		End Get
	End Property

#End Region

#Region " GRAPHVIZ CODE GENERATOR "

	Private Function PrvGraphvizFsm(ByVal CurrFsm As HFSM(Of TState, TEvent), ByVal NewLine As String, ByRef ClusterID As Integer) As String
		Dim StRes As String
		Dim NewTab As Char() = System.Text.Encoding.ASCII.GetChars({&H9})

		StRes = ""

		StRes &= NewLine
		StRes &= NewLine
		StRes &= NewTab & "subgraph cluster" & ClusterID & " {" & NewLine

		For Each TmpState In CurrFsm.VtStates

			'Generate a state description string like "a[label="state description"];"
			StRes &= NewTab & NewTab & TmpState.CurrState.ToString() & "[label="""
			If Not TmpState.OnEntry Is Nothing Then StRes &= "[" & TmpState.OnEntry.Method.Name() & "()]\n"
			StRes &= TmpState.CurrState.ToString() & "\n"
			If Not TmpState.OnExecute Is Nothing Then StRes &= TmpState.OnExecute.Method.Name() & "()\n"
			If Not TmpState.OnSubFsm Is Nothing Then StRes &= "<" & TmpState.OnSubFsm.PrvFsmName & ">\n"
			If Not TmpState.OnExit Is Nothing Then StRes &= "[" & TmpState.OnExit.Method.Name() & "()]\n"
			StRes &= """"
			If Not TmpState.OnSubFsm Is Nothing Then StRes &= ",style=bold"
			StRes &= "];" & NewLine

			For Each TmpTransaction In TmpState.TransactionList
				Dim StLineFrom As String
				Dim StLineTo As String
				Dim StLineAction As String
				Dim StLineStyle As String

				'Generate a transaction description string like "a -> b[label="transaction condition"];"
				StLineFrom = TmpState.CurrState.ToString()
				StLineTo = TmpTransaction.TrDestState.ToString()
				StLineAction = ""
				StLineStyle = ""
				If Not TmpTransaction.TrEvent Is Nothing Then
					StLineAction = TmpTransaction.TrEvent.ToString()
				ElseIf Not TmpTransaction.TrStateEvent Is Nothing Then
					StLineAction = TmpTransaction.TrStateEvent.ToString()
					StLineStyle = ",style=bold"
				End If
				StRes &= NewTab & NewTab & StLineFrom & " -> " & StLineTo & "[label=""" & StLineAction & """" & StLineStyle & "];" & NewLine
			Next

			'Generate default transaction description string like "a -> b[label="*"];"
			If Not TmpState.DefaultTransaction Is Nothing Then
				Dim StLineStyle As String

				StLineStyle = ""
				If TmpState.TransactionList.Count > 0 Then StLineStyle = ",style=dotted"
				StRes &= NewTab & NewTab & TmpState.CurrState.ToString() & " -> " & TmpState.DefaultTransaction.TrDestState.ToString() & "[label=""" & "*" & """" & StLineStyle & "];" & NewLine
			End If
		Next

		'Close subgraph
		StRes &= NewTab & NewTab & "label = """ & CurrFsm.PrvFsmName & """;" & NewLine
		StRes &= NewTab & "}" & NewLine

		'Call for every SubFSM
		For Each TmpSubFsm In CurrFsm.VtSubFSM
			ClusterID += 1
			StRes &= PrvGraphvizFsm(TmpSubFsm, NewLine, ClusterID)
		Next

		Return StRes

	End Function

	Public Function ToGraphviz(ByVal NewLine As String) As String
		Dim StRes As String

		If NewLine = "" Then NewLine = System.Text.Encoding.ASCII.GetChars({&HA})
		StRes = ""

		StRes &= NewLine
		StRes &= "digraph {" & NewLine

		StRes &= PrvGraphvizFsm(Me, NewLine, 0)

		StRes &= NewLine
		StRes &= "}"

		Return StRes

	End Function

#End Region

#Region " C CODE GENERATOR "

	Public Function ToC_FSMHeader(ByVal NewLine As String) As String
		Return ToC_FSMHeader(NewLine, Me)
	End Function

	Public Function ToC_FSMHeader(ByVal NewLine As String, ByVal CurrFSM As HFSM(Of TState, TEvent)) As String
		Dim StRes As String
		Dim VarEnStatesName As String
		Dim VarEnEventsName As String
		Dim NewTab As Char() = System.Text.Encoding.ASCII.GetChars({&H9})
		Dim ListEvent As System.Collections.Generic.List(Of TEvent)
		Dim ListStates As System.Collections.Generic.List(Of TState)

		If NewLine = "" Then NewLine = System.Text.Encoding.ASCII.GetChars({&HA})
		StRes = ""

		StRes &= NewLine

		VarEnStatesName = CurrFSM.PrvFsmName & "_EnStates"
		VarEnEventsName = CurrFSM.PrvFsmName & "_EnEvents"

		'Add states to temporary list
		ListStates = New System.Collections.Generic.List(Of TState)
		For Each TmpState In CurrFSM.VtStates
			If ListStates.Contains(TmpState.CurrState) = False Then ListStates.Add(TmpState.CurrState)
			If Not TmpState.DefaultTransaction Is Nothing AndAlso ListStates.Contains(TmpState.DefaultTransaction.TrDestState) = False Then
				ListStates.Add(TmpState.DefaultTransaction.TrDestState)
			End If
			For Each TmpTransaction In TmpState.TransactionList
				If Not TmpTransaction.TrDestState Is Nothing AndAlso ListStates.Contains(TmpTransaction.TrDestState) = False Then
					ListStates.Add(TmpTransaction.TrDestState)
				End If
			Next
		Next

		'Put states to file
		StRes &= "typedef enum {" & NewLine
		For Each TmpState In ListStates
			StRes &= NewTab & TmpState.ToString() & "," & NewLine
		Next
		StRes &= "} " & VarEnStatesName & ";" & NewLine
		StRes &= NewLine

		'Add events to temporary list
		ListEvent = New System.Collections.Generic.List(Of TEvent)
		For Each TmpState In CurrFSM.VtStates
			For Each TmpTransaction In TmpState.TransactionList
				If Not TmpTransaction.TrEvent Is Nothing AndAlso ListEvent.Contains(TmpTransaction.TrEvent) = False Then
					ListEvent.Add(TmpTransaction.TrEvent)
				End If
			Next
		Next

		'Put events to file
		StRes &= "typedef enum {" & NewLine
		For Each TmpEvent In ListEvent
			StRes &= NewTab & TmpEvent.ToString() & "," & NewLine
		Next
		StRes &= "} " & VarEnEventsName & ";" & NewLine
		StRes &= NewLine

		StRes &= "void FSM_" & CurrFSM.PrvFsmName & "_Restart(void);" & NewLine
		StRes &= VarEnStatesName & " FSM_" & CurrFSM.PrvFsmName & "(void * Parameters);" & NewLine

		StRes &= NewLine
		StRes &= NewLine

		'Jump to sub FSM
		For Each TmpState In CurrFSM.VtStates
			If Not TmpState.OnSubFsm Is Nothing Then
				StRes &= ToC_FSMHeader(NewLine, TmpState.OnSubFsm)
			End If
		Next

		Return StRes

	End Function

	Public Function ToC_FSMSource(ByVal NewLine As String) As String
		Return ToC_FSMSource(NewLine, Me)
	End Function

	Private Function ToC_FSMSource(ByVal NewLine As String, ByVal CurrFSM As HFSM(Of TState, TEvent)) As String
		Dim StRes As String
		Dim VarEnStatesName As String
		Dim VarEnEventsName As String
		Dim VarStateEntryName As String
		Dim VarCurrStateName As String
		Dim NewTab As Char() = System.Text.Encoding.ASCII.GetChars({&H9})

		If NewLine = "" Then NewLine = System.Text.Encoding.ASCII.GetChars({&HA})
		StRes = ""

		StRes &= NewLine

		StRes &= CurrFSM.PrvFsmHeader
		StRes &= NewLine

		VarEnStatesName = CurrFSM.PrvFsmName & "_EnStates"
		VarEnEventsName = CurrFSM.PrvFsmName & "_EnEvents"
		VarStateEntryName = CurrFSM.PrvFsmName & "_StateEntry"
		VarCurrStateName = CurrFSM.PrvFsmName & "_CurrState"
		StRes &= "static unsigned char " & VarStateEntryName & " = 1;" & NewLine
		StRes &= "static " & VarEnStatesName & " " & VarCurrStateName & ";" & NewLine
		StRes &= NewLine

		StRes &= "void FSM_" & CurrFSM.PrvFsmName & "_Restart(void){" & NewLine
		StRes &= NewTab & VarCurrStateName & " = " & PrvInitialState.ToString() & ";" & NewLine
		StRes &= "}" & NewLine
		StRes &= NewLine

		StRes &= VarEnStatesName & " FSM_" & CurrFSM.PrvFsmName & "(void * Parameters){" & NewLine

		StRes &= NewTab & "switch (" & VarCurrStateName & "){" & NewLine
		StRes &= NewLine
		For Each TmpState In CurrFSM.VtStates

			StRes &= NewTab & NewTab & "case " & TmpState.CurrState.ToString() & ":{" & NewLine
			StRes &= NewTab & NewTab & NewTab & VarEnEventsName & " TmpEvent;" & NewLine
			StRes &= NewLine

			'OnEntry
			If Not TmpState.OnEntry Is Nothing Or Not TmpState.OnSubFsm Is Nothing Then
				StRes &= NewTab & NewTab & NewTab & "if (" & VarStateEntryName & " == 1){" & NewLine
				If Not TmpState.OnEntry Is Nothing Then StRes &= NewTab & NewTab & NewTab & NewTab & "FSM_" & CurrFSM.PrvFsmName & "_" & TmpState.OnEntry.Method.Name() & "(Parameters);" & NewLine
				If Not TmpState.OnSubFsm Is Nothing Then StRes &= NewTab & NewTab & NewTab & NewTab & "FSM_" & TmpState.OnSubFsm.PrvFsmName & "_Restart();" & NewLine
				StRes &= NewTab & NewTab & NewTab & NewTab & VarStateEntryName & " = 0;" & NewLine
				StRes &= NewTab & NewTab & NewTab & "}" & NewLine
				StRes &= NewLine
			End If

			'OnExecuting
			If Not TmpState.OnExecute Is Nothing Then
				StRes &= NewTab & NewTab & NewTab & "TmpEvent = FSM_" & CurrFSM.PrvFsmName & "_" & TmpState.OnExecute.Method.Name() & "(Parameters);" & NewLine
			End If

			'SubFSM
			If Not TmpState.OnSubFsm Is Nothing Then
				StRes &= NewTab & NewTab & NewTab & "TmpEvent = FSM_" & TmpState.OnSubFsm.PrvFsmName & "(Parameters);" & NewLine
			End If

			'Evaluate transaction
			StRes &= NewTab & NewTab & NewTab & "switch (TmpEvent){" & NewLine
			For Each TmpTransaction In TmpState.TransactionList
				'SubFSM
				If Not TmpState.OnSubFsm Is Nothing Then
					'SubFSM or normal transaction
					StRes &= NewTab & NewTab & NewTab & NewTab & "case " & TmpTransaction.TrStateEvent.ToString() & ":{ " & VarCurrStateName & " = " & TmpTransaction.TrDestState.ToString() & "; break; }" & NewLine
				Else
					'Normal transaction
					StRes &= NewTab & NewTab & NewTab & NewTab & "case " & TmpTransaction.TrEvent.ToString() & ":{ " & VarCurrStateName & " = " & TmpTransaction.TrDestState.ToString() & "; break; }" & NewLine
				End If
			Next
			If Not TmpState.DefaultTransaction Is Nothing Then
				StRes &= NewTab & NewTab & NewTab & NewTab & "default:{ " & VarCurrStateName & " = " & TmpState.DefaultTransaction.TrDestState.ToString() & "; break; }" & NewLine
			End If
			StRes &= NewTab & NewTab & NewTab & "}" & NewLine
			StRes &= NewLine

			'OnExit
			StRes &= NewTab & NewTab & NewTab & "if (" & VarCurrStateName & " != " & TmpState.CurrState.ToString() & "){" & NewLine
			If Not TmpState.OnExit Is Nothing Then
				StRes &= NewTab & NewTab & NewTab & NewTab & "FSM_" & CurrFSM.PrvFsmName & "_" & TmpState.OnExit.Method.Name() & "(Parameters);" & NewLine
			End If
			StRes &= NewTab & NewTab & NewTab & NewTab & VarStateEntryName & " = 1;" & NewLine
			StRes &= NewTab & NewTab & NewTab & "}" & NewLine
			StRes &= NewLine

			StRes &= NewTab & NewTab & NewTab & "break;" & NewLine
			StRes &= NewTab & NewTab & "}" & NewLine
			StRes &= NewLine

		Next

		StRes &= NewTab & "}" & NewLine

		StRes &= NewTab & "return " & VarCurrStateName & ";" & NewLine
		StRes &= "}" & NewLine

		'Jump to sub FSM
		For Each TmpState In CurrFSM.VtStates
			If Not TmpState.OnSubFsm Is Nothing Then
				StRes &= ToC_FSMSource(NewLine, TmpState.OnSubFsm)
			End If
		Next

		Return StRes

	End Function

	Public Function ToC_FSMMain(ByVal NewLine As String) As String
		Return ToC_FSMMain(NewLine, Me)
	End Function

	Private Function ToC_FSMMain(ByVal NewLine As String, ByVal CurrFSM As HFSM(Of TState, TEvent)) As String
		Dim StRes As String
		Dim NewTab As Char() = System.Text.Encoding.ASCII.GetChars({&H9})
		Dim FunctionPrefix As String
		Dim ListFunctions As System.Collections.Generic.List(Of String)

		If NewLine = "" Then NewLine = System.Text.Encoding.ASCII.GetChars({&HA})
		StRes = ""

		If CurrFSM.PrvFsmHeader <> "" Then
			StRes &= NewLine
			StRes &= CurrFSM.PrvFsmHeader & NewLine
			StRes &= NewLine
			StRes &= "int main(){" & NewLine
			StRes &= "}" & NewLine
			StRes &= NewLine
		End If

		'Add states to temporary list
		ListFunctions = New System.Collections.Generic.List(Of String)
		FunctionPrefix = "FSM_" & CurrFSM.PrvFsmName & "_"
		For Each TmpState In CurrFSM.VtStates
			If Not TmpState.OnEntry Is Nothing AndAlso ListFunctions.Contains(FunctionPrefix & TmpState.OnEntry.Method.Name) = False Then
				ListFunctions.Add(FunctionPrefix & TmpState.OnEntry.Method.Name)
			End If
			If Not TmpState.OnExecute Is Nothing AndAlso ListFunctions.Contains(FunctionPrefix & TmpState.OnExecute.Method.Name) = False Then
				ListFunctions.Add(FunctionPrefix & TmpState.OnExecute.Method.Name)
			End If
			If Not TmpState.OnExit Is Nothing AndAlso ListFunctions.Contains(FunctionPrefix & TmpState.OnExit.Method.Name) = False Then
				ListFunctions.Add(FunctionPrefix & TmpState.OnExit.Method.Name)
			End If
		Next

		'Add empty functions
		For Each TmpFunction In ListFunctions
			StRes &= CurrFSM.PrvFsmName & "_EnStates " & TmpFunction.ToString() & "(void * Parameters){" & NewLine
			StRes &= NewTab & "return 0x00;" & NewLine
			StRes &= "}" & NewLine
			StRes &= NewLine
		Next
		StRes &= NewLine

		'Jump to sub FSM
		For Each TmpState In CurrFSM.VtStates
			If Not TmpState.OnSubFsm Is Nothing Then
				StRes &= ToC_FSMMain(NewLine, TmpState.OnSubFsm)
			End If
		Next

		StRes &= NewLine

		Return StRes

	End Function

#End Region

#Region " GOJS CODE GENERATOR "

	Public Function ToGoJs(ByVal NewLine As String) As String
		Dim StRes As String
		Dim NewTab As Char() = System.Text.Encoding.ASCII.GetChars({&H9})
		Dim LineStyle As String

		If NewLine = "" Then NewLine = System.Text.Encoding.ASCII.GetChars({&HA})
		StRes = ""

		StRes &= NewLine
		StRes &= "myDiagram.model = new go.GraphLinksModel(" & NewLine

		'State list
		StRes &= "[" & NewLine
		For Each TmpState In VtStates

			'Generate a state description string like "{ key: "state name", text: "state description" },"
			StRes &= NewTab & "{ "
			StRes &= " key: """ & TmpState.CurrState.ToString() & """ "
			StRes &= ", text: """
			If Not TmpState.OnEntry Is Nothing Then StRes &= "[" & TmpState.OnEntry.Method.Name() & "()]\n"
			StRes &= TmpState.CurrState.ToString() & "\n"
			If Not TmpState.OnExecute Is Nothing Then StRes &= TmpState.OnExecute.Method.Name() & "()\n"
			If Not TmpState.OnExit Is Nothing Then StRes &= "[" & TmpState.OnExit.Method.Name() & "()]\n"
			StRes &= """ }," & NewLine

		Next
		StRes &= "]"

		'Transaction list
		StRes &= ", [" & NewLine
		For Each TmpState In VtStates

			For Each TmpTransaction In TmpState.TransactionList
				'Generate a transaction description string like "{ from: "state key", to: "state key",  text: "transaction condition" },"
				StRes &= NewTab & "{ from: """ & TmpState.CurrState.ToString() & """, to: """ & TmpTransaction.TrDestState.ToString() & """, text: """ & TmpTransaction.TrEvent.ToString() & """ }," & NewLine
			Next

			'Generate default transaction description string like "{ from: "state key", to: "state key",  text: "*" },"
			If Not TmpState.DefaultTransaction Is Nothing Then
				LineStyle = ""
				If TmpState.TransactionList.Count > 0 Then LineStyle = ", color: ""gray"""
				StRes &= NewTab & "{ from: """ & TmpState.CurrState.ToString() & """, to: """ & TmpState.DefaultTransaction.TrDestState.ToString() & """, text: """ & "*" & """ " & LineStyle & " }," & NewLine
			End If
		Next
		StRes &= "]);"

		Return StRes

	End Function

#End Region

#Region " READ FROM STREAM "

	Public Shared Function LoadFromFile(ByVal FilePath As String, ByVal ClassCallback As Object, ByRef MyStates As String(), ByRef MyEvents As String()) As HFSM(Of String, String)
		Return LoadFromStream(New System.IO.StreamReader(FilePath), ClassCallback, MyStates, MyEvents)
	End Function

	Public Shared Function LoadFromString(ByVal StFSM As String, ByVal ClassCallback As Object, ByRef MyStates As String(), ByRef MyEvents As String()) As HFSM(Of String, String)
		Return LoadFromStream(New System.IO.StringReader(StFSM), ClassCallback, MyStates, MyEvents)
	End Function

	Public Shared Function LoadFromStream(ByVal InputStream As System.IO.TextReader, ByVal ClassCallback As Object, ByRef MyStates As String(), ByRef MyEvents As String()) As HFSM(Of String, String)
		Dim NewClsFsm As HFSM(Of String, String)
		Dim MyFsmConfiguration As HFSM(Of String, String)
		Dim MyStateConfiguration As HFSM(Of String, String).StateConfiguration

		If InputStream Is Nothing Then Return Nothing

		MyStates = New String() {}
		MyEvents = New String() {}
		NewClsFsm = New HFSM(Of String, String)("", "") With {
			.PrvClassCallback = ClassCallback
		}
		MyFsmConfiguration = NewClsFsm
		MyStateConfiguration = Nothing

		Do
			Dim StLine As String
			Dim StCommand As String
			Dim StParameter As String

			StLine = InputStream.ReadLine()
			If StLine Is Nothing Then Exit Do
			StLine = StLine.Trim
			StCommand = StLine.ToLower

			If StLine.StartsWith("#") Or StLine.Length = 0 Then Continue Do

			StParameter = ""
			If StLine.IndexOf(":") >= 0 Then StParameter = StLine.Substring(StLine.IndexOf(":") + 1).Trim

			If StCommand Like "fsm:*" Then
				MyFsmConfiguration.PrvFsmName = StParameter

			ElseIf StCommand Like "subfsm:*" Then
				If NewClsFsm.VtSubFSM.Exists(Function(T) T.PrvFsmName = StParameter) Then
					MyFsmConfiguration = NewClsFsm.VtSubFSM.Find(Function(T) T.PrvFsmName = StParameter)
				Else
					MyFsmConfiguration = New HFSM(Of String, String)(StParameter, Nothing)
					NewClsFsm.VtSubFSM.Add(MyFsmConfiguration)
				End If

			ElseIf StCommand Like "fsmheader:*" Then
				MyFsmConfiguration.PrvFsmHeader &= StParameter & System.Text.Encoding.ASCII.GetChars({&HA})

			ElseIf StCommand Like "initialstate:*" Then
				'Add new state, if necessary ;)
				If System.Array.Exists(Of String)(MyStates, Function(T) T = StParameter) = False Then
					ReDim Preserve MyStates(MyStates.Length)
					MyStates(MyStates.Length - 1) = StParameter
				End If
				MyFsmConfiguration.PrvInitialState = StParameter
				MyFsmConfiguration.CurrState = StParameter

			ElseIf StCommand Like "state:*" Then
				'Add new state, if necessary ;)
				If System.Array.Exists(Of String)(MyStates, Function(T) T = StParameter) = False Then
					ReDim Preserve MyStates(MyStates.Length)
					MyStates(MyStates.Length - 1) = StParameter
				End If
				MyStateConfiguration = MyFsmConfiguration.Configure(StParameter)

			ElseIf StCommand Like "onentry:*" Then
				If MyStateConfiguration Is Nothing Then
					System.Diagnostics.Debug.WriteLine("Error in '" & StLine & "'")
					Return LoadFileError(InputStream)
				End If
				If MyStateConfiguration.OnEntry(StParameter, ClassCallback) Is Nothing Then Return LoadFileError(InputStream)

			ElseIf StCommand Like "onexit:*" Then
				If MyStateConfiguration Is Nothing Then
					System.Diagnostics.Debug.WriteLine("Error in '" & StLine & "'")
					Return LoadFileError(InputStream)
				End If
				If MyStateConfiguration.OnExit(StParameter, ClassCallback) Is Nothing Then Return LoadFileError(InputStream)

			ElseIf StCommand Like "onexecute:*" Then
				If MyStateConfiguration Is Nothing Then
					System.Diagnostics.Debug.WriteLine("Error in '" & StLine & "'")
					Return LoadFileError(InputStream)
				End If
				If MyStateConfiguration.OnExecute(StParameter, ClassCallback) Is Nothing Then Return LoadFileError(InputStream)

			ElseIf StCommand Like "runsubfsm:*" Then
				If MyStateConfiguration Is Nothing Then
					System.Diagnostics.Debug.WriteLine("Error in '" & StLine & "'")
					Return LoadFileError(InputStream)
				End If
				If Not NewClsFsm.VtSubFSM.Exists(Function(T) T.PrvFsmName = StParameter) Then
					Dim PrvNewFsmConfiguration As HFSM(Of String, String)
					PrvNewFsmConfiguration = New HFSM(Of String, String)(StParameter, Nothing)
					NewClsFsm.VtSubFSM.Add(PrvNewFsmConfiguration)
				End If
				MyStateConfiguration.OnSubFSM(NewClsFsm.VtSubFSM.Find(Function(T) T.PrvFsmName = StParameter))

			ElseIf StCommand Like "transaction on *" Or StCommand Like "transaction when *" Then
				If MyStateConfiguration Is Nothing Then
					System.Diagnostics.Debug.WriteLine("Error in '" & StLine & "'")
					Return LoadFileError(InputStream)
				End If
				If Not StCommand Like "* on *:*" And Not StCommand Like "* when *:*" Then
					System.Diagnostics.Debug.WriteLine("Error splitting '" & StLine & "'")
					Return LoadFileError(InputStream)
				End If

				If StCommand Like "* on *:*" Then
					StLine = StLine.Substring(StCommand.IndexOf(" on ") + 4).Trim
					StLine = StLine.Split(":"c)(0).Trim
					MyStateConfiguration.Transaction(StLine, StParameter)
				ElseIf StCommand Like "* when *:*" Then
					StLine = StLine.Substring(StCommand.IndexOf(" when ") + 6).Trim
					StLine = StLine.Split(":"c)(0).Trim
					MyStateConfiguration.StateTransaction(StLine, StParameter)
				End If

			ElseIf StCommand Like "defaulttransaction:*" Then
				If MyStateConfiguration Is Nothing Then
					System.Diagnostics.Debug.WriteLine("Error in '" & StLine & "'")
					Return LoadFileError(InputStream)
				End If
				MyStateConfiguration.DefaultTransaction(StParameter)

			Else
				System.Diagnostics.Debug.WriteLine("Unknown command '" & StLine & "'")
				Return LoadFileError(InputStream)

			End If
		Loop

		InputStream.Close()

		Return NewClsFsm

	End Function

	Private Shared Function LoadFileError(ByVal FInput As System.IO.TextReader) As HFSM(Of String, String)
		FInput.Close()
		Return Nothing
	End Function

#End Region

End Class
