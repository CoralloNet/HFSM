
#Const GENERATE_EVENTS = 0

'FSM Manager
'	Put FSM into your project .NET, C and generate GraphViz documentation
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

Friend Class HFSM(Of TState, TEvent)

	Friend Delegate Function VerifyCallback(ByVal Parameters() As Object) As Boolean
	Friend Delegate Sub EntryCallback(ByVal EntryState As TState)
	Friend Delegate Sub ExitCallback(ByVal ExitState As TState)
	Friend Delegate Function ExecuteCallback(ByVal ExecuteState As TState, ByRef Parameters() As Object) As TEvent

#If GENERATE_EVENTS Then
	Friend Event OnEntry(ByVal EntryState As TState)
	Friend Event OnExit(ByVal ExitState As TState)
	Friend Event OnExecute(ByVal ExecuteState As TState)
#End If

	Private VtStati As System.Collections.Generic.List(Of ClsState)
	Private CurrState As TState
	Private PrvInitialState As TState
	Private PrvFsmName As String
	Private PrvFsmHeader As String
	Private PrvClassCallback As Object

	Friend Sub New(ByVal FsmName As String, ByVal InitialState As TState)
		PrvFsmName = FsmName
		VtStati = New System.Collections.Generic.List(Of ClsState)
		CurrState = InitialState
		PrvInitialState = InitialState

	End Sub

#Region " STATE MANAGEMENT "

	Private Class ClsTransaction
		Friend TrEvent As TEvent
		Friend TrDestState As TState
		Friend TrVerifyCallback As VerifyCallback
	End Class

	Private Class ClsState
		Friend CurrState As TState
		Friend OnEntry As EntryCallback
		Friend OnExit As ExitCallback
		Friend OnExecute As ExecuteCallback
		Friend TransactionList As System.Collections.Generic.List(Of ClsTransaction)
		Friend DefaultTransaction As ClsTransaction

		'For late associations
		Friend OnEntryString As String
		Friend OnExitString As String
		Friend OnExecuteString As String

		Friend Sub New()
			TransactionList = New System.Collections.Generic.List(Of ClsTransaction)
			DefaultTransaction = Nothing
		End Sub

	End Class


	Private Function GetState(ByVal SearchState As TState) As ClsState
		Dim NewState As ClsState

		For Each NewState In VtStati
			If NewState.CurrState.Equals(SearchState) Then
				Return NewState
			End If
		Next

		NewState = New ClsState() With {
			.CurrState = SearchState
		}
		VtStati.Add(NewState)

		Return NewState

	End Function

#End Region

#Region " CONFIGURATION "

	Friend Class StateConfiguration

		Friend ConfigState As TState
		Friend ParentFSM As HFSM(Of TState, TEvent)

		Friend Function OnEntry(ByVal FunctionCallback As EntryCallback) As StateConfiguration
			ParentFSM.GetState(Me.ConfigState).OnEntry = FunctionCallback
			Return Me
		End Function
		Friend Function OnEntry(ByVal FunctionCallback As String) As StateConfiguration
			ParentFSM.GetState(Me.ConfigState).OnEntryString = FunctionCallback
			Return Me
		End Function

		Friend Function OnExit(ByVal FunctionCallback As ExitCallback) As StateConfiguration
			ParentFSM.GetState(Me.ConfigState).OnExit = FunctionCallback
			Return Me
		End Function
		Friend Function OnExit(ByVal FunctionCallback As String) As StateConfiguration
			ParentFSM.GetState(Me.ConfigState).OnExitString = FunctionCallback
			Return Me
		End Function

		Friend Function OnExecute(ByVal FunctionCallback As ExecuteCallback) As StateConfiguration
			ParentFSM.GetState(Me.ConfigState).OnExecute = FunctionCallback
			Return Me
		End Function
		Friend Function OnExecute(ByVal FunctionCallback As String) As StateConfiguration
			ParentFSM.GetState(Me.ConfigState).OnExecuteString = FunctionCallback
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

		Friend Function DefaultTransaction(ByVal DestinationState As TState) As StateConfiguration
			ParentFSM.GetState(Me.ConfigState).DefaultTransaction = New ClsTransaction With {
				.TrDestState = DestinationState
			}
			Return Me
		End Function

	End Class

	Friend Function Configure(ByVal ConfigState As TState) As StateConfiguration
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

		PrvState = VtStati.Find(Function(T) T.CurrState.Equals(CurrState))
		If Not PrvState Is Nothing Then
			PrvTransaction = PrvState.TransactionList.Find(Function(T) T.TrEvent.Equals(EventTrigger))
			If PrvTransaction Is Nothing Then
				PrvTransaction = PrvState.DefaultTransaction
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
					If PrvState.OnExitString <> "" Then
						PrvClassCallback.GetType.GetMethod(PrvState.OnExitString, CType(-1, System.Reflection.BindingFlags)).Invoke(Me, {CurrState})
					End If

					'Change state
					CurrState = PrvTransaction.TrDestState

					'Execute Entry callback
