#include "stm32f4xx_hal.h"
#include "string.h"
#include "stdbool.h"
#include "main.h"
#define DS2482
#include "1wire_ds2482.h"
#include "sys_data.h"
#include "qc.h"

#define POLL_LIMIT (200)

static unsigned char dscrc_table[] = {
0, 94,188,226, 97, 63,221,131,194,156,126, 32,163,253, 31, 65,
157,195, 33,127,252,162, 64, 30, 95, 1,227,189, 62, 96,130,220,
35,125,159,193, 66, 28,254,160,225,191, 93, 3,128,222, 60, 98,
190,224, 2, 92,223,129, 99, 61,124, 34,192,158, 29, 67,161,255,
70, 24,250,164, 39,121,155,197,132,218, 56,102,229,187, 89, 7,
219,133,103, 57,186,228, 6, 88, 25, 71,165,251,120, 38,196,154,
101, 59,217,135, 4, 90,184,230,167,249, 27, 69,198,152,122, 36,
248,166, 68, 26,153,199, 37,123, 58,100,134,216, 91, 5,231,185,
140,210, 48,110,237,179, 81, 15, 78, 16,242,172, 47,113,147,205,
17, 79,173,243,112, 46,204,146,211,141,111, 49,178,236, 14, 80,
175,241, 19, 77,206,144,114, 44,109, 51,209,143, 12, 82,176,238,
50,108,142,208, 83, 13,239,177,240,174, 76, 18,145,207, 45,115,
202,148,118, 40,171,245, 23, 73, 8, 86,180,234,105, 55,213,139,
87, 9,235,181, 54,104,138,212,149,203, 41,119,244,170, 72, 22,
233,183, 85, 11,136,214, 52,106, 43,117,151,201, 74, 20,246,168,
116, 42,200,150, 21, 75,169,247,182,232, 10, 84,215,137,107, 53};

// Search state
/** ROM_NO is a global variable to hold the ROM number of the SHA-1 device */
uchar ROM_NO[8];
int LastDiscrepancy;
int LastFamilyDiscrepancy;
int LastDeviceFlag;
uchar crc8;
uint8_t oneW_status = 0;
int rslt, cnt;

// DS2482 state
static uchar I2C_address;
int short_detected;
int c1WS, cSPU, cPPM, cAPU;

// Internal I2C Write
HAL_StatusTypeDef I2C_Write(uint8_t *data, uint16_t length) {
    return HAL_I2C_Master_Transmit(&hi2c2, I2C_address, data, length, HAL_MAX_DELAY);
}

// Internal I2C Read
HAL_StatusTypeDef I2C_Read(uint8_t *data, uint16_t length) {
    return HAL_I2C_Master_Receive(&hi2c2, I2C_address, data, length, HAL_MAX_DELAY);
}

uchar calc_crc8(uchar data);

#if(1)
uchar Wait_For_1WB()
{
	uchar status;
	int poll_count = 0;
	do
	{
	   HAL_StatusTypeDef ret = I2C_Read(&status, 1);

	   if(ret != HAL_OK)
	   {
		   return 0;
	   }

	} while ((status & STATUS_1WB) && (poll_count++ < POLL_LIMIT));

	// check for failure due to poll limit reached
	if (poll_count >= POLL_LIMIT)
	{
		// handle error
		// ...
		//DS2482_reset();
		return 0;
	 }

	 return status;
}
#endif


/**
* Send 8 bits of communication to the 1-Wire Net and verify that the
* 8 bits read from the 1-Wire Net is the same (write operation).
* The parameter 'sendbyte' least significant 8 bits are used.
*
* @param[in] sendbyte
* 8 bits to send (least significant byte)
*
* @return TRUE - bytes written and echo was the same @n
*         FALSE - echo was not the same
*/
void OWWriteByte(uchar sendbyte)
{
   // 1-Wire Write Byte (Case B)
   //   S AD,0 [A] 1WWB [A] DD [A] Sr AD,1 [A] [Status] A [Status] A\ P
   //                                          \--------/
   //                             Repeat until 1WB bit has changed to 0
   //  [] indicates from slave
   //  DD data to write

   uchar status;
   int poll_count = 0;

   uint8_t cmd[2] = { CMD_1WWB, sendbyte };

   if (I2C_Write((uint8_t *)&cmd[0], 2) != HAL_OK) {
		DS2482_reset();
		return;
   }

   do
   {
	   HAL_StatusTypeDef ret = I2C_Read(&status, 1);

	   if(ret != HAL_OK)
	   {
			DS2482_reset();
			return;
	   }

	} while ((status & STATUS_1WB) && (poll_count++ < POLL_LIMIT));

	// check for failure due to poll limit reached
	if (poll_count >= POLL_LIMIT)
	{
		// handle error
		// ...
		DS2482_reset();
		return;
	 }
}


