#include "QList.h"

typedef struct MasterResponse {
	boolean isValid;
	QList<byte> rawData;
	String message;
	QList<String> lines;
};

