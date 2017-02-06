

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