/**
* Send 8 bits of read communication to the 1-Wire Net and return the
* result 8 bits read from the 1-Wire Net.
*
* @return  8 bits read from 1-Wire Net
*/
uchar OWReadByte(void)
{
   uchar data;
   HAL_StatusTypeDef ret;

   /* 1-Wire Read Bytes (Case C)
      S AD,0 [A] 1WRB [A] Sr AD,1 [A] [Status] A [Status] A\
                                      \--------/
                        Repeat until 1WB bit has changed to 0
      Sr AD,0 [A] SRP [A] E1 [A] Sr AD,1 [A] DD A\ P

     [] indicates from slave
     DD data read
   */

	uint8_t cmd = CMD_1WRB;

	if (I2C_Write(&cmd, 1) != HAL_OK) {
		return 0xFF;
	}

	if( Wait_For_1WB() == 0) //Fail to detect 1WB idle
	{
		return 0xFF;
	}

	//Write command 1WRB is successfully transfer Set Read Pointer to Read_Data_Reg

	uchar buf[2] = { CMD_SRP, READ_DATA_REG };

	if ( I2C_Write((uint8_t *)&buf[0], 2) != HAL_OK )
	{
		return 0xFF;
	}

	// Set Read Pointer Successfully read back ROM data
	ret = I2C_Read(&data, 1);

	if(ret != HAL_OK)
	{
		return 0xFF;
	}

	return data;

};

//---------------------------------------------------------------------------
//-------- DS2482 Helper functions
//---------------------------------------------------------------------------

/**
* @internal
*
* DS2482 Detect routine that sets the I2C address and then performs a
* device reset followed by writing the configuration byte to default values: @n
* 1-Wire speed (c1WS) = standard (0) @n
* Strong pull-up (cSPU) = off (0) @n
* Presence pulse masking (cPPM) = off (0) @n
* Active pull-up (cAPU) = on (CONFIG_APU = 0x01) @n
*
* @param addr
* Global I2C address
*
* @return
* true if device was detected and written
* false if device not detected or failure to write configuration byte
*
* @endinternal
*/
int DS2482_detect(uchar addr)
{
   // set global address
   I2C_address = addr;

   // reset the DS2482 ON selected address
   if (!DS2482_reset())
      return false;

   // default configuration
   c1WS = false; //CONFIG_1WS; //OD
   cSPU = false;
   cPPM = false;
   cAPU = CONFIG_APU;

   // write the default configuration setup
   if (!DS2482_write_config(c1WS | cSPU | cPPM | cAPU))
      return false;

   return true;
}

/**
* @internal
*
* Perform a device reset on the DS2482
*
* @return
* true if device was reset @n
* false if device not detected or failure to perform reset
*
* @endinternal
*/
int DS2482_reset(void)
{
   uchar status;

   I2C_ForceBusRecovery(&hi2c2);
   // Device Reset
   //   S AD,0 [A] DRST [A] Sr AD,1 [A] [SS] A\ P
   //  [] indicates from slave
   //  SS status byte to read to verify state

   uint8_t buf = CMD_DRST;

   I2C_Write(&buf, 1);

   I2C_Read(&status, 1);

   // check for failure due to incorrect read back of status
   return ((status & 0xF7) == 0x10);
}

