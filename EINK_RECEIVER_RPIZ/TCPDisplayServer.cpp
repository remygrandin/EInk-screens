#include "TCPDisplayServer.h"
#include <iostream>
#include <string>
#include <vector>
#include <boost/bind.hpp>
#include <boost/asio.hpp>
#include <boost/asio/read_until.hpp>
#include <wiringPi.h>
#include <wiringPiI2C.h>
#include <regex>
#include <gd.h>
#include <gdfontg.h>
#include <sys/sysinfo.h>
#include <list>
#include <cstdint> 
#include <iostream>
#include <memory>
#include <fstream>
#include <boost/filesystem/convenience.hpp>
#include <boost/archive/xml_oarchive.hpp>
#include "Config.h"
#include <unistd.h>
#include <sys/reboot.h>
#include <future>

// include headers that implement a archive in simple text format


using boost::asio::ip::tcp;
using namespace std;


union ByteArrayToInteger {
	char byteArray[4];
	uint32_t integer;
};

union Byte
{
	char byte;

	struct
	{
		bool bit0 : 1;
		bool bit1 : 1;
		bool bit2 : 1;
		bool bit3 : 1;
		bool bit4 : 1;
		bool bit5 : 1;
		bool bit6 : 1;
		bool bit7 : 1;
	};
};

struct Connection {
	boost::asio::ip::tcp::socket socket;
	boost::asio::streambuf read_buffer;
	Connection(boost::asio::io_service & io_service) : socket(io_service), read_buffer() { }
	Connection(boost::asio::io_service & io_service, size_t max_buffer_size) : socket(io_service), read_buffer(max_buffer_size) { }
	std::vector<char> RGBData;
	std::vector<char> BWData;
};


class TCPDisplayServer
{
	boost::asio::io_service m_ioservice;
	boost::asio::ip::tcp::acceptor m_acceptor;
	std::list<Connection> m_connections;
	using con_handle_t = std::list<Connection>::iterator;
public:
	TCPDisplayServer() : m_ioservice(), m_acceptor(m_ioservice), m_connections() { }
	TCPDisplayServer(const TCPDisplayServer &) : m_ioservice(), m_acceptor(m_ioservice), m_connections() { }

	int I2CId = 104; // 0x68
	
