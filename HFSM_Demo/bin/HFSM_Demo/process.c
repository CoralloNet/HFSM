

#include "process.h"


static unsigned char Process_StateEntry = 1;
static Process_EnStates Process_CurrState;

void FSM_Process_Restart(void){
	Process_CurrState = PROCESS_INIT;
}

Process_EnStates FSM_Process(void * Parameters){
	switch (Process_CurrState){

		case PROCESS_INIT:{
			Process_EnEvents TmpEvent;

			TmpEvent = FSM_Process_ProcessInit(Parameters);
			switch (TmpEvent){
				default:{ Process_CurrState = PROCESS_READY; break; }
			}

			if (Process_CurrState != PROCESS_INIT){
				Process_StateEntry = 1;
			}

			break;
		}

		case PROCESS_READY:{
			Process_EnEvents TmpEvent;

			TmpEvent = FSM_Process_ProcessReady(Parameters);
			switch (TmpEvent){
				case EVENT_DISPATCH:{ Process_CurrState = PROCESS_RUNNING; break; }
			}

			if (Process_CurrState != PROCESS_READY){
				Process_StateEntry = 1;
			}

			break;
		}

		case PROCESS_RUNNING:{
			Process_EnEvents TmpEvent;

			if (Process_StateEntry == 1){
				FSM_PROCESS_RUN_DETAIL_Restart();
				Process_StateEntry = 0;
			}

			TmpEvent = FSM_PROCESS_RUN_DETAIL(Parameters);
			switch (TmpEvent){
				case DETAIL_INTERRUPT:{ Process_CurrState = PROCESS_READY; break; }
				case DETAIL_WAITING:{ Process_CurrState = PROCESS_WAITING; break; }
				case DETAIL_TERMINATED:{ Process_CurrState = PROCESS_TERMINATED; break; }
			}

			if (Process_CurrState != PROCESS_RUNNING){
				Process_StateEntry = 1;
			}

			break;
		}

		case PROCESS_WAITING:{
			Process_EnEvents TmpEvent;

			TmpEvent = FSM_Process_ProcessWaiting(Parameters);
			switch (TmpEvent){
				case EVENT_IO_READY:{ Process_CurrState = PROCESS_READY; break; }
			}

			if (Process_CurrState != PROCESS_WAITING){
				Process_StateEntry = 1;
			}

			break;
		}

	}
	return Process_CurrState;
}


static unsigned char PROCESS_RUN_DETAIL_StateEntry = 1;
static PROCESS_RUN_DETAIL_EnStates PROCESS_RUN_DETAIL_CurrState;

void FSM_PROCESS_RUN_DETAIL_Restart(void){
	PROCESS_RUN_DETAIL_CurrState = PROCESS_INIT;
}

PROCESS_RUN_DETAIL_EnStates FSM_PROCESS_RUN_DETAIL(void * Parameters){
	switch (PROCESS_RUN_DETAIL_CurrState){

		case DETAIL_INIT:{
			PROCESS_RUN_DETAIL_EnEvents TmpEvent;

			TmpEvent = FSM_PROCESS_RUN_DETAIL_ProcessDetailInit(Parameters);
			switch (TmpEvent){
				default:{ PROCESS_RUN_DETAIL_CurrState = DETAIL_RUNNING; break; }
			}

			if (PROCESS_RUN_DETAIL_CurrState != DETAIL_INIT){
				PROCESS_RUN_DETAIL_StateEntry = 1;
			}

			break;
		}

		case DETAIL_RUNNING:{
			PROCESS_RUN_DETAIL_EnEvents TmpEvent;

			TmpEvent = FSM_PROCESS_RUN_DETAIL_ProcessDetailRunning(Parameters);
			switch (TmpEvent){
				case EVENT_INTERRUPT:{ PROCESS_RUN_DETAIL_CurrState = DETAIL_INTERRUPT; break; }
				case EVENT_IO_WAIT:{ PROCESS_RUN_DETAIL_CurrState = DETAIL_WAITING; break; }
				case EVENT_EXIT:{ PROCESS_RUN_DETAIL_CurrState = DETAIL_TERMINATED; break; }
			}

			if (PROCESS_RUN_DETAIL_CurrState != DETAIL_RUNNING){
				PROCESS_RUN_DETAIL_StateEntry = 1;
			}

			break;
		}

		case DETAIL_INTERRUPT:{
			PROCESS_RUN_DETAIL_EnEvents TmpEvent;

			switch (TmpEvent){
			}

			if (PROCESS_RUN_DETAIL_CurrState != DETAIL_INTERRUPT){
				PROCESS_RUN_DETAIL_StateEntry = 1;
			}

			break;
		}

		case DETAIL_WAITING:{
			PROCESS_RUN_DETAIL_EnEvents TmpEvent;

			switch (TmpEvent){
			}

			if (PROCESS_RUN_DETAIL_CurrState != DETAIL_WAITING){
				PROCESS_RUN_DETAIL_StateEntry = 1;
			}

			break;
		}

		case DETAIL_TERMINATED:{
			PROCESS_RUN_DETAIL_EnEvents TmpEvent;

			switch (TmpEvent){
			}

			if (PROCESS_RUN_DETAIL_CurrState != DETAIL_TERMINATED){
				PROCESS_RUN_DETAIL_StateEntry = 1;
			}

			break;
		}

	}
	return PROCESS_RUN_DETAIL_CurrState;
}
