
#include <SPI.h>
#include <Wire.h>
#include <Adafruit_SSD1306.h>
#include <gfxfont.h>
#include <Adafruit_GFX.h>
#include "Screen1.h"
#include "CustomFunctions.h"
#include <ArduinoSTL.h>
#include "SlowSoftWire.h"
#include <vector>
#include "Screens.h"
#include <digitalWriteFast.h>
#include <Math.h>


Adafruit_SSD1306 display(4);



union ByteArrayToInteger {
	byte byteArray[4];
	uint32_t integer;
};


Screens screens = Screens();

void setup()
{
	// Defaults
	/*
	screens.CurrentScr = screens.Scr1;*/
	//*__digitalPinToPortRegOWDR(S1P_D0) = ((uint32_t)0) - 1;

	//printText("ODSR : " + String(PIOC->PIO_ODSR, BIN) + " | OWSR : " + String(PIOC->PIO_OWSR, BIN));


	screens.Scr1->Init();
	//printText("ODSR : " + String(PIOC->PIO_ODSR, BIN) + " | OWSR : " + String(PIOC->PIO_OWSR, BIN));
	printText("Screen OK");


	pinMode(26, INPUT);
	pinMode(27, INPUT);
	pinMode(28, INPUT);
	pinMode(28, INPUT);

	pinMode(A0, INPUT);
}


void loop()
{
	if (SerialUSB.available())
		serialUSBEvent();
}


