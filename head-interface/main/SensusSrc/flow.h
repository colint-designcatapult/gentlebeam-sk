#ifndef SENSUSSRC_FLOW_H_
#define SENSUSSRC_FLOW_H_


#define NUM_FLOW_SAMPLES	11
#define TICKS_TO_LPM		((float)(916.66/ (NUM_FLOW_SAMPLES-1)))

void init_flow();
void process_flow();


#endif /* FLOW_H_ */
