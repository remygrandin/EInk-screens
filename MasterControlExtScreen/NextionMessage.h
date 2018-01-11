#include "QList.h"

typedef struct NextionMessage {

	byte opCode;
	QList<byte> rawData;
	String message;
	QList<String> args;
};