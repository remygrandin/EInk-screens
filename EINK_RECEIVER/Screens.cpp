#include "Screens.h"

static inline void clockdelay()
{// maybe just 4 ?
	__asm__("nop\n\t");
	__asm__("nop\n\t");
	__asm__("nop\n\t");
	__asm__("nop\n\t");
	__asm__("nop\n\t");
}


Screen::Screen()
{
}

Screens::Screens()
{
	Scr1 = new Screen1();

}