#If GENERATE_EVENTS Then
					RaiseEvent OnEntry(CurrState)
#End If
					PrvState = VtStati.Find(Function(T) T.CurrState.Equals(CurrState))
					If Not PrvState Is Nothing Then
						If Not PrvState.OnEntry Is Nothing Then
							PrvState.OnEntry(CurrState)
						End If
						If PrvState.OnEntryString <> "" Then
							PrvClassCallback.GetType.GetMethod(PrvState.OnEntryString, CType(-1, System.Reflection.BindingFlags)).Invoke(Me, {CurrState})
						End If
					End If
				End If

			End If
		End If
	End Sub

	Friend Function Evolve() As TState
		Dim PrvState As ClsState
		Dim PrvEvent As TEvent
		Dim Parameters() As Object

		'1) Call the state callback
		'2) Use the callback return value to fire new state transaction
		'
		'If state callback is nothing, try to use default transaction

		PrvState = VtStati.Find(Function(T) T.CurrState.Equals(CurrState))
		If Not PrvState Is Nothing Then

#If GENERATE_EVENTS Then
			RaiseEvent OnExecute(CurrState)
#End If
			If Not PrvState.OnExecute Is Nothing Then
				Parameters = Nothing
				PrvEvent = PrvState.OnExecute(CurrState, Parameters)
				Fire(PrvEvent, Parameters)
			ElseIf PrvState.OnExecuteString <> "" Then
				Parameters = Nothing
				PrvEvent = CType(PrvClassCallback.GetType.GetMethod(PrvState.OnExecuteString, CType(-1, System.Reflection.BindingFlags)).Invoke(Me, {CurrState}), TEvent)
				Fire(PrvEvent, Parameters)
			Else
				'Try to use default transaction
				Fire(Nothing)
			End If

		End If

		Return CurrState

	End Function

#End Region

