/*
*	Empyrean Medical Systems
*	08/2018
*	Project: Gryphon System Control Firmware
*	Module: QC well
*	Author: Carlton Chow
*	Description:
*/


#ifndef QC_WELL_H_
#define QC_WELL_H_

#define QC_RX_START_BYTE		0x80
#define QC_RX_START_VALUE		(uint16_t)(0x8080)
#define QC_RX_START_COUNT		2
#define QC_DATA_COUNT			48
//size is start count + data bytes + crc bytes
#define QC_RX_SIZE				(QC_RX_START_COUNT+QC_DATA_COUNT+1)*(sizeof(uint16_t))
#define QC_RX_CRC_POS			QC_RX_START_COUNT+QC_DATA_COUNT

#define QC_TX_START_BYTE		0xA5
#define QC_TX_CMD_BYTE			120
#define QC_TX_CHECK_BYTE		(0xFF-QC_TX_CMD_BYTE)

#define QC_ERROR_VALUE			-4000
#define QC_VOLTAGE_SCALE		16000
//Set to NaN but do not use full "F" as that is reserved for PC sync value
#define QC_NAN_OUTPUT			0xFFFFFFF0


#define QC_CHECK_MIN_SEC	40


void init_qc_well();
void process_qc();
void request_qc_info(float seconds);

extern uint16_t qc_raw_buf[QC_DATA_COUNT];


#endif /* QC_WELL_H_ */