#include "UDPDiscoveryServer.h"
#include <string>
#include <boost/array.hpp>
#include <boost/bind.hpp>
#include <boost/asio.hpp>
#include <regex>
#include "Config.h"

using boost::asio::ip::udp;

class UDPDiscoveryServer
{
public:
	UDPDiscoveryServer(boost::asio::io_service& io_service)
		: socket_(io_service, udp::endpoint(udp::v4(), 2501))
	{
		

		puts("UDP Started");

		start_receive();
	}

private:

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

	void start_receive()
	{
		socket_.async_receive_from(
			boost::asio::buffer(recv_buffer_), remote_endpoint_,
			boost::bind(&UDPDiscoveryServer::handle_receive, this,
				boost::asio::placeholders::error,
				boost::asio::placeholders::bytes_transferred));


	}

	void handle_receive(const boost::system::error_code& error,
		std::size_t /*bytes_transferred*/)
	{
		if (!error || error == boost::asio::error::message_size)
		{


			std::string ifconfigResult = exec("ifconfig");

			std::smatch match;

			std::regex ipRegex(R"aa(wlan0[\s\S]*inet addr:([\d.]{7,15}))aa");

			std::string outputIp = "IP";
			using reg_itr = std::regex_token_iterator<std::string::iterator>;
			for (reg_itr it{ ifconfigResult.begin(), ifconfigResult.end(), ipRegex, 1 }, end{}; it != end;) {
				outputIp = *it++;
			}

			std::regex hwRegex(R"aa(wlan0[\s\S]*HWaddr ([a-f\d:]{17}))aa");

			std::string outputMac = "MAC";
			using reg_itr = std::regex_token_iterator<std::string::iterator>;
			for (reg_itr it{ ifconfigResult.begin(), ifconfigResult.end(), hwRegex, 1 }, end{}; it != end;) {
				outputMac = *it++;
			}

			

			std::string idStr = "NoIDSet";


			try {
				Config* config = new Config();

				std::ifstream ifs("/etc/DisplayServer/config.xml");

				boost::archive::xml_iarchive ia(ifs);
				ia >> BOOST_SERIALIZATION_NVP(config);

				ifs.close();

				idStr = config->deviceId;
			}
			catch (...)
			{
			}
			
			std::string computedResponse = "|DISCOVERY_RESPONSE;" + outputIp + ";" + outputMac + ";" + idStr + "|";

			boost::shared_ptr<std::string> message(new std::string(computedResponse));

			socket_.async_send_to(boost::asio::buffer(*message), remote_endpoint_,
				boost::bind(&UDPDiscoveryServer::handle_send, this, message,
					boost::asio::placeholders::error,
					boost::asio::placeholders::bytes_transferred));

			start_receive();
		}
	}

	void handle_send(boost::shared_ptr<std::string> /*message*/,
		const boost::system::error_code& /*error*/,
		std::size_t /*bytes_transferred*/)
	{
	}

	udp::socket socket_;
	udp::endpoint remote_endpoint_;
	boost::array<char, 1> recv_buffer_;
};

void StartUDPDiscoveryServer() {
	boost::asio::io_service io_service;
	UDPDiscoveryServer server(io_service);
	io_service.run();
}