	void handle_read(con_handle_t con_handle, boost::system::error_code const & err, size_t bytes_transfered) {
		if (bytes_transfered > 0) {


			std::istream is(&con_handle->read_buffer);

			wiringPiSetup();

			int I2Chandle = wiringPiI2CSetup(I2CId);

			// 1st byte : Command Code
			unsigned char commandCode = is.get();

			ByteArrayToInteger dataLengthConverter;

			dataLengthConverter.byteArray[0] = is.get();
			dataLengthConverter.byteArray[1] = is.get();
			dataLengthConverter.byteArray[2] = is.get();
			dataLengthConverter.byteArray[3] = is.get();

			uint dataLength = dataLengthConverter.integer;
			uint dataLengthOffsetted = dataLength + 4;

			std::vector<char> dataVec;

			for (int i = 5; i <= dataLengthOffsetted; ++i)
			{
				dataVec.push_back(is.get());
			}

			// End token discard
			for (int i = 0; i <= end_token.length(); ++i)
			{
				is.get();
			}


			switch (commandCode)
			{
				// ==== 0X : Diagnositcs ====
				// -- 01 : Ping --
			case 1:
			{
				write(con_handle, "Pong");
				break;
			}

			// -- 02 : Echo --
			case 2:
			{
				write(con_handle, dataVec);
				break;
			}

			// -- 03 : GetIp --
			case 3:
			{
				std::string ifconfigResult = exec("ifconfig");

				std::smatch match;

				std::regex ipRegex(R"aa(wlan0[\s\S]*inet addr:([\d.]{7,15}))aa");

				std::string outputIp = "IP";
				using reg_itr = std::regex_token_iterator<std::string::iterator>;
				for (reg_itr it{ ifconfigResult.begin(), ifconfigResult.end(), ipRegex, 1 }, end{}; it != end;) {
					outputIp = *it++;
				}

				write(con_handle, outputIp);
				break;
			}

			// -- 04 : GetMac --
			case 4:
			{
				std::string ifconfigResult = exec("ifconfig");

				std::smatch match;

				std::regex hwRegex(R"aa(wlan0[\s\S]*HWaddr ([a-f\d:]{17}))aa");

				std::string outputMac = "MAC";
				using reg_itr = std::regex_token_iterator<std::string::iterator>;
				for (reg_itr it{ ifconfigResult.begin(), ifconfigResult.end(), hwRegex, 1 }, end{}; it != end;) {
					outputMac = *it++;
				}

				write(con_handle, outputMac);
				break;
			}

			// -- 05 : Count frame in 1sec --
			// Deprecated
			/*
			case 5:
			{
				Screen_Init();

				buffer.assign(screenHeight * screenWidth, 0);

				PowerOn(I2Chandle);

				uint end = millis() + 1000;

				char count = 0;

				while (millis() < end)
				{
					count++;
					Screen_WriteFrameBW(I2Chandle);
				}

				PowerOff(I2Chandle);

				write(con_handle, count);
				break;
			}*/

			// -- 06 : Count frame in 5sec --
			// Deprecated
			/*
			case 6:
			{
				Screen_Init();

				buffer.assign(screenHeight * screenWidth, 0);

				PowerOn(I2Chandle);

				uint end = millis() + 5000;

				char count = 0;

				while (millis() < end)
				{
					count++;
					Screen_WriteFrameBW(I2Chandle);
				}

				PowerOff(I2Chandle);

				write(con_handle, count);
				break;
			}*/

			// -- 07 : Diagnostic Screen --
			case 7:
			{
				/* Declare and initialise variables for a new image.
				These steps are done once before using drawing functions. */
				gdImagePtr gdImage = gdImageCreate(screenHeight, screenWidth);

				/* The first call to gdImageColorAllocate() always sets the image background.
				This step can only be done once before using drawing functions. */
				gdImageColorAllocate(gdImage, 255, 255, 255);

				int black = gdImageColorAllocate(gdImage, 0, 0, 0);

				vector<std::string> lines;

				lines.push_back("====================================================================================");
				lines.push_back("================================  Dignostics Screen  ===============================");
				lines.push_back("====================================================================================");
				lines.push_back("");
				lines.push_back("                           \\o/ - Welcome Back Sempai - \\o/                          ");
				lines.push_back("");
				lines.push_back("-------------------------------------- System --------------------------------------");
				// Datetime
				std::array<char, 64> dateBuffer;
				dateBuffer.fill(0);
				time_t rawtime;
				time(&rawtime);
				const auto timeinfo = localtime(&rawtime);
				strftime(dateBuffer.data(), sizeof(dateBuffer), "%H:%M:%S - %d/%m/%Y ", timeinfo);
				std::string timeStr(dateBuffer.data());

				lines.push_back("  Date : " + timeStr);


				// Sysinfo

				struct sysinfo sys_info;

				if (sysinfo(&sys_info) != 0)
					perror("sysinfo");

				// Uptime
				int days = sys_info.uptime / 86400;
				int hours = (sys_info.uptime / 3600) - (days * 24);
				int mins = (sys_info.uptime / 60) - (days * 1440) - (hours * 60);
				int secs = sys_info.uptime % 60;

				lines.push_back("Uptime : " + std::to_string(days) + "d " +
					(std::to_string(hours).size() == 1 ? "0" : "") + std::to_string(hours) + ":" +
					(std::to_string(mins).size() == 1 ? "0" : "") + std::to_string(mins) + ":" +
					(std::to_string(secs).size() == 1 ? "0" : "") + std::to_string(secs));

				lines.push_back("");

				// RAM
				double barWidth = 82;
				double charWidth = 100 / barWidth;

				int32_t totalRam = ((uint64_t)sys_info.totalram * sys_info.mem_unit) / 1024 / 1024;
				int32_t freeRam = ((uint64_t)sys_info.freeram * sys_info.mem_unit) / 1024 / 1024;
				int32_t usedRam = totalRam - freeRam;
				double ratioRam = usedRam / (double)totalRam;
				lines.push_back("   RAM : " + std::to_string(usedRam) + "/" + std::to_string(totalRam) + " MB");

				std::string barStr = "[";

				for (int i = 0; i < barWidth; i++)
				{
					if (i * charWidth < (ratioRam * 100))
						barStr += "#";
					else
						barStr += "-";

				}
				barStr += "]";

				lines.push_back(barStr);

				// SWAP
				int32_t totalSwap = ((uint64_t)sys_info.totalswap * sys_info.mem_unit) / 1024 / 1024;
				int32_t freeSwap = ((uint64_t)sys_info.freeswap * sys_info.mem_unit) / 1024 / 1024;
				int32_t usedSwap = totalSwap - freeSwap;
				double ratioSwap = usedSwap / (double)totalSwap;
				lines.push_back("  Swap : " + std::to_string(usedSwap) + "/" + std::to_string(totalSwap) + " MB");

				barStr = "[";

				for (int i = 0; i < barWidth; i++)
				{
					if (i * charWidth < (ratioSwap * 100))
						barStr += "#";
					else
						barStr += "-";

				}
				barStr += "]";
				lines.push_back(barStr);

				lines.push_back("");

				lines.push_back("--------------------------------------- WiFi ---------------------------------------");

				// IP
				std::string ifconfigResult = exec("ifconfig");

				std::smatch match;

				std::regex ipRegex(R"aa(wlan0[\s\S]*inet addr:([\d.]{7,15}))aa");

				std::string outputIp = "IP";
				using reg_itr = std::regex_token_iterator<std::string::iterator>;
				for (reg_itr it{ ifconfigResult.begin(), ifconfigResult.end(), ipRegex, 1 }, end{}; it != end;) {
					outputIp = *it++;
				}
				lines.push_back("           IP : " + outputIp);


				// MAC
				std::regex hwRegex(R"aa(wlan0[\s\S]*HWaddr ([a-f\d:]{17}))aa");

				std::string outputMac = "MAC";
				using reg_itr = std::regex_token_iterator<std::string::iterator>;
				for (reg_itr it{ ifconfigResult.begin(), ifconfigResult.end(), hwRegex, 1 }, end{}; it != end;) {
					outputMac = *it++;
				}
				lines.push_back("          MAC : " + outputMac);

				lines.push_back("");

				// Wifi Detail
				std::string iwconfigResult = exec("iwconfig");


				std::regex wifiRegex(R"aa(ESSID:"(\S*)"[\s\S]*Access Point: (\S*)[\s\S]*Link Quality=(\S*)[\s\S]*Signal level=([\S ]*))aa");

				smatch sm;

				regex_search(iwconfigResult, sm, wifiRegex);

				std::string ESSID = sm[1];
				std::string BSSID = sm[2];
				std::string LinkQuality = sm[3];
				std::string SignalLevel = sm[4];

				lines.push_back("        ESSID : " + ESSID);
				lines.push_back("        BSSID : " + BSSID);
				lines.push_back(" Link Quality : " + LinkQuality);
				lines.push_back(" Signel Level : " + SignalLevel);




				int step = 20;
				for (int i = 0; i < lines.size(); i++)
				{
					unsigned char *strPtr = new unsigned char[lines[i].length() + 1];
					strcpy((char *)strPtr, lines[i].c_str());

					gdImageString(gdImage, gdFontGetGiant(), 20, (i + 1) * step, strPtr, black);
				}


				// Border

				int borderMargin = 5;
				bool dottedBorder = true;
				int borderWidth = 15;

				for (int i = borderMargin; i <= borderWidth; i++)
				{
					if (dottedBorder)
					{
						gdImageRectangle(gdImage, i, i, screenHeight - i, screenWidth - i, black);
					}
					dottedBorder = !dottedBorder;
				}


				// Wifi Sympbol
				gdImageFilledEllipse(gdImage, 450, 450, 20, 20, black);

				for (int i = 50; i < 300; i++)
				{
					if (i % 100 == 0)
						i += 50;

					gdImageArc(gdImage, 450, 450, i, i, -45, 45, black);
				}

				buffer.assign(screenHeight * screenWidth, 0);

				int offset = 0;
				for (int i = 0; i < screenWidth; i++)
				{
					for (int j = 0; j < screenHeight; j++)
					{
						int pix = gdImageGetPixel(gdImage, j, i);
						buffer[offset] = pix;
						offset++;
					}
				}

				gdImageDestroy(gdImage);

				Screen_WriteBufferBW(I2Chandle);

				write(con_handle, "DONE");
				break;

			}

			// -- 08 : Test Debit --
			case 8:
			{
				con_handle->RGBData = dataVec;
				write(con_handle, "OK");
			}

			// ==== 1X : System ====
			// -- 11 : GetId --
			case 11:
			{
				write(con_handle, config->deviceId);
				break;
			}

			// -- 12 : SetId --
			case 12:
			{
				std::string devId(dataVec.begin(), dataVec.end());

				config->deviceId = devId;

				writeConfig();
				break;
			}

			// -- 13 : ResetId --
			case 13:
			{
				std::string ifconfigResult = exec("ifconfig");

				std::smatch match;

				std::regex hwRegex(R"aa(wlan0[\s\S]*HWaddr ([a-f\d:]{17}))aa");

				std::string outputMac = "MAC";
				using reg_itr = std::regex_token_iterator<std::string::iterator>;
				for (reg_itr it{ ifconfigResult.begin(), ifconfigResult.end(), hwRegex, 1 }, end{}; it != end;) {
					outputMac = *it++;
				}


				config->deviceId = outputMac;

				writeConfig();
				break;
			}

			// -- 14 : Reboot --
			case 14:
			{
				// Closing Connexion
				boost::system::error_code ignored_error;
				boost::asio::write(con_handle->socket, boost::asio::buffer(end_token), boost::asio::transfer_all(), ignored_error);

				sleep(1);

				sync();
				reboot(RB_AUTOBOOT);

				break;
			}

			// -- 15 : Shutdown --
			case 15:
			{
				// Closing Connexion
				boost::system::error_code ignored_error;
				boost::asio::write(con_handle->socket, boost::asio::buffer(end_token), boost::asio::transfer_all(), ignored_error);

				sleep(1);

				sync();
				reboot(RB_POWER_OFF);

				break;
			}

			// ==== 2X : Raw Registers ====
			// -- 21 : GetAllRegisters --
			case 21:
			{
				std::vector<char> outputVec;

				for (int i = 0; i <= 17; ++i)
				{
					outputVec.push_back(static_cast<char>(wiringPiI2CReadReg8(I2Chandle, i)));
				}

				write(con_handle, outputVec);
				break;
			}

			// -- 22 : SetAllRegisters (17 bytes) --
			case 22:
			{
				for (int i = 0; i <= 17; ++i)
				{
					wiringPiI2CWriteReg8(I2Chandle, i, dataVec[i]);
				}

				break;
			}

			// -- 23 : GetRegister (1 byte address) --
			case 23:
			{
				char reading = static_cast<char>(wiringPiI2CReadReg8(I2Chandle, dataVec[0]));

				write(con_handle, reading);
				break;
			}

			// -- 24 : SetRegister (1 byte address, 1 byte value) --
			case 24:
			{
				wiringPiI2CWriteReg8(I2Chandle, dataVec[0], dataVec[1]);

				break;
			}

			// ==== 3X : Power Function ====
			// -- 31 : Get Status --
			case 31:
			{
				// Output : 
				//|    7    |    6    |    5    |    4    |    3    |    2    |    1    |    0    |
				//| VPOS PG | VNEG PG |    0    | VDDH PG | VEE  PG |    0    | V3P3 EN |    0    |

				Byte byteConverterOutput;

				byteConverterOutput.byte = 0;

				char reading = static_cast<char>(wiringPiI2CReadReg8(I2Chandle, 15));

				Byte byteConverterReader;
				byteConverterReader.byte = reading;

				// VPOS PG
				byteConverterOutput.bit7 = byteConverterReader.bit4;
				// VNEG PG
				byteConverterOutput.bit6 = byteConverterReader.bit1;

				// VDDH PG
				byteConverterOutput.bit4 = byteConverterReader.bit6;
				// VEE PG
				byteConverterOutput.bit3 = byteConverterReader.bit4;

				reading = (char)wiringPiI2CReadReg8(I2Chandle, 1);

				byteConverterReader.byte = reading;

				// V3P3 EN
				byteConverterOutput.bit1 = byteConverterReader.bit5;

				write(con_handle, byteConverterOutput.byte);
				break;
			}

			// -- 32 : Power On --
			case 32:
			{
				PowerOn(I2Chandle);

				break;
			}

			// -- 33 : Power Off --
			case 33:
			{
				PowerOff(I2Chandle);

				break;
			}

			// -- 34 : Toggle Power --
			case 34:
			{
				char reading = static_cast<char>(wiringPiI2CReadReg8(I2Chandle, 15));

				Byte byteConverter;
				byteConverter.byte = reading;

				bool SourcePower = byteConverter.bit4 && byteConverter.bit1;

				bool GatePower = byteConverter.bit6 && byteConverter.bit3;

				reading = (char)wiringPiI2CReadReg8(I2Chandle, 1);

				byteConverter.byte = reading;

				bool V3P3 = byteConverter.bit5;

				if (SourcePower || GatePower || V3P3)
					PowerOff(I2Chandle);
				else
					PowerOn(I2Chandle);

				break;
			}

			// ==== 4X : Power Adjust ====
			// -- 41 : Get VCOM --
			case 41:
			{
				char readingVCOM1 = static_cast<char>(wiringPiI2CReadReg8(I2Chandle, 3));

				char readingVCOM2 = static_cast<char>(wiringPiI2CReadReg8(I2Chandle, 4));

				ByteArrayToInteger intConverter;
				Byte byteReader;
				Byte byteConverter;

				intConverter.integer = 0;
				intConverter.byteArray[0] = readingVCOM1;

				byteReader.byte = readingVCOM2;
				byteConverter.byte = 0;
				byteConverter.bit0 = byteReader.bit0;
				intConverter.byteArray[1] = byteConverter.byte;

				int outInt = intConverter.integer;

				outInt *= -1;
				outInt *= 10;

				write(con_handle, outInt);
				break;
			}

			// -- 42 : Set VCOM --
			case 42:
			{
				ByteArrayToInteger intConverter;
				intConverter.byteArray[0] = dataVec[0];
				intConverter.byteArray[1] = dataVec[1];
				intConverter.byteArray[2] = dataVec[2];
				intConverter.byteArray[3] = dataVec[3];


				int value = intConverter.integer;

				value /= 10;
				value /= -1;

				intConverter.integer = value;

				Byte byteConverter;
				Byte byteReader;
				byteConverter.byte = intConverter.byteArray[1];



				wiringPiI2CWriteReg8(I2Chandle, 3, intConverter.byteArray[0]);

				byteReader.byte = static_cast<char>(wiringPiI2CReadReg8(I2Chandle, 4));

				byteReader.bit0 = byteConverter.bit0;
				byteReader.bit6 = true; // prog bit

				wiringPiI2CWriteReg8(I2Chandle, 4, byteReader.byte);
				break;
			}

			// -- 43 : Get VADJ --
			case 43:
			{
				char readingVADJ = static_cast<char>(wiringPiI2CReadReg8(I2Chandle, 2));


				ByteArrayToInteger intConverter;
				intConverter.integer = 0;

				Byte byteReader;
				byteReader.byte = readingVADJ;
				byteReader.bit7 = false;
				byteReader.bit6 = false;
				byteReader.bit5 = false;
				byteReader.bit4 = false;
				byteReader.bit3 = false;

				intConverter.byteArray[0] = byteReader.byte;

				unsigned int value = intConverter.integer;

				value = 15000 - ((value - 3) * 250);

				write(con_handle, static_cast<int>(value));
				break;
			}

			// -- 44 : Set VADJ --
			case 44:
			{
				ByteArrayToInteger intConverter;
				intConverter.byteArray[0] = dataVec[0];
				intConverter.byteArray[1] = dataVec[1];
				intConverter.byteArray[2] = dataVec[2];
				intConverter.byteArray[3] = dataVec[3];


				int value = intConverter.integer;

				value = (value - 15000) / -250 + 3;

				intConverter.integer = value;

				Byte byteReader;
				Byte byteInput;
				byteReader.byte = static_cast<char>(wiringPiI2CReadReg8(I2Chandle, 2));

				byteInput.byte = intConverter.byteArray[0];
				byteReader.bit0 = byteInput.bit0;
				byteReader.bit1 = byteInput.bit1;
				byteReader.bit2 = byteInput.bit2;


				wiringPiI2CWriteReg8(I2Chandle, 2, byteReader.byte);

				break;
			}

			// ==== 5X : Temperature function ====
			// -- 51 : Get Temperature --
			case 51:
			{
				Byte byteConverter;

				char initVal = (char)wiringPiI2CReadReg8(I2Chandle, 13);
				byteConverter.byte = initVal;

				byteConverter.bit7 = true;

				wiringPiI2CWriteReg8(I2Chandle, 13, byteConverter.byte);

				delayMicroseconds(10);

				bool convEnded = false;

				while (!convEnded)
				{
					char readVal = (char)wiringPiI2CReadReg8(I2Chandle, 13);
					byteConverter.byte = readVal;

					convEnded = byteConverter.bit5;
				}

				char tempVal = (char)wiringPiI2CReadReg8(I2Chandle, 0);

				write(con_handle, tempVal);

				break;
			}

			// -- 52 : Get TCOLD --
			case 52:
			{
				Byte byteConverterReader;
				Byte byteConverterOutput;

				byteConverterOutput.byte = 0;

				char initVal = (char)wiringPiI2CReadReg8(I2Chandle, 14);
				byteConverterReader.byte = initVal;

				byteConverterOutput.bit0 = byteConverterReader.bit4;
				byteConverterOutput.bit1 = byteConverterReader.bit5;
				byteConverterOutput.bit2 = byteConverterReader.bit6;
				byteConverterOutput.bit3 = byteConverterReader.bit7;

				signed char value = byteConverterOutput.byte - 7;

				write(con_handle, value);
				break;
			}

			// -- 53 : Set TCOLD --
			case 53:
			{
				Byte byteConverterReader;
				Byte byteConverterInput;

				byteConverterInput.byte = dataVec[0] + 7;

				char initVal = (char)wiringPiI2CReadReg8(I2Chandle, 14);
				byteConverterReader.byte = initVal;

				byteConverterReader.bit4 = byteConverterInput.bit0;
				byteConverterReader.bit5 = byteConverterInput.bit1;
				byteConverterReader.bit6 = byteConverterInput.bit2;
				byteConverterReader.bit7 = byteConverterInput.bit3;

				wiringPiI2CWriteReg8(I2Chandle, 14, byteConverterReader.byte);

				break;
			}

			// -- 54 : Get THOT --
			case 54:
			{
				Byte byteConverterReader;
				Byte byteConverterOutput;

				byteConverterOutput.byte = 0;

				char initVal = (char)wiringPiI2CReadReg8(I2Chandle, 14);
				byteConverterReader.byte = initVal;

				byteConverterOutput.bit0 = byteConverterReader.bit1;
				byteConverterOutput.bit1 = byteConverterReader.bit2;
				byteConverterOutput.bit2 = byteConverterReader.bit3;
				byteConverterOutput.bit3 = byteConverterReader.bit4;

				signed char value = byteConverterOutput.byte + 42;

				write(con_handle, value);
				break;
			}

			// -- 55 : Set THOT --
			case 55:
			{
				Byte byteConverterReader;
				Byte byteConverterInput;

				byteConverterInput.byte = dataVec[0] - 42;

				char initVal = (char)wiringPiI2CReadReg8(I2Chandle, 14);
				byteConverterReader.byte = initVal;

				byteConverterReader.bit1 = byteConverterInput.bit0;
				byteConverterReader.bit2 = byteConverterInput.bit1;
				byteConverterReader.bit3 = byteConverterInput.bit2;
				byteConverterReader.bit4 = byteConverterInput.bit3;

				wiringPiI2CWriteReg8(I2Chandle, 14, byteConverterReader.byte);

				break;
			}

			// ==== 6X : Power Up/Down Sequence & Timing ====
			// -- 61 : Get Power Up Sequence --
			case 61:
			{
				// order VPOS, VNEG, VDDH, VEE

				char regVal = (char)wiringPiI2CReadReg8(I2Chandle, 9);


				Byte byteConverterReader;
				Byte byteConverterOutput;

				byteConverterReader.byte = regVal;

				std::vector<char> output;

				// VPOS
				byteConverterOutput.byte = 0;
				byteConverterOutput.bit1 = byteConverterReader.bit5;
				byteConverterOutput.bit0 = byteConverterReader.bit4;
				output.push_back(byteConverterOutput.byte);

				// VNEG
				byteConverterOutput.byte = 0;
				byteConverterOutput.bit1 = byteConverterReader.bit1;
				byteConverterOutput.bit0 = byteConverterReader.bit0;
				output.push_back(byteConverterOutput.byte);

				// VDDH
				byteConverterOutput.byte = 0;
				byteConverterOutput.bit1 = byteConverterReader.bit7;
				byteConverterOutput.bit0 = byteConverterReader.bit6;
				output.push_back(byteConverterOutput.byte);

				// VEE
				byteConverterOutput.byte = 0;
				byteConverterOutput.bit1 = byteConverterReader.bit3;
				byteConverterOutput.bit0 = byteConverterReader.bit2;
				output.push_back(byteConverterOutput.byte);

				write(con_handle, output);
				break;
			}

			// -- 62 : Set Power Up Sequence --
			case 62:
			{
				// order VPOS, VNEG, VDDH, VEE

				Byte byteConverterOutput;
				Byte byteConverterInput;

				// VPOS
				byteConverterInput.byte = dataVec[0];
				byteConverterOutput.bit5 = byteConverterInput.bit1;
				byteConverterOutput.bit4 = byteConverterInput.bit0;

				// VNEG
				byteConverterInput.byte = dataVec[1];
				byteConverterOutput.bit1 = byteConverterInput.bit1;
				byteConverterOutput.bit0 = byteConverterInput.bit0;

				// VDDH
				byteConverterInput.byte = dataVec[2];
				byteConverterOutput.bit7 = byteConverterInput.bit1;
				byteConverterOutput.bit6 = byteConverterInput.bit0;

				// VEE
				byteConverterInput.byte = dataVec[3];
				byteConverterOutput.bit3 = byteConverterInput.bit1;
				byteConverterOutput.bit2 = byteConverterInput.bit0;

				wiringPiI2CWriteReg8(I2Chandle, 9, byteConverterOutput.byte);
				break;
			}

			// -- 63 : Get Power Down Sequence --
			case 63:
			{
				// order VPOS, VNEG, VDDH, VEE

				char regVal = (char)wiringPiI2CReadReg8(I2Chandle, 11);


				Byte byteConverterReader;
				Byte byteConverterOutput;

				byteConverterReader.byte = regVal;

				std::vector<char> output;

				// VPOS
				byteConverterOutput.byte = 0;
				byteConverterOutput.bit1 = byteConverterReader.bit5;
				byteConverterOutput.bit0 = byteConverterReader.bit4;
				output.push_back(byteConverterOutput.byte);

				// VNEG
				byteConverterOutput.byte = 0;
				byteConverterOutput.bit1 = byteConverterReader.bit1;
				byteConverterOutput.bit0 = byteConverterReader.bit0;
				output.push_back(byteConverterOutput.byte);

				// VDDH
				byteConverterOutput.byte = 0;
				byteConverterOutput.bit1 = byteConverterReader.bit7;
				byteConverterOutput.bit0 = byteConverterReader.bit6;
				output.push_back(byteConverterOutput.byte);

				// VEE
				byteConverterOutput.byte = 0;
				byteConverterOutput.bit1 = byteConverterReader.bit3;
				byteConverterOutput.bit0 = byteConverterReader.bit2;
				output.push_back(byteConverterOutput.byte);

				write(con_handle, output);
				break;
			}

			// -- 64 : Set Power Down Sequence --
			case 64:
			{
				// order VPOS, VNEG, VDDH, VEE

				Byte byteConverterOutput;
				Byte byteConverterInput;

				// VPOS
				byteConverterInput.byte = dataVec[0];
				byteConverterOutput.bit5 = byteConverterInput.bit1;
				byteConverterOutput.bit4 = byteConverterInput.bit0;

				// VNEG
				byteConverterInput.byte = dataVec[1];
				byteConverterOutput.bit1 = byteConverterInput.bit1;
				byteConverterOutput.bit0 = byteConverterInput.bit0;

				// VDDH
				byteConverterInput.byte = dataVec[2];
				byteConverterOutput.bit7 = byteConverterInput.bit1;
				byteConverterOutput.bit6 = byteConverterInput.bit0;

				// VEE
				byteConverterInput.byte = dataVec[3];
				byteConverterOutput.bit3 = byteConverterInput.bit1;
				byteConverterOutput.bit2 = byteConverterInput.bit0;

				wiringPiI2CWriteReg8(I2Chandle, 11, byteConverterOutput.byte);
				break;
			}

			// -- 65 : Get Power Up Timing --
			case 65:
			{
				char regVal = (char)wiringPiI2CReadReg8(I2Chandle, 10);

				Byte byteConverterReader;
				Byte byteConverterTemp;

				byteConverterReader.byte = regVal;
				byteConverterTemp.byte = 0;

				char tempVal = 0;

				std::vector<char> output;

				// start => strobe 1
				byteConverterTemp.bit1 = byteConverterReader.bit1;
				byteConverterTemp.bit0 = byteConverterReader.bit0;
				tempVal = byteConverterTemp.byte;
				tempVal = (tempVal + 1) * 3;
				output.push_back(tempVal);

				// strobe 1 => strobe 2
				byteConverterTemp.bit1 = byteConverterReader.bit3;
				byteConverterTemp.bit0 = byteConverterReader.bit2;
				tempVal = byteConverterTemp.byte;
				tempVal = (tempVal + 1) * 3;
				output.push_back(tempVal);

				// strobe 2 => strobe 3
				byteConverterTemp.bit1 = byteConverterReader.bit5;
				byteConverterTemp.bit0 = byteConverterReader.bit4;
				tempVal = byteConverterTemp.byte;
				tempVal = (tempVal + 1) * 3;
				output.push_back(tempVal);

				// strobe 3 => strobe 4
				byteConverterTemp.bit1 = byteConverterReader.bit7;
				byteConverterTemp.bit0 = byteConverterReader.bit6;
				tempVal = byteConverterTemp.byte;
				tempVal = (tempVal + 1) * 3;
				output.push_back(tempVal);

				write(con_handle, output);
				break;
			}

			// -- 66 : Set Power Up Timing --
			case 66:
			{
				Byte byteConverterOutput;
				Byte byteConverterInput;

				// start => strobe 1
				byteConverterInput.byte = dataVec[0] / 3 - 1;
				byteConverterOutput.bit1 = byteConverterInput.bit1;
				byteConverterOutput.bit0 = byteConverterInput.bit0;

				// strobe 1 => strobe 2
				byteConverterInput.byte = dataVec[1] / 3 - 1;
				byteConverterOutput.bit3 = byteConverterInput.bit1;
				byteConverterOutput.bit2 = byteConverterInput.bit0;

				// strobe 2 => strobe 3
				byteConverterInput.byte = dataVec[2] / 3 - 1;
				byteConverterOutput.bit5 = byteConverterInput.bit1;
				byteConverterOutput.bit4 = byteConverterInput.bit0;

				// strobe 3 => strobe 4
				byteConverterInput.byte = dataVec[3] / 3 - 1;
				byteConverterOutput.bit7 = byteConverterInput.bit1;
				byteConverterOutput.bit6 = byteConverterInput.bit0;

				wiringPiI2CWriteReg8(I2Chandle, 10, byteConverterOutput.byte);
				break;
			}

			// -- 67 : Get Power Down Timing --
			case 67:
			{
				char regVal = (char)wiringPiI2CReadReg8(I2Chandle, 12);

				Byte byteConverterReader;
				Byte byteConverterTemp;

				byteConverterReader.byte = regVal;
				byteConverterTemp.byte = 0;

				char tempVal = 0;

				std::vector<char> output;

				// start => strobe 1
				byteConverterTemp.bit0 = byteConverterReader.bit1;
				tempVal = byteConverterTemp.byte;
				tempVal = (tempVal + 1) * 3;
				output.push_back(tempVal);

				// strobe 1 => strobe 2
				byteConverterTemp.bit1 = byteConverterReader.bit3;
				byteConverterTemp.bit0 = byteConverterReader.bit2;
				tempVal = byteConverterTemp.byte;
				tempVal = 6 * pow(double(2), double(tempVal));
				output.push_back(tempVal);

				// strobe 2 => strobe 3
				byteConverterTemp.bit1 = byteConverterReader.bit5;
				byteConverterTemp.bit0 = byteConverterReader.bit4;
				tempVal = byteConverterTemp.byte;
				tempVal = 6 * pow(double(2), double(tempVal));
				output.push_back(tempVal);

				// strobe 3 => strobe 4
				byteConverterTemp.bit1 = byteConverterReader.bit7;
				byteConverterTemp.bit0 = byteConverterReader.bit6;
				tempVal = byteConverterTemp.byte;
				tempVal = 6 * pow(double(2), double(tempVal));
				output.push_back(tempVal);

				// multiplicator
				byteConverterTemp.byte = 0;
				byteConverterTemp.bit0 = byteConverterReader.bit0;
				tempVal = byteConverterTemp.byte;
				tempVal = pow(double(16), double(tempVal));
				output.push_back(tempVal);

				write(con_handle, output);
				break;
			}

			// -- 68 : Set Power Down Timing --
			case 68:
			{
				Byte byteConverterOutput;
				Byte byteConverterInput;

				// start => strobe 1
				byteConverterInput.byte = dataVec[0] / 3 - 1;
				byteConverterOutput.bit1 = byteConverterInput.bit0;

				// strobe 1 => strobe 2
				byteConverterInput.byte = log(dataVec[1] / 6) / log(2);
				byteConverterOutput.bit3 = byteConverterInput.bit1;
				byteConverterOutput.bit2 = byteConverterInput.bit0;

				// strobe 2 => strobe 3
				byteConverterInput.byte = log(dataVec[1] / 6) / log(2);
				byteConverterOutput.bit5 = byteConverterInput.bit1;
				byteConverterOutput.bit4 = byteConverterInput.bit0;

				// strobe 3 => strobe 4
				byteConverterInput.byte = log(dataVec[1] / 6) / log(2);
				byteConverterOutput.bit7 = byteConverterInput.bit1;
				byteConverterOutput.bit6 = byteConverterInput.bit0;

				// multiplicator
				byteConverterInput.byte = pow(dataVec[4], 1.0 / 16);
				byteConverterOutput.bit0 = byteConverterInput.bit0;

				wiringPiI2CWriteReg8(I2Chandle, 12, byteConverterOutput.byte);
				break;

			}

			// ==== 10X : Test Screens ====
			// -- 101 : White --
			case 101:
			{
				buffer.assign(screenHeight * screenWidth, 0);

				Screen_WriteBufferBW(I2Chandle);

				break;
			}

			// -- 102 : Black --
			case 102:
			{
				buffer.assign(screenHeight * screenWidth, 1);

				Screen_WriteBufferBW(I2Chandle);

				break;
			}

			// -- 103 : White / Black / White --
			case 103:
			{
				Screen_Init();

				PowerOn(I2Chandle);

				buffer.assign(screenHeight * screenWidth, 0);

				Screen_WriteFrameBW(I2Chandle);

				buffer.assign(screenHeight * screenWidth, 1);

				Screen_WriteFrameBW(I2Chandle);

				buffer.assign(screenHeight * screenWidth, 0);

				Screen_WriteFrameBW(I2Chandle);

				PowerOff(I2Chandle);

				break;
			}

			// -- 104 : line --
			case 104:
			{
				char lineSize;

				if (dataVec.empty())
					lineSize = 0;
				else
					lineSize = dataVec[0];

				if (lineSize == 0)
					lineSize = 5;

				int offset = 0;

				buffer.assign(screenHeight * screenWidth, 0);

				for (int i = 0; i < screenWidth; i++)
				{
					int lineCount = 0;
					bool lineVal = false;
					for (int j = 0; j < screenHeight; j++)
					{
						buffer[offset++] = lineVal;

						lineCount++;
						if (lineCount >= lineSize)
						{
							lineCount = 0;
							lineVal = !lineVal;
						}
					}

				}

				Screen_WriteBufferBW(I2Chandle);

				break;
			}

			// -- 105 : Col --
			case 105:
			{
				char colSize;

				if (dataVec.empty())
					colSize = 0;
				else
					colSize = dataVec[0];

				if (colSize == 0)
					colSize = 5;

				int offset = 0;

				buffer.assign(screenHeight * screenWidth, 0);

				int colCount = 0;
				bool colVal = false;

				for (int i = 0; i < screenWidth; i++)
				{
					for (int j = 0; j < screenHeight; j++)
					{
						buffer[offset++] = colVal;
					}

					colCount++;
					if (colCount >= colSize)
					{
						colCount = 0;
						colVal = !colVal;
					}

				}

				Screen_WriteBufferBW(I2Chandle);

				break;
			}

			// -- 106 : square --
			case 106:
			{
				char squareSize;

				if (dataVec.empty())
					squareSize = 0;
				else
					squareSize = dataVec[0];

				if (squareSize == 0)
					squareSize = 5;

				int offset = 0;

				buffer.assign(screenHeight * screenWidth, 0);

				int colCount = 0;
				bool colVal = false;

				for (int i = 0; i < screenWidth; i++)
				{
					int lineCount = 0;
					bool lineVal = false;

					for (int j = 0; j < screenHeight; j++)
					{
						buffer[offset++] = !lineVal != !colVal;

						lineCount++;
						if (lineCount >= squareSize)
						{
							lineCount = 0;
							lineVal = !lineVal;
						}
					}

					colCount++;
					if (colCount >= squareSize)
					{
						colCount = 0;
						colVal = !colVal;
					}

				}

				Screen_WriteBufferBW(I2Chandle);

				break;
			}

			// -- 107 : rand --
			case 107:
			{
				buffer.assign(screenHeight * screenWidth, 0);

				std::generate(buffer.begin(), buffer.end(), []() {
					return rand() % 2;
				});

				Screen_WriteBufferBW(I2Chandle);

				break;
			}

			// -- 108 : scale --
			case 108:
			{
				buffer.assign(screenHeight * screenWidth, 0);

				int offset = 0;

				for (int i = 0; i < firstHalfLineNB; i++)
				{
					for (int j = 0; j < screenHeight; j++)
					{
						buffer[offset++] = j - 21 <= i * 2 && j > 20;
					}
				}

				for (int i = 0; i < secondHalfLineNB; i++)
				{
					for (int j = 0; j < screenHeight; j++)
					{
						buffer[offset++] = j - 21 <= i * 2 && j > 20;
					}
				}

				Screen_WriteBufferBW(I2Chandle);

				break;
			}

			// -- 109 : Grayscale Test --
			case 109:
			{
				char grayScale;

				if (dataVec.empty())
					grayScale = 0;
				else
					grayScale = dataVec[0];

				if (grayScale == 0)
					grayScale = 8;

				int oldGrayScales = grayScales;

				grayScales = grayScale;

				buffer.assign(screenHeight * screenWidth, 0);

				int offset = 0;

				for (int i = 0; i < screenWidth; i++)
				{
					int colVal = (i - 2) / (600 / grayScale);
					for (int j = 0; j < screenHeight; j++)
					{
						buffer[offset++] = colVal;
					}
				}

				Screen_WriteBufferGray(I2Chandle);

				grayScales = oldGrayScales;

				break;
			}

			// ==== 15X : Prints ====
			case 151:
			{

				buffer = dataVec;

				Screen_WriteBufferGray(I2Chandle);

				break;
			}
			//7X : Print
			//71 : Print from Gray(scale, 800 * 600 gray data(1 byte per pix))
			//72 : Print from Color(scale, avg method, dithering method, 800 * 600 data(3 byte per pix(RGB))
			//73 : Load from Gray(scale, 800 * 600 gray data(1 byte per pix))
			//74 : Load from Color(scale, avg method, dithering method, 800 * 600 data(3 byte per pix(RGB))
			//75 : Trigger Buffer Print


			}
			// Closing Connexion
			boost::system::error_code ignored_error;
			boost::asio::write(con_handle->socket, boost::asio::buffer(end_token), boost::asio::transfer_all(), ignored_error);
		}




		if (!err) {
			do_async_read(con_handle);
		}
		else {
			std::cerr << "We had an error: " << err.message() << std::endl;
			m_connections.erase(con_handle);
		}
	}
	std::string end_token = "ENDOFTRANSMISSION";
	std::string packet_start_token = "STARTTRANSMISSION";
	void do_async_read(con_handle_t con_handle) {
		auto handler = boost::bind(&TCPDisplayServer::handle_read, this, con_handle, boost::asio::placeholders::error, boost::asio::placeholders::bytes_transferred);
		boost::asio::async_read_until(con_handle->socket, con_handle->read_buffer, end_token, handler);
	}

