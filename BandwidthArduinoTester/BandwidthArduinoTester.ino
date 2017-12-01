/*
Name:		EINK.ino
Created:	16/10/2016 16:09:18
Author:	12480_000
*/

#include <unwind-cxx.h>
#include <system_configuration.h>
int bauds = 115200;

unsigned long start;
unsigned long current;

int pos = 0;

char message[27] = "abcdefghijklmnopqrstuvwxyz";

void setup() {
	SerialUSB.begin(bauds);
	Serial.begin(bauds);

	pinMode(42, OUTPUT);
	pinMode(52, OUTPUT);
}

void loop() {
	/*
	if (SerialUSB.available())
		serialUSBEvent();

	if (Serial.available())
		serialEvent();
		*/

		/*
		while (Serial.available()) {
		char incoming = Serial.read();
		Serial.print(incoming);
		}*/

	if (SerialUSB.available())
		serialUSBEvent();
	/*
	if (Serial.available())
	SerialEvent();
	*/
	//SerialUSB.println(SerialUSB.baud());


	//Wait(100);
}

bool readMode = false;

char buf[92000] = {0};



void serialUSBEvent() {
	

	if (readMode)
	{
		char data = Serial.read();

		if (data != -1)
		{
			buf[pos] = data;
			pos++;

			if (pos == 92000)
			{
				readMode = false;

				SerialUSB.write("0");
				digitalWrite(42, LOW);
			}
		}
	}
	else
	{
		String str = SerialUSB.readString();
		digitalWrite(52, HIGH);
		if (str == "Start")
		{

			digitalWrite(42, HIGH);
			pos = 0;
			start = micros();

			while (micros() - start <= 10000000) // 10 secs
			{
				//SerialUSB.write(message[pos]);
				SerialUSB.println("BigDataPacketBigDataPacketBigData");
				/*
				pos++;

				if (pos >= 26)
				pos = 0;*/

			}

			SerialUSB.write("0");
			digitalWrite(42, LOW);
			
		}

		if (str == "Start2")
		{
			readMode = true;
			digitalWrite(42, HIGH);
			pos = 0;
		}
		digitalWrite(52, LOW);
	}


}

