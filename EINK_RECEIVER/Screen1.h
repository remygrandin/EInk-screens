#pragma once
#include <stdint.h>
#include <ArduinoSTL.h>
#include <vector>
#include "SlowSoftWire.h"
 #include "dueDigitalWriteFast.h"

const char S1P_WAKUP = 31;
/*
const char S1P_CKV = 9;
const char S1P_SPV = 8;
*/

const char S1P_CE21 = 8;
const char S1P_CE22 = 9;

const char S1P_CE11 = 10;
const char S1P_CE12 = 11;

const char S1P_SPV1 = 12;
const char S1P_SPV2 = 13;

const char S1P_CKV1 = 5;
const char S1P_CKV2 = 6;


const char S1P_SCL = 41;
const char S1P_SDA = 39;

const char S1P_GMODE1 = 3;
const char S1P_GMODE2 = 4;

const char S1P_D0 = 44;
const char S1P_D1 = 45;
const char S1P_D2 = 46;
const char S1P_D3 = 47;
const char S1P_D4 = 48;
const char S1P_D5 = 49;
const char S1P_D6 = 50;
const char S1P_D7 = 51;

const char S1P_SPH = 36;

const char S1P_OE = 14;
const char S1P_CL = 15;
const char S1P_LE = 16;

class Screen1 
{
public:
	Screen1();
	

	uint8_t I2CAdress = 104; // 0x68

	SlowSoftWire Wire;

	void Init();

	// Gate Actions (line selection)
	void StartFrame();

	void CustomPulse(int GMODE1, int GMODE2, int CE11, int CE12, int CE21, int CE22);


	void FirtstHalfPulse();

	void SecondHalfPulse();

	void WriteRow();

	void SkipRow();

	void ClearRow();

	void EndFrame();

	// Source Actions (row writing)
	void StartRow();

	void WriteRowData(std::vector<uint8_t> data);

	void EndRow();

	void WriteByte(bool D7, bool D6, bool D5, bool D4, bool D3, bool D2, bool D1, bool D0);
	void WriteByte(uint8_t data);

	void CKVPulse();
	void CLPulse();
private:

};