	void start_accept()
	{
		auto con_handle = m_connections.emplace(m_connections.begin(), m_ioservice);
		auto handler = boost::bind(&TCPDisplayServer::handle_accept, this, con_handle, boost::asio::placeholders::error);
		m_acceptor.async_accept(con_handle->socket, handler);
	}

	// handle_accept() is called when the asynchronous accept operation 
	// initiated by start_accept() finishes. It services the client request
	void handle_accept(con_handle_t con_handle, boost::system::error_code const & err)
	{

		if (!err) {
			std::cout << "Connection from: " << con_handle->socket.remote_endpoint().address().to_string() << "\n";

			do_async_read(con_handle);
		}
		else {
			std::cerr << "We had an error: " << err.message() << std::endl;
			m_connections.erase(con_handle);
		}
		start_accept();

	}

	void listen(uint16_t port) {
		readConfig();

		puts("TCP Starting ...");
		auto endpoint = boost::asio::ip::tcp::endpoint(boost::asio::ip::tcp::v4(), port);
		m_acceptor.open(endpoint.protocol());
		m_acceptor.set_option(boost::asio::ip::tcp::acceptor::reuse_address(true));
		m_acceptor.bind(endpoint);
		m_acceptor.listen();
		start_accept();

		puts("TCP Started");
		m_ioservice.run();
	}