#Region " GRAPHVIZ CODE GENERATOR "

	Friend Function ToGraphviz(ByVal NewLine As String) As String
		Dim StRes As String
		Dim NewTab As Char() = System.Text.Encoding.ASCII.GetChars({&H9})
		Dim LineStyle As String

		If NewLine = "" Then NewLine = System.Text.Encoding.ASCII.GetChars({&HA})
		StRes = ""

		StRes &= NewLine
		StRes &= "digraph {" & NewLine

		For Each TmpStato In VtStati

			'Generate a state description string like "a[label="state description"];"
			StRes &= NewTab & TmpStato.CurrState.ToString() & "[label="""
			If Not TmpStato.OnEntry Is Nothing Then StRes &= "[" & TmpStato.OnEntry.Method.Name() & "()]\n"
			If Not TmpStato.OnEntryString Is Nothing Then StRes &= "[" & TmpStato.OnEntryString & "()]\n"
			StRes &= TmpStato.CurrState.ToString() & "\n"
			If Not TmpStato.OnExecute Is Nothing Then StRes &= TmpStato.OnExecute.Method.Name() & "()\n"
			If Not TmpStato.OnExecuteString Is Nothing Then StRes &= TmpStato.OnExecuteString & "()\n"
			If Not TmpStato.OnExit Is Nothing Then StRes &= "[" & TmpStato.OnExit.Method.Name() & "()]\n"
			If Not TmpStato.OnExitString Is Nothing Then StRes &= "[" & TmpStato.OnExitString & "()]\n"
			StRes &= """];" & NewLine

			For Each TmpTransaction In TmpStato.TransactionList
				'Generate a transaction description string like "a -> b[label="transaction condition"];"
				StRes &= NewTab & TmpStato.CurrState.ToString() & " -> " & TmpTransaction.TrDestState.ToString() & "[label=""" & TmpTransaction.TrEvent.ToString() & """];" & NewLine
			Next

			'Generate default transaction description string like "a -> b[label="*"];"
			If Not TmpStato.DefaultTransaction Is Nothing Then
				LineStyle = ""
				If TmpStato.TransactionList.Count > 0 Then LineStyle = ",style=""dotted"""
				StRes &= NewTab & TmpStato.CurrState.ToString() & " -> " & TmpStato.DefaultTransaction.TrDestState.ToString() & "[label=""" & "*" & """" & LineStyle & "];" & NewLine
			End If
		Next

		StRes &= "}"

		Return StRes

	End Function

#End Region

#Region " C CODE GENERATOR "

	Friend Function ToC_FSMHeader(ByVal NewLine As String) As String
		Dim StRes As String
		Dim NewTab As Char() = System.Text.Encoding.ASCII.GetChars({&H9})
		Dim ListEvent As System.Collections.Generic.List(Of TEvent)

		If NewLine = "" Then NewLine = System.Text.Encoding.ASCII.GetChars({&HA})
		StRes = ""

		StRes &= NewLine

		StRes &= "typedef enum {" & NewLine
		For Each TmpState In VtStati
			StRes &= NewTab & TmpState.CurrState.ToString() & "," & NewLine
		Next
		StRes &= "} EnStates;" & NewLine
		StRes &= NewLine

		StRes &= "typedef enum {" & NewLine
		ListEvent = New System.Collections.Generic.List(Of TEvent)
		For Each TmpState In VtStati
			For Each TmpTransaction In TmpState.TransactionList
				If ListEvent.Contains(TmpTransaction.TrEvent) = False Then
					ListEvent.Add(TmpTransaction.TrEvent)
				End If
			Next
		Next
		For Each TmpEvent In ListEvent
			StRes &= NewTab & TmpEvent.ToString() & "," & NewLine
		Next
		StRes &= "} EnEvents;" & NewLine
		StRes &= NewLine

		StRes &= "void FSM_" & PrvFsmName & "_Restart(void);" & NewLine
		StRes &= "EnStates FSM_" & PrvFsmName & "(void * Parameters);" & NewLine

		StRes &= NewLine
		StRes &= NewLine

		Return StRes

	End Function

	Friend Function ToC_FSMSource(ByVal NewLine As String) As String
		Dim StRes As String
		Dim NewTab As Char() = System.Text.Encoding.ASCII.GetChars({&H9})

		If NewLine = "" Then NewLine = System.Text.Encoding.ASCII.GetChars({&HA})
		StRes = ""

		StRes &= NewLine

		StRes &= PrvFsmHeader
		StRes &= NewLine

		StRes &= "static unsigned char StateEntry = 1;" & NewLine
		StRes &= "static EnStates CurrState;" & NewLine
		StRes &= NewLine

		StRes &= "void FSM_" & PrvFsmName & "_Restart(void){" & NewLine
		StRes &= NewTab & "CurrState = " & PrvInitialState.ToString() & ";" & NewLine
		StRes &= "}" & NewLine
		StRes &= NewLine

		StRes &= "EnStates FSM_" & PrvFsmName & "(void * Parameters){" & NewLine

		StRes &= NewTab & "switch (CurrState){" & NewLine
		StRes &= NewLine
		For Each TmpStato In VtStati

			StRes &= NewTab & NewTab & "case " & TmpStato.CurrState.ToString() & ":{" & NewLine
			StRes &= NewTab & NewTab & NewTab & "EnEvents TmpEvent;" & NewLine

			'OnEntry
			If Not TmpStato.OnEntry Is Nothing Or Not TmpStato.OnEntryString Is Nothing Then
				StRes &= NewTab & NewTab & NewTab & "if (StateEntry == 1){" & NewLine
				If Not TmpStato.OnEntry Is Nothing Then StRes &= NewTab & NewTab & NewTab & NewTab & "FSM_" & PrvFsmName & "_" & TmpStato.OnEntry.Method.Name() & "(Parameters);" & NewLine
				If Not TmpStato.OnEntryString Is Nothing Then StRes &= NewTab & NewTab & NewTab & NewTab & "FSM_" & PrvFsmName & "_" & TmpStato.OnEntryString & "(Parameters);" & NewLine
				StRes &= NewTab & NewTab & NewTab & NewTab & "StateEntry = 0;" & NewLine
				StRes &= NewTab & NewTab & NewTab & "}" & NewLine
				StRes &= NewLine
			End If

			'OnExecuting
			If TmpStato.OnExecute Is Nothing And TmpStato.OnExecuteString Is Nothing Then
				If Not TmpStato.DefaultTransaction Is Nothing Then
					StRes &= NewTab & NewTab & NewTab & "TmpEvent = " & TmpStato.DefaultTransaction.TrDestState.ToString() & ";" & NewLine
				End If
			ElseIf Not TmpStato.OnExecute Is Nothing Then
				StRes &= NewTab & NewTab & NewTab & "TmpEvent = FSM_" & PrvFsmName & "_" & TmpStato.OnExecute.Method.Name() & "(Parameters);" & NewLine
			ElseIf Not TmpStato.OnExecuteString Is Nothing Then
				StRes &= NewTab & NewTab & NewTab & "TmpEvent = FSM_" & PrvFsmName & "_" & TmpStato.OnExecuteString & "(Parameters);" & NewLine
			End If

			'Evaluate transaction
			StRes &= NewTab & NewTab & NewTab & "switch (TmpEvent){" & NewLine
			For Each TmpTransaction In TmpStato.TransactionList
				StRes &= NewTab & NewTab & NewTab & NewTab & "case " & TmpTransaction.TrEvent.ToString() & ":{ CurrState = " & TmpTransaction.TrDestState.ToString() & "; break; }" & NewLine
			Next
			If Not TmpStato.DefaultTransaction Is Nothing Then
				StRes &= NewTab & NewTab & NewTab & NewTab & "default:{ CurrState = " & TmpStato.DefaultTransaction.TrDestState.ToString() & "; break; }" & NewLine
			End If
			StRes &= NewTab & NewTab & NewTab & "}" & NewLine
			StRes &= NewLine

			'OnExit
			StRes &= NewTab & NewTab & NewTab & "if (CurrState != " & TmpStato.CurrState.ToString() & "){" & NewLine
			If Not TmpStato.OnExit Is Nothing Then
				StRes &= NewTab & NewTab & NewTab & NewTab & "FSM_" & PrvFsmName & "_" & TmpStato.OnExit.Method.Name() & "(Parameters);" & NewLine
			End If
			If Not TmpStato.OnExitString Is Nothing Then
				StRes &= NewTab & NewTab & NewTab & NewTab & "FSM_" & PrvFsmName & "_" & TmpStato.OnExitString & "(Parameters);" & NewLine
			End If
			StRes &= NewTab & NewTab & NewTab & NewTab & "StateEntry = 1;" & NewLine
			StRes &= NewTab & NewTab & NewTab & "}" & NewLine
			StRes &= NewLine

			StRes &= NewTab & NewTab & NewTab & "break;" & NewLine
			StRes &= NewTab & NewTab & "}" & NewLine
			StRes &= NewLine

		Next

		StRes &= NewTab & "}" & NewLine

		StRes &= NewTab & "return CurrState;" & NewLine
		StRes &= "}" & NewLine

		Return StRes

	End Function

#End Region

#Region " GOJS CODE GENERATOR "

	Friend Function ToGoJs(ByVal NewLine As String) As String
		Dim StRes As String
		Dim NewTab As Char() = System.Text.Encoding.ASCII.GetChars({&H9})
		Dim LineStyle As String

		If NewLine = "" Then NewLine = System.Text.Encoding.ASCII.GetChars({&HA})
		StRes = ""

		StRes &= NewLine
		StRes &= "myDiagram.model = new go.GraphLinksModel(" & NewLine

		'State list
		StRes &= "[" & NewLine
		For Each TmpStato In VtStati

			'Generate a state description string like "{ key: "state name", text: "state description" },"
			StRes &= NewTab & "{ "
			StRes &= " key: """ & TmpStato.CurrState.ToString() & """ "
			StRes &= ", text: """
			If Not TmpStato.OnEntry Is Nothing Then StRes &= "[" & TmpStato.OnEntry.Method.Name() & "()]\n"
			If Not TmpStato.OnEntryString Is Nothing Then StRes &= "[" & TmpStato.OnEntryString & "()]\n"
			StRes &= TmpStato.CurrState.ToString() & "\n"
			If Not TmpStato.OnExecute Is Nothing Then StRes &= TmpStato.OnExecute.Method.Name() & "()\n"
			If Not TmpStato.OnExecuteString Is Nothing Then StRes &= TmpStato.OnExecuteString & "()\n"
			If Not TmpStato.OnExit Is Nothing Then StRes &= "[" & TmpStato.OnExit.Method.Name() & "()]\n"
			If Not TmpStato.OnExitString Is Nothing Then StRes &= "[" & TmpStato.OnExitString & "()]\n"
			StRes &= """ }," & NewLine

		Next
		StRes &= "]"

		'Transaction list
		StRes &= ", [" & NewLine
		For Each TmpStato In VtStati

			For Each TmpTransaction In TmpStato.TransactionList
				'Generate a transaction description string like "{ from: "state key", to: "state key",  text: "transaction condition" },"
				StRes &= NewTab & "{ from: """ & TmpStato.CurrState.ToString() & """, to: """ & TmpTransaction.TrDestState.ToString() & """, text: """ & TmpTransaction.TrEvent.ToString() & """ }," & NewLine
			Next

			'Generate default transaction description string like "{ from: "state key", to: "state key",  text: "*" },"
			If Not TmpStato.DefaultTransaction Is Nothing Then
				LineStyle = ""
				If TmpStato.TransactionList.Count > 0 Then LineStyle = ", color: ""gray"""
				StRes &= NewTab & "{ from: """ & TmpStato.CurrState.ToString() & """, to: """ & TmpStato.DefaultTransaction.TrDestState.ToString() & """, text: """ & "*" & """ " & LineStyle & " }," & NewLine
			End If
		Next
		StRes &= "]);"

		Return StRes

	End Function

#End Region

#Region " READ FROM FILE "

	Public Shared Function LoadFromFile(ByVal FilePath As String, ByVal ClassCallback As Object, ByRef MyStates As String(), ByRef MyEvents As String()) As HFSM(Of String, String)
		Dim NewClsFsm As HFSM(Of String, String)
		Dim FInput As System.IO.StreamReader
		Dim MyStateConfiguration As HFSM(Of String, String).StateConfiguration

		MyStates = New String() {}
		MyEvents = New String() {}
		NewClsFsm = New HFSM(Of String, String)("", "") With {
			.PrvClassCallback = ClassCallback
		}
		MyStateConfiguration = Nothing

		FInput = New System.IO.StreamReader(FilePath)
		If FInput Is Nothing Then Return Nothing

		Do Until FInput.EndOfStream
			Dim StLine As String
			Dim StCommand As String
			Dim StParameter As String

			StLine = FInput.ReadLine().Trim
			StCommand = StLine.ToLower

			If StLine.StartsWith("#") Or StLine.Length = 0 Then Continue Do

			StParameter = ""
			If StLine.IndexOf(":") >= 0 Then StParameter = StLine.Substring(StLine.IndexOf(":") + 1).Trim

			If StCommand Like "fsm:*" Then
				NewClsFsm.PrvFsmName = StParameter

			ElseIf StCommand Like "fsmheader:*" Then
				NewClsFsm.PrvFsmHeader &= StParameter & System.Text.Encoding.ASCII.GetChars({&HA})

			ElseIf StCommand Like "initialstate:*" Then
				'Add new state, if necessary ;)
				If System.Array.Exists(Of String)(MyStates, Function(T) T = StParameter) = False Then
					ReDim Preserve MyStates(MyStates.Length)
					MyStates(MyStates.Length - 1) = StParameter
				End If
				NewClsFsm.PrvInitialState = StParameter
				NewClsFsm.CurrState = StParameter

			ElseIf StCommand Like "state:*" Then
				'Add new state, if necessary ;)
				If System.Array.Exists(Of String)(MyStates, Function(T) T = StParameter) = False Then
					ReDim Preserve MyStates(MyStates.Length)
					MyStates(MyStates.Length - 1) = StParameter
				End If
				MyStateConfiguration = NewClsFsm.Configure(StParameter)

			ElseIf StCommand Like "onentry:*" Then
				If MyStateConfiguration Is Nothing Then
					System.Diagnostics.Debug.WriteLine("Error in '" & StLine & "'")
					Stop
					Return Nothing
				End If
				''(optional) Verify if callback exists
				'If ClassCallback.GetType.GetMethod(StParameter, CType(-1, System.Reflection.BindingFlags)) Is Nothing Then
				'	System.Diagnostics.Debug.WriteLine("Function not found as requested in '" & StLine & "'")
				'	Stop
				'	Return Nothing
				'End If
				MyStateConfiguration.OnEntry(StParameter)

			ElseIf StCommand Like "onexit:*" Then
				If MyStateConfiguration Is Nothing Then
					System.Diagnostics.Debug.WriteLine("Error in '" & StLine & "'")
					Return Nothing
				End If
				''(optional) Verify if callback exists
				'If ClassCallback.GetType.GetMethod(StParameter, CType(-1, System.Reflection.BindingFlags)) Is Nothing Then
				'	System.Diagnostics.Debug.WriteLine("Function not found as requested in '" & StLine & "'")
				'	Stop
				'	Return Nothing
				'End If
				MyStateConfiguration.OnExit(StParameter)

			ElseIf StCommand Like "onexecute:*" Then
				If MyStateConfiguration Is Nothing Then
					System.Diagnostics.Debug.WriteLine("Error in '" & StLine & "'")
					Stop
					Return Nothing
				End If
				''(optional) Verify if callback exists
				'If ClassCallback.GetType.GetMethod(StParameter, CType(-1, System.Reflection.BindingFlags)) Is Nothing Then
				'	System.Diagnostics.Debug.WriteLine("Function not found as requested in '" & StLine & "'")
				'	Stop
				'	Return Nothing
				'End If
				MyStateConfiguration.OnExecute(StParameter)

			ElseIf StCommand Like "transaction on *" Then
				If MyStateConfiguration Is Nothing Then
					System.Diagnostics.Debug.WriteLine("Error in '" & StLine & "'")
					Stop
					Return Nothing
				End If
				If Not StCommand Like "* on *:*" Then
					System.Diagnostics.Debug.WriteLine("Error splitting '" & StLine & "'")
					Stop
					Return Nothing
				End If
				StLine = StLine.Substring(StCommand.IndexOf(" on ") + 4).Trim
				StLine = StLine.Split(":"c)(0).Trim
				MyStateConfiguration.Transaction(StLine, StParameter)

			ElseIf StCommand Like "defaulttransaction:*" Then
				If MyStateConfiguration Is Nothing Then
					System.Diagnostics.Debug.WriteLine("Error in '" & StLine & "'")
					Stop
					Return Nothing
				End If
				MyStateConfiguration.DefaultTransaction(StParameter)

			Else
				System.Diagnostics.Debug.WriteLine("Unknown command '" & StLine & "'")
				Stop
				Return Nothing

			End If
		Loop

		FInput.Close()

		Return NewClsFsm

	End Function

	Friend Function CheckCallback() As Boolean
		'ToDo: verificare l'associazione dinamica delle callback
		Return False

	End Function

#End Region

End Class
