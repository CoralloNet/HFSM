
'Add a reference to "Microsoft.VisualStudio.QualityTools.UnitTestFramework"

<Microsoft.VisualStudio.TestTools.UnitTesting.TestClass()>
Public Class SiMonATestUnit

	Private _testContext As Microsoft.VisualStudio.TestTools.UnitTesting.TestContext
	Public Property TestContext() As Microsoft.VisualStudio.TestTools.UnitTesting.TestContext
		Get
			Return _testContext
		End Get
		Set
			_testContext = Value
		End Set
	End Property

	'<Microsoft.VisualStudio.TestTools.UnitTesting.ClassInitialize()>
	'Public Shared Sub TestInit(ByVal context As Microsoft.VisualStudio.TestTools.UnitTesting.TestContext)
	'	'Insert your code here...

	'End Sub

#Region " SIMPLE EVOLVING FSM "

	Private Enum SimpleEvolvingFSM_State
		Init
		Middle
		Completed
	End Enum

	Private Enum SimpleEvolvingFSM_Event
		MoveNext
		NothingToDo
	End Enum

	<Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod()>
	Public Sub SimpleEvolvingFSM_Test()
		Dim CurrFSM As HFSM(Of SimpleEvolvingFSM_State, SimpleEvolvingFSM_Event)

		'Create new FSM
		CurrFSM = New HFSM(Of SimpleEvolvingFSM_State, SimpleEvolvingFSM_Event)("TestFSM", SimpleEvolvingFSM_State.Init)
		Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(SimpleEvolvingFSM_State.Init, CurrFSM.CurrentState)

		'Configure states and transactions
		CurrFSM.Configure(SimpleEvolvingFSM_State.Init) _
			.DefaultTransaction(SimpleEvolvingFSM_State.Middle)
		CurrFSM.Configure(SimpleEvolvingFSM_State.Middle) _
			.Transaction(SimpleEvolvingFSM_Event.MoveNext, SimpleEvolvingFSM_State.Completed)

		'Evolve! :)
		CurrFSM.Evolve()
		Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(SimpleEvolvingFSM_State.Middle, CurrFSM.CurrentState)

		'Event ignored
		CurrFSM.Fire(SimpleEvolvingFSM_Event.NothingToDo)
		Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(SimpleEvolvingFSM_State.Middle, CurrFSM.CurrentState)

		'Move next
		CurrFSM.Fire(SimpleEvolvingFSM_Event.MoveNext)
		Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(SimpleEvolvingFSM_State.Completed, CurrFSM.CurrentState)

		'Nothing to do
		CurrFSM.Evolve()
		Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(SimpleEvolvingFSM_State.Completed, CurrFSM.CurrentState)

	End Sub

#End Region

	'load from file

	'#Region " FUNCTION CALLBACK "

	'	Private Enum SimpleEvolvingFSM_State
	'		Init
	'		Middle
	'		Completed
	'	End Enum

	'	Private Enum SimpleEvolvingFSM_Event
	'		MoveNext
	'		NothingToDo
	'	End Enum

	'	Private SimpleEvolvingFSM_InitCount As Integer = 0
	'	Private SimpleEvolvingFSM_MiddleCount As Integer = 0

	'	Private Function Init(ByVal CurrentState As SimpleEvolvingFSM_State, ByRef Parameters As Object()) As SimpleEvolvingFSM_Event
	'		SimpleEvolvingFSM_InitCount += 1
	'		Return SimpleEvolvingFSM_Event.NothingToDo
	'	End Function

	'	Private Function Middle(ByVal CurrentState As SimpleEvolvingFSM_State, ByRef Parameters As Object()) As SimpleEvolvingFSM_Event
	'		SimpleEvolvingFSM_MiddleCount += 1
	'		Return SimpleEvolvingFSM_Event.NothingToDo
	'	End Function

	'	<Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod()>
	'	Public Sub SimpleEvolvingFSM_Test()
	'		Dim CurrFSM As HFSM(Of SimpleEvolvingFSM_State, SimpleEvolvingFSM_Event)

	'		'Create new FSM
	'		CurrFSM = New HFSM(Of SimpleEvolvingFSM_State, SimpleEvolvingFSM_Event)("TestFSM", SimpleEvolvingFSM_State.Init)
	'		Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(SimpleEvolvingFSM_State.Init, CurrFSM.CurrentState)

	'		'Configure states and transactions
	'		CurrFSM.Configure(SimpleEvolvingFSM_State.Init) _
	'			.OnExecute(AddressOf Init) _
	'			.DefaultTransaction(SimpleEvolvingFSM_State.Middle)
	'		CurrFSM.Configure(SimpleEvolvingFSM_State.Middle) _
	'			.OnExecute(AddressOf Middle) _
	'			.Transaction(SimpleEvolvingFSM_Event.MoveNext, SimpleEvolvingFSM_State.Completed)

	'		'Evolve! :)
	'		CurrFSM.Evolve()
	'		Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(SimpleEvolvingFSM_State.Middle, CurrFSM.CurrentState)

	'		'Event ignored
	'		CurrFSM.Fire(SimpleEvolvingFSM_Event.NothingToDo)
	'		Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(SimpleEvolvingFSM_State.Middle, CurrFSM.CurrentState)

	'		'Event ignored
	'		CurrFSM.Evolve()
	'		Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(SimpleEvolvingFSM_State.Middle, CurrFSM.CurrentState)

	'		'Move next
	'		CurrFSM.Fire(SimpleEvolvingFSM_Event.MoveNext)
	'		Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(SimpleEvolvingFSM_State.Completed, CurrFSM.CurrentState)

	'		'Nothing to do
	'		CurrFSM.Evolve()
	'		Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(SimpleEvolvingFSM_State.Completed, CurrFSM.CurrentState)

	'		Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(1, SimpleEvolvingFSM_InitCount)
	'		Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(1, SimpleEvolvingFSM_MiddleCount)

	'	End Sub

	'#End Region


End Class