/**
* @internal
*
* Write the configuration register in the DS2482. The configuration
* options are provided in the lower nibble of the provided config byte.
* The uppper nibble in bitwise inverted when written to the DS2482.
*
* @param config
* single byte that represents the DS2482's configuration register
*
* @return
* true config written and response correct @n
* false response incorrect
*
* @endinternal
*/
int DS2482_write_config(uint8_t config)
{
   uint8_t read_config;
   //int result;

   // Write configuration (Case A)
   //   S AD,0 [A] WCFG [A] CF [A] Sr AD,1 [A] [CF] A\ P
   //  [] indicates from slave
   //  CF configuration byte to write

	uint8_t data[2] = { CMD_WCFG, (config | (~config << 4)) };

	HAL_StatusTypeDef ret = I2C_Write(data, 2);

	if(ret != HAL_OK)
	{
		//printf("Write Config Error!\n");
		return 0;
	}

	ret = I2C_Read(&read_config, 1);

   // check for failure due to incorrect read back
   if (config != read_config)
   {
      // handle error
      // ...
      DS2482_reset();

      return 0;
   }

   return true;
}

/**
* @internal
*
* Select the 1-Wire channel on a DS2482-800.
*
* @param[in] channel
* Integer that represents the DS2482-800 channel
*
* @return
* true if channel selected @n
* false device not detected or failure to perform select
*
* @endinternal
*/
//int DS2482_channel_select(int channel)
//{
//   uchar ch, ch_read, check;
//   int result;
//
//   // Channel Select (Case A)
//   //   S AD,0 [A] CHSL [A] CC [A] Sr AD,1 [A] [RR] A\ P
//   //  [] indicates from slave
//   //  CC channel value
//   //  RR channel read back
//
//   I2C_start();
//   I2C_write(I2C_address | I2C_WRITE, EXPECT_ACK);
//   I2C_write(CMD_CHSL, EXPECT_ACK);
//
//   switch (channel)
//   {
//      default: case 0: ch = 0xF0; ch_read = 0xB8; break;
//      case 1: ch = 0xE1; ch_read = 0xB1; break;
//      case 2: ch = 0xD2; ch_read = 0xAA; break;
//      case 3: ch = 0xC3; ch_read = 0xA3; break;
//      case 4: ch = 0xB4; ch_read = 0x9C; break;
//      case 5: ch = 0xA5; ch_read = 0x95; break;
//      case 6: ch = 0x96; ch_read = 0x8E; break;
//      case 7: ch = 0x87; ch_read = 0x87; break;
//   };
//
//   I2C_write(ch, EXPECT_ACK);
//   I2C_rep_start();
//   I2C_write(I2C_address | I2C_READ, EXPECT_ACK);
//   check = I2C_read(NACK,&result);
//   I2C_stop();
//
//   // check for failure due to incorrect read back of channel
//   return (check == ch_read);
//}

//---------------------------------------------------------------------------
//-------- Basic 1-Wire functions
//---------------------------------------------------------------------------

/**
* Reset all of the devices on the 1-Wire Net and return the result.
*
* @return
* true(1)  presense pulse(s) detected, device(s) reset @n
* false(0) no presense pulses detected
*/
int OWReset(void)
{
	// 1-Wire reset (Case B)
	//   S AD,0 [A] 1WRS [A] Sr AD,1 [A] [Status] A [Status] A\ P
	//                                   \--------/
	//                       Repeat until 1WB bit has changed to 0
	//  [] indicates from slave

	uchar status;
	int poll_count = 0;

	uint8_t cmd = CMD_1WRS;

	if (I2C_Write(&cmd, 1) != HAL_OK) {
		return 0;
	}

	do
	{
	   HAL_StatusTypeDef ret = I2C_Read(&status, 1);

	   if(ret != HAL_OK)
	   {
		   return 0;
	   }

	} while ((status & STATUS_1WB) && (poll_count++ < POLL_LIMIT));

	// check for failure due to poll limit reached
	if (poll_count >= POLL_LIMIT)
	{
		// handle error
		// ...
		DS2482_reset();
		return 0;
	 }

	// check for short condition
	if (status & STATUS_SD)
	  short_detected = 1;
	else
	  short_detected = 0;

	// check for presence detect
	if (status & STATUS_PPD)
	  return 1;
	else
	  return 0;
}


/**
* Find the 'first' devices on the 1-Wire network
* @return
* true device found, ROM number in ROM_NO buffer @n
* false no device present
*/
int OWFirst(void)
{
   // reset the search state
   LastDiscrepancy       = 0;
   LastDeviceFlag        = 0;
   LastFamilyDiscrepancy = 0;

   return OWSearch();
}

