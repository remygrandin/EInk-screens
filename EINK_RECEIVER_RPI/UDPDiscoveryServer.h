#pragma once
#include <boost/asio.hpp>
void StartUDPDiscoveryServer();
/*
class UDPDiscoveryServer
{
public:
	UDPDiscoveryServer(boost::asio::io_service& io_service);
private :
	std::string computedResponse;
	std::string exec(const char* cmd);
	void start_receive();
	void handle_receive(const boost::system::error_code& error, std::size_t);

	void handle_send(boost::shared_ptr<std::string>, const boost::system::error_code&, std::size_t);


	udp::socket socket_;
	udp::endpoint remote_endpoint_;
	boost::array<char, 1> recv_buffer_;

};*/