	void write(con_handle_t con_handle, std::string message)
	{
		std::string outMessage = packet_start_token;

		ByteArrayToInteger converter;
		converter.integer = message.length();

		outMessage += converter.byteArray[0];
		outMessage += converter.byteArray[1];
		outMessage += converter.byteArray[2];
		outMessage += converter.byteArray[3];

		outMessage += message;

		boost::system::error_code ignored_error;
		boost::asio::write(con_handle->socket, boost::asio::buffer(outMessage), boost::asio::transfer_all(), ignored_error);
	}

	void write(con_handle_t con_handle, std::vector<char> data)
	{
		ByteArrayToInteger converter;
		converter.integer = data.size();

		data.insert(data.begin(), converter.byteArray[3]);
		data.insert(data.begin(), converter.byteArray[2]);
		data.insert(data.begin(), converter.byteArray[1]);
		data.insert(data.begin(), converter.byteArray[0]);

		for (int i = packet_start_token.size() - 1; i >= 0; --i) {
			data.insert(data.begin(), packet_start_token[i]);
		}

		/*
		std::vector<char> startTokenData(packet_start_token.begin(), packet_start_token.end());

		std::copy(packet_start_token.begin(), packet_start_token.end(), std::front_inserter(data));
		*/


		boost::system::error_code ignored_error;
		boost::asio::write(con_handle->socket, boost::asio::buffer(data), boost::asio::transfer_all(), ignored_error);
	}