/**
* Find the 'next' devices on the 1-Wire network
* @return true device found, ROM number in ROM_NO buffer @n
*         false device not found, end of search
*/
int OWNext()
{
   // leave the search state alone
   return OWSearch();
}

/**
* Verify the device with the ROM number in ROM_NO buffer is present.
* @return
* true device verified present @n
* false device not present
*/
int OWVerify(void)
{
   uchar rom_backup[8];
   int i,rslt,ld_backup,ldf_backup,lfd_backup;

   // keep a backup copy of the current state
   for (i = 0; i < 8; i++)
      rom_backup[i] = ROM_NO[i];
   ld_backup = LastDiscrepancy;
   ldf_backup = LastDeviceFlag;
   lfd_backup = LastFamilyDiscrepancy;

   // set search to find the same device
   LastDiscrepancy = 64;
   LastDeviceFlag = false;

   if (OWSearch())
   {
      // check if same device found
      rslt = true;
      for (i = 0; i < 8; i++)
      {
         if (rom_backup[i] != ROM_NO[i])
         {
            rslt = false;
            break;
         }
      }
   }
   else
     rslt = false;

   // restore the search state
   for (i = 0; i < 8; i++)
      ROM_NO[i] = rom_backup[i];
   LastDiscrepancy = ld_backup;
   LastDeviceFlag = ldf_backup;
   LastFamilyDiscrepancy = lfd_backup;

   // return the result of the verify
   return rslt;
}


/**
* The 'OWSearch' function does a general search.  This function
* continues from the previous search state. The search state
* can be reset by using the 'OWFirst' function.
* This function contains one parameter 'alarm_only'.
* When 'alarm_only' is true (1) the find alarm command
* 0xEC is sent instead of the normal search command 0xF0.
* Using the find alarm command 0xEC will limit the search to only
* 1-Wire devices that are in an 'alarm' state.
*
* @return
* true (1) when a 1-Wire device was found and it's @n
* Serial Number placed in the global ROM  @n @n
* false (0) when no new device was found.  Either the @n
* last search was the last device or there @n
* are no devices on the 1-Wire Net.
*/
int OWSearch()
{
	int id_bit_number;
	int last_zero, rom_byte_number, search_result;
	int id_bit, cmp_id_bit;
	uchar rom_byte_mask, search_direction;

	// init for search
	id_bit_number   = 1;
	last_zero       = 0;
	rom_byte_number = 0;
	rom_byte_mask   = 1;
	search_result   = 0;
	crc8            = 0;

	// if the last call was not the last one
	if(!LastDeviceFlag)
	{
		// 1-Wire Rest
		if(!OWReset())
		{
			// reset the search
			// reset the search
			LastDiscrepancy = 0;
			LastDeviceFlag = 0;
			LastFamilyDiscrepancy = 0;
			return 0;
		}

		// issue the search command
		OWWriteByte(0xF0);

	    // loop to do the search
	    do
	    {
			// if this discrepancy if before the Last Discrepancy
			// on a previous next then pick the same as last time
			if (id_bit_number < LastDiscrepancy)
			{
			   if ((ROM_NO[rom_byte_number] & rom_byte_mask) > 0)
				   search_direction = 1;
			   else
				   search_direction = 0;
			}
			else
			{
				// if equal to last pick 1, if not then pick 0
				if (id_bit_number == LastDiscrepancy)
				   search_direction = 1;
				else
				   search_direction = 0;
			}

			// Peform a triple operation on the DS2482 which will perform 2 read bits and 1 write bit
			uchar status = DS2482_search_triplet(search_direction);

			// check bit results in status byte
			id_bit     = ((status & STATUS_SBR) == STATUS_SBR);
			cmp_id_bit = ((status & STATUS_TSB) == STATUS_TSB);
		    search_direction = ((status & STATUS_DIR) == STATUS_DIR) ? 1 : 0;

			 // check for no devices on 1-wire
			 if ((id_bit) && (cmp_id_bit))
				break;
			 else
			 {
				if ((!id_bit) && (!cmp_id_bit) && (search_direction == 0))
				{
				   last_zero = id_bit_number;

				   // check for Last discrepancy in family
				   if (last_zero < 9)
					  LastFamilyDiscrepancy = last_zero;
				}

			// set or clear the bit in the ROM byte rom_byte_number
			// with mask rom_byte_mask
			if (search_direction == 1)
			   ROM_NO[rom_byte_number] |= rom_byte_mask;
			else
			   ROM_NO[rom_byte_number] &= (uchar)~rom_byte_mask;

			// increment the byte counter id_bit_number
			// and shift the mask rom_byte_mask
			id_bit_number++;
			rom_byte_mask <<= 1;

			// if the mask is 0 then go to new SerialNum byte rom_byte_number and reset mask
			if (rom_byte_mask == 0)
			{
			   calc_crc8(ROM_NO[rom_byte_number]);  // accumulate the CRC
			   rom_byte_number++;
			   rom_byte_mask = 1;
			}
		 }
	  }
	  while(rom_byte_number < 8);  // loop until through all ROM bytes 0-7

	  // if the search was successful then
	  if (!((id_bit_number < 65) || (crc8 != 0)))
	  {
		 // search successful so set LastDiscrepancy,LastDeviceFlag,search_result
		 LastDiscrepancy = last_zero;

		 // check for last device
		 if (LastDiscrepancy == 0)
			LastDeviceFlag = 1;

		 search_result = 1;
	  }
	}

	// if no device found then reset counters so next 'search' will be like a first
	if (!search_result || (ROM_NO[0] == 0))
	{
	  LastDiscrepancy = 0;
	  LastDeviceFlag = 0;
	  LastFamilyDiscrepancy = 0;
	  search_result = 0;
	}

	return search_result;
}

