
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


