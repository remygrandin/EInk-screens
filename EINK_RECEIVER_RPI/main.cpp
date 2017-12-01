
#include<stdio.h> //printf
#include<string.h> //memset
#include<stdlib.h> //exit(0);
#include <iostream>
#include <thread>
#include "UDPDiscoveryServer.h"

int main(void)
{
	std::thread t1(StartUDPDiscoveryServer);
	
	//Join the thread with the main thread
	t1.join();

	for (;;)
	{
		int a = 5;

	}
}