	void write(con_handle_t con_handle, int data)
	{
		std::vector<char> outData;
		ByteArrayToInteger conv;

		conv.integer = data;

		outData.push_back(conv.byteArray[0]);
		outData.push_back(conv.byteArray[1]);
		outData.push_back(conv.byteArray[2]);
		outData.push_back(conv.byteArray[3]);

		write(con_handle, outData);
	}

	void write(con_handle_t con_handle, char data)
	{
		std::vector<char> outData;

		outData.push_back(data);

		write(con_handle, outData);
	}

	Config* config = new Config();

	void readConfig()
	{
		try {
			std::ifstream ifs("/etc/DisplayServer/config.xml");

			boost::archive::xml_iarchive ia(ifs);
			ia >> BOOST_SERIALIZATION_NVP(config);

			ifs.close();
		}
		catch (...)
		{
		}
	}

	void writeConfig()
	{
		boost::filesystem::path rootPath("/etc/DisplayServer/");
		boost::system::error_code returnedError;
		boost::filesystem::create_directories(rootPath, returnedError);

		std::ofstream ofs("/etc/DisplayServer/config.xml");

		boost::archive::xml_oarchive oa(ofs);
		oa << BOOST_SERIALIZATION_NVP(config);

		ofs.close();
	}

private:
	void PowerOn(int I2Chandle) {
		Byte byteConverter;

		char reading = (char)wiringPiI2CReadReg8(I2Chandle, 1);

		byteConverter.byte = reading;
		byteConverter.bit5 = true;

		wiringPiI2CWriteReg8(I2Chandle, 1, byteConverter.byte);

		delayMicroseconds(100);

		reading = (char)wiringPiI2CReadReg8(I2Chandle, 1);

		byteConverter.byte = reading;
		byteConverter.bit7 = true;

		wiringPiI2CWriteReg8(I2Chandle, 1, byteConverter.byte);

		delay(50);
	}