void DS2484_ReadRom()
{
//	//send out write byte read ROM to OW chip
//    int i;
//    crc8 = 0;
//
//    if(OWWriteByte( OW_CMD_READ_ROM ))
//	{
//		for(i = 0; i < 8; i++)
//		{
//			ROM_NO[i] = OWReadByte();
//		}
//	}
}

uchar calc_crc8(uchar data)
{
   int i;

   // See Application Note 27
   crc8 = crc8 ^ data;
   for (i = 0; i < 8; ++i)
   {
      if (crc8 & 1)
         crc8 = (crc8 >> 1) ^ 0x8c;
      else
         crc8 = (crc8 >> 1);
   }

   return crc8;
}

/**
* Send 8 bits of communication to the 1-Wire Net and verify that the
* 8 bits read from the 1-Wire Net is the same (write operation).
* The parameter 'sendbyte' least significant 8 bits are used.  After the
* 8 bits are sent change the level of the 1-Wire net.
*
* @param[in] sendbyte
* 8 bits to send (least significant bit)
*/
void OWWriteBytePower(uchar sendbyte)
{
   // set strong pull-up enable
   cSPU = CONFIG_SPU;

   // write the new config
   if (!DS2482_write_config(c1WS | cSPU | cPPM | cAPU))
      return;

   // perform write byte
   OWWriteByte(sendbyte);
}

/**
* Read 8 bits of communication from the 1-Wire Net.  After the
* 8 bits are read then change the level of the 1-Wire net.
*
* @return 8 bits read from 1-Wire Net
*/
uchar OWReadBytePower(void)
{
   // set strong pull-up enable
   cSPU = CONFIG_SPU;

   // write the new config
   if (!DS2482_write_config(c1WS | cSPU | cPPM | cAPU))
      return 0;

   // do the read byte
   return OWReadByte();
}

/**
* Send 1 bit of communication to the 1-Wire Net.
* The parameter 'sendbit' least significant bit is used.
*
* @param[in] sendbit
* 1 bit to send (least significant byte)
*/
void OWWriteBit(uchar sendbit)
{
   OWTouchBit(sendbit);
}


/**
* Reads 1 bit of communication from the 1-Wire Net and returns the
* result
*
* @return 1 bit read from 1-Wire Net
*/
uchar OWReadBit(void)
{
   return OWTouchBit(0x01);
}

