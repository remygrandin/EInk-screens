#include "Screen1.h"

void Screen1::Init()
{
	// Init I2C
	Wire = SlowSoftWire(S1P_SDA, S1P_SCL);
	Wire.begin();

	// Init all Pins
	pinMode(S1P_WAKUP, OUTPUT);

	pinMode(S1P_CE11, OUTPUT);
	pinMode(S1P_CE12, OUTPUT);

	pinMode(S1P_CE21, OUTPUT);
	pinMode(S1P_CE22, OUTPUT);

	pinMode(S1P_CKV1, OUTPUT);
	pinMode(S1P_CKV2, OUTPUT);

	pinMode(S1P_SPV1, OUTPUT);
	pinMode(S1P_SPV2, OUTPUT);

	pinMode(S1P_GMODE1, OUTPUT);
	pinMode(S1P_GMODE2, OUTPUT);

	pinMode(S1P_D0, OUTPUT);
	pinMode(S1P_D1, OUTPUT);
	pinMode(S1P_D2, OUTPUT);
	pinMode(S1P_D3, OUTPUT);
	pinMode(S1P_D4, OUTPUT);
	pinMode(S1P_D5, OUTPUT);
	pinMode(S1P_D6, OUTPUT);
	pinMode(S1P_D7, OUTPUT);

	pinMode(S1P_SPH, OUTPUT);

	pinMode(S1P_OE, OUTPUT);
	pinMode(S1P_CL, OUTPUT);
	pinMode(S1P_LE, OUTPUT);

	/*
	DirectRegisterEnable(S1P_D0);
	DirectRegisterEnable(S1P_D1);
	DirectRegisterEnable(S1P_D2);
	DirectRegisterEnable(S1P_D3);

	DirectRegisterEnable(S1P_D4);
	DirectRegisterEnable(S1P_D5);
	DirectRegisterEnable(S1P_D6);
	DirectRegisterEnable(S1P_D7);*/

	digitalWrite(S1P_WAKUP, HIGH);

	//*__digitalPinToPortRegOWER(S1P_D0) = BITS_OFFSET((int)B11111111, 12);
}


void Screen1::StartFrame()
{
	digitalWrite(S1P_CKV1, HIGH);
	digitalWrite(S1P_CKV2, HIGH);
}

void Screen1::EndFrame()
{
	digitalWrite(S1P_CKV1, LOW);
	digitalWrite(S1P_CKV2, LOW);

	digitalWrite(S1P_CE11, LOW);
	digitalWrite(S1P_CE12, LOW);

	digitalWrite(S1P_CE21, LOW);
	digitalWrite(S1P_CE22, LOW);

	digitalWrite(S1P_CKV1, LOW);
	digitalWrite(S1P_CKV2, LOW);

	digitalWrite(S1P_SPV1, LOW);
	digitalWrite(S1P_SPV2, LOW);

	digitalWrite(S1P_GMODE1, LOW);
	digitalWrite(S1P_GMODE2, LOW);

	digitalWrite(S1P_D0, LOW);
	digitalWrite(S1P_D1, LOW);
	digitalWrite(S1P_D2, LOW);
	digitalWrite(S1P_D3, LOW);
	digitalWrite(S1P_D4, LOW);
	digitalWrite(S1P_D5, LOW);
	digitalWrite(S1P_D6, LOW);
	digitalWrite(S1P_D7, LOW);

	digitalWrite(S1P_SPH, LOW);

	digitalWrite(S1P_OE, LOW);
	digitalWrite(S1P_CL, LOW);
	digitalWrite(S1P_LE, LOW);



}

void Screen1::CustomPulse(int GMODE1, int GMODE2, int CE11, int CE12, int CE21, int CE22)
{
	digitalWrite(S1P_GMODE1, GMODE1);
	digitalWrite(S1P_GMODE2, GMODE2);

	digitalWrite(S1P_CE11, CE11);
	digitalWrite(S1P_CE12, CE12);
	digitalWrite(S1P_CE21, CE21);
	digitalWrite(S1P_CE22, CE22);

	digitalWrite(S1P_CKV1, HIGH);
	digitalWrite(S1P_CKV2, HIGH);

	CKVPulse();

	digitalWrite(S1P_SPV1, HIGH);
	digitalWrite(S1P_SPV2, HIGH);

	CKVPulse();
	CKVPulse();
	CKVPulse();

	digitalWrite(S1P_SPV1, LOW);
	digitalWrite(S1P_SPV2, LOW);

}

void Screen1::FirtstHalfPulse()
{
	digitalWrite(S1P_GMODE1, HIGH);
	digitalWrite(S1P_GMODE2, HIGH);

	digitalWrite(S1P_CE11, LOW);
	digitalWrite(S1P_CE12, LOW);
	digitalWrite(S1P_CE21, LOW);
	digitalWrite(S1P_CE22, HIGH);

	digitalWrite(S1P_SPV1, HIGH);
	digitalWrite(S1P_SPV2, HIGH);

	digitalWrite(S1P_CKV1, LOW);
	digitalWrite(S1P_CKV2, LOW);

	digitalWrite(S1P_SPV1, LOW);
	digitalWrite(S1P_SPV2, LOW);

	digitalWrite(S1P_CKV1, HIGH);
	digitalWrite(S1P_CKV2, HIGH);

	digitalWrite(S1P_SPV1, HIGH);
	digitalWrite(S1P_SPV2, HIGH);
}

