#include "QList.h"

typedef struct MasterResponse {

	QList<byte> rawData;
	String message;
	QList<String> lines;
};

