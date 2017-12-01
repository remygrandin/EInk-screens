#pragma once
#include <stdint.h>
#include <ArduinoSTL.h>
#include <vector>
#include "SlowSoftWire.h"
#include "dueDigitalWriteFast.h"
#include "Screen1.h"

class Screen
{
public:
	Screen();

	
private:

};



class Screens
{
public:
	Screens();

	Screen1 *Scr1;

};