/**
* Send 1 bit of communication to the 1-Wire Net and return the
* result 1 bit read from the 1-Wire Net.  The parameter 'sendbit'
* least significant bit is used and the least significant bit
* of the result is the return bit.
*
* @param[in] sendbit
* the least significant bit is the bit to send
*
* @return
* 0   0 bit read from sendbit @n
* 1   1 bit read from sendbit
*/
uchar OWTouchBit(uchar sendbit)
{
   uchar status;
   //int poll_count = 0;
   //int result;

   // 1-Wire bit (Case B)
   //   S AD,0 [A] 1WSB [A] BB [A] Sr AD,1 [A] [Status] A [Status] A\ P
   //                                          \--------/
   //                           Repeat until 1WB bit has changed to 0
   //  [] indicates from slave
   //  BB indicates byte containing bit value in msbit
   uchar cmd[2] = { CMD_1WSB, 0};
   cmd[1] = sendbit ? 0x80 : 0x00;

	if (I2C_Write((uint8_t *)&cmd[0], 2) != HAL_OK) {
		return 0xFF; //invalid number
	}

	status = Wait_For_1WB();

	// return bit state
	if (status & STATUS_SBR)
      return 1;
	else
      return 0;
}

//--------------------------------------------------------------------------
// Calculate the CRC8 of the byte value provided with the current
// global 'crc8' value.
// Returns current global crc8 value
//
unsigned char docrc8(unsigned char value)
{
	// See Application Note 27
	// TEST BUILD
	crc8 = dscrc_table[crc8 ^ value];
	return crc8;
}

/**
* @internal
*
* Use the DS2482 help command '1-Wire triplet' to perform one bit of a 1-Wire
* search. This command does two read bits and one write bit. The write bit
* is either the default direction (all device have same bit) or in case of
* a discripancy, the 'search_direction' parameter is used.
*
* @param[in] search_direction
* an integer that represents the search direction
*
* @return
* The DS2482 status byte result from the triplet command
*
* @endinternal
*/
uchar DS2482_search_triplet(int search_direction)
{
   uchar status;
   int poll_count = 0;
   //int result;

   // 1-Wire Triplet (Case B)
   //   S AD,0 [A] 1WT [A] SS [A] Sr AD,1 [A] [Status] A [Status] A\ P
   //                                         \--------/
   //                           Repeat until 1WB bit has changed to 0
   //  [] indicates from slave
   //  SS indicates byte containing search direction bit value in msbit

   uchar cmd[2] = { CMD_1WT, 0};
   cmd[1] = search_direction ? 0x80 : 0x00;

	if (I2C_Write((uint8_t *)&cmd[0], 2) != HAL_OK) {
		return 0;
	}

   // loop checking 1WB bit for completion of 1-Wire operation
   // abort if poll limit reached

	do
	{
	   HAL_StatusTypeDef ret = I2C_Read(&status, 1);

	   if(ret != HAL_OK)
	   {
		   return 0;
	   }

	} while ((status & STATUS_1WB) && (poll_count++ < POLL_LIMIT));

	// check for failure due to poll limit reached
	if (poll_count >= POLL_LIMIT)
	{
		// handle error
		// ...
		DS2482_reset();
		return 0;
	 }

	// return status byte
    return status;
}

void init_1wire()
{
	if (DS2482_detect(0x30))
	{
		//Found DS2482 I2C 1wire transceiver Online!
		//oneW_status variable only here for debug purpose, can be removed later
		oneW_status = 0;
	}
	else
	{
	    //There is no DS2482 I2C transceiver Connected
		oneW_status = 1;
	}

	//UartPrintf("FIND ALL 1-Wire EEPROM:\n");
	cnt = 0;
	rslt = OWFirst();

	while (rslt)
	{
	 // UartPrintf("Device: %d-%02X%02X%02X%02X%02X%02X%02X%02X\n", ++cnt,
	 //		  ROM_NO[7], ROM_NO[6], ROM_NO[5], ROM_NO[4],
	 //		  ROM_NO[3], ROM_NO[2], ROM_NO[1], ROM_NO[0]);

		//double check the 56-bit of data
		int i;
		for(i = 0; i < 7; i++)
		{
			docrc8(ROM_NO[i]);
		}

		if(crc8 == ROM_NO[7])
		{
			cnt++;
		}

        rslt = OWNext();
	}

	oneW_status = cnt;
}

void get_col_id(uint8_t *buf, int size) {

	//read 1-wire now
	init_1wire();
	
    if(cnt)
	{
		for(int i = 0; i < size; i++){
			buf[i] = (uint8_t)ROM_NO[i];
		}
	}
    else
    {
    	for(int i = 0; i < size; i++){
    		buf[i] = 0;
    	}
    }
}



