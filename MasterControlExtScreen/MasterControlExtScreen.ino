#include "QList.h"
#include "NextionMessage.h"
#include "MasterResponse.h"

// freemem monitor
#include <malloc.h>
#include <stdlib.h>
#include <stdio.h>

extern char _end;
extern "C" char *sbrk(int i);
char *ramstart = (char *)0x20070000;
char *ramend = (char *)0x20088000;

// Relay
bool RelayMainState = false;
bool RelayComputerSwitchState = false;
bool Relay5vGeneratorState = false;
bool RelayScreenPowerState = false;

int RelayMainPin = 10;
int RelayComputerSwitchPin = 11;
int Relay5vGeneratorPin = 12;
int RelayScreenPowerPin = 13;

union ByteArrayToInteger {
	byte byteArray[4];
	uint32_t integer;
};

typedef struct t {
	unsigned long tStart;
	unsigned long tTimeout;
};

t t_funcCheckMaster = { 0, 5000 }; //Run every 5000ms

bool tCheck(struct t *t) {
	if (millis() > t->tStart + t->tTimeout)
		return true;
	else
		return false;
}

void tRun(struct t *t) {
	t->tStart = millis();
}

NextionMessage readNextionMessage() {

	NextionMessage message;
	message.message = "";
	while (true)
	{
		int readByte = Serial3.read();

		if (readByte == -1)
			continue;

		message.rawData.push_back((byte)readByte);

		int size = message.rawData.size();

		if (size >= 3 && message.rawData.get(size - 1) == 255
			&& message.rawData.get(size - 2) == 255
			&& message.rawData.get(size - 3) == 255)
		{
			message.opCode = message.rawData.front();
			message.rawData.pop_front();

			// getting rid of message terminator
			message.rawData.pop_back();
			message.rawData.pop_back();
			message.rawData.pop_back();

			for (int i = 0; i < message.rawData.size(); i++)
			{
				message.message += (char)message.rawData.get(i);
			}

			String tempArg = "";

			for (int i = 0; i < message.message.length(); i++)
			{
				if (message.message[i] == ' ')
				{
					message.args.push_back(tempArg);
					tempArg = "";
				}
				else if (i == message.message.length() - 1)
				{
					tempArg += (char)message.message[i];
					message.args.push_back(tempArg);
				}
				else {
					tempArg += (char)message.message[i];
				}
			}

			return message;
		}

	}

}

void sendNextionMessage(String message) {
	Serial3.print(message);
	Serial3.write(0xFF);
	Serial3.write(0xFF);
	Serial3.write(0xFF);
}

boolean masterAlive = false;

MasterResponse requestMaster(String message, unsigned long allowed_response_time = 1000)
{
	while (SerialUSB.available()) {  //is there anything to read?
		char getData = SerialUSB.read();  //if yes, read it
	}

	MasterResponse response;

	response.isValid = false;

	if (message != "AreYouStillThere" && !masterAlive)
	{
		return response;
	}

	ByteArrayToInteger converter;
	converter.integer = message.length();

	SerialUSB.write(converter.byteArray[0]);
	SerialUSB.write(converter.byteArray[1]);
	SerialUSB.write(converter.byteArray[2]);
	SerialUSB.write(converter.byteArray[3]);

	SerialUSB.print(message);


	int expectedSize = -1;


	unsigned long maxTime = millis() + allowed_response_time;

	while (true)
	{
		if (millis() > maxTime)
		{
			return response;
		}

		int readByte = SerialUSB.read();

		if (readByte == -1)
			continue;

		response.rawData.push_back(readByte);

		if (expectedSize == -1 && response.rawData.size() >= 4)
		{
			converter.byteArray[0] = response.rawData.get(0);
			converter.byteArray[1] = response.rawData.get(1);
			converter.byteArray[2] = response.rawData.get(2);
			converter.byteArray[3] = response.rawData.get(3);

			expectedSize = converter.integer;

			response.rawData.pop_front();
			response.rawData.pop_front();
			response.rawData.pop_front();
			response.rawData.pop_front();
		}

		if (expectedSize == response.rawData.size())
		{
			response.isValid = true;
			for (int i = 0; i < response.rawData.size(); i++)
			{
				response.message += (char)response.rawData.get(i);
			}

			String tempArg = "";

			for (int i = 0; i < response.message.length(); i++)
			{
				if (response.message[i] == '\n')
				{
					response.lines.push_back(tempArg);
					tempArg = "";
				}
				else if (i == response.message.length() - 1)
				{
					tempArg += (char)response.message[i];
					response.lines.push_back(tempArg);
				}
				else {
					tempArg += (char)response.message[i];
				}
			}

			return response;
		}
	}
}