	void PowerOff(int I2Chandle) {
		Byte byteConverter;

		char reading = (char)wiringPiI2CReadReg8(I2Chandle, 1);

		byteConverter.byte = reading;
		byteConverter.bit6 = true;

		wiringPiI2CWriteReg8(I2Chandle, 1, byteConverter.byte);

		delayMicroseconds(100);

		reading = (char)wiringPiI2CReadReg8(I2Chandle, 1);

		byteConverter.byte = reading;
		byteConverter.bit5 = false;

		wiringPiI2CWriteReg8(I2Chandle, 1, byteConverter.byte);
	}

	std::string exec(const char* cmd) {
		std::array<char, 128> buffer;
		std::string result;
		std::shared_ptr<FILE> pipe(popen(cmd, "r"), pclose);
		if (!pipe) throw std::runtime_error("popen() failed!");
		while (!feof(pipe.get())) {
			if (fgets(buffer.data(), 128, pipe.get()) != nullptr)
				result += buffer.data();
		}
		return result;
	}


	// ==== Screen Control ====

	const char PIN_D0 = 0;
	const char PIN_D1 = 1;
	const char PIN_D2 = 2;
	const char PIN_D3 = 3;
	const char PIN_D4 = 4;
	const char PIN_D5 = 5;
	const char PIN_D6 = 6;
	const char PIN_D7 = 7;