void serialUSBEvent() {
	int rawCommandCode = SerialUSB.read();

	if (rawCommandCode == -1) // In case read "fail"
		return;

	uint8_t commandCode = rawCommandCode;

	ByteArrayToInteger dataLengthConverter;
	dataLengthConverter.byteArray[0] = SerialUSB.read();
	dataLengthConverter.byteArray[1] = SerialUSB.read();
	dataLengthConverter.byteArray[2] = SerialUSB.read();
	dataLengthConverter.byteArray[3] = SerialUSB.read();


	uint32_t dataLength = dataLengthConverter.integer;

	std::vector<uint8_t> data(dataLength);
	for (int i = 0; i < dataLength; ++i) {
		data[i] = SerialUSB.read();
	}

	switch (commandCode)
	{
		// 0X : System
	case 1: // Echo
	{
		for (uint32_t i = 0; i < dataLength; ++i) {
			SerialUSB.write(data[i]);
		}
		break;
	}
	case 2: // i2c locator
	{
		SlowSoftWire Wire = SlowSoftWire(20, 21);
		Wire.begin();

		byte error, address;
		int nDevices;

		SerialUSB.println("Scanning...");

		nDevices = 0;
		for (address = 1; address < 127; address++)
		{
			// The i2c_scanner uses the return value of
			// the Write.endTransmisstion to see if
			// a device did acknowledge to the address.
			Wire.beginTransmission(address);
			error = Wire.endTransmission();

			if (error == 0)
			{
				SerialUSB.print("I2C device found at address 0x");
				if (address < 16)
					SerialUSB.print("0");
				SerialUSB.print(address, HEX);
				SerialUSB.println("  !");

				nDevices++;
			}
			else if (error == 4)
			{
				SerialUSB.print("Unknown error at address 0x");
				if (address < 16)
					SerialUSB.print("0");
				SerialUSB.println(address, HEX);
			}
		}
		if (nDevices == 0)
			SerialUSB.println("No I2C devices found");
		else
			SerialUSB.println("done");

		break;
	}

	case 3: // screen test
	{
		printText("mili : " + String(millis()));
		break;
	}

	// 1X : Ti register Get
	case 11: // Get All Register
	{
		screens.Scr1->Wire.beginTransmission(screens.Scr1->I2CAdress);
		screens.Scr1->Wire.write(0);
		screens.Scr1->Wire.endTransmission();
		screens.Scr1->Wire.requestFrom(screens.Scr1->I2CAdress, (uint8_t)17);

		while (screens.Scr1->Wire.available()) {
			int byte = screens.Scr1->Wire.read();
			SerialUSB.write((uint8_t)byte);
		}
		break;
	}
	case 12: // Get Register(s)
	{
		screens.Scr1->Wire.beginTransmission(screens.Scr1->I2CAdress);
		screens.Scr1->Wire.write(data[0]);
		screens.Scr1->Wire.endTransmission();
		screens.Scr1->Wire.requestFrom(screens.Scr1->I2CAdress, data[1]);

		while (screens.Scr1->Wire.available()) {
			int byte = screens.Scr1->Wire.read();
			SerialUSB.write((uint8_t)byte);
		}
		break;
	}
	// 2X : Ti register Set
	case 21: // Set All Register
	{
		screens.Scr1->Wire.beginTransmission(screens.Scr1->I2CAdress);
		screens.Scr1->Wire.write(0);
		for (int i = 0; i < data.size(); ++i) {
			screens.Scr1->Wire.write(data[i]);
		}
		screens.Scr1->Wire.endTransmission();
		SerialUSB.write((uint8_t)1); // return : OK/Done
		break;
	}
	case 22: // Set Register(s)
	{
		screens.Scr1->Wire.beginTransmission(screens.Scr1->I2CAdress);
		screens.Scr1->Wire.write(data[0]);
		for (int i = 1; i < data.size(); ++i) {
			screens.Scr1->Wire.write(data[i]);
		}
		screens.Scr1->Wire.endTransmission();
		SerialUSB.write((uint8_t)1); // return : OK/Done
		break;
	}
	// 3X : Ti Functions
	case 31: // Get Temperature
	{
		// Step 1 : Reading 0D register
		screens.Scr1->Wire.beginTransmission(screens.Scr1->I2CAdress);
		screens.Scr1->Wire.write(0x0D);
		screens.Scr1->Wire.endTransmission();
		screens.Scr1->Wire.requestFrom(screens.Scr1->I2CAdress, (uint8_t)1);
		uint8_t reg = screens.Scr1->Wire.read();

		// Step 2 : Setting 0D register with read bit enabled
		reg = reg | B10000000;
		screens.Scr1->Wire.beginTransmission(screens.Scr1->I2CAdress);
		screens.Scr1->Wire.write(0x0D);
		screens.Scr1->Wire.write(reg);
		screens.Scr1->Wire.endTransmission();

		// Step 3 : Waiting for EOC interrupt on register 08
		bool EOCDone = false;
		uint8_t tries = 0;
		while (!EOCDone)
		{
			tries++;
			screens.Scr1->Wire.beginTransmission(screens.Scr1->I2CAdress);
			screens.Scr1->Wire.write(0x08);
			screens.Scr1->Wire.endTransmission();
			screens.Scr1->Wire.requestFrom(screens.Scr1->I2CAdress, (uint8_t)1);
			uint8_t EOC = screens.Scr1->Wire.read();

			if (EOC & B00000001)
				EOCDone = true;
			else
				delay(1);
		}

		// Step 4 : Return temperature value
		screens.Scr1->Wire.beginTransmission(screens.Scr1->I2CAdress);
		screens.Scr1->Wire.write(0);
		screens.Scr1->Wire.endTransmission();
		screens.Scr1->Wire.requestFrom(screens.Scr1->I2CAdress, (uint8_t)1);
		uint8_t temperature = screens.Scr1->Wire.read();
		SerialUSB.write(temperature);
		SerialUSB.write(tries);
		break;

	}

	// 4X : Display Test Function
	case 41: // White
	{
		screens.Scr1->StartFrame();

		screens.Scr1->StartRow();
		for (int j = 0; j < 200; ++j) {
			screens.Scr1->WriteByte(255);
		}
		screens.Scr1->EndRow();

		screens.Scr1->FirtstHalfPulse();

		for (int i = 0; i < 300; ++i) {
			screens.Scr1->WriteRow();

			screens.Scr1->CKVPulse();
		}

		screens.Scr1->SecondHalfPulse();

		for (int i = 0; i < 300; ++i) {
			screens.Scr1->WriteRow();

			screens.Scr1->CKVPulse();
		}

		screens.Scr1->EndFrame();

		break;
	}

	case 42: // Black
	{
		screens.Scr1->StartFrame();

		screens.Scr1->StartRow();
		for (int j = 0; j < 200; ++j) {
			screens.Scr1->WriteByte(0);
		}
		screens.Scr1->EndRow();

		screens.Scr1->FirtstHalfPulse();

		for (int i = 0; i < 300; ++i) {
			screens.Scr1->WriteRow();

			screens.Scr1->CKVPulse();
		}

		screens.Scr1->SecondHalfPulse();

		for (int i = 0; i < 300; ++i) {
			screens.Scr1->WriteRow();

			screens.Scr1->CKVPulse();
		}

		screens.Scr1->EndFrame();

		break;
	}

	case 43: // white -> Black -> white -> Black  -> white  (Clear)
	{
		int frameDelay = 300;

		screens.Scr1->StartFrame();

		screens.Scr1->StartRow();
		for (int j = 0; j < 200; ++j) {
			screens.Scr1->WriteByte(255);
		}
		screens.Scr1->EndRow();

		screens.Scr1->FirtstHalfPulse();

		for (int i = 0; i < 300; ++i) {
			screens.Scr1->WriteRow();

			screens.Scr1->CKVPulse();
		}

		screens.Scr1->SecondHalfPulse();

		for (int i = 0; i < 300; ++i) {
			screens.Scr1->WriteRow();

			screens.Scr1->CKVPulse();
		}

		screens.Scr1->EndFrame();

		delay(frameDelay);

		screens.Scr1->StartFrame();

		screens.Scr1->StartRow();
		for (int j = 0; j < 200; ++j) {
			screens.Scr1->WriteByte(0);
		}
		screens.Scr1->EndRow();

		screens.Scr1->FirtstHalfPulse();

		for (int i = 0; i < 300; ++i) {
			screens.Scr1->WriteRow();

			screens.Scr1->CKVPulse();
		}

		screens.Scr1->SecondHalfPulse();

		for (int i = 0; i < 300; ++i) {
			screens.Scr1->WriteRow();

			screens.Scr1->CKVPulse();
		}

		screens.Scr1->EndFrame();

		delay(frameDelay);

		screens.Scr1->StartFrame();

		screens.Scr1->StartRow();
		for (int j = 0; j < 200; ++j) {
			screens.Scr1->WriteByte(255);
		}
		screens.Scr1->EndRow();

		screens.Scr1->FirtstHalfPulse();

		for (int i = 0; i < 300; ++i) {
			screens.Scr1->WriteRow();

			screens.Scr1->CKVPulse();
		}

		screens.Scr1->SecondHalfPulse();

		for (int i = 0; i < 300; ++i) {
			screens.Scr1->WriteRow();

			screens.Scr1->CKVPulse();
		}

		screens.Scr1->EndFrame();

		delay(frameDelay);

		screens.Scr1->StartFrame();

		screens.Scr1->StartRow();
		for (int j = 0; j < 200; ++j) {
			screens.Scr1->WriteByte(0);
		}
		screens.Scr1->EndRow();

		screens.Scr1->FirtstHalfPulse();

		for (int i = 0; i < 300; ++i) {
			screens.Scr1->WriteRow();

			screens.Scr1->CKVPulse();
		}

		screens.Scr1->SecondHalfPulse();

		for (int i = 0; i < 300; ++i) {
			screens.Scr1->WriteRow();

			screens.Scr1->CKVPulse();
		}

		screens.Scr1->EndFrame();

		delay(frameDelay);

		screens.Scr1->StartFrame();

		screens.Scr1->StartRow();
		for (int j = 0; j < 200; ++j) {
			screens.Scr1->WriteByte(255);
		}
		screens.Scr1->EndRow();

		screens.Scr1->FirtstHalfPulse();

		for (int i = 0; i < 300; ++i) {
			screens.Scr1->WriteRow();

			screens.Scr1->CKVPulse();
		}

		screens.Scr1->SecondHalfPulse();

		for (int i = 0; i < 300; ++i) {
			screens.Scr1->WriteRow();

			screens.Scr1->CKVPulse();
		}

		screens.Scr1->EndFrame();

		break;
	}

	case 44: // Squares
	{
		screens.Scr1->StartFrame();
		screens.Scr1->FirtstHalfPulse();

		for (int i = 0; i < 300; ++i) {

			bool linePolarity = ((i / 4) + 1) % 2 == 1;
			screens.Scr1->StartRow();

			for (int j = 0; j < 200; ++j) {
				if (linePolarity)
					screens.Scr1->WriteByte(j % 2 == 1 ? 255 : 0);
				else
					screens.Scr1->WriteByte(j % 2 == 1 ? 0 : 255);
			}
			screens.Scr1->EndRow();

			screens.Scr1->WriteRow();

			screens.Scr1->CKVPulse();
		}

		screens.Scr1->SecondHalfPulse();

		for (int i = 0; i < 300; ++i) {

			bool linePolarity = ((i / 4) + 1) % 2 == 1;
			screens.Scr1->StartRow();

			for (int j = 0; j < 200; ++j) {
				if (linePolarity)
					screens.Scr1->WriteByte(j % 2 == 1 ? 0 : 255);
				else
					screens.Scr1->WriteByte(j % 2 == 1 ? 255 : 0);
			}

			screens.Scr1->EndRow();

			screens.Scr1->WriteRow();

			screens.Scr1->CKVPulse();
		}

		screens.Scr1->EndFrame();

		break;
	}

	case 45: // Noise
	{
		screens.Scr1->StartFrame();
		screens.Scr1->FirtstHalfPulse();

		for (int i = 0; i < 300; ++i) {

			bool linePolarity = ((i / 4) + 1) % 2 == 1;
			screens.Scr1->StartRow();

			for (int j = 0; j < 200; ++j) {
				screens.Scr1->WriteByte(random(0, 256));
			}
			screens.Scr1->EndRow();

			screens.Scr1->WriteRow();

			screens.Scr1->CKVPulse();
		}

		screens.Scr1->SecondHalfPulse();

		for (int i = 0; i < 300; ++i) {

			bool linePolarity = ((i / 4) + 1) % 2 == 1;
			screens.Scr1->StartRow();

			for (int j = 0; j < 200; ++j) {
				screens.Scr1->WriteByte(random(0, 256));
			}

			screens.Scr1->EndRow();

			screens.Scr1->WriteRow();

			screens.Scr1->CKVPulse();
		}

		screens.Scr1->EndFrame();

		break;
	}

	case 51: // BW image
	{
		for (int loop = 0; loop < 5; loop++)
		{
			screens.Scr1->StartFrame();
			screens.Scr1->FirtstHalfPulse();

			for (int i = 0; i < 300; ++i) {

				bool linePolarity = ((i / 4) + 1) % 2 == 1;
				screens.Scr1->StartRow();

				for (int j = 0; j < 100; ++j) {
					screens.Scr1->WriteByte(data[i * 100 + j]);
				}
				screens.Scr1->EndRow();

				screens.Scr1->WriteRow();

				screens.Scr1->CKVPulse();
			}

			screens.Scr1->SecondHalfPulse();

			for (int i = 0; i < 300; ++i) {

				bool linePolarity = ((i / 4) + 1) % 2 == 1;
				screens.Scr1->StartRow();

				for (int j = 0; j < 100; ++j) {
					screens.Scr1->WriteByte(data[(100 * 300) + i * 100 + j]);
				}

				screens.Scr1->EndRow();

				screens.Scr1->WriteRow();

				screens.Scr1->CKVPulse();
			}

			screens.Scr1->EndFrame();
		}
		break;
	}
	}


	

}

String padLeft(String str, int length, char filler)
{
	if (str.length() > length)
		return str;

	// Declaration
	char* strArray = 0;

	// Allocation (let's suppose size contains some value discovered at runtime,
	// e.g. obtained from some external source or through other program logic)
	if (strArray != 0) {
		delete[] strArray;
	}
	strArray = new char[length];

	for (int i = 0; i < length - str.length(); i++)
	{
		strArray[i] = filler;
	}

	for (int i = length - str.length(); i < length; i++)
	{
		strArray[i] = str[i - (length - str.length())];
	}

	return String(strArray);
}



void printText(String str)
{
	display.begin(SSD1306_SWITCHCAPVCC, 0x3C);

	display.clearDisplay();

	display.setTextSize(1);
	display.setTextColor(WHITE);
	display.setCursor(0, 0);
	display.println(str);

	display.display();

}