
typedef enum {
	PROCESS_INIT,
	PROCESS_READY,
	PROCESS_RUNNING,
	PROCESS_WAITING,
	PROCESS_TERMINATED,
} Process_EnStates;

typedef enum {
	EVENT_DISPATCH,
	EVENT_IO_READY,
} Process_EnEvents;

void FSM_Process_Restart(void);
Process_EnStates FSM_Process(void * Parameters);



typedef enum {
	DETAIL_INIT,
	DETAIL_RUNNING,
	DETAIL_INTERRUPT,
	DETAIL_WAITING,
	DETAIL_TERMINATED,
} PROCESS_RUN_DETAIL_EnStates;

typedef enum {
	EVENT_INTERRUPT,
	EVENT_IO_WAIT,
	EVENT_EXIT,
} PROCESS_RUN_DETAIL_EnEvents;

void FSM_PROCESS_RUN_DETAIL_Restart(void);
PROCESS_RUN_DETAIL_EnStates FSM_PROCESS_RUN_DETAIL(void * Parameters);


