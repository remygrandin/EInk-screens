#include<stdio.h> //printf
#include<string.h> //memset
#include<stdlib.h> //exit(0);
#include <iostream>
#include <thread>
#include "UDPDiscoveryServer.h"
#include "TCPDisplayServer.h"

int main(void)
{
	std::thread t1(StartUDPDiscoveryServer);

	std::thread t2(StartTCPDisplayServer);

	t2.join();

	for (;;)
	{
		int a = 5;

	}
}