	const char PIN_SPH = 12;
	const char PIN_OE = 13;
	const char PIN_LE = 14;
	const char PIN_CL = 10;

	const char PIN_CKV = 11;
	const char PIN_SPV = 21;

	const char PIN_CE21 = 22;
	const char PIN_CE22 = 23;


	const char PIN_GMODE = 26;
	const char PIN_GMODE1 = 26;
	const char PIN_GMODE2 = 30;

	void Screen_Init()
	{
		// Set Output
		pinMode(PIN_D0, OUTPUT);
		pinMode(PIN_D1, OUTPUT);
		pinMode(PIN_D2, OUTPUT);
		pinMode(PIN_D3, OUTPUT);
		pinMode(PIN_D4, OUTPUT);
		pinMode(PIN_D5, OUTPUT);
		pinMode(PIN_D6, OUTPUT);
		pinMode(PIN_D7, OUTPUT);

		pinMode(PIN_SPH, OUTPUT);
		pinMode(PIN_OE, OUTPUT);
		pinMode(PIN_LE, OUTPUT);
		pinMode(PIN_CL, OUTPUT);

		pinMode(PIN_CKV, OUTPUT);
		pinMode(PIN_SPV, OUTPUT);

		pinMode(PIN_CE21, OUTPUT);
		pinMode(PIN_CE22, OUTPUT);

		pinMode(PIN_GMODE, OUTPUT);

		// Set pull down
		pullUpDnControl(PIN_D0, PUD_DOWN);
		pullUpDnControl(PIN_D1, PUD_DOWN);
		pullUpDnControl(PIN_D2, PUD_DOWN);
		pullUpDnControl(PIN_D3, PUD_DOWN);
		pullUpDnControl(PIN_D4, PUD_DOWN);
		pullUpDnControl(PIN_D5, PUD_DOWN);
		pullUpDnControl(PIN_D6, PUD_DOWN);
		pullUpDnControl(PIN_D7, PUD_DOWN);

		pullUpDnControl(PIN_SPH, PUD_DOWN);
		pullUpDnControl(PIN_OE, PUD_DOWN);
		pullUpDnControl(PIN_LE, PUD_DOWN);
		pullUpDnControl(PIN_CL, PUD_DOWN);

		pullUpDnControl(PIN_CKV, PUD_DOWN);
		pullUpDnControl(PIN_SPV, PUD_DOWN);

		pullUpDnControl(PIN_CE21, PUD_DOWN);
		pullUpDnControl(PIN_CE22, PUD_DOWN);

		pullUpDnControl(PIN_GMODE, PUD_DOWN);
	}

	// ==== Frame ====
	void Screen_StartFrame()
	{
		digitalWriteByte(0);

		digitalWrite(PIN_SPH, LOW);
		digitalWrite(PIN_OE, HIGH);
		digitalWrite(PIN_LE, LOW);
		digitalWrite(PIN_CL, LOW);

		digitalWrite(PIN_CKV, HIGH);
		digitalWrite(PIN_SPV, HIGH);

		digitalWrite(PIN_CE21, LOW);
		digitalWrite(PIN_CE22, LOW);

		digitalWrite(PIN_GMODE, HIGH);

	}

	void Screen_EndFrame()
	{
		digitalWriteByte(0);

		digitalWrite(PIN_SPH, LOW);
		digitalWrite(PIN_OE, LOW);
		digitalWrite(PIN_LE, LOW);
		digitalWrite(PIN_CL, LOW);

		digitalWrite(PIN_CKV, LOW);
		digitalWrite(PIN_SPV, LOW);

		digitalWrite(PIN_CE21, LOW);
		digitalWrite(PIN_CE22, LOW);

		digitalWrite(PIN_GMODE, LOW);
	}


	// ==== Pulses ====
	const int firstPulseClockCount = 3;
	void Screen_FirstPulse()
	{
		digitalWrite(PIN_CE21, LOW);
		digitalWrite(PIN_CE22, HIGH);
		delayMicroseconds(1);
		digitalWrite(PIN_SPV, HIGH);
		delayMicroseconds(1);
		digitalWrite(PIN_CKV, LOW);
		delayMicroseconds(1);
		digitalWrite(PIN_SPV, LOW);
		delayMicroseconds(1);
		digitalWrite(PIN_CKV, HIGH);
		delayMicroseconds(1);
		digitalWrite(PIN_SPV, HIGH);
		delayMicroseconds(1);
		for (int i = 0; i < firstPulseClockCount; i++)
			Screen_CKVClock();
	}
	const int secondPulseClockCount = 4;
	void Screen_SecondPulse()
	{
		digitalWrite(PIN_CE21, HIGH);
		digitalWrite(PIN_CE22, LOW);
		delayMicroseconds(1);
		digitalWrite(PIN_SPV, HIGH);
		delayMicroseconds(1);
		digitalWrite(PIN_CKV, LOW);
		delayMicroseconds(1);
		digitalWrite(PIN_SPV, LOW);
		delayMicroseconds(1);
		digitalWrite(PIN_CKV, HIGH);
		delayMicroseconds(1);
		digitalWrite(PIN_SPV, HIGH);
		delayMicroseconds(1);
		for (int i = 0; i < secondPulseClockCount; i++)
			Screen_CKVClock();
	}