// ==== SETUP ====
void setup() {
	Serial3.begin(115200);
	SerialUSB.begin(115200);

	// Relay
	pinMode(RelayMainPin, OUTPUT);
	pinMode(RelayComputerSwitchPin, OUTPUT);
	pinMode(Relay5vGeneratorPin, OUTPUT);
	pinMode(RelayScreenPowerPin, OUTPUT);

	digitalWrite(RelayMainPin, LOW);
	digitalWrite(RelayComputerSwitchPin, LOW);
	digitalWrite(Relay5vGeneratorPin, LOW);
	digitalWrite(RelayScreenPowerPin, LOW);

	sendNextionMessage("txtInfo.txt=\"Ready\"");
}

void testMaster()
{
	MasterResponse response = requestMaster("AreYouStillThere", 100);

	if (response.isValid)
	{
		masterAlive = true;
	}
	else
	{
		masterAlive = false;
	}
}
// ==== LOOP ====
void loop() {
	if (tCheck(&t_funcCheckMaster)) {
		testMaster();
		tRun(&t_funcCheckMaster);
	}

	if (Serial3.available())
	{
		NextionMessage message = readNextionMessage();
		/*
		SerialUSB.println(message.message);
		SerialUSB.println("args count : " + String(message.args.size()));

		for (int i = 0; i < message.args.size(); i++) {
			SerialUSB.println("arg " + String(i) + " : " + message.args.get(i));
		}
		*/
		String mainArg = message.args.get(0);

		if (mainArg == "Init")
		{
			String secondArg = message.args.get(1);

			if (secondArg == "Screens")
			{
				String condensedArgs = "";
				for (int i = 3; i < message.args.size(); i++)
					condensedArgs += message.args.get(i);

				String pageNb = message.args[2];

				MasterResponse response = requestMaster("GetScreens;" + pageNb + ";" + condensedArgs);
				if (!response.isValid)
					sendNextionMessage("txtInfo.txt=\"Master Not connected\"");
				else
				{
					sendNextionMessage("txtInfo.txt=\"Page " + response.lines[0] + "/" + response.lines[1] + " Search: \\\"" + response.lines[2] + "\\\"\"");

					if (response.lines[3] == "")
					{
						sendNextionMessage("btnScr1.picc=blankPic.val");
						sendNextionMessage("txtScr1ID.txt=\"\"");
						sendNextionMessage("txtScr1IP.txt=\"\"");
					}
					else
					{
						sendNextionMessage("btnScr1.picc=pagePic.val");
						sendNextionMessage("txtScr1ID.txt=\"" + response.lines[3] + "\"");
						sendNextionMessage("txtScr1IP.txt=\"" + response.lines[4] + "\"");
					}

					if (response.lines[5] == "")
					{
						sendNextionMessage("btnScr2.picc=blankPic.val");
						sendNextionMessage("txtScr2ID.txt=\"\"");
						sendNextionMessage("txtScr2IP.txt=\"\"");
					}
					else
					{
						sendNextionMessage("btnScr2.picc=pagePic.val");
						sendNextionMessage("txtScr2ID.txt=\"" + response.lines[5] + "\"");
						sendNextionMessage("txtScr2IP.txt=\"" + response.lines[6] + "\"");
					}

					if (response.lines[7] == "")
					{
						sendNextionMessage("btnScr3.picc=blankPic.val");
						sendNextionMessage("txtScr3ID.txt=\"\"");
						sendNextionMessage("txtScr3IP.txt=\"\"");
					}
					else
					{
						sendNextionMessage("btnScr3.picc=pagePic.val");
						sendNextionMessage("txtScr3ID.txt=\"" + response.lines[7] + "\"");
						sendNextionMessage("txtScr3IP.txt=\"" + response.lines[8] + "\"");

					}
					if (response.lines[9] == "")
					{
						sendNextionMessage("btnScr4.picc=blankPic.val");
						sendNextionMessage("txtScr4ID.txt=\"\"");
						sendNextionMessage("txtScr4IP.txt=\"\"");
					}
					else
					{
						sendNextionMessage("btnScr4.picc=pagePic.val");
						sendNextionMessage("txtScr4ID.txt=\"" + response.lines[9] + "\"");
						sendNextionMessage("txtScr4IP.txt=\"" + response.lines[10] + "\"");
					}

				}

			}
		}
		if (mainArg == "Relay")
		{
			//SerialUSB.println("Sending...");
			//sendNextionMessage("page MainMenu");
		}




		// freemem monitor
		/*
		SerialUSB.println("FreeMem:");
		char *heapend = sbrk(0);
		register char * stack_ptr asm("sp");
		struct mallinfo mi = mallinfo();
		/*
		SerialUSB.println("\nDynamic ram used: " + String(mi.uordblks));
		SerialUSB.println("Program static ram used " + String(&_end - ramstart));
		SerialUSB.println("Stack ram used " + String(ramend - stack_ptr));*/
		//SerialUSB.println("My guess at free mem: " + String(stack_ptr - heapend + mi.fordblks));
	}
}