void Screen1::SecondHalfPulse()
{
	digitalWrite(S1P_GMODE1, HIGH);
	digitalWrite(S1P_GMODE2, HIGH);

	digitalWrite(S1P_CE11, LOW);
	digitalWrite(S1P_CE12, LOW);
	digitalWrite(S1P_CE21, HIGH);
	digitalWrite(S1P_CE22, LOW);

	digitalWrite(S1P_SPV1, HIGH);
	digitalWrite(S1P_SPV2, HIGH);

	digitalWrite(S1P_CKV1, LOW);
	digitalWrite(S1P_CKV2, LOW);

	digitalWrite(S1P_SPV1, LOW);
	digitalWrite(S1P_SPV2, LOW);

	digitalWrite(S1P_CKV1, HIGH);
	digitalWrite(S1P_CKV2, HIGH);

	digitalWrite(S1P_SPV1, HIGH);
	digitalWrite(S1P_SPV2, HIGH);

}

void Screen1::CKVPulse() {

	digitalWrite(S1P_CKV1, LOW);
	digitalWrite(S1P_CKV2, LOW);

	digitalWrite(S1P_CKV1, HIGH);
	digitalWrite(S1P_CKV2, HIGH);
}

void Screen1::CLPulse()
{
	digitalWrite(S1P_CL, HIGH);
	digitalWrite(S1P_CL, LOW);
}




void Screen1::StartRow()
{
	digitalWrite(S1P_SPH, HIGH);
}


void Screen1::EndRow()
{
	digitalWrite(S1P_SPH, LOW);
}

void Screen1::WriteByte(bool D7, bool D6, bool D5, bool D4, bool D3, bool D2, bool D1, bool D0)
{
	digitalWrite(S1P_D7, D0 ? HIGH : LOW);
	digitalWrite(S1P_D6, D0 ? LOW : HIGH);
	digitalWrite(S1P_D5, D1 ? HIGH : LOW);
	digitalWrite(S1P_D4, D1 ? LOW : HIGH);

	digitalWrite(S1P_D3, D2 ? HIGH : LOW);
	digitalWrite(S1P_D2, D2 ? LOW : HIGH);
	digitalWrite(S1P_D1, D3 ? HIGH : LOW);
	digitalWrite(S1P_D0, D3 ? LOW : HIGH);

	CLPulse();

	digitalWrite(S1P_D7, D4 ? HIGH : LOW);
	digitalWrite(S1P_D6, D4 ? LOW : HIGH);
	digitalWrite(S1P_D5, D5 ? HIGH : LOW);
	digitalWrite(S1P_D4, D5 ? LOW : HIGH);

	digitalWrite(S1P_D3, D6 ? HIGH : LOW);
	digitalWrite(S1P_D2, D6 ? LOW : HIGH);
	digitalWrite(S1P_D1, D7 ? HIGH : LOW);
	digitalWrite(S1P_D0, D7 ? LOW : HIGH);

	CLPulse();
}

void Screen1::WriteByte(uint8_t data)
{
	WriteByte(bitRead(data, 7),
		bitRead(data, 6),
		bitRead(data, 5),
		bitRead(data, 4),

		bitRead(data, 3),
		bitRead(data, 2),
		bitRead(data, 1),
		bitRead(data, 0));

}

void Screen1::WriteRow()
{
	digitalWrite(S1P_LE, HIGH);
	digitalWrite(S1P_LE, LOW);

	digitalWrite(S1P_OE, LOW);
	digitalWrite(S1P_OE, HIGH);
}

/*
void Screen1::SkipRow()
{
	digitalWrite(S1P_CKV, HIGH);
	delayMicroseconds(1);
	digitalWrite(S1P_CKV, LOW);
	delayMicroseconds(1);
}

void Screen1::ClearRow()
{
	digitalWrite(S1P_CKV, HIGH);
	delayMicroseconds(1);
	digitalWrite(S1P_CKV, LOW);
	delayMicroseconds(1);
}

void Screen1::EndFrame()
{
	digitalWrite(S1P_GMODE1, LOW);
	digitalWrite(S1P_GMODE2, LOW);
	CKVPulse();
	CKVPulse();
	CKVPulse();
	CKVPulse();
	CKVPulse();
}

void Screen1::StartRow()
{
	digitalWrite(S1P_SPH, HIGH);
	digitalWrite(S1P_LE, LOW);
	digitalWrite(S1P_OE, LOW);
	delayMicroseconds(1);
	digitalWrite(S1P_SPH, LOW);
	delayMicroseconds(1);
}

void Screen1::WriteRowData(std::vector<uint8_t> data)
{
	size_t vectorSize = data.size();
	for (int i = 0; i < vectorSize; ++i) {
		uint8_t currentByte = data[i];

		WriteByte(currentByte);

	}
}

void Screen1::EndRow()
{
	digitalWrite(S1P_SPH, HIGH);
	delayMicroseconds(1);
	CLPulse();

	digitalWrite(S1P_LE, HIGH);
	delayMicroseconds(1);
	digitalWrite(S1P_LE, LOW);
	delayMicroseconds(1);
}
*/
Screen1::Screen1()
{
}