	// ==== Clocks ====
	void Screen_CKVClock()
	{
		digitalWrite(PIN_CKV, LOW);
		delayMicroseconds(1);
		digitalWrite(PIN_CKV, HIGH);
		delayMicroseconds(1);
	}

	void Screen_CLClock()
	{
		digitalWrite(PIN_CL, HIGH);
		digitalWrite(PIN_CL, LOW);
	}

	// ==== Row ====
	void Screen_StartRow()
	{
		digitalWrite(PIN_SPH, HIGH);
	}

	void Screen_EndRow()
	{
		digitalWrite(PIN_SPH, LOW);
	}

	void Screen_WriteRow()
	{
		digitalWrite(PIN_LE, HIGH);
		digitalWrite(PIN_LE, LOW);

		digitalWrite(PIN_OE, LOW);
		digitalWrite(PIN_OE, HIGH);
	}

	void Screen_WriteDataBW(char px1, char px2, char px3, char px4)
	{
		Byte byteConverter;
		byteConverter.byte = 0;

		// 10 white
		// 01 black
		// 00-11 uncahnged ?

		switch (px1)
		{
		case 0:
			byteConverter.bit7 = true;
			byteConverter.bit6 = false;
			break;
		case 1:
			byteConverter.bit7 = false;
			byteConverter.bit6 = true;
			break;
		}

		switch (px2)
		{
		case 0:
			byteConverter.bit5 = true;
			byteConverter.bit4 = false;
			break;
		case 1:
			byteConverter.bit5 = false;
			byteConverter.bit4 = true;
			break;
		}

		switch (px3)
		{
		case 0:
			byteConverter.bit3 = true;
			byteConverter.bit2 = false;
			break;
		case 1:
			byteConverter.bit3 = false;
			byteConverter.bit2 = true;
			break;
		}

		switch (px4)
		{
		case 0:
			byteConverter.bit1 = true;
			byteConverter.bit0 = false;
			break;
		case 1:
			byteConverter.bit1 = false;
			byteConverter.bit0 = true;
			break;
		}

		digitalWriteByte(byteConverter.byte);
	}

	void Screen_WriteDataGray(char& px1, char& px2, char& px3, char& px4)
	{
		Byte byteConverter;
		byteConverter.byte = 0;

		// 10 white
		// 01 black
		// 00-11 uncahnged ?

		if (px1 != 0)
		{
			byteConverter.bit7 = false;
			byteConverter.bit6 = true;

			px1--;
		}

		if (px2 != 0)
		{
			byteConverter.bit5 = false;
			byteConverter.bit4 = true;

			px2--;
		}

		if (px3 != 0)
		{
			byteConverter.bit3 = false;
			byteConverter.bit2 = true;

			px3--;
		}

		if (px4 != 0)
		{
			byteConverter.bit1 = false;
			byteConverter.bit0 = true;

			px4--;
		}

		digitalWriteByte(byteConverter.byte);
	}

	std::vector<char> buffer;
	const int firstHalfLineNB = 301;
	const int secondHalfLineNB = 300;
	const int screenWidth = firstHalfLineNB + secondHalfLineNB;
	const int screenHeight = 800;
	const int screenHeightInData = screenHeight / 4;

	void Screen_WriteBufferBW(int I2Chandle)
	{
		Screen_Init();

		PowerOn(I2Chandle);

		Screen_WriteFrameBW(I2Chandle);

		PowerOff(I2Chandle);
	}

	void Screen_WriteBufferGray(int I2Chandle)
	{
		Screen_Init();

		PowerOn(I2Chandle);

		Screen_WriteFrameGray(I2Chandle);

		PowerOff(I2Chandle);
	}

	int BWPasses = 16;
	void Screen_WriteFrameBW(int I2Chandle)
	{
		for (int frame = 0; frame < BWPasses; frame++)
		{
			Screen_StartFrame();

			Screen_FirstPulse();
			int offset = 0;
			for (int i = 0; i < firstHalfLineNB; i++)
			{
				Screen_StartRow();
				for (int j = 0; j < screenHeightInData; j++)
				{
					Screen_WriteDataBW(buffer[offset++], buffer[offset++], buffer[offset++], buffer[offset++]);

					Screen_CLClock();
				}
				Screen_EndRow();
				Screen_WriteRow();

				Screen_CKVClock();
			}

			Screen_SecondPulse();

			//offset = 300 * 200 * 4;

			for (int i = 0; i < secondHalfLineNB; i++)
			{
				Screen_StartRow();
				for (int j = 0; j < screenHeightInData; j++)
				{
					Screen_WriteDataBW(buffer[offset++], buffer[offset++], buffer[offset++], buffer[offset++]);

					Screen_CLClock();
				}
				Screen_EndRow();
				Screen_WriteRow();

				Screen_CKVClock();
			}

			Screen_EndFrame();

		}

	}
	int grayScales = 8;
	void Screen_WriteFrameGray(int I2Chandle)
	{
		// Whitewashing 
		for (int frame = 0; frame < grayScales * 2; frame++)
		{
			Screen_StartFrame();

			Screen_FirstPulse();
			for (int i = 0; i < firstHalfLineNB; i++)
			{
				Screen_StartRow();
				for (int j = 0; j < screenHeightInData; j++)
				{
					Screen_WriteDataBW(0, 0, 0, 0);

					Screen_CLClock();
				}
				Screen_EndRow();
				Screen_WriteRow();

				Screen_CKVClock();
			}

			Screen_SecondPulse();

			for (int i = 0; i < secondHalfLineNB; i++)
			{
				Screen_StartRow();
				for (int j = 0; j < screenHeightInData; j++)
				{
					Screen_WriteDataBW(0, 0, 0, 0);

					Screen_CLClock();
				}
				Screen_EndRow();
				Screen_WriteRow();

				Screen_CKVClock();
			}

			Screen_EndFrame();
		}

		for (int frame = 0; frame < grayScales; frame++)
		{

			Screen_StartFrame();

			Screen_FirstPulse();
			int offset = 0;
			for (int i = 0; i < firstHalfLineNB; i++)
			{
				Screen_StartRow();
				for (int j = 0; j < screenHeightInData; j++)
				{
					Screen_WriteDataGray(buffer[offset++], buffer[offset++], buffer[offset++], buffer[offset++]);

					Screen_CLClock();
				}
				Screen_EndRow();
				Screen_WriteRow();

				Screen_CKVClock();
			}

			Screen_SecondPulse();

			for (int i = 0; i < secondHalfLineNB; i++)
			{
				Screen_StartRow();
				for (int j = 0; j < screenHeightInData; j++)
				{
					Screen_WriteDataGray(buffer[offset++], buffer[offset++], buffer[offset++], buffer[offset++]);

					Screen_CLClock();
				}
				Screen_EndRow();
				Screen_WriteRow();

				Screen_CKVClock();
			}

			Screen_EndFrame();
		}

	}
};

void StartTCPDisplayServer() {
	auto srv = TCPDisplayServer();
	srv.listen(2501);
}
